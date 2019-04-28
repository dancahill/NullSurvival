using UnityEngine;

public class AnimalAnimatorEvents : MonoBehaviour
{
	Animal animal;

	private void Awake()
	{
		animal = GetComponentInParent<Animal>();
	}

	public void DoDamage()
	{
		animal.DoDamage();
	}
}
