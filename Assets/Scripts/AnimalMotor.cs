using UnityEngine;

public class AnimalMotor : MonoBehaviour
{
	Animal animal;
	CharacterController characterController;
	Vector3 terrainSize;
	public bool isMoving;
	public bool isFalling;
	public float groundDistance;
	public Vector3 destinationV;
	public Vector3 localVelocity;
	float destinationCooldown;
	float idleCooldown;

	private void Awake()
	{
		animal = GetComponent<Animal>();
		characterController = GetComponent<CharacterController>();
	}

	private void Start()
	{
		terrainSize = SceneManager.instance.GetTerrainSize();
		idleCooldown = Time.time;
		destinationV = transform.forward;
		SetNewDestination();
	}

	private void Update()
	{
		Vector3 v = new Vector3(
			Mathf.Clamp(transform.position.x, 1, terrainSize.x - 1),
			Mathf.Clamp(transform.position.y, 1, terrainSize.y - 1),
			Mathf.Clamp(transform.position.z, 1, terrainSize.z - 1)
		);
		transform.position = v;
	}

	private void FixedUpdate()
	{
		FixedUpdateCC();
		animal.animator.SetAnim(localVelocity.z, animal.character.maxSpeed);
	}

	private void FixedUpdateCC()
	{
		// need to look at this later:
		// https://answers.unity.com/questions/242648/force-on-character-controller-knockback.html

		if (animal.character.isDead) return;
		if (!isMoving) return;
		if (!characterController) return;

		float range = animal.AttackRange();
		if (range > 0 && GetDistanceToTarget() < range)
		{
			characterController.Move(Vector3.zero);
			destinationV = transform.position;
			animal.Attack();
		}

		if (animal.character.target) destinationV = animal.character.target.position;

		// some defaults to test with
		characterController.slopeLimit = 30; // should probably be lower than 45
						     //characterController.stepOffset = animal.bodyLength / 10;
		characterController.minMoveDistance = animal.character.bodyLength / 10 * Time.deltaTime;

		FaceDestination();
		groundDistance = transform.position.y - Util.GetHeightAtPoint(transform.position).groundHeight;
		if (SetDestination()) return;

		isFalling = !characterController.isGrounded;

		float gravity = 9.81f * Time.deltaTime;//  i know. don't care.
		if (animal.habitat == Animal.Habitat.Sea) gravity = 0;

		float speed = animal.character.maxSpeed * Time.deltaTime;
		if (!animal.character.target) speed /= 2;
		else if (GetHDistanceToDestination() < animal.character.bodyLength * 1f) speed /= 2f;
		////transform.position = Vector3.Lerp(transform.position, transform.position + transform.forward, rate);
		characterController.Move(transform.forward * speed - transform.up * gravity);

		// fix height - probably don't need this if we include gravity in Move()
		// height = GetHeight(transform.position);
		// if (height > 0.1f) transform.position += Vector3.down * Mathf.Clamp(height - 0.1f, 0.0f, 1f);
		// if (height <= 0) transform.position += Vector3.up * 0.1f;

		//https://docs.unity3d.com/ScriptReference/CharacterController-velocity.html
		/*
		 controller = GetComponent<CharacterController>();
		 Vector3 horizontalVelocity = controller.velocity;
		 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
		 // The speed on the x-z plane ignoring any speed
		 float horizontalSpeed = horizontalVelocity.magnitude;
		 // The speed from gravity or jumping
		 float verticalSpeed = controller.velocity.y;
		 // The overall speed
		 float overallSpeed = controller.velocity.magnitude;
		*/
		localVelocity = transform.InverseTransformDirection(characterController.velocity);
		if (characterController.velocity.magnitude < .1)
		{
			//Debug.Log("moving too slow - setting new destination");
			SetNewDestination();
		}
	}

	bool SetDestination()
	{
		if (Time.time < idleCooldown)
		{
			return true;
		}
		if (Time.time > destinationCooldown)
		{
			//Debug.Log("took too long - looking for a new place to go");
			SetNewDestination();
			return true;
		}
		//if (animal.character.target) return true;
		//if (GetHDistanceToDestination() < 2f)
		if (GetHDistanceToDestination() < animal.character.bodyLength / 2f)
		{
			if (Random.Range(0, 100) > 50)
			{
				idleCooldown = Time.time + Random.Range(5, 15);
				animal.animator.SetAnim(AnimalAnimator.Anim.Idle);
				return true;
			}
			animal.animator.SetAnim(AnimalAnimator.Anim.Run);
			SetNewDestination();
			return true;
		}
		return false;
	}

	void SetNewDestination()
	{
		Vector3 newpos;
		float maxdistance = 40f;

		destinationCooldown = Time.time + 30f;
		if (animal.character.target) return;
		do
		{
			newpos = new Vector3(Random.Range(-maxdistance, maxdistance), 10f, Random.Range(-maxdistance, maxdistance));
			if (transform.position.x + newpos.x < 0) continue;
			if (transform.position.z + newpos.z < 0) continue;
			if (transform.position.x + newpos.x > terrainSize.x) continue;
			if (transform.position.z + newpos.z > terrainSize.z) continue;

			//Debug.Log("vector3: setting new destination");
			Vector3 dest = transform.position + newpos;
			Util.PointHeight h = Util.GetHeightAtPoint(dest);
			if (h.waterHeight > 0)
			{
				if (animal.habitat == Animal.Habitat.Sea)
				{
					destinationV = new Vector3(dest.x, h.groundHeight + h.waterDepth / 2, dest.z); // put it in the water at half depth
					return;
				}
			}
			else
			{
				if (animal.habitat == Animal.Habitat.Land)
				{
					destinationV = new Vector3(dest.x, h.groundHeight, dest.z);
					return;
				}
			}
		} while (true);
	}

	void FaceDestination()
	{
		Vector3 dir = destinationV - transform.position;
		if (dir == Vector3.zero) return;
		Quaternion lookRotation = Quaternion.LookRotation(dir);
		Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * animal.character.turnSpeed).eulerAngles;
		transform.rotation = Quaternion.Euler(0, rotation.y, 0);
	}

	public float GetDistanceToDestination()
	{
		return Vector3.Distance(transform.position, destinationV);
	}

	float GetDistanceToTarget()
	{
		if (!animal.character.target) return Mathf.Infinity;
		return Vector3.Distance(transform.position, animal.character.target.position);
	}

	/// <summary>
	/// get x,z distance, ignore height
	/// </summary>
	/// <returns></returns>
	float GetHDistanceToDestination()
	{
		Vector3 pos = transform.position;
		Vector3 dest = destinationV;
		pos.y = 0;
		dest.y = 0;
		return Vector3.Distance(pos, dest);
	}

	private void OnDrawGizmosSelected()
	{
		if (!enabled) return;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(destinationV, 1);
		Gizmos.DrawLine(transform.position, destinationV);
	}
}
