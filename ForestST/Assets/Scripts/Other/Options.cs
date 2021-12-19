using UnityEngine;
using System.Collections.Generic;

public static class Options
{
	const string fname = "config.ini";
	static FileConfigManager.FCM cfg = new FileConfigManager.FCM();
	
	static bool loaded = false;
	
	//Basic
	public static string Name;
	public static float MouseSens;
	public static float ScopeSens;
	public static bool InvertMouse;
	public static float FOV;
	public static bool AutoReload;
	public static bool AutoRespawn;
	//Volume
	//MaxFPS
	public static int ClientRate;
	public static int ServerRate;
	public static int MaxPlayers;
	public static bool LimPing;
	public static int MaxPing;
	//Graphics
	public static bool OvrdGfx;
	public static AnisotropicFiltering Aniso;
	public static int AALevel;
	public static BlendWeights BoneQual;
	public static int Vsync;
	public static float ShadowDist;
	//Terrain
	public static bool OvrdTer;
	public static float TerrainQual;
	public static int TreeMesh;
	public static float TreeFade;
	public static float TreeBill;
	public static bool Grass;
	public static int GrassDens;
	public static float GrassDis;
	public static bool Wind;
	
	static string[] data, val;
	
	public static void LoadOptions()
	{
		if(loaded) return;
		
		int tmp;
		float ftmp;
		bool btmp;
		cfg.ReadAllData(fname, out data, out val);
		for(int i = 0; i < data.Length; i++)
		{
			switch(data[i])
			{
				case "Name":
					Name = val[i];
					if(Name.Length > 16) Name.Substring(0,16);
					break;
				case "MouseSens":
					if(float.TryParse(val[i], out ftmp))
						MouseSens = ftmp;
					break;
				case "ScopeSens":
					if(float.TryParse(val[i], out ftmp))
						ScopeSens = ftmp;
					break;
				case "InvertMouse":
					if(bool.TryParse(val[i], out btmp))
						InvertMouse = btmp;
					break;
				case "FOV":
					if(float.TryParse(val[i], out ftmp))
					{
						if(ftmp < 60) ftmp = 60;
						else if(ftmp > 150) ftmp = 15;
						FOV = ftmp;
					}
					break;
				case "AutoReload":
					if(bool.TryParse(val[i], out btmp))
						AutoReload = btmp;
					break;
				case "AutoRespawn":
					if(bool.TryParse(val[i], out btmp))
						AutoRespawn = btmp;
					break;
				case "Volume":
					if(float.TryParse(val[i], out ftmp))
						AudioListener.volume = ftmp;
					break;
				case "MaxFPS":
					if(int.TryParse(val[i], out tmp))
						Application.targetFrameRate = tmp;
					break;
				case "ClientRate":
					if(int.TryParse(val[i], out tmp))
						ClientRate = tmp;
					break;
				case "ServerRate":
					if(int.TryParse(val[i], out tmp))
						ServerRate = tmp;
					break;
				case "MaxPlayers":
					if(int.TryParse(val[i], out tmp))
						MaxPlayers = tmp;
					break;
				case "LimPing":
					if(bool.TryParse(val[i], out btmp))
						LimPing = btmp;
					break;
				case "MaxPing":
					if(int.TryParse(val[i], out tmp))
						MaxPing = tmp;
					break;
				case "OvrdGfx":
					if(bool.TryParse(val[i], out btmp))
						OvrdGfx = btmp;
					break;
				case "Aniso":
					if(int.TryParse(val[i], out tmp))
					{
						if(tmp < 0) tmp = 0;
						else if(tmp > 2) tmp = 2;
						Aniso = (AnisotropicFiltering)tmp;
					}
					break;
				case "AALevel":
					if(int.TryParse(val[i], out tmp))
					{
						if(tmp < 0) tmp = 0;
						else if(tmp > 8) tmp = 8;
						AALevel = tmp;
					}
					break;
				case "BoneQual":
					if(int.TryParse(val[i], out tmp))
					{
						if(tmp < 0) tmp = 0;
						else if(tmp > 2) tmp = 2;
						BoneQual = (BlendWeights)tmp;
					}
					break;
				case "Vsync":
					if(int.TryParse(val[i], out tmp))
					{
						if(tmp < 0) tmp = 0;
						else if(tmp > 2) tmp = 2;
						Vsync = tmp;
					}
					break;
				case "ShadowDist":
					if(float.TryParse(val[i], out ftmp))
						ShadowDist = ftmp;
					break;
				case "OvrdTer":
					if(bool.TryParse(val[i], out btmp))
						OvrdTer = btmp;
					break;
				case "TerrainQual":
					if(float.TryParse(val[i], out ftmp))
					{
						if(ftmp < 0) ftmp = 0;
						else if(ftmp > 200) ftmp = 200;
						TerrainQual = ftmp;
					}
					break;
				case "TreeMesh":
					if(int.TryParse(val[i], out tmp))
						TreeMesh = tmp;
					break;
				case "TreeFade":
					if(float.TryParse(val[i], out ftmp))
						TreeFade = ftmp;
					break;
				case "TreeBill":
					if(float.TryParse(val[i], out ftmp))
						TreeBill = ftmp;
					break;
				case "Grass":
					if(bool.TryParse(val[i], out btmp))
						Grass = btmp;
					break;
				case "GrassDens":
					if(int.TryParse(val[i], out tmp))
					{
						if(tmp < 0) tmp = 0;
						else if(tmp > 3) tmp = 3;
						GrassDens = tmp;
					}
					break;
				case "GrassDis":
					if(float.TryParse(val[i], out ftmp))
						GrassDis = ftmp;
					break;
				case "Wind":
					if(bool.TryParse(val[i], out btmp))
						Wind = btmp;
					break;
			}
		}
	}
	
	public static void ApplyQuality(Terrain ter, GameObject wnd)
	{
		//Quality
		if(OvrdGfx)
		{
			QualitySettings.anisotropicFiltering = Aniso;
			QualitySettings.antiAliasing = AALevel;
			QualitySettings.blendWeights = BoneQual;
			QualitySettings.vSyncCount = Vsync;
			QualitySettings.shadowDistance = ShadowDist;
		}
		//Terrain
		if(OvrdTer)
		{
			ter.heightmapPixelError = 200-TerrainQual;
			ter.treeMaximumFullLODCount = TreeMesh;
			ter.treeCrossFadeLength = TreeFade;
			ter.treeBillboardDistance = TreeBill;
			if(!Grass)
				ter.detailObjectDensity = 0;
			switch(GrassDens)
			{
				case 0:
					ter.detailObjectDensity = 0.002f;
					break;
				case 1:
					ter.detailObjectDensity = 0.003f;
					break;
				case 2:
					ter.detailObjectDensity = 0.004f;
					break;
				case 3:
					ter.detailObjectDensity = 0.005f;
					break;
			}
			ter.detailObjectDistance = GrassDis;
		}
		//Wind
		if(!Wind)
		{
			ter.terrainData.wavingGrassAmount = 0;
			ter.terrainData.wavingGrassSpeed = 0;
			ter.terrainData.wavingGrassStrength = 0;
			GameObject.Destroy(wnd);
		}
	}
}