using UnityEngine;
using System.Collections;

public class WeaponScript : MonoBehaviour
{
	public static WeaponScript scr;
	
	//Quaterion
	static Quaternion quad_zero = new Quaternion(0,0,0,0);
	
	public Transform proj_point;
	public GameObject[] weapons;
	public Renderer[] muzzle;
	public AudioClip[] weapsnd;
	public AudioClip[] weaprld;
	public Transform tr_weapon;
	public Transform tr_knife;
	public Object[] impact;
	public Renderer[] sweapons;
	public Renderer[] smuzzle;
	GUIText clips;
	
	float[] weapon_speed = {8.0f,0.025f,0.05f,0.01f};
	
	//Ammo handling
	int[] ammo_curr = {1,15,30,10};
	int[] ammo_clip = {1,15,30,10};
	int[] ammo_maxc = {0,30,90,10};
	int[] ammo_max = {0,30,90,10};
	int[,] weap_dam =
	{
		{50,70},
		{20,30},
		{15,20},
		{50,101}
	};
	int ammo_diff;
	
	Vector3 pos_weapon;
	
	//Current weapon
	int wid, oldwid;
	bool firing, rlding, changing, rldwait;
	bool actdir;
	
	//Knife animation
	Quaternion knife_normal;
	Quaternion knife_attack;
	Quaternion knife_attack2;
	
	//Hud
	public Texture hud_tex;
	Rect hud_area;
	Rect hud_ammo;
	Color hud_color;
	float hud_fwidth;
	
	//Mouse wheel switch
	int tmpwid;
	float wheel;
	
	//Raycasting
	RaycastHit hit;
	Collider tmpcol;
	int layer;
	
	bool freeze;
	
	//Pickup sound
	public AudioClip snd_pickup;
	
	//Buffered weapon
	public NetworkView sync_weapon;
	
	public bool Sniper() { return !freeze&&!firing&&!rlding&&wid==3; }
	string GetClips() { return ((ammo_maxc[wid]+ammo_clip[wid]-1)/ammo_clip[wid]).ToString(); }
	
	void Start()
	{
		scr = this;
		freeze = false;
		//Weapon bar
		hud_area = new Rect(
			32*NetworkMenu.sc_w,
			Screen.height-32*NetworkMenu.sc_h,
			hud_tex.width*NetworkMenu.sc_h,
			hud_tex.height*NetworkMenu.sc_h);
		hud_ammo = new Rect(0,0,hud_area.width,hud_area.height);
		hud_color = new Color(0.5f,0.5f,0f);
		hud_fwidth = hud_area.width;
		//Other
		pos_weapon = tr_weapon.localPosition;
		wid = 2;
		oldwid = 2;
		sweapons[wid].renderer.enabled = true;
		//Change to invisible shadow caster
		for(int i = 0; i < sweapons.Length; i++)
			sweapons[i].renderer.material = MainScript.scr.shadow_mat;
		sweapons[wid].renderer.material = MainScript.scr.shadow_mat;
		clips = GameObject.Find("hud").transform.Find("ammo").transform.Find("clips").guiText;
		clips.text = GetClips();
		//Knife
		knife_normal = tr_knife.localRotation;
		knife_attack = Quaternion.Euler(0,-80,90);
		knife_attack2 = Quaternion.Euler(0,40,90);
		for(int i = 0; i < weapons.Length; i++)
			if(wid != i)
				weapons[i].SetActiveRecursively(false);
		Network.RemoveRPCs(sync_weapon.viewID);
		sync_weapon.RPC("Call1",RPCMode.AllBuffered,wid);
	}
	
	void ChangeWeapon(int id)
	{
		if(id == wid) return;
		if(wid == 3)
			MainScript.scr.DisableScope();
		rlding = true;
		changing = true;
		oldwid = wid;
		wid = id;
	}
	
	void Update()
	{
		if(freeze) return;
		
		if(!firing && !rlding)
		{
			wheel = Input.GetAxis("Mouse ScrollWheel");
			if(wheel < 0)
			{
				tmpwid = wid+1;
				if(tmpwid >= weapons.Length) tmpwid = 0;
				ChangeWeapon(tmpwid);
			}
			else if(wheel > 0)
			{
				tmpwid = wid-1;
				if(tmpwid < 0) tmpwid = weapons.Length-1;
				ChangeWeapon(tmpwid);
			}
		}
	}
	
	void DoImpact(float dist)
	{
		bool skip = false;
		next:
		tmpcol = hit.collider;
		layer = tmpcol.gameObject.layer;
		if(layer == 9 || layer == 10)
		{
			networkView.RPC("SendImpact",RPCMode.All,hit.point,0);
			if(layer == 9)
				tmpcol.transform.root.GetComponent<MainScript>().SendDamage(Random.Range(weap_dam[wid,0],weap_dam[wid,1]), transform.position);
			else
				tmpcol.transform.root.GetComponent<MainScript>().SendDamage(101, transform.position);
		}
		else if(tmpcol is TerrainCollider)
			networkView.RPC("SendImpact",RPCMode.All,hit.point,1);
		else if(tmpcol is MeshCollider)
			networkView.RPC("SendImpact",RPCMode.All,hit.point,2);
		else
		{
			networkView.RPC("SendImpact",RPCMode.All,hit.point,3);
			if(Physics.Raycast(proj_point.position,proj_point.forward,out hit,dist,1003) && !skip)
			{
				skip = true;
				goto next;
			}
		}
	}
	
	void FixedUpdate()
	{
		if(freeze) return;
		
		if(!firing && !rlding)
		{
			if(Input.GetKey(KeyCode.Alpha1)) ChangeWeapon(0);
			else if(Input.GetKey(KeyCode.Alpha2)) ChangeWeapon(1);
			else if(Input.GetKey(KeyCode.Alpha3)) ChangeWeapon(2);
			else if(Input.GetKey(KeyCode.Alpha4)) ChangeWeapon(3);
			
			else if(Input.GetButton("Fire1") && ammo_curr[wid] > 0)
			{
				firing = true;
				if(wid > 0)
				{
					if(wid > 1)
						MainScript.scr.GetAnim("fr");
					else
						MainScript.scr.GetAnim("fp");
					light.enabled = true;
					muzzle[wid].enabled = true;
					ammo_curr[wid]--;
					hud_area.width = (float)ammo_curr[wid] / ammo_clip[wid] * hud_fwidth;
					if(Physics.Raycast(proj_point.position,proj_point.forward,out hit,200.0f))
						DoImpact(200.0f);
				}
				else
				{
					MainScript.scr.GetAnim("kn");
					tr_knife.localRotation = knife_attack;
					if(Physics.Raycast(proj_point.position,proj_point.forward,out hit,2.0f))
						DoImpact(2.0f);
				}
				audio.PlayOneShot(weapsnd[wid]);
				networkView.RPC("SendShot",RPCMode.Others,wid);
			}
			else if(Input.GetButton("Rld") && wid > 0 && ammo_curr[wid] < ammo_clip[wid] && ammo_maxc[wid] > 0)
			{
				ammo_diff = ammo_clip[wid] - ammo_curr[wid];
				if(ammo_diff <= ammo_maxc[wid])
				{
					ammo_maxc[wid] -= ammo_diff;
					ammo_curr[wid] = ammo_clip[wid];
				}
				else
				{
					ammo_curr[wid] += ammo_maxc[wid];
					ammo_maxc[wid] = 0;
				}
				MainScript.scr.GetAnim("rl");
				rlding = true;
				changing = false;
				light.enabled = false;
				muzzle[wid].enabled = false;
				audio.PlayOneShot(weaprld[wid]);
				networkView.RPC("SendReload",RPCMode.Others,wid);
			}
		}
		else if(firing)
		{
			if(wid > 0)
			{
				if(actdir)
				{
					if(tr_weapon.localPosition.z < 0)
					{
						pos_weapon.z+=weapon_speed[wid];
						tr_weapon.localPosition = pos_weapon;
					}
					else
					{
						pos_weapon = Vector3.zero;
						tr_weapon.localPosition = Vector3.zero;
						actdir = false;
						firing = false;
					}
				}
				else
				{
					if(tr_weapon.localPosition.z > -0.1f)
					{
						pos_weapon.z-=weapon_speed[wid];
						tr_weapon.localPosition = pos_weapon;
					}
					else
					{
						light.enabled = false;
						muzzle[wid].enabled = false;
						actdir = true;
					}
				}
			}
			else
			{
				if(actdir)
				{
					tr_knife.localRotation = Quaternion.RotateTowards(tr_knife.localRotation,knife_normal,weapon_speed[wid]);
					if(tr_knife.localRotation == knife_normal)
					{
						firing = false;
						actdir = false;
					}
				}
				else
				{
					tr_knife.localRotation = Quaternion.RotateTowards(tr_knife.localRotation,knife_attack2,weapon_speed[wid]);
					if(tr_knife.localRotation == knife_attack2)
						actdir = true;
				}
			}
		}
		else if(rlding && !rldwait)
		{
			if(actdir)
			{
				if(tr_weapon.localPosition.y < 0)
				{
					pos_weapon.y+=0.025f;
					tr_weapon.localPosition = pos_weapon;
				}
				else
				{
					pos_weapon = Vector3.zero;
					tr_weapon.localPosition = Vector3.zero;
					clips.text = GetClips();
					hud_area.width = (float)ammo_curr[wid] / ammo_clip[wid] * hud_fwidth;
					actdir = false;
					rlding = false;
				}
			}
			else
			{
				if(tr_weapon.localPosition.y > -0.2f)
				{
					pos_weapon.y-=0.025f;
					tr_weapon.localPosition = pos_weapon;
				}
				else
				{
					actdir = true;
					if(!changing)
					{
						rldwait = true;
						StartCoroutine(ReloadWait());
					}
					else
					{
						weapons[oldwid].SetActiveRecursively(false);
						weapons[wid].SetActiveRecursively(true);
						Network.RemoveRPCs(sync_weapon.viewID);
						sync_weapon.RPC("Call1",RPCMode.AllBuffered,wid);
					}
				}
			}
		}
	}
	
	[RPC]
	void SendShot(int id)
	{
		audio.PlayOneShot(weapsnd[id]);
		if(id > 0)
		{
			light.enabled = true;
			smuzzle[id].enabled = true;
			StartCoroutine(ResetFire(id));
		}
	}
	
	[RPC]
	void SendImpact(Vector3 pos, int id)
	{
		GameObject.Instantiate(impact[id],pos,quad_zero);
	}
	
	[RPC]
	void SendReload(int id)
	{
		audio.PlayOneShot(weaprld[id]);
	}
	
	public void SendWeapon(int id)
	{
		for(int i = 0; i < sweapons.Length; i++)
			sweapons[i].enabled = false;
		for(int i = 1; i < sweapons.Length; i++)
			smuzzle[i].enabled = false;
		light.enabled = false;
		sweapons[id].enabled = true;
	}
	
	public void SetHudState(bool input)
	{
		freeze = !input;
		
		if(!input)
			for(int i = 0; i < weapons.Length; i++)
				weapons[i].SetActiveRecursively(false);
		else
		{
			tr_weapon.localPosition = Vector3.zero;
			light.enabled = false;
			if(wid > 0)
				muzzle[wid].enabled = false;
			weapons[wid].SetActiveRecursively(true);
			for(int i = 0; i < ammo_curr.Length; i++)
			{
				ammo_curr[i] = ammo_clip[i];
				ammo_maxc[i] = ammo_max[i];
			}
			hud_area.width = (float)ammo_curr[wid] / ammo_clip[wid] * hud_fwidth;
		}
	}
	
	public bool PickupAmmo(int wid)
	{
		if(ammo_maxc[wid] + ammo_curr[wid] <= ammo_max[wid])
		{
			ammo_maxc[wid] += ammo_clip[wid];
			clips.text = GetClips();
			audio.PlayOneShot(snd_pickup);
			return true;
		}
		return false;
	}
	
	IEnumerator ReloadWait()
	{
		yield return new WaitForSeconds(1.0f);
		rldwait = false;
	}
	
	IEnumerator ResetFire(int id)
	{
		yield return new WaitForSeconds(0.2f);
		if(id > 0)
		{
			smuzzle[id].enabled = false;
			light.enabled = false;
		}
	}
	
	void OnGUI()
	{
		if(Event.current.type == EventType.Repaint)
		{
			GUI.BeginGroup(hud_area);
			GUI.color = hud_color;
			GUI.DrawTexture(hud_ammo,hud_tex);
			GUI.EndGroup();
		}
	}
}