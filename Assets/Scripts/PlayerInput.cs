using UnityEngine;

public class PlayerInput : MonoBehaviour
{
	Player player;

	private void Awake()
	{
		player = GetComponent<Player>();
	}

	private void Update()
	{
		GetInput();
	}

	void GetInput()
	{
#if MOBILE_INPUT
#else
		if (CanvasManager.IsCharPanelActive())
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				CanvasManager.SetCharPanelActive(!CanvasManager.IsCharPanelActive());
			}
			return;
		}
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			player.animator.isRunning = true;
		else
			player.animator.isRunning = false;

		if (Input.GetMouseButton(0))
			player.animator.isAttacking = true;
		else
			player.animator.isAttacking = false;

		if (Input.GetMouseButtonDown(0))
		{
			player.Attack();
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			CanvasManager.SetCharPanelActive(!CanvasManager.IsCharPanelActive());
		}
#endif
	}
}
