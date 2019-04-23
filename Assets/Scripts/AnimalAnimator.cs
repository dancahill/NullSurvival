using System;
using System.Collections;
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
			animalAnimator.Play("die");
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
		if (animalAnimator)
		{ // newer animation type
			string newclip = anim.ToString().ToLower();

			//int stateHash = Animator.StringToHash("Base Layer.Run");


			if (animalAnimator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
			{
				//Debug.Log("Attacking");
				return;
			}
			if (animal.habitat == Animal.Habitat.Sea)
			{
				if (newclip == "idle" || newclip == "walk") newclip = "swim";
				else if (newclip == "run") newclip = "fastswim";
			}
			if (!animalAnimator.HasState(0, Animator.StringToHash(newclip)))
			{
				Debug.Log(name + " has no clip for " + newclip + "?");
				return;
			}
			//Debug.Log("new clip=" + newclip);
			animalAnimator.Play(newclip);
		}
		else if (animalAnimation) // old animation type
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
				// putting an event in the animation would be much nicer than this
				if (attacks.Contains(newclip))
				{
					float damagedelay = 1f;
					switch (name)
					{
						case "Ankylosaurus": damagedelay = 1f; break;
						case "Argentinosaurus":
						case "Brontosaurus": damagedelay = 3.6f; break;
						case "Carnotaurus": damagedelay = 0.6f; break;
						case "Parasaurolophus": damagedelay = 0f; break;
						case "Spinosaurus": damagedelay =0.6f; break;
						case "Stegosaurus": damagedelay = 1f; break;
						case "Triceratops": damagedelay = 0.6f; break;
						case "Tyrannosaurus Rex": damagedelay = 0.6f; break;
						case "Velociraptor": damagedelay = 0.6f; break;
						default: damagedelay = 1; break;
					}
					StartCoroutine(DoDamage(damagedelay));
				}
				return;
			}
			if (anim == Anim.Run) SetAnim(Anim.Walk);
			else Debug.Log("clip " + newclip + " not found for " + name);
		}
		else
		{
			Debug.LogWarning("no animation found");
		}
	}

	IEnumerator DoDamage(float delay)
	{
		yield return new WaitForSeconds(delay);
		animal.DoDamage();
	}
}
