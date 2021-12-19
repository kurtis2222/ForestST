using UnityEngine;
using System.Collections;

public class WaterScript : MonoBehaviour
{
	Vector2 offs;
	
	void Awake()
	{
		offs = renderer.material.mainTextureOffset;
	}
	
	void FixedUpdate()
	{
		offs.x+=0.0001f;
		offs.x%=1.0f;
		offs.y+=0.0001f;
		offs.x%=1.0f;
		renderer.material.SetTextureOffset("_BumpMap",offs);
	}
}