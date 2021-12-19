using UnityEngine;
using System.Collections;

public class AmmoPickup : MonoBehaviour
{
	public int weapon;
	int time;
	
	void Awake()
	{
		time = 0;
		StartCoroutine(Respawn());
	}
	
	void OnTriggerEnter(Collider col)
	{
		if(col.name == "Player" && time == 0 && col.networkView.isMine &&
			col.GetComponent<WeaponScript>().PickupAmmo(weapon))
		{
			renderer.enabled = false;
			time = 30;
		}
    }
	
	IEnumerator Respawn()
	{
		while(true)
		{
			yield return new WaitForSeconds(1.0f);
			if(time > 0)
			{
				time--;
				if(time == 0)
				{
					renderer.enabled = true;
				}
			}
		}
	}
}