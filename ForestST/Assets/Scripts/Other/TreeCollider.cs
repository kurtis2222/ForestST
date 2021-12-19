using UnityEngine;
using System.Collections;

public class TreeCollider : MonoBehaviour
{
	public Terrain terrain;
	public Object[] trees;
	
	void Awake()
	{
		Transform colroot = new GameObject("terrain_colliders").transform;
		GameObject tmp;
		foreach(TreeInstance tree in terrain.terrainData.treeInstances)
		{
			if(tree.prototypeIndex > 5) continue;
			tmp = (GameObject)GameObject.Instantiate(trees[tree.prototypeIndex]);
			tmp.transform.position = Vector3.Scale(tree.position, terrain.terrainData.size);
			tmp.transform.parent = colroot;
		}
	}
}