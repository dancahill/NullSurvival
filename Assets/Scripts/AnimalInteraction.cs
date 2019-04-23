using UnityEngine;

// we need to attach this object to each animal on the object where the collider lives
// which is probably not at the top level of the prefab
[RequireComponent(typeof(MeshCollider))]
public class AnimalInteraction : Interactable
{
	Animal animal;

	private void Start()
	{
		animal = GetComponentInParent<Animal>();
	}

	public override bool Interact(RaycastHit hit)
	{
		return animal.Interact(hit);
	}

	public override string Describe(RaycastHit hit)
	{
		if (animal) return animal.Describe(hit);
		return "";
	}
}
