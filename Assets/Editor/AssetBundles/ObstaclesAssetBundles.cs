using UnityEditor;
using UnityEngine;
using System.IO;

public class ObstaclesAssetBunldes
{
    [MenuItem("Assets/AssetBundles/Obstacles/Build Active Target")]
    static void BuildObstacleAssetsBundleActiveTarget()
    {
        AssetBundleBuild[] builds = new AssetBundleBuild[1] { GetAssetBundleBuildFromName("obstacles") };
        BuildAssetBundle(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "ObstaclesAssetBundle"), builds, EditorUserBuildSettings.activeBuildTarget);
    }

    [MenuItem("Assets/AssetBundles/Obstacles/Build WebGL")]
    static void BuildObstacleAssetsBundleWebGL()
    {
        AssetBundleBuild[] builds = new AssetBundleBuild[1] { GetAssetBundleBuildFromName("obstacles") };
        BuildAssetBundle(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "ObstaclesAssetBundle"), builds, BuildTarget.WebGL);
    }

    [MenuItem("Assets/AssetBundles/Obstacles/Build Android")]
    static void BuildObstacleAssetsBundleAndroid()
    {
        AssetBundleBuild[] builds = new AssetBundleBuild[1] { GetAssetBundleBuildFromName("obstacles") };
        BuildAssetBundle(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "ObstaclesAssetBundle"), builds, BuildTarget.Android);
    }

    [MenuItem("Assets/AssetBundles/Obstacles/Build IOS")]
    static void BuildObstacleAssetsBundleOIphone()
    {
        AssetBundleBuild[] builds = new AssetBundleBuild[1] { GetAssetBundleBuildFromName("obstacles") };
        BuildAssetBundle(Path.Combine(Application.streamingAssetsPath, "AssetBundles", "ObstaclesAssetBundle"), builds, BuildTarget.iOS);
    }

    static AssetBundleBuild GetAssetBundleBuildFromName(string name)
    {
        string[] assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(name);

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = name;
        build.assetNames = assetPaths;

        return build;
    }


    static void BuildAssetBundle(string directory, AssetBundleBuild[] builds, BuildTarget target, BuildAssetBundleOptions options = BuildAssetBundleOptions.None)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        BuildPipeline.BuildAssetBundles(directory, builds, options, target);
    }
}