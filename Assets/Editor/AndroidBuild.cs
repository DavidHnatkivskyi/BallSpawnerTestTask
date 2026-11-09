using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace BallGame.Editor
{
    internal static class AndroidBuild
    {
        private const string OutputPath = "Builds/Android/BallPath.apk";

        [MenuItem("Ball Game/Build Android APK")]
        public static void BuildApk()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/SampleScene.unity" },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException($"Android build failed: {report.summary.result}");
            }
        }
    }
}
