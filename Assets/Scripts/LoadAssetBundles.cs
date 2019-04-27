using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadAssetBundles : MonoBehaviour
{
	AssetBundle myLoadedAssetMap;
#pragma warning disable 0649
	AssetBundle modernAnimalBundle;
	AssetBundle prehistoricAnimalBundle;
#pragma warning restore 0649
	//public List<string> bundlePaths;
	//EditorUserBuildSettings.activeBuildTarget
#if UNITY_ANDROID
	string basepath = @"Assets/AssetBundles/Android/";
#elif UNITY_WEBGL
	string basepath = @"Assets/AssetBundles/WebGL/";
#endif

	private void Start()
	{
		//string basepath = @"Assets/AssetBundles/" + EditorUserBuildSettings.activeBuildTarget + "/";
		//foreach (string bundle in bundlePaths)
		//{
		//LoadAssetBundle(bundle);
		//}
		LoadAssetBundle(modernAnimalBundle, basepath + "modernanimals");
		LoadAssetBundle(prehistoricAnimalBundle, basepath + "prehistoricanimals");
	}

	public bool LoadAssetBundleScene(string scenename)
	{


		if (scenename == "Intro") return false;
		myLoadedAssetMap = AssetBundle.LoadFromFile(basepath + "Map" + scenename);
		foreach (string s in myLoadedAssetMap.GetAllScenePaths())
		{
			Debug.Log("bundle contains '" + s + "'");
			string scene = Path.GetFileNameWithoutExtension(s);

			StartCoroutine(LoadAssetBundleSceneAsync(scene));

			//SceneManager.LoadScene(scene);
			return true;
		}
		return false;
	}

	IEnumerator LoadAssetBundleSceneAsync(string scene)
	{
		AsyncOperation _SceneAsync = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scene, UnityEngine.SceneManagement.LoadSceneMode.Additive);

		while (_SceneAsync.progress < 0.9f)
		{
			Debug.Log("Loading scene " + " [][] Progress: " + _SceneAsync.progress);
			yield return null;
		}

		//Activate the Scene
		_SceneAsync.allowSceneActivation = true;

		while (!_SceneAsync.isDone)
		{
			// wait until it is really finished
			yield return null;
		}


		UnityEngine.SceneManagement.Scene newlyLoadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);

		Debug.Log("setting '" + newlyLoadedScene.name + "'");

		// Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next).
		UnityEngine.SceneManagement.SceneManager.SetActiveScene(newlyLoadedScene);

		Debug.Log("scene set");


		GameObject pp = FindObjectOfType<AnimalPrefabs>().playerPrefab;
		if (pp)
		{
			GameObject p = Instantiate(pp, Vector3.zero, Quaternion.identity);
			p.name = "Player";
		}


		yield return null;
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
