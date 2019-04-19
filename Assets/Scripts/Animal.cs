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
	public AnimalCharacter character;
	public Era era;
	public Habitat habitat;
	//public Aggression defaultAggression;
	public Aggression currentAggression;
	//public bool isDead;

	public Vector3 modelDimensions;

	[HideInInspector] public AnimalAnimator animator;
	[HideInInspector] public AnimalMotor motor;
	float pauseCheckCooldown = 0;
	float lifeTime;
	float despawnTimer;
	float attackCooldown;

	protected override void Awake()
	{
		base.Awake();
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
			Debug.Log("no match found in stats for " + name);
		}
		modelDimensions = GetSize();
		if (character.bodyLength == 0)
		{
			character.bodyLength = modelDimensions.z;
			Debug.Log("couldn't find body length for " + name + " guessing " + character.bodyLength + " based on model length");
		}
		if (character.bodyMass == 0) character.bodyMass = character.bodyLength / 2;
		if (character.maxSpeed == 0) character.maxSpeed = character.bodyLength / 2;
		if (character.turnSpeed == 0) character.turnSpeed = 1;
		character.health = character.baseHealth;
		if (SceneManager.instance.maxSpawns < 50) lifeTime = Random.Range(900, 1200);
		else lifeTime = Random.Range(60, 120);
		attackCooldown = Time.time;
	}

	private void Update()
	{
		//bool close = PauseIfDistant();
		//if (!close) return;
		character.AutoHeal();
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
		s += "\n" + currentAggression;
		if (character.target && !character.isDead)
		{
			s += "\nAttacking: " + character.target.name;
		}
		s += "\nLength: " + character.bodyLength.ToString("0.0") + " metres";
		s += "\nMax Speed: " + character.maxSpeed.ToString("0.0") + " m/s";
		s += "\nTran Dist: " + Vector3.Distance(hit.transform.position, player.transform.position).ToString("0.0") + " metres";
		s += "\nRay Dist: " + hit.distance.ToString("0.0") + " metres";
		return s;
	}

	public void TakeDamage(Transform attacker, float damage)
	{
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
		//Debug.Log("attacking?");
		if (Time.time < attackCooldown) return;
		attackCooldown = Time.time + 2;

		if (currentAggression == Aggression.Passive) return;

		Animal a = character.target.GetComponent<Animal>();
		Player p = character.target.GetComponent<Player>();
		if (a)
		{
			if ((currentAggression == Aggression.Passive || currentAggression == Aggression.Neutral) && a.character.isDead) return;
			a.TakeDamage(transform, character.baseDamage);
		}
		else if (p)
		{
			if ((currentAggression == Aggression.Passive || currentAggression == Aggression.Neutral) && p.character.isDead) return;
			p.TakeDamage(transform, character.baseDamage);
		}
		animator.SetAnim(AnimalAnimator.Anim.Attack);
	}

	public float AttackRange()
	{
		if (!character.target) return 0;
		Animal a = character.target.GetComponent<Animal>();
		Player p = character.target.GetComponent<Player>();
		Character c = a ? (Character)a.character : (Character)p.character;
		return character.bodyLength / 2f + c.bodyLength / 2f + 0.5f;
	}

	private void FindNewPrey()
	{
		float maxrange = 40f;
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
		}
		else if (Vector3.Distance(transform.position, p.transform.position) < maxrange && !p.character.isDead)
		{
			character.target = p.transform;
			Debug.Log(name + " is mad at you");
		}
		else if (nearestneutralt)
		{
			character.target = nearestneutralt;
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
		Animal a = GetComponent<Animal>();
		SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
		if (smr != null)
		{
			Vector3 size = smr.bounds.size;
			float adjustment = a.character.bodyLength / size.z;
			//Debug.Log(string.Format("'" + name + "',size={0},{1},{2}, should be {3}", size.x, size.y, size.z, properSize));
			//if (1f - adjustment > .01) 
			if (adjustment < 0.98f || adjustment > 1.02f)
				Debug.Log(string.Format("'" + name + "',size={0}, should be {1}, adjustment={2}", size.z, a.character.bodyLength, adjustment));
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
