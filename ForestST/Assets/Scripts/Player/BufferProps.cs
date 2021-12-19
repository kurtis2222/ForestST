using UnityEngine;
using System.Collections;

public class BufferProps : MonoBehaviour
{
	[RPC]
	void Call1(int data)
	{
		transform.parent.GetComponent<WeaponScript>().SendWeapon(data);
	}
	
	[RPC]
	void Call2(bool data, bool data2)
	{
		transform.parent.GetComponent<MainScript>().SendState(data, data2);
	}
}