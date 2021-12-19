using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
	//Quaterion
	static Quaternion quad_zero = new Quaternion(0,0,0,0);
	
	//Main Camera
	public Transform cam;
	
	//Movement parameters
	public float speed = 2.0f;
	public float runspeed = 4.0f;
	public float crspeed = 2.0f;
	public float crrunspeed = 4.0f;
	public float jumpspeed = 5.0f;
	public float gravity = 8.0f;
	public float mousesens = 5.0f;
	public float minmouse_y = -60.0f, maxmouse_y = 60.0f;
	public float crouchdiff = 1.0f;
	public bool freeze = false;
	
	//Sounds
	public AudioClip[] snd_dirt;
	public AudioClip[] snd_water;
	public AudioClip snd_jump;
	int snd_idx;
	bool snd_wait;
	Vector3 move_vlc;
	
	//Water effect
	public Object splash;
	Collider water;
	bool inwater = false;
	float raylen;
	
	//Input
	float hor, ver;
	float mouse_x, mouse_y;
	bool run;
	
	//Crouch, jump
	bool crouch;
	bool crouching;
	public bool grounded;
	bool oldgrounded;

	//Movement
	Vector3 mdir = Vector3.zero;
	float inputmod;
	
	//Char controller
	CharacterController ctrl;
	
	//Crouch control
	float ctrlrad;
	float ctrlhei;
	float ctrlcrc;
	Vector3 crouchup;
	Vector3 cr_cam, st_cam;
	
	//Other
	Ray ray;
	RaycastHit hit;
	
	void Start()
	{
		//Load Char controller
		ctrl = GetComponent<CharacterController>();
		//Set crouching vars
		ctrlrad = ctrl.radius;
		ctrlhei = ctrl.height;
		ctrlcrc = ctrlhei - crouchdiff;
		raylen = ctrlhei/2;
		crouchup = new Vector3(0f,0.025f,0f);
		st_cam = cam.transform.localPosition;
		cr_cam = st_cam - new Vector3(0f,crouchdiff-0.2f,0f);
		ray.direction = transform.up;
		inputmod = 1f;
		crouch = false;
		crouching = false;
		grounded = false;
		snd_idx = 0;
		//Water
		water = GameObject.Find("Water").transform.Find("Water").collider;
	}
	
	void FixedUpdate()
	{
		//If frozen, disable unnecessary calculations
		if(freeze)
		{
			if(!grounded)
			{
				mdir.y -= gravity * Time.fixedDeltaTime;
				grounded = (ctrl.Move(mdir * Time.fixedDeltaTime) & CollisionFlags.Below) != 0;
			}
			if(Input.GetButton("Rsp"))
			{
				MainScript.scr.Respawn();
				freeze = false;
			}
			return;
		}
		
		//Input reading
		hor = Input.GetAxis("Horizontal");
		ver = Input.GetAxis("Vertical");
		
		//Limit diag speed
		if(hor != 0 && ver != 0) inputmod = 0.7071f;
		else inputmod = 1f;
		
		//Running
		run = Input.GetButton("Run");
		if(!crouch)
		{
			if(run)
			{
				mdir.x = hor * runspeed * inputmod;
				mdir.z = ver * runspeed * inputmod;
			}
			else
			{
				mdir.x = hor * speed * inputmod;
				mdir.z = ver * speed * inputmod;
			}
		}
		else
		{
			if(run)
			{
				mdir.x = hor * crrunspeed * inputmod;
				mdir.z = ver * crrunspeed * inputmod;
			}
			else
			{
				mdir.x = hor * crspeed * inputmod;
				mdir.z = ver * crspeed * inputmod;
			}
		}
		
		//Jumping
		if(grounded)
		{
			if(Input.GetButton("Jump"))
			{
				audio.PlayOneShot(snd_jump);
				networkView.RPC("SendJump",RPCMode.Others);
				mdir.y = jumpspeed;
			}
			
			//Animation
			if(ctrl.velocity.magnitude > 0.5f)
			{
				if(run && !crouch)
					MainScript.scr.GetAnim("rn");
				else
				{
					if(crouch)
						MainScript.scr.GetAnim("crw");
					else
						MainScript.scr.GetAnim("wk");
				}
			}
			else
			{
				if(!crouch)
					MainScript.scr.GetAnim("id");
				else
					MainScript.scr.GetAnim("cr");
			}
		}
		else
		{
			MainScript.scr.GetAnim("fl");
			mdir.y -= gravity * Time.fixedDeltaTime;
		}
		
		//Crouching
		if(crouching)
		{
			if(crouch)
			{
				if(ctrl.height < ctrlhei)
				{
					ctrl.height += 0.05f;
					transform.position += crouchup;
				}
				else
				{
					ctrl.height = ctrlhei;
					crouching = false;
					crouch = false;
				}
			}
			else
			{
				if(ctrl.height > ctrlcrc)
					ctrl.height -= 0.05f;
				else
				{
					ctrl.height = ctrlcrc;
					crouching = false;
					crouch = true;
				}
			}
		}
		
		//Moving, ground check
		mdir = transform.TransformDirection(mdir);
		grounded = (ctrl.Move(mdir * Time.fixedDeltaTime) & CollisionFlags.Below) != 0;
		
		//Impact effect
		move_vlc = ctrl.velocity;
		if(grounded != oldgrounded && move_vlc.y < -2.0f)
		{
			if(!inwater)
				audio.PlayOneShot(snd_dirt[snd_idx]);
			else
				audio.PlayOneShot(snd_water[snd_idx]);
		}
		oldgrounded = grounded;

		//Walk sound handling
		move_vlc.y = 0;
		if(!snd_wait && grounded && move_vlc.magnitude > 3.0f)
		{
			//Surface type
			if(!inwater)
			{
				audio.PlayOneShot(snd_dirt[snd_idx]);
				networkView.RPC("SendDirt",RPCMode.Others,snd_idx);
			}
			else
			{
				audio.PlayOneShot(snd_water[snd_idx]);
				networkView.RPC("SendDirt",RPCMode.Others,snd_idx);
			}
			
			//Walking, running sound
			snd_wait = true;
			if(run && !crouch)
				StartCoroutine(SndWait(0.3f));
			else
				StartCoroutine(SndWait(0.5f));
			
			//Changing sound
			snd_idx++;
			snd_idx%=snd_dirt.Length;
		}
		
		//Watersplash handling
		if(Physics.Raycast(transform.position,-transform.up,out hit,raylen))
		{
			if(hit.collider == water)
			{
				if(!inwater && ctrl.velocity.y < -2.0f)
					networkView.RPC("SendSplash",RPCMode.All,hit.point);
				inwater = true;
			}
			else inwater = false;
		}
		else if(water.bounds.Contains(transform.position)) inwater = true;
		else inwater = false;
	}
	
	[RPC]
	void SendJump()
	{
		audio.PlayOneShot(snd_jump);
	}
	
	[RPC]
	void SendDirt(int id)
	{
		audio.PlayOneShot(snd_dirt[id]);
	}
	
	[RPC]
	void SendWater(int id)
	{
		audio.PlayOneShot(snd_water[id]);
	}
	
	[RPC]
	void SendSplash(Vector3 pos)
	{
		Object.Instantiate(splash,pos,quad_zero);
	}
	
	void Update()
	{
		//If frozen, disable camera movement
		if(freeze) return;
		
		mouse_y += Input.GetAxis("Mouse Y") * mousesens;
		mouse_y = Mathf.Clamp(mouse_y, minmouse_y, maxmouse_y);
		cam.localRotation = Quaternion.Euler(-mouse_y, 0, 0);
		transform.Rotate(0, Input.GetAxis("Mouse X") * mousesens, 0);
		
		//Crouching
		if(Input.GetButtonDown("Crouch") && !crouching)
		{
			if(!crouch)
			{
				cam.transform.localPosition = cr_cam;
				raylen = ctrlcrc/2;
				crouching = true;
			}
			else
			{
				ray.origin = transform.position;
				if(!Physics.SphereCast(ray,ctrlrad,crouchdiff))
				{
					cam.transform.localPosition = st_cam;
					//transform.position += crouchup;
					raylen = ctrlhei/2;
					crouching = true;
				}
			}
		}
	}
	
	IEnumerator SndWait(float time)
	{
		yield return new WaitForSeconds(time);
		snd_wait = false;
	}
}