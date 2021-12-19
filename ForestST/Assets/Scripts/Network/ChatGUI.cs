using UnityEngine;
using System.Collections;

public class ChatGUI : MonoBehaviour {

	GUIText chat;
	
	const int MAX_LINES = 5;
	
	int lines = 0;
	string tmp = null;
	
	public void LoadChat()
	{
		chat = GameObject.Find("HudChat").guiText;
	}
	
	public void MessageTrigger(string msg)
	{
		networkView.RPC("ShowChat",RPCMode.All,msg);
	}
	
	public void MessageTrigger2(string msg)
	{
		ShowChat(msg);
	}
	
	[RPC]
	void ShowChat(string msg)
	{
		chat.text += msg + "\n";
		lines++;
		if(lines > MAX_LINES)
		{
			tmp = chat.text;
			tmp = tmp.Remove(0,tmp.IndexOf('\n',0)+1);
			chat.text = tmp;
		}
		tmp = null;
	}
}