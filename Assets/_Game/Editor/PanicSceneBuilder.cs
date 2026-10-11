using System.Collections.Generic;
using PanicMechanics.Core;
using PanicMechanics.Data;
using PanicMechanics.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace PanicMechanics.EditorTools
{
    public static class PanicSceneBuilder
    {
        private const string ContentDir = "Assets/_Game/Content";
        private const string IncidentsDir = ContentDir + "/Incidents";
        private const string MissionsDir = ContentDir + "/Missions";
        private const string PrefabsDir = "Assets/_Game/Prefabs";
        private const string ScenesDir = "Assets/_Game/Scenes";
        private const string PlayerPrefabPath = PrefabsDir + "/Player.prefab";
        private const string ScenePath = ScenesDir + "/00_Bootstrap.unity";

        [MenuItem("Panic Mechanics/Build Playable Slice")]
        public static void BuildAll()
        {
            EnsureFolders();
            EnsureTmpEssentials();

            var overheat = CreateIncident(
                "overheat", IncidentKind.Overheat, TrainSubsystem.Engine, 5f, 1f);
            var powerFault = CreateIncident(
                "powerfault", IncidentKind.PowerFault, TrainSubsystem.Electrical, 45f, 1f);
            var brakeLeak = CreateIncident(
                "brakeleak", IncidentKind.BrakeLeak, TrainSubsystem.Brakes, 90f, 1f);

            var mission = CreateMission(new[] { overheat, powerFault, brakeLeak });

            var playerPrefab = CreatePlayerPrefab();

            CreateScene(mission, playerPrefab);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[PanicSceneBuilder] Build complete: " + ScenePath);
        }

        [MenuItem("Panic Mechanics/Import TMP Essentials")]
        public static void ImportTmpEssentials()
        {
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0)
            {
                Debug.Log("[PanicSceneBuilder] TMP essentials already present.");
                EditorApplication.Exit(0);
                return;
            }

            string package = System.IO.Path.GetFullPath("Packages/com.unity.ugui") +
                "/Package Resources/TMP Essential Resources.unitypackage";
            if (!System.IO.File.Exists(package))
            {
                Debug.LogError("[PanicSceneBuilder] TMP package not found at " + package);
                EditorApplication.Exit(1);
                return;
            }

            double deadline = EditorApplication.timeSinceStartup + 180.0;
            AssetDatabase.importPackageCompleted += OnTmpImported;
            AssetDatabase.importPackageFailed += OnTmpFailed;
            EditorApplication.update += Watchdog;
            AssetDatabase.ImportPackage(package, false);

            void Watchdog()
            {
                if (EditorApplication.timeSinceStartup > deadline)
                {
                    Debug.LogError("[PanicSceneBuilder] TMP import timed out.");
                    EditorApplication.update -= Watchdog;
                    EditorApplication.Exit(1);
                }
            }
        }

        private static void OnTmpImported(string packageName)
        {
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            AssetDatabase.importPackageFailed -= OnTmpFailed;
            Debug.Log("[PanicSceneBuilder] Imported package: " + packageName);
            EditorApplication.Exit(0);
        }

        private static void OnTmpFailed(string packageName, string error)
        {
            AssetDatabase.importPackageCompleted -= OnTmpImported;
            AssetDatabase.importPackageFailed -= OnTmpFailed;
            Debug.LogError("[PanicSceneBuilder] TMP import failed: " + error);
            EditorApplication.Exit(1);
        }

        private static void EnsureFolders()
        {
            CreateFolder(ContentDir, "Content");
            CreateFolder(IncidentsDir, "Incidents");
            CreateFolder(MissionsDir, "Missions");
            CreateFolder(PrefabsDir, "Prefabs");
            CreateFolder(ScenesDir, "Scenes");
        }

        private static void CreateFolder(string path, string name)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            int slash = path.LastIndexOf('/');
            string parent = path.Substring(0, slash);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static void EnsureTmpEssentials()
        {
            if (AssetDatabase.FindAssets("t:TMP_Settings").Length > 0)
            {
                return;
            }

            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Import TMP Essential Resources");
            AssetDatabase.Refresh();
            int found = AssetDatabase.FindAssets("t:TMP_Settings").Length;
            Debug.Log("[PanicSceneBuilder] TMP Settings assets after import: " + found);
        }

        private static TMP_FontAsset GetDefaultFont()
        {
            var guids = AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset");
            if (guids.Length == 0)
            {
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static IncidentDefinitionSO CreateIncident(
            string id, IncidentKind kind, TrainSubsystem subsystem,
            float activateAfter, float severity)
        {
            string path = IncidentsDir + "/Incident_" + id + ".asset";
            var so = AssetDatabase.LoadAssetAtPath<IncidentDefinitionSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<IncidentDefinitionSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.Id = id;
            so.Kind = kind;
            so.Subsystem = subsystem;
            so.ActivateAfterSeconds = activateAfter;
            so.SeverityWeight = severity;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static MissionDefinitionSO CreateMission(IncidentDefinitionSO[] incidents)
        {
            string path = MissionsDir + "/Mission_Derail.asset";
            var so = AssetDatabase.LoadAssetAtPath<MissionDefinitionSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<MissionDefinitionSO>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.Id = "mission.derail";
            so.Title = "Runaway Freight";
            so.Description = "Keep the train intact until the next station.";
            so.TargetDurationSeconds = 180f;
            so.MinPlayers = 2;
            so.MaxPlayers = 4;
            so.DifficultyScale = 1f;
            so.Incidents = new List<IncidentDefinitionSO>(incidents);
            EditorUtility.SetDirty(so);
            return so;
        }

        private static GameObject CreatePlayerPrefab()
        {
            var root = new GameObject("Player");
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            var cameraGo = new GameObject("ViewCamera");
            cameraGo.transform.SetParent(root.transform, false);
            cameraGo.transform.localPosition = new Vector3(0f, 1.65f, 0f);
            var camera = cameraGo.AddComponent<Camera>();
            camera.nearClipPlane = 0.05f;

            root.AddComponent<NetworkObject>();
            var avatar = root.AddComponent<PlayerAvatar>();
            SetPrivate(avatar, "viewCamera", camera);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void CreateScene(MissionDefinitionSO mission, GameObject playerPrefab)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f);

            CreateStation("Station_Overheat", new Vector3(0f, 0.5f, 6f),
                "overheat", "open_cooling_valve", "Open coolant valve [E]");
            CreateStation("Station_PowerFault", new Vector3(4f, 0.5f, 6f),
                "powerfault", "reset_breaker", "Reset breaker [E]");
            CreateStation("Station_BrakeLeak", new Vector3(-4f, 0.5f, 6f),
                "brakeleak", "close_brake_valve", "Close brake valve [E]");

            var bootstrapGo = new GameObject("GameBootstrap");
            var bootstrap = bootstrapGo.AddComponent<GameBootstrap>();
            SetPrivate(bootstrap, "mission", mission);
            SetPrivate(bootstrap, "fallbackPlayerCount", 2);

            CreateNetworkManager(playerPrefab);
            CreateHud();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
        }

        private static void CreateStation(
            string name, Vector3 position, string targetId, string actionId, string prompt)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = new Vector3(1.2f, 1f, 1.2f);
            var station = go.AddComponent<InteractableStation>();
            SetPrivate(station, "targetId", targetId);
            SetPrivate(station, "actionId", actionId);
            SetPrivate(station, "prompt", prompt);
        }

        private static void CreateNetworkManager(GameObject playerPrefab)
        {
            var nmGo = new GameObject("NetworkManager");
            var manager = nmGo.AddComponent<NetworkManager>();
            var transport = nmGo.AddComponent<UnityTransport>();

            manager.NetworkConfig.NetworkTransport = transport;
            manager.NetworkConfig.PlayerPrefab = playerPrefab;
            manager.NetworkConfig.ConnectionApproval = false;
            manager.AddNetworkPrefab(playerPrefab);

            var gameManagerGo = new GameObject("NetworkGameManager");
            var gameManagerNet = gameManagerGo.AddComponent<NetworkObject>();
            gameManagerGo.AddComponent<NetworkGameManager>();
            SetPrivate(gameManagerNet, "m_InScenePlaced", true);
            EditorUtility.SetDirty(gameManagerNet);
        }

        private static void CreateHud()
        {
            var canvasGo = NewUI("HUD Canvas", null);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasGo.AddComponent<GraphicRaycaster>();

            var phase = CreateLabel(canvasGo.transform, "PhaseLabel",
                new Vector2(0f, 1f), new Vector2(20f, -20f), "Boot");
            var timer = CreateLabel(canvasGo.transform, "TimerLabel",
                new Vector2(0f, 1f), new Vector2(20f, -60f), "0.0 s");
            var subsystems = CreateLabel(canvasGo.transform, "SubsystemLabel",
                new Vector2(0f, 1f), new Vector2(20f, -100f), string.Empty);
            var feedback = CreateLabel(canvasGo.transform, "FeedbackLabel",
                new Vector2(0.5f, 0f), new Vector2(0f, 40f), string.Empty);
            feedback.alignment = TextAlignmentOptions.Center;

            var hud = canvasGo.AddComponent<HudController>();
            SetPrivate(hud, "phaseLabel", phase);
            SetPrivate(hud, "timerLabel", timer);
            SetPrivate(hud, "subsystemLabel", subsystems);
            SetPrivate(hud, "feedbackLabel", feedback);

            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();

            CreateMenu(canvasGo.transform);
        }

        private static void CreateMenu(Transform parent)
        {
            var menuGo = NewUI("Session Menu", parent);
            var menuRect = menuGo.GetComponent<RectTransform>();
            menuRect.anchorMin = new Vector2(1f, 1f);
            menuRect.anchorMax = new Vector2(1f, 1f);
            menuRect.pivot = new Vector2(1f, 1f);
            menuRect.anchoredPosition = new Vector2(-20f, -20f);
            menuRect.sizeDelta = new Vector2(200f, 120f);

            var host = CreateButton(menuGo.transform, "HostButton", "Host", new Vector2(0f, 0f));
            var join = CreateButton(menuGo.transform, "JoinButton", "Join", new Vector2(0f, -60f));

            var menu = menuGo.AddComponent<SessionMenuController>();
            SetPrivate(menu, "hostButton", host);
            SetPrivate(menu, "joinButton", join);
            SetPrivate(menu, "joinCode", LocalGameSessionService.LoopbackCode);
            SetPrivate(menu, "maxPlayers", 4);
        }

        private static GameObject NewUI(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent, string name, Vector2 anchor, Vector2 offset, string text)
        {
            var go = NewUI(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(600f, 40f);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 26f;
            var font = GetDefaultFont();
            if (font != null)
            {
                label.font = font;
            }

            return label;
        }

        private static Button CreateButton(Transform parent, string name, string text, Vector2 offset)
        {
            var go = NewUI(name, parent);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(0f, 48f);

            var image = go.AddComponent<Image>();
            image.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var label = CreateLabel(go.transform, "Label",
                new Vector2(0.5f, 0.5f), Vector2.zero, text);
            var labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = Vector2.zero;
            labelRect.sizeDelta = Vector2.zero;
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 24f;

            return button;
        }

        private static void AddSceneToBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(ScenePath, true) };
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SetPrivate(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError("[PanicSceneBuilder] Missing field '" + field + "' on " + target.GetType().Name);
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError("[PanicSceneBuilder] Missing field '" + field + "' on " + target.GetType().Name);
                return;
            }

            prop.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError("[PanicSceneBuilder] Missing field '" + field + "' on " + target.GetType().Name);
                return;
            }

            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetPrivate(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError("[PanicSceneBuilder] Missing field '" + field + "' on " + target.GetType().Name);
                return;
            }

            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
