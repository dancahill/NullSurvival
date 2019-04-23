using UnityEngine;

[RequireComponent(typeof(AnimalAnimator))]
[RequireComponent(typeof(AnimalMotor))]
public class Animal : Interactable
{
	public enum Aggression
	{
		Passive,
		Neutral,
		Aggressive
	}
	public enum Habitat
	{
		Land,
		Sea,
		Air
	}
	public enum Era
	{
		Modern,
		Prehistoric
	}

	public Era era;
	public Habitat habitat;
	public Aggression currentAggression;
	public Vector3 modelDimensions;
	public AnimalCharacter character;

	[HideInInspector] public AnimalAnimator animator;
	[HideInInspector] public AnimalMotor motor;
	float pauseCheckCooldown = 0;
	float lifeTime;
	float despawnTimer;
	float attackCooldown;
	float aggroCooldown;

	protected override void Awake()
	{
		base.Awake();
		character.transform = transform;
		character.attackVector = transform.Find("AttackVector");
		animator = GetComponent<AnimalAnimator>();
		motor = GetComponent<AnimalMotor>();
	}

	private void Start()
	{
		AnimalStats.Stats stats = AnimalStats.Animals.Find(s => s.name == name);
		if (stats != null)
		{
			if (stats.baseDamage > 0) character.baseDamage = stats.baseDamage;
			if (stats.baseHealth > 0) character.baseHealth = stats.baseHealth;
			if (stats.bodyLength > 0) character.bodyLength = stats.bodyLength;
			if (stats.bodyMass > 0) character.bodyMass = stats.bodyMass;
			if (stats.maxSpeed > 0) character.maxSpeed = stats.maxSpeed;
			if (stats.turnSpeed > 0) character.turnSpeed = stats.turnSpeed;
		}
		else
		{
			Debug.Log("no stats found for " + name);
		}
		modelDimensions = GetSize();
		if (character.bodyLength == 0)
		{
			character.bodyLength = modelDimensions.z;
			Debug.Log("couldn't find body length for " + name + " guessing " + character.bodyLength.ToString("0.##") + " metres based on model length");
		}
		if (character.bodyMass == 0) character.bodyMass = character.bodyLength / 2;
		if (character.maxSpeed == 0) character.maxSpeed = character.bodyLength / 2;
		if (character.maxSpeed < 0.5f) character.maxSpeed = 0.5f;
		if (character.turnSpeed == 0) character.turnSpeed = 1;
		if (character.attackRadius == 0) character.attackRadius = character.bodyLength / 4f + 0.5f;
		character.health = character.baseHealth;
		//if (SceneManager.instance.maxSpawns < 50) 
		lifeTime = Random.Range(900, 1800);
		//else lifeTime = Random.Range(60, 120);
		attackCooldown = Time.time;
		aggroCooldown = Time.time;
	}

	private void Update()
	{
		//bool close = PauseIfDistant();
		//if (!close) return;
		character.AutoHeal();
		AgeDeath();
		lifeTime -= Time.deltaTime;
		FindNewPrey();
	}

	public override bool Interact(RaycastHit hit)
	{
		base.Interact(hit);
		if (hit.distance > 2) return false;
		character.target = player.transform;
		AudioSource aud = player.transform.GetComponent<AudioSource>();
		AudioClip clip = (AudioClip)Resources.Load("Sounds/PlayerSwing1");
		if (clip != null) aud.PlayOneShot(clip);
		TakeDamage(player.transform, character.baseDamage);
		return true;
	}

	public override string Describe(RaycastHit hit)
	{
		//return base.Describe(hit);
		string s = transform.name;
		if (character.isDead) s = "Dead " + s;
		else s += " (" + currentAggression + ")";
		if (character.target && !character.isDead) s += "\nAttacking: " + character.target.name;
		s += "\nLength: " + character.bodyLength.ToString("0.#") + "m";
		s += "\nMax Speed: " + character.maxSpeed.ToString("0.#") + "m/s";
		s += "\nHealth: " + character.health.ToString("0.#") + "/" + character.baseHealth.ToString("0.#");
		//s += "\nTran Dist: " + Vector3.Distance(hit.transform.position, player.transform.position).ToString("0.0") + " metres";
		float colliderdist = character.NearestMeshPoint(player.character.attackVector.position);
		s += "\nRay/Coll Dist: " + hit.distance.ToString("0.#") + "/" + colliderdist.ToString("0.#") + "m";
		//if (!player) Debug.Log("player is null");
		//if (player.character == null) Debug.Log("player.character is null");
		//if (!player.character.attackVector) Debug.Log("player.character.attackVector is null");

		//Collider c = GetComponent<Collider>();
		//if (c) 
		//s += "\nColl Dist: " + dist.ToString("0.0") + " metres";
		return s;
	}

	public override void TakeDamage(Transform attacker, float damage)
	{
		aggroCooldown = Time.time + 15;
		character.lastAttacker = attacker;
		if (!character.target) character.target = attacker;
		if (!character.isDead)
		{
			character.TakeDamage(damage);
			if (character.isDead)
			{
				currentAggression = Aggression.Passive;
				despawnTimer = Time.time + 10;
				animator.Die();
				GameCanvas gc = FindObjectOfType<GameCanvas>();
				if (gc) gc.SetToastText("<size=18><color=yellow>" + name + " was killed by a " + attacker.name + "</color></size>");
			}
		}
	}

	public void Attack()
	{
		if (!InAttackPosition()) return;
		if (Time.time < attackCooldown) return;
		Interactable ia = character.target.GetComponent<Interactable>();
		if (!ia) return;
		if (ia.isDead && currentAggression != Aggression.Aggressive) return;
		attackCooldown = Time.time + 2;
		animator.SetAnim(AnimalAnimator.Anim.Attack);
		aggroCooldown = Time.time + 15;
	}

	/// <summary>
	/// called by AnimalAnimatorEvents as a callback event from the animator
	/// do the damage at the time of impact _if_ the animal is still alive
	/// </summary>
	public void DoDamage()
	{
		if (!InAttackPosition()) return;
		Interactable ia = character.target.gameObject.GetComponent<Interactable>();
		if (ia) ia.TakeDamage(transform, character.baseDamage);
	}

	public bool InAttackPosition()
	{
		if (isDead || currentAggression == Aggression.Passive || !character.target) return false;
		if (motor.GetDistanceToTarget() >= AttackRange()) return false;
		Vector3 targetDir = character.target.position - transform.position;
		float angle = Vector3.Angle(targetDir, transform.forward);
		if (angle > 45) return false;
		return true;
	}

	public float AttackRange()
	{
		if (!character.target) return Mathf.Infinity;
		Animal a = character.target.GetComponent<Animal>();
		Player p = character.target.GetComponent<Player>();
		Character targetcharacter = a ? (Character)a.character : (Character)p.character;
		return character.attackRadius + targetcharacter.bodyLength / 2f;
	}

	private void FindNewPrey()
	{
		float maxrange = 40f;
		if (Time.time > aggroCooldown) character.target = null;
		if (currentAggression != Aggression.Aggressive) return;
		if (character.target && Vector3.Distance(transform.position, character.target.transform.position) < maxrange) return;
		//Debug.Log(name + " is aggressive and needs a target to attack");
		Transform nearestpassivet = null;
		Transform nearestneutralt = null;
		float nearestpassived = Mathf.Infinity;
		float nearestneutrald = Mathf.Infinity;
		foreach (Animal a in FindObjectsOfType<Animal>())
		{
			float dist = Vector3.Distance(transform.position, a.transform.position);
			if (dist > maxrange) continue;
			if (a.currentAggression == Aggression.Passive && dist < nearestpassived)
			{
				nearestpassivet = a.transform;
				nearestpassived = dist;
			}
			else if (a.currentAggression == Aggression.Neutral && dist < nearestneutrald)
			{
				nearestneutralt = a.transform;
				nearestneutrald = dist;
			}
		}
		Player p = FindObjectOfType<Player>();
		if (nearestpassivet)
		{
			character.target = nearestpassivet;
			aggroCooldown = Time.time + 15;
		}
		else if (Vector3.Distance(transform.position, p.transform.position) < maxrange && !p.character.isDead)
		{
			Debug.Log(name + " is mad at you");
			character.target = p.transform;
			aggroCooldown = Time.time + 15;
		}
		else if (nearestneutralt)
		{
			character.target = nearestneutralt;
			aggroCooldown = Time.time + 15;
		}
	}

	private void AgeDeath()
	{
		if (animator.enabled && lifeTime < 0 && !character.isDead)
		{
			character.isDead = true;
			currentAggression = Aggression.Passive;
			despawnTimer = Time.time + 15;
			GameCanvas gc = FindObjectOfType<GameCanvas>();
			if (gc) gc.SetToastText(name + " died of old age");
			animator.Die();
			return;
		}
		if (character.isDead && despawnTimer < Time.time)
		{
			Destroy(gameObject);
			return;
		}
	}

	public bool IsPlayerInRange()
	{
		Player p = FindObjectOfType<Player>();
		if (Vector3.Distance(transform.position, p.transform.position) <= SceneManager.instance.maxDinoActiveDistance) return true;
		return false;
	}

	public bool PauseIfDistant()
	{
		if (pauseCheckCooldown > Time.time) return motor.enabled;
		pauseCheckCooldown = Time.time + 2f;
		bool inrange = IsPlayerInRange();
		animator.enabled = inrange;
		motor.enabled = inrange;
		return inrange;
	}

	Vector3 GetSize()
	{
		//Animal a = GetComponent<Animal>();
		SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
		if (smr != null)
		{
			Vector3 size = smr.bounds.size;
			float adjustment = character.bodyLength / size.z;
			//Debug.Log(string.Format("'" + name + "',size={0},{1},{2}, should be {3}", size.x, size.y, size.z, properSize));
			//if (1f - adjustment > .01) 
			if (adjustment < 0.98f || adjustment > 1.02f)
				Debug.Log(string.Format("'" + name + "',size={0}, should be {1}, adjustment={2}", size.z, character.bodyLength, adjustment));
			return size;
		}
		else
		{
			Debug.Log("no skinned mesh found in " + name);
		}
		MeshRenderer mr = GetComponent<MeshRenderer>();
		if (mr != null)
		{
			Vector3 size = mr.bounds.size;
			Debug.Log(string.Format("'" + name + "',size={0},{1},{2}", size.x, size.y, size.z));
			return size;
		}
		return Vector3.zero;
	}
}
