using UnityEngine;

public class CanvasManager : MonoBehaviour
{
	#region Singleton
	public static CanvasManager instance;
	#endregion
	public GameObject gameCanvas;
	public GameObject HUDPanel;
	public GameObject characterPanel;

	void Awake()
	{
		instance = this;
	}

	public static void SetGameCanvasActive(bool active)
	{
		instance.gameCanvas.SetActive(active);
		if (!active)
		{
			SetHUDActive(active);
			SetCharPanelActive(active);
		}
	}

	public static bool IsGameCanvasActive()
	{
		return instance.gameCanvas.activeSelf;
	}

	public static void SetHUDActive(bool active)
	{
		//if (active && !instance.gameCanvas.activeSelf) instance.gameCanvas.SetActive(active);
		if (active) SetGameCanvasActive(true);
		instance.HUDPanel.SetActive(active);
	}

	public static bool IsHUDActive()
	{
		return instance.HUDPanel.activeSelf;
	}

	public static void SetCharPanelActive(bool active)
	{
		//if (active && !instance.gameCanvas.activeSelf) instance.gameCanvas.SetActive(active);
		if (active) SetGameCanvasActive(true);
		instance.characterPanel.SetActive(active);
		//if (active)
		//{
		//	Debug.Log("disabling mouse lock");
		//	Cursor.lockState = CursorLockMode.None;
		//	Cursor.visible = true;
		//}
		//else if (!active)
		//{
		//	Debug.Log("enabling mouse lock");
		//	Cursor.lockState = CursorLockMode.Locked;
		//	Cursor.visible = false;
		//}
	}

	public static bool IsCharPanelActive()
	{
		return instance.characterPanel.activeSelf;
	}
}
