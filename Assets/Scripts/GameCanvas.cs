using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
	class ToastLog
	{
		public string message;
		public float expiry;
	}
	List<ToastLog> log;

	float updateInterval = 1.0f;
	float lastInterval; // Last interval end time
	float frames = 0; // Frames over current interval

	public Player player;


	[Header("HUD")]
	public Text FPSText;
	public Text toastText;
	public Text crosshairText;
	public Text crosshairDescText;
	public Image healthBar;

	public float FPS { get; private set; }

	[Header("Character Stats")]
	public Text charHealthText;
	public Image charHealthBar;

	void Start()
	{
		log = new List<ToastLog>();
		frames = 0;
		lastInterval = Time.realtimeSinceStartup;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	void Update()
	{
		player = FindObjectOfType<Player>();
		frames++;
		float now = Time.time;
		if (now > lastInterval + updateInterval)
		{
			FPS = frames / (now - lastInterval);
			frames = 0;
			lastInterval = now;
		}
		FPSText.text = FPS.ToString("0.0") + " FPS";
		SpawnManager sm = FindObjectOfType<SpawnManager>();
		if (sm)
		{
			FPSText.text += string.Format("\nActive: {0}/{1}", sm.totalActive, sm.totalAnimals);
		}
		//ShowMem();
		ShowLog();
		ShowTargetDescription();

		if (player) healthBar.fillAmount = player.character.health / player.character.baseHealth;
		if (CanvasManager.IsCharPanelActive()) UpdateCharacterPanel();
	}

	private void ShowMem()
	{
		//FPSText.text += string.Format("\nGPU memory : {0}    Sys Memory : {1}\n", SystemInfo.graphicsMemorySize, SystemInfo.systemMemorySize);
		long mem = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
		FPSText.text += string.Format("\nAllocated Memory: {0}MB\nReserved Memory: {1}MB\nUnused Reserved: {2}MB",
			UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1048576,
			UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong() / 1048576,
			UnityEngine.Profiling.Profiler.GetTotalUnusedReservedMemoryLong() / 1048576
		);
	}

	private void ShowLog()
	{
		log.RemoveAll(l => l.expiry < Time.time);
		string text = "";
		if (log.Count > 2) text += log[log.Count - 3].message + "\n";
		if (log.Count > 1) text += log[log.Count - 2].message + "\n";
		if (log.Count > 0) text += log[log.Count - 1].message + "\n";
		toastText.text = text.Trim();
	}

	private void ShowTargetDescription()
	{
		int layerMask = ~LayerMask.GetMask("Player") & ~LayerMask.GetMask("Water");
		RaycastHit hit;

		crosshairDescText.text = "";
		if (!Camera.main) return;
		if (!Physics.Raycast(Camera.main.transform.position, Camera.main.transform.TransformDirection(Vector3.forward), out hit, SceneManager.instance.maxDinoRenderDistance, layerMask)) return;
		int layer = hit.transform.gameObject.layer;
		if (LayerMask.LayerToName(layer) == "Animal")
		{
			GameObject go = hit.transform.gameObject;
			Animal a = hit.transform.gameObject.GetComponent<Animal>();
			string s = go.name;

			for (int i = 0; i < 3; i++)
			{
				if (a) break;
				go = go.transform.parent.gameObject;
				a = go.GetComponent<Animal>();
			}
			if (a)
			{
				s = go.name;
				if (a.character.isDead) s = "Dead " + s;
				//s += "\nHabitat: " + a.habitat;
				s += "\n" + a.currentAggression;
				if (a.character.target)
				{
					s += "\nAttacking: " + a.character.target.name;
				}
				s += "\nLength: " + a.character.bodyLength.ToString("0.0") + " metres";
				//s += "\nMass: " + a.bodyMass;
				s += "\nMax Speed: " + a.character.maxSpeed.ToString("0.0") + " m/s";
				s += "\nDistance: " + hit.distance.ToString("0.0") + " metres";
				s += "\nDist: " + Vector3.Distance(hit.transform.position, FindObjectOfType<Player>().transform.position).ToString("0.0") + " metres";
				//s += "\nLayer: " + hit.transform.gameObject.layer + "," + LayerMask.GetMask("Animal");
			}
			crosshairDescText.text = "\n" + s;
		}
		else if (LayerMask.LayerToName(layer) == "Ground" && TreeManager.GetSampleHeight(hit) > 0.01f)
		{
			crosshairDescText.text = "\n" + TreeManager.GetTreeNameAt(hit.point) + " (" + hit.transform.name + ")";
			//CapsuleCollider cc = GetComponent<CapsuleCollider>();
			//if (cc) crosshairDescText.text += "\ncc.name=" + cc.name;
			//BoxCollider bc = GetComponent<BoxCollider>();
			//if (bc) crosshairDescText.text += "\nbc.name=" + cc.name;
		}
	}

	void UpdateCharacterPanel()
	{
		if (!player) return;
		charHealthBar.fillAmount = player.character.health / player.character.baseHealth;
		charHealthText.text = string.Format("{0:0.#}/{1}", player.character.health, player.character.baseHealth);
	}

	public void SetToastText(string text)
	{
		// just keep the last three and pretend we know they're in the same order we added them
		if (log.Count > 3) log.RemoveAt(0);
		log.Add(new ToastLog { message = text, expiry = Time.time + 5f });
	}

	public void SetToastInfo(string text)
	{
		SetToastText("<size=18><color=grey>" + text + "</color></size>");
	}

	public void SetToastWarning(string text)
	{
		SetToastText("<size=30><color=red>" + text + "</color></size>");
		Debug.LogWarning(text);
	}
}
