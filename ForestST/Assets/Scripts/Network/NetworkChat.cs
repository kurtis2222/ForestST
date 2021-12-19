using UnityEngine;
using System.Collections;

public class NetworkChat : MonoBehaviour
{
	string message = "";
	
	//Check variables
	NetworkViewID tmpid;
	string tmpstr = null;
	byte tmpbyte = 0;
	
	//Script ref
	NetScore score;
	ChatGUI gui;
	
	void Awake()
	{
		score = GetComponent<NetScore>();
		gui = GetComponent<ChatGUI>();
	}
	
	void OnGUI()
	{
		if (Event.current.Equals(Event.KeyboardEvent("return")))
		{
			if(message.Length > 0)
			{
				if(message.StartsWith("/"))
				{
					if(Network.isServer)
					{
						if(message.StartsWith("/kick "))
						{
							tmpstr = message.Replace("/kick ","");
							if(string.Compare(tmpstr,NetworkMenu.pname,true) != 0)
								DoKick(tmpstr);
						}
						else if(message.StartsWith("/ban "))
						{
							tmpstr = message.Replace("/ban ","");
							if(string.Compare(tmpstr,NetworkMenu.pname,true) != 0)
								DoBan(tmpstr);
						}
						else if(message.StartsWith("/reloadbans"))
						{
							ReloadBans();
							gui.MessageTrigger2("Tiltólista újratöltve!");
						}
					}
				}
				else
					gui.MessageTrigger(NetworkMenu.pname + ": " + message);
			}
			message = "";
			enabled = false;
		}
		else if (Event.current.Equals(Event.KeyboardEvent("escape")))
		{
			message = "";
			enabled = false;
		}
		GUI.Label(new Rect(92,128,24,24),"Say:");
		GUI.SetNextControlName("ChatBox");
		message = GUI.TextField(new Rect(128,128,256,24),message,64);	
		GUI.FocusControl("ChatBox");
	}
	
	void DoKick(string pname)
	{
		if(!byte.TryParse(pname,out tmpbyte))
			tmpid = score.GetPlayerViewID(pname);
		else
			tmpid = score.GetPlayerViewID(tmpbyte, out pname);
		if(tmpid != default(NetworkViewID) && !tmpid.isMine)
		{
			GetComponent<NetworkMenu>().CallError(tmpid.owner,"Ki lettél rúgva a szerverről!");
			Network.CloseConnection(tmpid.owner,true);
			gui.MessageTrigger(pname + " ki lett rúgva.");
		}
		tmpid = default(NetworkViewID);
	}
	
	void DoBan(string pname)
	{
		if(!byte.TryParse(pname,out tmpbyte))
			tmpid = score.GetPlayerViewID(pname);
		else
			tmpid = score.GetPlayerViewID(tmpbyte, out pname);
		if(tmpid != default(NetworkViewID) && !tmpid.isMine)
		{
			NetworkMenu.AddBan(tmpid.owner.ipAddress);
			GetComponent<NetworkMenu>().CallError(tmpid.owner,"Ki lettél tiltva a szerverről!");
			Network.CloseConnection(tmpid.owner,true);
			gui.MessageTrigger(pname + " ki lett tiltva.");
		}
		tmpid = default(NetworkViewID);
	}
	
	void ReloadBans()
	{
		NetworkMenu.LoadBans();
	}
}