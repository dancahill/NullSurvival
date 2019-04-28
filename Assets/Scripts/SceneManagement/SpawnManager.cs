using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
	[System.Serializable]
	public class SpawnCount
	{
		public string name;
		public int count;
	}
	public int totalAnimals;
	public int totalActive;
	public List<SpawnCount> animalCount;
	public List<GameObject> animalPrefabs;
	float spawnCooldown;
	Vector3 terrainSize;
	GameCanvas gc;

	GameObject spawns;
	GameObject spawnsActive;
	GameObject spawnsInactive;

	void Start()
	{
		if (animalPrefabs.Count < 1)
		{
			animalPrefabs.Clear();
			AnimalPrefabs ap = FindObjectOfType<AnimalPrefabs>();
			foreach (GameObject a in ap.modernAnimalPrefabs) animalPrefabs.Add(a);
			foreach (GameObject a in ap.prehistoricAnimalPrefabs) animalPrefabs.Add(a);
		}

		spawns = GameObject.Find("Spawns");
		if (!spawns) spawns = new GameObject("Spawns");
		spawnsActive = GameObject.Find("Spawns/Active");
		if (!spawnsActive) { spawnsActive = new GameObject("Active"); spawnsActive.transform.parent = spawns.transform; }
		spawnsInactive = GameObject.Find("Spawns/Inactive");
		if (!spawnsInactive) { spawnsInactive = new GameObject("Inactive"); spawnsInactive.transform.parent = spawns.transform; }

		gc = FindObjectOfType<GameCanvas>();
		terrainSize = SceneManager.instance.GetTerrainSize();
		Debug.Log("terrain size: x=" + terrainSize.x + ",y=" + terrainSize.y + ",z=" + terrainSize.z);
		animalCount = new List<SpawnCount>();
		//InvokeRepeating("UpdateSpawnCounts", 5f, 5f);
		InvokeRepeating("UpdateSpawnIdle", 0f, 1f);
		InvokeRepeating("Spawn", 0f, 1.0f);
	}

	void UpdateSpawnIdle()
	{
		StartCoroutine(UpdateSpawns());
	}

	IEnumerator UpdateSpawns()
	{
		Animal[] animals = GameObject.Find("Spawns").GetComponentsInChildren<Animal>(true);
		List<SpawnCount> animalcount = new List<SpawnCount>();
		Player p = FindObjectOfType<Player>();
		if (!p) yield break;
		animalcount.Clear();
		int i = 0;
		int ac = 0;
		foreach (Animal a in animals)
		{
			bool inrange = Vector3.Distance(a.transform.position, p.transform.position) <= SceneManager.instance.maxDinoActiveDistance;
			a.gameObject.SetActive(inrange);
			a.gameObject.transform.parent = inrange ? spawnsActive.transform : spawnsInactive.transform;
			if (inrange) ac++;

			SpawnCount sc = animalcount.Find(x => x.name == a.name);
			if (sc != null) { sc.count++; continue; }
			animalcount.Add(new SpawnCount { name = a.name, count = 1 });
			if (i++ > 100) { i = 0; yield return null; }
		}
		animalcount.Sort((a, b) => a.name.CompareTo(b.name));
		animalCount = animalcount;
		totalActive = ac;
		yield return null;
	}

	void AddSpawnToCount(string spawnname)
	{
		SpawnCount sc = animalCount.Find(x => x.name == spawnname);
		if (sc != null) { sc.count++; return; }
		animalCount.Add(new SpawnCount { name = spawnname, count = 1 });
	}

	void RemoveSpawnFromCount(string spawnname)
	{
		SpawnCount sc = animalCount.Find(x => x.name == spawnname);
		if (sc != null) { sc.count--; return; }
		animalCount.Add(new SpawnCount { name = spawnname, count = 0 });
	}

	void Spawn()
	{
		if (Time.time < spawnCooldown) return;
		if (gc.FPS < 15f)
		{
			if (totalAnimals < SceneManager.instance.maxSpawns)
			{
				string s1 = string.Format("spawns={0}/{1}, low fps: {2:0.0}, not spawning", totalAnimals, SceneManager.instance.maxSpawns, gc.FPS);
				gc.SetToastInfo(s1);
			}
			spawnCooldown = Time.time + 1f;
			return;
		}
		//Animal[] animals = FindObjectsOfType<Animal>();
		//lastSpawnsCount = animals.Length;
		for (int i = 0; i < 20; i++)
		{
			//UpdateSpawnCounts();
			totalAnimals = animalCount.Sum(x => x.count);
			if (totalAnimals >= SceneManager.instance.minSpawns) spawnCooldown = Time.time + 1f;
			if (totalAnimals >= SceneManager.instance.maxSpawns) return;
			GameObject spawns = GameObject.Find("Spawns");
			List<GameObject> prefabsearchlist;
			GameObject prefab;
			Vector3 spawnpoint;
			Ground.PointHeight h;
			do
			{
				spawnpoint = new Vector3(Random.Range(1f, terrainSize.x - 1f), terrainSize.y, Random.Range(1f, terrainSize.z - 1f));
				h = Ground.GetHeightAtPoint(spawnpoint);
				// don't spawn anything in shallow water
				if (h.waterDepth > 0f && h.waterDepth < 0.5f) continue;
				// don't spawn anything over 300 metres - at least for now
				if (h.groundHeight > 300) continue;
				break;
			} while (true);
			if (h.waterHeight > 0)
			{
				spawnpoint.y = h.groundHeight + h.waterDepth / 2; // put it in the water at half depth
				prefabsearchlist = animalPrefabs.FindAll(x => x.GetComponent<Animal>().habitat == Animal.Habitat.Sea && x.GetComponent<Animal>().character.bodyLength / 2f < h.waterDepth);
#if false
				SpawnCount c = animalCount.Find(x => x.name == prefabsearchlist[0].name);
				if (c != null && c.count >= 3)
				{
					//Debug.Log("too many sharks");
					//continue;
				}
#endif
			}
			else
			{
				spawnpoint.y = h.groundHeight;
				prefabsearchlist = animalPrefabs.FindAll(x => x.GetComponent<Animal>().habitat == Animal.Habitat.Land || x.GetComponent<Animal>().habitat == Animal.Habitat.Air);
#if false
				SpawnCount c = animalCount.Find(x => x.name == "Tyrannosaurus Rex");
				if (c == null || c.count < 1)
				{
					//Debug.Log("not enough rexes");
					List<GameObject> rexes = animalPrefabs.FindAll(x => x.name == "Tyrannosaurus Rex");
					if (rexes.Count > 0) prefabsearchlist = rexes;
				}
#endif
			}
			if (prefabsearchlist.Count < 1)
			{
				Debug.LogWarning(string.Format("no suitable prefabs found ({0},{1},{2})", h.waterDepth, h.waterHeight, h.groundHeight));
				return;
			}
			prefab = prefabsearchlist[Random.Range(0, prefabsearchlist.Count)];
			//Debug.Log("prefab to spawn: " + prefab.name);

			AnimalStats.Stats stats = AnimalStats.Animals.Find(x => x.name == prefab.name);
			if (stats != null)
			{
				float chance = Random.Range(0f, 100f);
				if (stats.chanceToSpawn < chance) continue;
			}

			//prefab.SetActive(false);

			GameObject g = Instantiate(prefab, spawnpoint, Quaternion.identity);
			g.transform.parent = spawnsActive.transform;
			g.name = prefab.name;
			totalAnimals++;

			AddSpawnToCount(g.name);

			Player p = FindObjectOfType<Player>();
			if (!p) return;
			bool inrange = Vector3.Distance(g.transform.position, p.transform.position) <= SceneManager.instance.maxDinoActiveDistance;
			g.gameObject.SetActive(inrange);

			//string s = string.Format("spawns={0}/{1}, new {2} at {3:0},{4:0}", totalAnimals, SceneManager.instance.maxSpawns, prefab.name, spawnpoint.x, spawnpoint.z);
			//gc.SetToastInfo(s);
		}
	}
}
