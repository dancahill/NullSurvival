using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimalAnimator : MonoBehaviour
{
	public enum Anim
	{
		Idle,
		Walk,
		Run,
		Attack,
		Die
	}
	Animal animal;
	Animator animalAnimator;
	Animation animalAnimation;
	GameCanvas gc;

	private void Awake()
	{
		animal = GetComponent<Animal>();
		animalAnimator = GetComponentInChildren<Animator>();
		animalAnimation = GetComponentInChildren<Animation>();
	}

	private void Start()
	{
		gc = FindObjectOfType<GameCanvas>();
	}

	private void OnDisable()
	{
		if (animalAnimator) animalAnimator.enabled = false;
		if (animalAnimation) animalAnimation.enabled = false;
	}

	private void OnEnable()
	{
		if (animalAnimator) animalAnimator.enabled = true;
		if (animalAnimation) animalAnimation.enabled = true;
	}

	public void Die()
	{
		if (animalAnimator)
		{
		}
		else if (animalAnimation) // old animation type
		{
			animalAnimation.Play("die");
		}
	}

	public void SetAnim(float speed, float maxspeed)
	{
		if (speed < maxspeed * .1f) SetAnim(Anim.Idle);
		else if (speed < maxspeed * .9f) SetAnim(Anim.Walk);
		else if (speed >= maxspeed * .9f) SetAnim(Anim.Run);
	}

	public void SetAnim(Anim anim)
	{
		if (animal.character.isDead) return;
		if (animalAnimation) // old animation type
		{
			List<string> attacks = new List<string> { "swing1", "swing2", "bite_or_attack", "bite_or_attack_slower", "bite", "swing", "attack" };
			string currentclip = "";
			string newclip = anim.ToString().ToLower();
			bool clipfound = false;
			bool attacking = false;

			//if (anim == Anim.Idle) newclip = "idle";
			//else if (anim == Anim.Walk) newclip = "walk";
			//else if (anim == Anim.Run) newclip = "run";
			//else if (anim == Anim.Attack) newclip = "attack";

			foreach (AnimationState animstate in animalAnimation)
			{
				if (animalAnimation.IsPlaying(animstate.name)) currentclip = animstate.name;
				if (newclip == "attack" && attacks.Contains(animstate.name)) newclip = animstate.name;
				if (newclip == animstate.name) clipfound = true;
			}
			if (attacks.Contains(currentclip)) attacking = true;
			if (attacking) return; // let it finish the attack clip
			if (newclip == currentclip) return; // already playing this clip
			if (clipfound)
			{
				animalAnimation.Play(newclip);
				return;
			}
			if (anim == Anim.Run) SetAnim(Anim.Walk);
			else Debug.Log("clip " + newclip + " not found for " + name);
		}
		else
		{ // newer animation type
		}
	}
}
