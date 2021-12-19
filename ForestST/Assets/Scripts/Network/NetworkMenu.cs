using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NetworkMenu : MonoBehaviour
{
	const string file_addr = "lastaddr.txt";
	const string file_bans = "bans.txt";
	
	public static NetworkMenu scr;
	
	public GameObject plobj;
	public Object plpref;
	public Object hudref;
	public static Transform[] spawns;
	public Texture2D tex_text;
	public Texture2D tex_button;
	public Texture2D tex_button2;
	
	public Terrain ter;
	public GameObject wnd;
	
	string ip = "127.0.0.1";
	float width, height;
	
	//GUI
	GUIStyle st_text;
	GUIStyle st_label;
	GUIStyle st_button;
	
	//Scale
	public static float sc_w;
	public static float sc_h;
	
	//Version
	int version = 10;
	
	//Player Name
	public static string pname;
	
	//Error messages
	static int error = 0;
	static string errmsg = null;
	
	//Banlist
	public static List<string> banlist = new List<string>();
	
	//Loaded static stuff
	static bool loaded = false;
	
	void Awake()
	{
		if(error > 0)
			StartCoroutine(HideError());
		if(!loaded)
		{
			if(File.Exists(file_addr))
			{
				StreamReader sr = new StreamReader(file_addr);
				if(sr.Peek() > -1)
					ip = sr.ReadLine();
				sr.Close();
			}
			Options.LoadOptions();
			LoadBans();
			pname = Options.Name;
			loaded = true;
		}
		Transform tmp = GameObject.Find("Spawns").transform;
		spawns = new Transform[tmp.childCount];
		for(int i = 0; i < spawns.Length; i++)
			spawns[i] = tmp.GetChild(i);
		Options.ApplyQuality(ter, wnd);
		scr = this;
		sc_w = (float)Screen.width/1024;
		sc_h = (float)Screen.height/768;
		st_text = new GUIStyle();
		st_text.fontSize = 32;
		st_text.alignment = TextAnchor.MiddleLeft;
		st_text.normal.background = tex_text;
		st_text.normal.textColor = st_text.hover.textColor = Color.white;
		st_label = new GUIStyle();
		st_label.fontSize = 32;
		st_label.alignment = TextAnchor.MiddleLeft;
		st_label.normal.textColor = st_text.hover.textColor = Color.white;
		st_button = new GUIStyle();
		st_button.fontSize = 32;
		st_button.normal.background = tex_button;
		st_button.alignment = TextAnchor.MiddleCenter;
		st_button.normal.textColor = st_button.hover.textColor = Color.white;
		st_button.hover.background = tex_button2;
	}
	
	void OnGUI()
	{
		width = Screen.width/2;
		height = Screen.height;
		GUI.Label(new Rect(0,0,296,32),"Forest Shootout v1.0",st_label);
		ip = GUI.TextField(new Rect(width-128,height-144,256,48),ip,st_text);
		if(GUI.Button(new Rect(width-128,height-96,256,48),"Connect",st_button))
		{
			Network.Connect(ip,7777);
			StreamWriter sw = new StreamWriter(file_addr,false);
			sw.WriteLine(ip);
			sw.Close();
		}
		if(GUI.Button(new Rect(width-128,height-48,256,48),"Host",st_button))
			Network.InitializeServer(Options.MaxPlayers-1,7777,false);
		if(error > 0)
			GUI.Label(new Rect(0,64,640,32),errmsg,st_label);
	}
	
	//Quit game ESC key
	void Update() { if(Input.GetKeyDown(KeyCode.Escape)) Application.Quit(); }
	
	void CreatePlayer()
	{
		int tmp;
		camera.enabled = false;
		GameObject hud;
		Rect hud_px;
		Vector2 hud_os;
		hud = (GameObject)Object.Instantiate(hudref);
		hud.name = "hud";
		foreach(GUITexture t in hud.GetComponentsInChildren<GUITexture>())
		{
			hud_px = t.pixelInset;
			hud_px.x *= sc_w;
			hud_px.y *= sc_h;
			hud_px.width *= sc_h;
			hud_px.height *= sc_h;
			t.pixelInset = hud_px;
		}
		foreach(GUIText t in hud.GetComponentsInChildren<GUIText>())
		{
			hud_os = t.pixelOffset;
			hud_os.x *= sc_w;
			hud_os.y *= sc_h;
			t.pixelOffset = hud_os;
			t.fontSize = (int)(t.fontSize * sc_h);
		}
		tmp = Random.Range(0,NetworkMenu.spawns.Length);
		plobj = (GameObject)Network.Instantiate(plpref,spawns[tmp].position,spawns[tmp].rotation,0);
		GetComponent<ChatGUI>().LoadChat();
		enabled = false;
		Screen.lockCursor = true;
		Screen.showCursor = false;
		if(Network.isClient)
			networkView.RPC("CheckVersion",RPCMode.Server,version);
		GetComponent<NetScore>().StartScanning();
	}
	
	void OnDisconnectedFromServer()
    {
		Screen.lockCursor = false;
		Screen.showCursor = true;
		Application.LoadLevel(0);
	}
	
	void OnPlayerDisconnected(NetworkPlayer pl)
    {
		Network.RemoveRPCs(pl);
		Network.DestroyPlayerObjects(pl);
		GetComponent<NetScore>().ServRemID(pl);
	}
	
	void OnConnectedToServer()
    {
		CreatePlayer();
	}
	
	void OnServerInitialized()
    {
		CreatePlayer();
	}
	
	void OnApplicationFocus(bool stat)
	{
		if(!Screen.showCursor && stat)
			Screen.lockCursor = true;
	}
	
	void OnFailedToConnect(NetworkConnectionError error)
	{
        SendError(error.ToString());
    }
	
	[RPC]
	void CheckVersion(int ver, NetworkMessageInfo info)
	{
		if(banlist.Contains(info.sender.ipAddress))
			CallError(info.sender,"You are banned from this server!");
		if(version != ver)
		{
			networkView.RPC("SendError",info.sender,"Version mismatch: v" + ver.ToString());
			Network.CloseConnection(info.sender,true);
		}
		else
			networkView.RPC("ReqestScore",info.sender);
	}
	
	[RPC]
	void ReqestScore()
	{
		GetComponent<NetScore>().ServAddPlayer(pname,plobj.networkView.viewID);
	}
	
	public void CallError(NetworkPlayer trg, string msg)
	{
		networkView.RPC("SendError",trg,msg);
		Network.CloseConnection(trg,true);
	}
	
	[RPC]
	void SendError(string msg)
	{
		errmsg = msg;
		error = 10;
	}
	
	public static void AddBan(string input)
	{
		banlist.Add(input);
		StreamWriter sw = new StreamWriter(file_bans,true);
		sw.WriteLine(input);
		sw.Close();
	}
	
	public static void LoadBans()
	{
		banlist.Clear();
		string tmp;
		StreamReader sr = new StreamReader(file_bans);
		while(sr.Peek() > -1)
		{
			tmp = sr.ReadLine();
			if(tmp.Length > 0)
				banlist.Add(tmp);
		}
		sr.Close();
	}
	
	IEnumerator HideError()
	{
		while(error > 0)
		{
			yield return new WaitForSeconds(1.0f);
			error--;
		}
	}
}