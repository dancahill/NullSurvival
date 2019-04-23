using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimator))]
public class Player : Interactable
{
	[HideInInspector] public PlayerAnimator animator;
	public PlayerCharacter character;
	float attackCooldown;
	new public bool isDead { get { return character.isDead; } }

	public bool runToggle;

	GameCanvas gc;

	protected override void Awake()
	{
		base.Awake();
		character = new PlayerCharacter();
		character.transform = transform;
		character.attackVector = transform.Find("AttackVector");
		animator = GetComponent<PlayerAnimator>();
		gc = FindObjectOfType<GameCanvas>();
		attackCooldown = Time.time;
	}

	private void Update()
	{
		character.AutoHeal();
		if (transform.position.y < -200) TakeDamage(null, 10000);
	}

	public override void TakeDamage(Transform attacker, float damage)
	{
		character.lastAttacker = attacker;
		if (!character.target) character.target = attacker;
		if (character.isDead) return;
		character.TakeDamage(damage);
		AudioSource a = GetComponent<AudioSource>();
		if (character.isDead)
		{
			AudioClip clip = (AudioClip)Resources.Load("Sounds/PlayerDie");
			if (clip != null) a.PlayOneShot(clip);
			if (attacker) gc.SetToastWarning(name + " was killed by a " + attacker.name);
			else gc.SetToastWarning(name + " died");
			Respawn();
		}
		else
		{
			AudioClip clip = (AudioClip)Resources.Load("Sounds/PlayerHit1");
			if (clip != null) a.PlayOneShot(clip);
		}
	}

	public void Attack()
	{
		int layerMask = ~LayerMask.GetMask("Player") & ~LayerMask.GetMask("Water");
		RaycastHit hit;
		if (!Camera.main) return;
		if (Time.time < attackCooldown) return;
		attackCooldown = Time.time + 0.5f;
		character.UseStamina(5);
		if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, SceneManager.instance.maxDinoRenderDistance, layerMask)) return;
		Interactable ia = hit.transform.gameObject.GetComponent<Interactable>();
		if (ia)
		{
			bool acted = ia.Interact(hit);
			//if (!acted) Debug.Log("ia.Interact(hit); returned false. no action taken?");
		}
	}

	public void Jump()
	{
		UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpc = FindObjectOfType<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>();
		fpc.AddJump();
	}

	private void Respawn()
	{
		StartCoroutine(Revive());
	}

	IEnumerator Revive()
	{
		GameObject spawns = GameObject.Find("PlayerSpawns");
		Transform t = spawns.transform.GetChild(Random.Range(0, spawns.transform.childCount));
		GameManager.instance.sceneController.FadeAndRespawn(t.position);
		// should de-aggro attacker, reset stats, etc before fadeout
		yield return new WaitForSeconds(1);
		foreach (Animal a in FindObjectsOfType<Animal>())
		{
			if (a.character.target == transform) a.character.target = null;
		}
		character.health = character.baseHealth;
		character.isDead = false;
	}

	private void OnDrawGizmosSelected()
	{
		if (!SceneManager.instance) return;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, SceneManager.instance.maxDinoActiveDistance);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, SceneManager.instance.maxDinoRenderDistance);
	}
}
