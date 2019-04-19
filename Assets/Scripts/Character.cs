using System;
using UnityEngine;

[Serializable]
public class Character
{
	public Transform target;
	public Transform lastAttacker;
	public float baseHealth;
	public float health;
	public float baseStamina;
	public float stamina;
	public float baseDamage;
	public bool isDead;

	public float bodyLength;
	public float bodyMass;
	public float maxSpeed;
	public float turnSpeed;

	float staminaRegenCooldown;

	public Character()
	{
		baseDamage = 10;
		baseHealth = 100;
		health = baseHealth;

		baseStamina = 100;
		stamina = baseStamina;

		isDead = false;
	}

	public void TakeDamage(float damage)
	{
		health -= damage;
		if (health < 0) health = 0;
		if (health == 0) isDead = true;
	}

	public bool UseStamina()
	{
		if (stamina < 1)
		{
			GameCanvas gc = GameObject.FindObjectOfType<GameCanvas>();
			if (gc) gc.SetToastText("not enough stamina");
			return false;
		}
		staminaRegenCooldown = Time.time + 1f;
		stamina -= 10f * Time.deltaTime;
		return true;
	}

	public bool UseStamina(float amount)
	{
		if (stamina < amount)
		{
			GameCanvas gc = GameObject.FindObjectOfType<GameCanvas>();
			if (gc) gc.SetToastText("not enough stamina");
			return false;
		}
		staminaRegenCooldown = Time.time + 1f;
		stamina -= amount;
		return true;
	}

	public void AutoHeal()
	{
		if (!isDead)
		{
			health += 0.5f * Time.deltaTime;
			if (Time.time > staminaRegenCooldown) stamina += 5f * Time.deltaTime;
		}
		health = Mathf.Clamp(health, 0, baseHealth);
		stamina = Mathf.Clamp(stamina, 0, baseStamina);
	}
}
