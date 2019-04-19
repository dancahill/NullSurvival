using UnityEngine;

public class Util
{
	public class PointHeight
	{
		public bool hit;
		public float groundHeight;
		public float waterHeight;
		public float waterDepth;
	}

	public static PointHeight GetHeightAtPoint(Vector3 point)
	{
		PointHeight h = new PointHeight
		{
			hit = Physics.Raycast(point, Vector3.down, out RaycastHit hit, Mathf.Infinity)
		};
		if (h.hit)
		{
			if (hit.transform.name == "Water")
			{
				h.waterHeight = hit.point.y;
				Physics.Raycast(new Vector3(point.x, hit.point.y, point.z), Vector3.down, out hit, Mathf.Infinity);
			}
			h.groundHeight = hit.point.y;
			h.waterDepth = h.waterHeight - h.groundHeight;
			if (h.waterDepth < 0) h.waterDepth = 0;
		}
		return h;
	}
}
