using UnityEngine;

public class Ground : Interactable
{
	protected override void Awake()
	{
		base.Awake();
	}

	public override bool Interact(RaycastHit hit)
	{
		base.Interact(hit);
		Harvest(hit);
		return false;
	}

	public override string Describe(RaycastHit hit)
	{
		//return base.Describe(hit);
		string s = "";
		if (TreeManager.GetSampleHeight(hit) < 0.01f) return s;
		string name = TreeManager.GetTreeNameAt(hit.point);
		if (name.StartsWith("tree")) s = "Tree";
		else if (name.StartsWith("bush")) s = "Bush";
		else s = TreeManager.GetTreeNameAt(hit.point) + " (" + hit.transform.name + ")";
		return s;
	}

	private bool Harvest(RaycastHit hit)
	{
		Debug.Log("harvesting (hit.distance=" + hit.distance + ")");
		//https://answers.unity.com/questions/650308/how-do-i-interact-with-terrain-trees.html

		//Debug.Log("harvesting " + hit.transform.gameObject.layer);
		Terrain terrain = hit.collider.gameObject.GetComponent<Terrain>();
		// Did we click a Terrain?
		if (terrain == null) return false;

		// Was it the terrain or a terrain tree, based on SampleHeight()
		float groundHeight = terrain.SampleHeight(hit.point);
		if (hit.point.y - groundHeight < 0.05f)
		{
			Debug.Log("can't harvest dirt");
			return false;
		}

		Vector3 pos = player.transform.position;
		Vector3 dest = hit.point;
		float y = player.transform.position.y - hit.point.y;
		pos.y = 0;
		dest.y = 0;
		float dist = Vector3.Distance(pos, dest);
		Debug.Log("y " + y + " xz dist " + dist);
		if (y < -1.0f || y > 1.5f)
		{
			Debug.Log("y too far");
			return false;
		}
		if (dist > 2f)
		{
			Debug.Log("dist too far");
			return false;
		}

		// It's a terrain tree, check Proximity and Harvest
		//if (hit.distance < 2f) Debug.Log("hit a tree");
		//if (CheckProximity())
		//	HarvestWood();

		//Debug.Log("removing tree at " + hit.point);
		TreeManager.RemoveAt(hit.point);

		return false;
	}
}
