using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimator))]
public class Player : MonoBehaviour
{
	[HideInInspector] public PlayerAnimator animator;
	public PlayerCharacter character;
	float attackCooldown;

	GameCanvas gc;

	private void Awake()
	{
		animator = GetComponent<PlayerAnimator>();
		character = new PlayerCharacter();
		gc = FindObjectOfType<GameCanvas>();
		attackCooldown = Time.time;
	}

	private void Update()
	{
		character.AutoHeal();
		if (transform.position.y < -200) TakeDamage(null, 10000);
	}

	public void TakeDamage(Transform attacker, float damage)
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
		if (Time.time < attackCooldown) return;
		attackCooldown = Time.time + 0.5f;
		if (!Camera.main) return;
		RaycastHit hit;

		int layerMask = ~LayerMask.GetMask("Player") & ~LayerMask.GetMask("Water");
		//int layerMask = ~LayerMask.GetMask("Player");

		if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, SceneManager.instance.maxDinoRenderDistance, layerMask)) return;
		int layer = hit.transform.gameObject.layer;
		//Debug.Log("harvestable=" + LayerMask.NameToLayer("Harvestable"));
		//Debug.Log("animal=" + LayerMask.NameToLayer("Animal"));
		//if (layer == LayerMask.NameToLayer("Harvestable")) { Harvest(hit); return; }
		if (layer == LayerMask.NameToLayer("Ground")) { Harvest(hit); return; }
		if (layer == LayerMask.NameToLayer("Ground")) { Harvest(hit); return; }
		if (layer != LayerMask.NameToLayer("Animal")) // 10 is Animals
		{
			return;
		}
		GameObject go = hit.transform.gameObject;
		Animal a = hit.transform.gameObject.GetComponent<Animal>();
		for (int i = 0; i < 3; i++)
		{
			if (a) break;
			go = go.transform.parent.gameObject;
			a = go.GetComponent<Animal>();
		}
		if (!a) return;
		if (hit.distance > 2) return;
		a.character.target = transform;
		AudioSource aud = GetComponent<AudioSource>();
		AudioClip clip = (AudioClip)Resources.Load("Sounds/PlayerSwing1");
		if (clip != null) aud.PlayOneShot(clip);
		a.TakeDamage(transform, character.baseDamage);
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

		Vector3 pos = transform.position;
		Vector3 dest = hit.point;
		float y = transform.position.y - hit.point.y;
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
