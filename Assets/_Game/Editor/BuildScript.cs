using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PanicMechanics.EditorTools
{
    public static class BuildScript
    {
        public static void BuildMacDevelopment()
        {
            string output = Environment.GetEnvironmentVariable("PM_BUILD_PATH");
            if (string.IsNullOrEmpty(output))
            {
                output = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "macOS", "PanicMechanics.app");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(output));

            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray();

            Debug.Log("[BuildScript] scenes: " + string.Join(", ", scenes));
            Debug.Log("[BuildScript] output: " + output);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[BuildScript] BUILD SUCCEEDED path=" + summary.outputPath +
                          " bytes=" + summary.totalSize +
                          " seconds=" + summary.totalTime.TotalSeconds.ToString("0.0"));
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[BuildScript] BUILD FAILED result=" + summary.result +
                               " errors=" + summary.totalErrors);
                EditorApplication.Exit(1);
            }
        }
    }
}
