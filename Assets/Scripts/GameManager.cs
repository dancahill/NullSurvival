using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	#region Singleton
	public static GameManager instance;
	#endregion
	[HideInInspector] public SceneController sceneController;

	void Awake()
	{
		instance = this;
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Persistent")
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Persistent");
			return;
		}
		sceneController = FindObjectOfType<SceneController>();
		if (!sceneController) throw new UnityException("Scene Controller missing. Make sure it exists in the Persistent scene.");
		if (sceneController.CurrentScene == "") sceneController.CurrentScene = "Intro";
	}
}
