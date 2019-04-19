using UnityEngine;

public class Interactable : MonoBehaviour
{
	protected Player player;

	protected virtual void Awake()
	{
		player = FindObjectOfType<Player>();
	}

	public virtual bool Interact(RaycastHit hit)
	{
		//Debug.Log("interacting with " + transform.name);
		return false;
	}

	public virtual string Describe(RaycastHit hit)
	{
		return transform.name;
	}
}
