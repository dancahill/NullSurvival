using System;
using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
	[Serializable]
	public class DeadTrees
	{
		public TreeInstance instance;
		public float respawnCooldown;
	}
	#region Singleton
	public static TreeManager instance;
	#endregion
	public List<DeadTrees> deadTrees;
	Terrain terrain;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		terrain = Terrain.activeTerrain;
		if (!terrain.preserveTreePrototypeLayers) Debug.LogWarning("terrain.preserveTreePrototypeLayers should be true?");
		//terrain.preserveTreePrototypeLayers = true;
		InvokeRepeating("SpawnTrees", 10f, 10f);
	}

	private void SpawnTrees()
	{
		foreach (DeadTrees dt in deadTrees)
		{
			if (dt.respawnCooldown > Time.time) continue;
			List<TreeInstance> TreeInstances;
			TreeInstances = new List<TreeInstance>(terrain.terrainData.treeInstances);

			TreePrototype tp = terrain.terrainData.treePrototypes[dt.instance.prototypeIndex];
			Debug.Log("planting " + tp.prefab.name);

			TreeInstances.Add(dt.instance);
			deadTrees.Remove(dt);
			terrain.terrainData.treeInstances = TreeInstances.ToArray();

			terrain.GetComponent<TerrainCollider>().enabled = false;
			terrain.GetComponent<TerrainCollider>().enabled = true;
			return;
		}
	}

	public static void RemoveAt(Vector3 treelocation)
	{
		//https://forum.unity.com/threads/finally-removing-trees-and-the-colliders.110354/
		//https://answers.unity.com/questions/650308/how-do-i-interact-with-terrain-trees.html
		//https://csharp.hotexamples.com/examples/UnityEngine/TreeInstance/-/php-treeinstance-class-examples.html

		TerrainData tdata = TreeManager.instance.terrain.terrainData;

		//TreeInstance[] treeInstances = terrain.terrainData.treeInstances;

		// Let's find the closest tree to the place we chopped and hit something
		int closestTreeIndex = GetTreeIndexAt(treelocation);
		if (closestTreeIndex < 0) return;

		// Remove the tree from the terrain tree list

		List<TreeInstance> TreeInstances = new List<TreeInstance>(tdata.treeInstances);

		TreeInstance t = tdata.treeInstances[closestTreeIndex];
		TreeManager.instance.deadTrees.Add(new DeadTrees { instance = t, respawnCooldown = Time.time + 10 });
		//Debug.Log("removed t.prototypeIndex=" + t.prototypeIndex);
		TreePrototype tp = tdata.treePrototypes[t.prototypeIndex];
		Debug.Log("removed " + tp.prefab.name);

		//terrain.terrainData.treeInstances.RemoveAt();
		TreeInstances.RemoveAt(closestTreeIndex);
		tdata.treeInstances = TreeInstances.ToArray();

		// Now refresh the terrain, getting rid of the darn collider
		//float[,] heights = terrain.terrainData.GetHeights(0, 0, 0, 0);
		//terrain.terrainData.SetHeights(0, 0, heights);

		TerrainCollider tc = TreeManager.instance.terrain.GetComponent<TerrainCollider>();
		tc.enabled = false;
		tc.enabled = true;

		// Put a falling tree in its place
		GameObject deadtree = Instantiate(tp.prefab, new Vector3(treelocation.x, TreeManager.instance.terrain.SampleHeight(treelocation), treelocation.z), Quaternion.identity);
		Rigidbody rb = deadtree.AddComponent<Rigidbody>();
		rb.mass = 5;
		Destroy(deadtree, 5);
		//Debug.Log("added tree at " + t.position + ", location=" + treelocation);
		//Debug.Log(DateTime.Now - start);

		//https://answers.unity.com/questions/953838/ray-does-not-hit-treeinstance.html
	}

	public static string GetTreeNameAt(Vector3 treelocation)
	{
		TerrainData tdata = TreeManager.instance.terrain.terrainData;
		string n = "";
		int closestTreeIndex = GetTreeIndexAt(treelocation);
		//Debug.Log("closestTreeIndex=" + closestTreeIndex);
		if (closestTreeIndex < 0) return n;
		TreeInstance t = tdata.treeInstances[closestTreeIndex];
		TreePrototype tp = tdata.treePrototypes[t.prototypeIndex];
		n = tp.prefab.name;
		return n;
	}

	public static int GetTreeIndexAt(Vector3 treelocation)
	{
		int closestTreeIndex = -1;
		// Our current closest tree initializes to far away
		float maxDistance = float.MaxValue;
		// Track our closest tree's position
		Vector3 closestTreePosition = new Vector3();
		TreeInstance[] treeInstances = TreeManager.instance.terrain.terrainData.treeInstances;
		for (int i = 0; i < treeInstances.Length; i++)
		{
			TreeInstance currentTree = treeInstances[i];
			// The the actual world position of the current tree we are checking
			Vector3 currentTreeWorldPosition = Vector3.Scale(currentTree.position, TreeManager.instance.terrain.terrainData.size) + Terrain.activeTerrain.transform.position;
			// Find the distance between the current tree and whatever we hit when chopping
			float distance = Vector3.Distance(currentTreeWorldPosition, treelocation);
			// Is this tree even closer?
			if (distance < maxDistance)
			{
				maxDistance = distance;
				closestTreeIndex = i;
				closestTreePosition = currentTreeWorldPosition;
			}
		}
		return closestTreeIndex;
	}

	public static float GetSampleHeight(RaycastHit hit)
	{
		int layer = hit.transform.gameObject.layer;
		if (LayerMask.LayerToName(layer) != "Ground") return 0;
		//Terrain terrain = hit.collider.gameObject.GetComponent<Terrain>();
		//if (terrain != null)
		//{
		float groundHeight = TreeManager.instance.terrain.SampleHeight(hit.point);
		float height = hit.point.y - groundHeight;
		//}
		return height;
	}
}
