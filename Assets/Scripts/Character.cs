using System;
using UnityEngine;

[Serializable]
public class Character
{
	public Transform target;
	public Transform lastAttacker;
	public float health;
	public float baseHealth;
	public float baseDamage;
	public bool isDead;

	public float bodyLength;
	public float bodyMass;
	public float maxSpeed;
	public float turnSpeed;

	public Character()
	{
		baseDamage = 10;
		baseHealth = 100;
		health = baseHealth;
		isDead = false;
	}

	public void TakeDamage(float damage)
	{
		health -= damage;
		if (health < 0) health = 0;
		if (health == 0) isDead = true;
	}

	public void AutoHeal()
	{
		if (!isDead) health += 0.5f * Time.deltaTime;
		if (health > baseHealth) health = baseHealth;
	}
}
