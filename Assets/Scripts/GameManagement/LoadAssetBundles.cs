using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadAssetBundles : MonoBehaviour
{
	public List<GameObject> dependencies;

	AssetBundle myLoadedAssetMap;
#pragma warning disable 0649
	AssetBundle modernAnimalBundle;
	AssetBundle prehistoricAnimalBundle;
#pragma warning restore 0649
#if UNITY_ANDROID
	string basepath = @"Assets/AssetBundles/Android/";
#elif UNITY_WEBGL
	string basepath = @"Assets/AssetBundles/WebGL/";
#else
	no basepath defined for this target
#endif

	private void Start()
	{
		LoadAssetBundle(modernAnimalBundle, basepath + "modernanimals");
		LoadAssetBundle(prehistoricAnimalBundle, basepath + "prehistoricanimals");
	}

	public bool LoadAssetBundleScene(string scenename)
	{
		if (scenename == "Intro") return false;
		myLoadedAssetMap = AssetBundle.LoadFromFile(basepath + "Map" + scenename);
		foreach (string s in myLoadedAssetMap.GetAllScenePaths())
		{
			Debug.Log("map bundle contains '" + s + "'");
			string scene = Path.GetFileNameWithoutExtension(s);
			StartCoroutine(LoadAssetBundleSceneAsync(scene));
			return true;
		}
		return false;
	}

	IEnumerator LoadAssetBundleSceneAsync(string scene)
	{
		AsyncOperation asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);
		while (asyncOperation.progress < 0.9f)
		{
			Debug.Log("Loading Progress: " + asyncOperation.progress);
			yield return null;
		}
		//asyncOperation.allowSceneActivation = true;
		while (!asyncOperation.isDone)
		{
			Debug.Log("!asyncOperation.isDone");
			yield return null;
		}
		// Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next).
		UnityEngine.SceneManagement.Scene newlyLoadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
		UnityEngine.SceneManagement.SceneManager.SetActiveScene(newlyLoadedScene);
		GameObject pp = FindObjectOfType<AnimalPrefabs>().playerPrefab;
		if (pp)
		{
			GameObject spawns = GameObject.Find("PlayerSpawns");
			Transform t = spawns.transform.GetChild(Random.Range(0, spawns.transform.childCount));
			GameObject p = Instantiate(pp, t.position, Quaternion.identity);
			p.name = "Player";
		}
		FixWater();
		yield return null;
	}

	// shouldn't need a function to fix water but loading water from an asset bundle 
	// seems to break the material in a way i haven't figured out how to fix yet
	private void FixWater()
	{
		Debug.Log("fixwater()");
		GameObject env = GameObject.Find("Environment");
		if (!env) return;

		LoadAssetBundles lab = FindObjectOfType<LoadAssetBundles>();

		foreach (Transform tran in env.transform)
		{
			if (!tran.name.StartsWith("Water")) continue;
			string tranname = tran.name;
			if (tranname.StartsWith("Water4Simple")) tranname = "Water4Simple";
			//Debug.Log("found water");
			Renderer[] objrends = tran.GetComponentsInChildren<Renderer>();
			foreach (Renderer objrend in objrends)
			{
				Debug.Log(string.Format("found renderer in {0}", tranname));
				//if (!objrend) continue;
				//Material objmat = objrend.material;
				GameObject proto = lab.dependencies.Find(x => x.name == tranname);
				if (!proto)
				{
					Debug.Log("no prototype found for " + tranname);
					continue;
				}
				Renderer prorend = proto.GetComponentInChildren<Renderer>();
				if (!prorend) Debug.Log("no renderer found in prototype");
				objrend.material = prorend.sharedMaterial;
			}
		}
	}


	//public void LoadAssetBundleMap(string mapname)
	//{
	//	if (string.IsNullOrEmpty(mapname) || mapname == "Intro") return;
	//	Debug.Log("mapname='" + mapname + "'" + (mapname == null ? " is null" : ""));
	//	myLoadedAssetMap = AssetBundle.LoadFromFile(basepath + mapname);
	//	Debug.Log("myLoadedAssetMap=" + basepath + mapname + (myLoadedAssetMap == null ? " is null" : " found"));
	//	//GameObject go = InstantiateFromBundle("TestMap1");
	//	GameObject go = myLoadedAssetMap.LoadAsset<GameObject>(mapname);
	//	//if (!prefab) Debug.Log("prefab for " + assetname + " is missing");
	//	if (go)
	//	{
	//		//go.transform.parent = GameObject.Find("Environment").transform;
	//		//GameObject g = Instantiate(go, spawnpoint, Quaternion.identity);
	//		GameObject g = Instantiate(go, GameObject.Find("Environment").transform);
	//		//g.transform.parent = spawnsActive.transform;
	//		//g.name = "";
	//	}
	//	//myLoadedAssetMap.Unload(false);
	//}

	private void LoadAssetBundle(AssetBundle bundle, string bundleURL)
	{
		bundle = AssetBundle.LoadFromFile(bundleURL);
		Debug.Log("myLoadedAssetBundle=" + bundleURL + (bundle == null ? " is null" : " found"));
		AnimalPrefabs ap = FindObjectOfType<AnimalPrefabs>();
		foreach (AnimalStats.Stats stats in AnimalStats.Animals)
		{
			GameObject go = InstantiateFromBundle(bundle, stats.name);
			//if (!go) Debug.Log("prefab for " + stats.name + " in " + bundleURL + " is missing");
			if (go) ap.prehistoricAnimalPrefabs.Add(go);
		}
		//myLoadedAssetBundle.Unload(false);
	}

	private GameObject InstantiateFromBundle(AssetBundle bundle, string assetname)
	{
		GameObject prefab = bundle.LoadAsset<GameObject>(assetname);
		return prefab;
	}
}
