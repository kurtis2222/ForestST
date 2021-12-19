using UnityEngine;
using System.Collections;
using System.Linq;

class NetPl
{
	public byte id;
	public string name;
	public int score;
	public NetworkViewID plid;
	public int ping;
}

public class NetScore : MonoBehaviour {
	
	int MAX_PLAYERS = 16;
	NetPl[] net_players;
	NetPl[] net_plcopy;
	
	string[] list;
	int currconn = 0;
	GUIText[] hud_score = new GUIText[4];
	
	//Game modes
	bool old_school;
	bool instagib;
	string filt_weapons;
	
	public void StartScanning()
	{
		Transform tmp;
		tmp = GameObject.Find("hud").transform.Find("score");
		hud_score[0] = tmp.Find("id").guiText;
		hud_score[1] = tmp.Find("name").guiText;
		hud_score[2] = tmp.Find("score").guiText;
		hud_score[3] = tmp.Find("ping").guiText;
		hud_score[0].material.color = Color.blue;
		hud_score[2].material.color = Color.red;
		hud_score[3].material.color = Color.green;
		tmp = null;
		list = new string[4]; //ID, name, score, ping
		if(Network.isServer)
		{
			currconn++;
			MAX_PLAYERS = Network.maxConnections+1;
			net_players = new NetPl[MAX_PLAYERS];
			net_players[0] = new NetPl();
			net_players[0].id = 0;
			net_players[0].name = NetworkMenu.pname;
			net_players[0].score = 0;
			net_players[0].plid = NetworkMenu.scr.plobj.networkView.viewID;
			net_players[0].ping = 0;
			ScoreUpdate();
			if(!Options.LimPing)
				StartCoroutine(CheckPing());
			else
				StartCoroutine(CheckPing2());
		}
	}
	
	[RPC]
	void AddPlayer(string pname, NetworkViewID id)
	{
		if(currconn > 0)
		for(int i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] == null) continue;
			if(string.Compare(net_players[i].name,pname,true) == 0)
			{
				//Kick player
				GetComponent<ChatGUI>().MessageTrigger(pname + " has been kicked. (name match)");
				GetComponent<NetworkMenu>().CallError(id.owner,
						"This name is being used!");
				return;
			}
		}
		//If no problems, recheck for empty slots
		for(byte i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] == null)
			{
				net_players[i] = new NetPl();
				net_players[i].id = i;
				net_players[i].name = pname;
				net_players[i].score = 0;
				net_players[i].plid = id;
				net_players[i].ping = Network.GetLastPing(net_players[i].plid.owner);
				networkView.RPC("SendPlSc2",RPCMode.Others,(int)i,pname,0,net_players[i].ping);
				currconn++;
				break;
			}
		}
		networkView.RPC("GetScores",id.owner,MAX_PLAYERS);
		for(int i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] != null)
				networkView.RPC("SendPlSc",id.owner,i,net_players[i].name,
					net_players[i].score,net_players[i].ping);
		}
		GetComponent<ChatGUI>().MessageTrigger(pname + " has joined the server.");
		networkView.RPC("UpdateSc",id.owner);
		ScoreUpdate();
	}
	
	[RPC]
	void AddScore(string pname, int score)
	{
		for(int i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] == null) continue;
			if(net_players[i].name == pname)
			{
				net_players[i].score += score;
				networkView.RPC("SendScore",RPCMode.Others,i,"",net_players[i].score,-1);
				break;
			}
		}
		ScoreUpdate();
	}
	
	void ScoreUpdate()
	{
		net_plcopy = net_players.Where(x => x != null).OrderByDescending(x => x.score).ToArray();
		list[0] = list[2] = list[3] = "\n";
		list[1] = "[" + currconn.ToString() + "/" + MAX_PLAYERS.ToString() + "]\n";
		for(int i = 0; i < net_plcopy.Length; i++)
		{
			list[0] += net_plcopy[i].id.ToString() + "\n";
			list[1] += net_plcopy[i].name + "\n";
			list[2] += net_plcopy[i].score.ToString() + "\n";
			list[3] += net_plcopy[i].ping.ToString() + "\n";
		}
		hud_score[0].text = list[0];
		hud_score[1].text = list[1];
		hud_score[2].text = list[2];
		hud_score[3].text = list[3];
	}
	
	public void ServRemID(NetworkPlayer pl)
	{
		for(int i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] == null) continue;
			if(net_players[i].plid.owner == pl)
			{
				GetComponent<ChatGUI>().MessageTrigger(net_players[i].name + " has left the server.");
				net_players[i] = null;
				networkView.RPC("SendDelSc",RPCMode.Others,i);
				currconn--;
				break;
			}
		}
		ScoreUpdate();
	}
	
	[RPC]
	void GetScores(int pl)
	{
		MAX_PLAYERS = pl;
		net_players = new NetPl[MAX_PLAYERS];
	}
	
	[RPC]
	void SendPlSc(int id, string data, int data2, int data3)
	{
		net_players[id] = new NetPl();
		net_players[id].id = (byte)id;
		net_players[id].name = data;
		net_players[id].score = data2;
		net_players[id].ping = data3;
		currconn++;
	}
	
	[RPC]
	void SendPlSc2(int id, string data, int data2, int data3)
	{
		if(net_players == null) return;
		net_players[id] = new NetPl();
		net_players[id].id = (byte)id;
		net_players[id].name = data;
		net_players[id].score = data2;
		net_players[id].ping = data3;
		currconn++;
		ScoreUpdate();
	}
	
	[RPC]
	void SendScore(int id, string data, int data2, int data3)
	{
		if(net_players[id] == null)
		{
			net_players[id] = new NetPl();
			net_players[id].id = (byte)id;
			currconn++;
		}
		//null not supported in RPC parameters
		if(data != "")
			net_players[id].name = data;
		if(data2 != -1)
			net_players[id].score = data2;
		if(data3 != -1)
			net_players[id].ping = data3;
		ScoreUpdate();
	}
	
	[RPC]
	void SendDelSc(int id)
	{
		net_players[id] = null;
		currconn--;
		ScoreUpdate();
	}
	
	[RPC]
	void UpdateSc()
	{
		ScoreUpdate();
	}
	
	public void ServAddPlayer(string pname, NetworkViewID id)
	{
		networkView.RPC("AddPlayer",RPCMode.Server,pname,id);
	}
	
	public void ServAddScore(string pname, int score)
	{
		if(Network.isClient)
			networkView.RPC("AddScore",RPCMode.Server, pname, score);
		else
		{
			AddScore(pname,score);
			ScoreUpdate();
		}
	}
	
	//Other Stuff
	public NetworkViewID GetPlayerViewID(string pname)
	{
		for(int i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] == null) continue;
			if(string.Compare(net_players[i].name,pname,true) == 0)
				return net_players[i].plid;
		}
		return default(NetworkViewID);
	}
	
	public NetworkViewID GetPlayerViewID(byte id, out string pname)
	{
		for(int i = 0; i < MAX_PLAYERS; i++)
		{
			if(net_players[i] == null) continue;
			if(net_players[i].id == id)
			{
				pname = net_players[i].name;
				return net_players[i].plid;
			}
		}
		pname = null;
		return default(NetworkViewID);
	}
	
	IEnumerator CheckPing()
	{
		int i;
		while(true)
		{
			yield return new WaitForSeconds(30.0f);
			for(i = 0; i < MAX_PLAYERS; i++)
			{
				if(net_players[i] == null) continue;
				net_players[i].ping = Network.GetLastPing(net_players[i].plid.owner);
				networkView.RPC("SendScore",RPCMode.Others,i,"",-1,net_players[i].ping);
			}
			ScoreUpdate();
		}
	}
	
	IEnumerator CheckPing2()
	{
		int i;
		while(true)
		{
			yield return new WaitForSeconds(30.0f);
			for(i = 0; i < MAX_PLAYERS; i++)
			{
				if(net_players[i] == null) continue;
				if(Network.GetAveragePing(net_players[i].plid.owner) > Options.MaxPing)
				{
					GetComponent<ChatGUI>().MessageTrigger(net_players[i].name + " has been kicked. (high ping)");
					GetComponent<NetworkMenu>().CallError(net_players[i].plid.owner,
						"You have been kicked because of high ping! (" + Options.MaxPing + ")");
				}
				net_players[i].ping = Network.GetLastPing(net_players[i].plid.owner);
				networkView.RPC("SendScore",RPCMode.Others,i,"",-1,net_players[i].ping);
			}
			ScoreUpdate();
		}
	}
}