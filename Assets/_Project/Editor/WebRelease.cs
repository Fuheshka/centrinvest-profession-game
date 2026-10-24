using UnityEditor;
using UnityEditor.Build.Profile;
using UnityEngine;

// Release Web build from the "Web - Desktop - Release" profile (Brotli + Decompression Fallback) into Builds/Web.
public static class WebRelease
{
    const string ProfilePath = "Assets/Settings/Build Profiles/Web - Desktop - Release.asset";

    [MenuItem("Tools/Build Web Release")]
    public static void Build()
    {
        var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(ProfilePath);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerWithProfileOptions
        {
            buildProfile = profile,
            locationPathName = "Builds/Web",
            options = BuildOptions.None,
        });
        Debug.Log($"Web release: {report.summary.result}, {report.summary.totalSize / 1048576f:F1} MB, {report.summary.totalTime}");
    }
}
