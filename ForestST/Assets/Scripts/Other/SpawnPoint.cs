using UnityEngine;
using System.Collections;

public class SpawnPoint : MonoBehaviour
{
	void OnDrawGizmos()
	{
		Gizmos.DrawRay(transform.position,transform.forward);
		Gizmos.color = Color.red;
		Gizmos.DrawCube(transform.position,new Vector3(1,2,1));
	}
}