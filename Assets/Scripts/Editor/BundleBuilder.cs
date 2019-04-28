using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class BundleBuilder : Editor
{
	//[MenuItem("Build/Build Asset Bundles")]
	//static void BuildAllAssetBundles()
	//{
	//	Debug.Log("building bundles for '" + EditorUserBuildSettings.activeBuildTarget + "'");
	//	BuildPipeline.BuildAssetBundles(@"Assets/AssetBundles/" + EditorUserBuildSettings.activeBuildTarget, BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
	//}
	[MenuItem("Build/Build Asset Bundles (Android)")]
	static void BuildAllAssetBundles_Android()
	{
		Debug.Log("building bundles for '" + BuildTarget.Android + "'");
		MkDir(BuildTarget.Android);
		BuildPipeline.BuildAssetBundles("Assets/AssetBundles/" + BuildTarget.Android, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
		Debug.Log("done building '" + BuildTarget.Android + "'");
	}

	[MenuItem("Build/Build Asset Bundles (WebGL)")]
	static void BuildAllAssetBundles_WebGL()
	{
		Debug.Log("building bundles for '" + BuildTarget.WebGL + "'");
		MkDir(BuildTarget.WebGL);
		BuildPipeline.BuildAssetBundles(@"Assets/AssetBundles/" + BuildTarget.WebGL, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.WebGL);
		Debug.Log("done building '" + BuildTarget.WebGL + "'");
	}

	private static void MkDir(BuildTarget target)
	{
		if (!Directory.Exists("Assets/AssetBundles")) Directory.CreateDirectory("Assets/AssetBundles");
		if (!Directory.Exists("Assets/AssetBundles/" + target)) Directory.CreateDirectory("Assets/AssetBundles/" + target);
	}
}

public class BuildPlayer
{
	[MenuItem("Build/Build WebGL")]
	private static void Build()
	{
		BuildPipeline.BuildPlayer(new BuildPlayerOptions
		{
			scenes = GetSceneNames(),
			locationPathName = @"C:\temp\Unity\out\temp\",
			assetBundleManifestPath = @"C:\temp\Unity\out\temp\AssetBundles.manifest",
			target = BuildTarget.WebGL,
			options = BuildOptions.None
		});
	}

	[MenuItem("Build/Build Android")]
	public static void MyBuild()
	{
		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
		{
			scenes = GetSceneNames(), //new[] { "Assets/Scene1.unity", "Assets/Scene2.unity" };
			locationPathName = "AndroidBuild",
			target = BuildTarget.Android,
			options = BuildOptions.None
		};

		BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded)
		{
			Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
		}

		if (summary.result == BuildResult.Failed)
		{
			Debug.Log("Build failed");
		}
	}

	private static string[] GetSceneNames()
	{
		int sceneCount = EditorBuildSettings.scenes.Length;
		string[] sceneNames = new string[sceneCount];
		for (int i = 0; i < sceneCount; ++i)
		{
			sceneNames[i] = EditorBuildSettings.scenes[i].path;
		}

		return sceneNames;
	}
}
