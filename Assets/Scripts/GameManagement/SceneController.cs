using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
	// for the fader
	private GameObject m_FaderCanvas;
	private Canvas m_Canvas;
	private CanvasScaler m_CanvasScaler;
	private GameObject m_Panel;
	private Image m_Image;

	public event Action AfterSceneLoad;
	public event Action BeforeSceneUnload;

	public string StartingSceneName = "Intro";
	public string CurrentScene = "";
	public string PreviousScene = "";
	private bool isFading;
	float time = 1;

	private void Awake()
	{
		MakeFaderCanvas();
	}

	IEnumerator Start()
	{
		yield return StartCoroutine(LoadSceneAndSetActive(StartingSceneName));
		yield return StartCoroutine(FadeIn());
	}

	public static string GetActiveSceneName()
	{
		return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
	}

	public void FadeAndRespawn(Vector3 spawnpoint)
	{
		if (!isFading) StartCoroutine(FadeAndRespawn2(spawnpoint));
	}

	public void FadeAndLoadScene(string scenename)
	{
		if (!isFading) StartCoroutine(FadeAndSwitchScenes(scenename));
	}

	private IEnumerator FadeAndRespawn2(Vector3 spawnpoint)
	{
		// reminder: in the fade, set canvas.blocksRaycasts = true, and false when done
		// Start fading to black and wait for it to finish before continuing.
		yield return StartCoroutine(FadeOut());
		// don't unload anything. just go to the indicated spawn point
		CharacterController cc = GameObject.Find("Player").GetComponent<CharacterController>();
		cc.enabled = false;
		cc.transform.position = spawnpoint;
		cc.enabled = true;
		// Start fading back in and wait for it to finish before exiting the function.
		yield return StartCoroutine(FadeIn());
	}

	private IEnumerator FadeAndSwitchScenes(string sceneName)
	{
		// reminder: in the fade, set canvas.blocksRaycasts = true, and false when done

		// Start fading to black and wait for it to finish before continuing.
		//yield return StartCoroutine(Fade(1f));
		yield return StartCoroutine(FadeOut());

		//if (sceneName == "Intro") CanvasManager.DisableGameCanvas(); else CanvasManager.EnableGameCanvas();
		CanvasManager.SetGameCanvasActive(sceneName != "Intro");

		if (BeforeSceneUnload != null) BeforeSceneUnload();
		// Unload the current active scene.
		yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
		// Start loading the given scene and wait for it to finish.
		yield return StartCoroutine(LoadSceneAndSetActive(sceneName));
		// If this event has any subscribers, call it.
		if (AfterSceneLoad != null) AfterSceneLoad();
		// Start fading back in and wait for it to finish before exiting the function.
		//yield return StartCoroutine(Fade(0f));
		if (sceneName != "GameOver") yield return StartCoroutine(FadeIn());
		//yield return null;
	}

	private IEnumerator LoadSceneAndSetActive(string sceneName)
	{
		bool rc = FindObjectOfType<LoadAssetBundles>().LoadAssetBundleScene(sceneName);
		if (!rc)
		{
			// Allow the given scene to load over several frames and add it to the already loaded scenes (just the Persistent scene at this point).
			yield return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
			// Find the scene that was most recently loaded (the one at the last index of the loaded scenes).
			Scene newlyLoadedScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
			// Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next).
			UnityEngine.SceneManagement.SceneManager.SetActiveScene(newlyLoadedScene);
		}

		PreviousScene = CurrentScene;
		CurrentScene = sceneName;
	}

	void MakeFaderCanvas()
	{
		m_FaderCanvas = new GameObject("FaderCanvas");

		m_FaderCanvas.gameObject.layer = 5;// UI

		m_Canvas = m_FaderCanvas.gameObject.AddComponent<Canvas>();
		m_CanvasScaler = m_FaderCanvas.gameObject.AddComponent<CanvasScaler>();

		m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		m_Canvas.sortingOrder = 999;

		m_Panel = new GameObject("Panel");
		m_Panel.transform.parent = m_FaderCanvas.gameObject.transform;
		m_Panel.AddComponent<CanvasRenderer>();
		m_Panel.layer = 5;// UI

		m_Image = m_Panel.AddComponent<Image>();
		m_Image.color = Color.black;

		RectTransform rt = m_Panel.GetComponent<RectTransform>();
		rt.localPosition = Vector3.zero;
		rt.anchorMin = new Vector2(0, 0);
		rt.anchorMax = new Vector2(1, 1);
	}

	IEnumerator FadeIn()
	{
		float t = time;

		isFading = true;
		//Time.timeScale = 0;
		while (t > 0f)
		{
			t -= Time.deltaTime;
			float a = t / time;
			//Debug.Log(string.Format("fading in - alpha = {0}", a));
			m_Image.color = new Color(0, 0, 0, a);
			yield return 0;
		}
		Time.timeScale = 1;
		isFading = false;
	}

	IEnumerator FadeOut()
	{
		float t = 0f;

		isFading = true;
		//Time.timeScale = 0;
		while (t < time)
		{
			t += Time.deltaTime;
			//curve.Evaluate(t);
			float a = t / time;
			//Debug.Log(string.Format("fading out - alpha = {0}", a));
			m_Image.color = new Color(0, 0, 0, a);
			yield return 0;
		}
		Time.timeScale = 1;
		//SceneManager.LoadScene(scene);
		//FadeAndLoadScene(scene);
		isFading = false;
	}
}
