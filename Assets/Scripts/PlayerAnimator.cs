using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	Player player;
	Animator playerAnimator;
	public GameObject weaponSlot;
	public List<GameObject> weaponBlueprints;

	public bool isRunning;
	public bool isArmed;
	public bool isAttacking;
	bool hit01;
	bool hit02;

	private void Awake()
	{
		player = GetComponent<Player>();
		playerAnimator = GetComponentInChildren<Animator>();
	}

	void Update()
	{
		if (playerAnimator)
		{
			if (isAttacking)
			{
				int ff = Random.Range(0, 100);
				if (ff > 50) { hit01 = true; hit02 = false; }
				else { hit01 = false; hit02 = true; }
			}
			else
			{
				hit01 = false; hit02 = false;
			}
			playerAnimator.SetBool("IsRunning", isRunning);
			playerAnimator.SetBool("WeaponIsOn", isArmed);
			playerAnimator.SetBool("Hit01", hit01);
			playerAnimator.SetBool("Hit02", hit02);
		}
	}
}
