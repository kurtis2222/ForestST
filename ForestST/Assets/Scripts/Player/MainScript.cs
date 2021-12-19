using UnityEngine;
using System.Collections;

public class MainScript : MonoBehaviour
{
	public static MainScript scr;
	PlayerControl ctrl;
	
	//Skin
	public GameObject skin;
	public Material shadow_mat;
	
	//Hud
	public Texture hud_tex;
	Rect hud_area;
	Rect hud_health;
	Color hud_color;
	float hud_fwidth;
	public static int health;
	static float fov;
	static float msens;
	static float ssens;
	
	//SVD Scope
	GUITexture[] svd;
	bool scope;
	//FOV
	Camera[] cam;
	
	//Hit indicator
	GUITexture hit_texture;
	public Transform hit_ind;
	Vector3 hit_dir;
	int hit_time;
	BoxCollider[] cols;
	
	//Crosshair
	public GUITexture cross;
	
	//Score
	public GameObject score;
	
	//Deathlist
	static GUIText dlist;
	static string dltmp = null;
	static int dlline = 0;
	const int MAX_LINES = 5;
	static int dres = 0;
	
	//Death info
	static GUIText dinfo;
	
	//Kill info
	static GUIText kinfo;
	
	//Esc menu
	static GUIText escmenu;
	static bool escopen;
	
	//Buffered death
	public NetworkView sync_state;
	
	void Awake()
	{
		//Cross-animation play
		skin.animation["fr"].layer = skin.animation["fp"].layer =
			skin.animation["kn"].layer = skin.animation["rl"].layer = 1;
		//Remove conflicting components
		if(!networkView.isMine)
		{
			Destroy(transform.GetChild(0).gameObject);
			Destroy(hit_ind.gameObject);
			cols = skin.GetComponentsInChildren<BoxCollider>();
			enabled = false;
			return;
		}
		scr = this;
		name = "Player";
		GetComponent<PlayerControl>().enabled = true;
		GetComponent<WeaponScript>().enabled = true;
		Transform thud = GameObject.Find("hud").transform;
		hit_texture = thud.Find("hit").guiTexture;
		cross = thud.Find("cross").guiTexture;
		score = thud.Find("score").gameObject;
		dlist = thud.Find("HudDeath").guiText;
		dinfo = thud.Find("HudResp").guiText;
		kinfo = thud.Find("HudKill").guiText;
		escmenu = thud.Find("HudEsc").guiText;
		score.SetActiveRecursively(false);
		//Change to invisible shadow caster
		skin.transform.Find("Skin_Mesh").renderer.material = shadow_mat;
		hit_dir = hit_ind.localRotation.eulerAngles;
		hit_time = 0;
		foreach(BoxCollider c in skin.GetComponentsInChildren<BoxCollider>())
			Destroy(c);
		ctrl = GetComponent<PlayerControl>();
		StartCoroutine(ResetHit());
		//Health bar
		health = 100;
		hud_area = new Rect(
			32*NetworkMenu.sc_w,
			Screen.height-64*NetworkMenu.sc_h,
			hud_tex.width*NetworkMenu.sc_h,
			hud_tex.height*NetworkMenu.sc_h);
		hud_health = new Rect(0,0,hud_area.width,hud_area.height);
		hud_color = new Color(0f,0.5f,0f);
		hud_fwidth = hud_area.width;
		hud_area.width = (float)health / 100 * hud_fwidth;
		//Scope
		float width = Screen.width, height = Screen.height;
		float tmp = width/2-height/2;
		svd = thud.Find("svd").GetComponentsInChildren<GUITexture>();
		svd[1].pixelInset = new Rect(tmp,0,height,height);
		svd[0].pixelInset = new Rect(0,0,tmp,height);
		tmp = width/2+height/2;
		svd[2].pixelInset = new Rect(tmp,0,width-tmp,height);
		scope = false;
		//Camera
		cam = new Camera[2];
		cam[0] = ctrl.cam.GetChild(0).camera;
		cam[1] = cam[0].transform.GetChild(0).camera;
		//Setup player
		if(Options.FOV != 0)
		{
			fov = Options.FOV;
			cam[0].fov = fov;
			cam[1].fov = fov;
		}
		if(Options.MouseSens > 0)
		{
			if(Options.InvertMouse)
				msens = -Options.MouseSens;
			else
				msens = Options.MouseSens;
		}
		if(Options.ScopeSens > 0)
		{
			if(Options.InvertMouse)
				ssens = -Options.ScopeSens;
			else
				ssens = Options.ScopeSens;
		}
		ctrl.mousesens = msens;
	}
	
	void Update()
	{
		//Esc menu
		if(escopen)
		{
			if(Input.GetKeyDown(KeyCode.Q)) Application.Quit();
			else if(Input.GetKeyDown(KeyCode.Y)) Network.Disconnect();
			else if(Input.GetKeyDown(KeyCode.N)) escmenu.enabled = escopen = false;
			return;
		}
		if(Input.GetKeyDown(KeyCode.Escape))
		{
			escmenu.enabled = escopen = true;
			return;
		}
		
		if(Input.GetButtonDown("Fire2") && WeaponScript.scr.Sniper())
		{
			cross.enabled = scope;
			scope = !scope;
			svd[0].enabled = svd[1].enabled = svd[2].enabled = scope;
			if(scope)
			{
				cam[0].fov = 10f;
				cam[1].fov = 10f;
				ctrl.mousesens = ssens;
			}
			else
			{
				cam[0].fov = fov;
				cam[1].fov = fov;
				ctrl.mousesens = msens;
			}
		}
		else if(Input.GetButtonUp("Talk"))
			NetworkMenu.scr.GetComponent<NetworkChat>().enabled = true;
		else if(Input.GetButtonUp("Sco"))
			score.SetActiveRecursively(!score.active);
		else if(Input.GetButtonUp("Scd"))
		{
			health = 0;
			ctrl.freeze = true;
			DisableScope();
			dinfo.enabled = true;
			cross.enabled = false;
			WeaponScript.scr.SetHudState(false);
			hud_area.width = (float)health / 100 * hud_fwidth;
			networkView.RPC("SendDeath2",RPCMode.All,NetworkMenu.pname);
			Network.RemoveRPCs(sync_state.viewID);
			sync_state.RPC("Call2",RPCMode.OthersBuffered,false,false);
		}
	}
	
	public void DisableScope()
	{
		scope = false;
		svd[0].enabled = svd[1].enabled = svd[2].enabled = false;
		cam[0].fov = 60f;
		cam[1].fov = 60f;
		cross.enabled = true;
		ctrl.mousesens = 5f;
	}
	
	public void GetAnim(string anim)
	{
		if(anim != skin.animation.clip.name)
			networkView.RPC("SendAnim",RPCMode.All,anim);
	}
	
	[RPC]
	void SendAnim(string anim)
	{
		if(anim == "fr" || anim == "fp" || anim == "kn" || anim == "rl")
		{
			skin.animation.Stop(anim);
			skin.animation.Play(anim);
		}
		else
		{
			skin.animation.clip = skin.animation.GetClip(anim);
			skin.animation.CrossFade(anim);
		}
	}
	
	public void SendDamage(int input, Vector3 pos)
	{
		networkView.RPC("DoDamage",networkView.owner,NetworkMenu.pname,input,pos,Network.player);
	}
	
	[RPC]
	void DoDamage(string pname, int input, Vector3 pos, NetworkPlayer pl)
	{
		if(!networkView.isMine)
		{
			networkView.RPC("DoDamage",networkView.owner,pname,input,pos,pl);
			return;
		}
		if(health < 1) return;
		health -= input;
		//Enable indicator
		hit_ind.renderer.enabled = true;
		//Getting hit direction
		hit_dir.z = Mathf.Atan2(transform.position.z - pos.z, transform.position.x - pos.x) * 180 / Mathf.PI;
		hit_dir.z += transform.rotation.eulerAngles.y-90;
		hit_dir.z = -hit_dir.z;
		hit_ind.localRotation = Quaternion.Euler(hit_dir);
		//Show hit texture
		hit_texture.enabled = true;
		hit_time = 2;
		if(health < 1)
		{
			health = 0;
			Network.RemoveRPCs(sync_state.viewID);
			sync_state.RPC("Call2",RPCMode.OthersBuffered,input == 101,false);
			networkView.RPC("SendDeath",RPCMode.All,pname,NetworkMenu.pname);
			networkView.RPC("SendDeathMsg",pl,NetworkMenu.pname);
			dinfo.enabled = true;
			kinfo.enabled = true;
			kinfo.text = "Killed by " + pname;
			dres = 10;
			ctrl.freeze = true;
			DisableScope();
			cross.enabled = false;
			WeaponScript.scr.SetHudState(false);
			if(pname != null)
				NetworkMenu.scr.GetComponent<NetScore>().ServAddScore(pname,1);
		}
		hud_area.width = (float)health / 100 * hud_fwidth;
	}
	
	[RPC]
	void SendDeathMsg(string pname)
	{
		kinfo.enabled = true;
		kinfo.text = "Killed " + pname;
		dres = 10;
	}
	
	[RPC]
	void SendDeath(string src, string trg)
	{
		dlist.text += src + " killed " + trg + "\n";
		dlline++;
		if(dlline > MAX_LINES)
		{
			dltmp = dlist.text;
			dltmp = dltmp.Remove(0,dltmp.IndexOf('\n',0)+1);
			dlist.text = dltmp;
		}
		dltmp = null;
	}
	
	[RPC]
	void SendDeath2(string trg)
	{
		dlist.text += trg + "killed himself\n";
		dlline++;
		if(dlline > MAX_LINES)
		{
			dltmp = dlist.text;
			dltmp = dltmp.Remove(0,dltmp.IndexOf('\n',0)+1);
			dlist.text = dltmp;
		}
		dltmp = null;
	}
	
	public void Respawn()
	{
		int tmp;
		health = 100;
		dinfo.enabled = false;
		cross.enabled = true;
		hud_area.width = (float)health / 100 * hud_fwidth;
		WeaponScript.scr.SetHudState(true);
		tmp = Random.Range(0,NetworkMenu.spawns.Length);
		transform.position = NetworkMenu.spawns[tmp].position;
		transform.rotation = NetworkMenu.spawns[tmp].rotation;
		Network.RemoveRPCs(sync_state.viewID);
		sync_state.RPC("Call2",RPCMode.Others,false,true);
	}
	
	public void SendState(bool input, bool input2)
	{
		if(!input2)
		{
			if(input)
				GetAnim("dth");
			else
				GetAnim("dt");
		}
		collider.enabled = input2;
		foreach(BoxCollider c in cols)
			c.enabled = input2;
	}
	
	IEnumerator ResetHit()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.2f);
			if(hit_time > 0)
			{
				hit_time--;
				if(hit_time == 0)
				{
					hit_ind.renderer.enabled = false;
					hit_texture.enabled = false;
				}
			}
			if(dres > 0)
			{
				dres--;
				if(dres == 0)
				{
					kinfo.enabled = false;
				}
			}
		}
	}
	
	void OnGUI()
	{
		if(Event.current.type == EventType.Repaint)
		{
			//HP
			GUI.BeginGroup(hud_area);
			GUI.color = hud_color;
			GUI.DrawTexture(hud_health,hud_tex);
			GUI.EndGroup();
		}
	}
}