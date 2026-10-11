using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PanicMechanics.EditorTools
{
    public static class ScenePreviewCapture
    {
        private const int Width = 1280;
        private const int Height = 720;

        public static void CaptureBootstrap()
        {
            string scenePath = "Assets/_Game/Scenes/00_Bootstrap.unity";
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            string outDir = Environment.GetEnvironmentVariable("PM_PREVIEW_DIR");
            if (string.IsNullOrEmpty(outDir))
            {
                outDir = "/tmp/pm-preview";
            }

            Directory.CreateDirectory(outDir);

            var shots = new[]
            {
                new object[] { "overview", new Vector3(0f, 9f, -7f), new Vector3(0f, 0.5f, 6f) },
                new object[] { "stations", new Vector3(0f, 5f, 1.5f), new Vector3(0f, 0.5f, 7f) },
                new object[] { "player", new Vector3(0f, 2.5f, -3f), new Vector3(0f, 1f, 6f) },
            };

            var go = new GameObject("PreviewCamera");
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 60f;
            cam.clearFlags = CameraClearFlags.Skybox;

            var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            cam.targetTexture = rt;

            var tex = new Texture2D(Width, Height, TextureFormat.RGB24, false);

            foreach (var shot in shots)
            {
                string name = (string)shot[0];
                cam.transform.position = (Vector3)shot[1];

                var look = new GameObject("look").transform;
                look.position = (Vector3)shot[2];
                cam.transform.LookAt(look);
                UnityEngine.Object.DestroyImmediate(look.gameObject);

                cam.Render();
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                tex.Apply();
                RenderTexture.active = null;

                string path = Path.Combine(outDir, name + ".png");
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Debug.Log("[ScenePreviewCapture] wrote " + path);
            }

            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(rt);
            EditorApplication.Exit(0);
        }
    }
}
