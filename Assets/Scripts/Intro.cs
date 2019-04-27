using UnityEngine;

public class Intro : MonoBehaviour
{
	private void Start()
	{
		CanvasManager.SetGameCanvasActive(false);
		CanvasManager.SetCharPanelActive(false);
	}

	void Update()
	{
		if (Time.time > 10)
		{
			GameManager.instance.sceneController.FadeAndLoadScene("Scene1");
		}
	}

	public void VisitNullLogic()
	{
		Application.OpenURL("https://nulllogic.ca/unity/");
	}

	public void LoadMap()
	{
		GameManager.instance.sceneController.FadeAndLoadScene("Scene1");
	}

	public void LoadTestMap()
	{
		GameManager.instance.sceneController.FadeAndLoadScene("Test1");
	}
}
