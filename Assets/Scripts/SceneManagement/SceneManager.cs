using UnityEngine;

public class SceneManager : MonoBehaviour
{
	#region Singleton
	public static SceneManager instance;
	#endregion
	Camera playerCamera;
	float[] cameraDistances = new float[32];
	[Range(0f, 200f)] public float maxDinoRenderDistance;
	[Range(0f, 282.8f)] public float maxDinoActiveDistance;
	[Range(0f, 10000f)] public int minSpawns;
	[Range(0f, 10000f)] public int maxSpawns;
	public string mapName;

	void Awake()
	{
		instance = this;
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Persistent")
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Persistent");
			return;
		}
	}

	void Start()
	{
		maxDinoRenderDistance = 100f;
		maxDinoActiveDistance = 141f;
		if (minSpawns == 0) minSpawns = 5000;
		if (maxSpawns == 0) maxSpawns = 5000;
		playerCamera = Camera.main;
		cameraDistances[10] = maxDinoRenderDistance; // don't render animals 100 metres or more away 
		if (playerCamera) playerCamera.layerCullDistances = cameraDistances;
		CanvasManager.SetHUDActive(true);
		//FindObjectOfType<LoadAssetBundles>().LoadAssetBundleMap(SceneController.GetActiveSceneName());
	}

	private void Update()
	{
		if (playerCamera != Camera.main && Camera.main)
		{
			playerCamera = Camera.main;
			if (playerCamera) playerCamera.layerCullDistances = cameraDistances;
		}
	}

	public Vector3 GetTerrainSize()
	{
		Terrain[] terrains = FindObjectsOfType<Terrain>();
		Vector3 size = new Vector3();
		float minx = 0, maxx = 0;
		float miny = 0, maxy = 0;
		float minz = 0, maxz = 0;

		foreach (Terrain terrain in terrains)
		{
			minx = Mathf.Min(minx, terrain.transform.position.x);
			miny = Mathf.Min(miny, terrain.transform.position.y);
			minz = Mathf.Min(minz, terrain.transform.position.z);
			maxx = Mathf.Max(maxx, terrain.transform.position.x + terrain.terrainData.size.x);
			maxy = Mathf.Max(maxy, terrain.transform.position.y + terrain.terrainData.size.y);
			maxz = Mathf.Max(maxz, terrain.transform.position.z + terrain.terrainData.size.z);
		}
		size.x = maxx - minx;
		size.y = maxy - miny;
		size.z = maxz - minz;
		return size;
	}
}
