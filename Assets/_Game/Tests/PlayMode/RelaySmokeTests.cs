using System.Collections;
using System.IO;
using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PanicMechanics.Tests.PlayMode
{
    public static class RelayMarkers
    {
        public static string GetCodePath()
        {
            string env = System.Environment.GetEnvironmentVariable("PM_RELAY_CODE");
            return string.IsNullOrEmpty(env) ? "/tmp/pm-relay-code" : env;
        }

        public static void RequireRelayEnvironment()
        {
            string enabled = System.Environment.GetEnvironmentVariable("PM_RELAY");
            if (enabled != "1")
            {
                Assert.Ignore("Relay tests require a linked Unity Cloud project; set PM_RELAY=1 to run.");
            }
        }
    }

    public sealed class RelayHostSmokeTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var load = SceneManager.LoadSceneAsync("00_Bootstrap", LoadSceneMode.Single);
            while (load != null && !load.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (GameBootstrap.Instance != null && GameBootstrap.Instance.Session != null)
            {
                var leaving = GameBootstrap.Instance.Session.LeaveSessionAsync();
                float deadline = Time.realtimeSinceStartup + 10f;
                while (!leaving.IsCompleted && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                }
            }

            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening)
            {
                nm.Shutdown();
            }

            yield return null;

            if (nm != null)
            {
                Object.Destroy(nm.gameObject);
            }

            if (GameBootstrap.Instance != null)
            {
                Object.Destroy(GameBootstrap.Instance.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator RelayHostPublishesCodeAndAcceptsClient()
        {
            RelayMarkers.RequireRelayEnvironment();

            float deadline = Time.realtimeSinceStartup + 10f;
            while (GameBootstrap.Instance == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "GameBootstrap present");

            var session = GameBootstrap.Instance.Session;
            Assert.That(session, Is.InstanceOf<MultiplayerServicesSessionService>(),
                "awaiting MultiplayerServices backend (set PM_SESSION_BACKEND=ugs)");

            bool resolved = false;
            SessionOperationResult result = default;
            session.CreatePrivateSessionAsync(4).ContinueWith(task =>
            {
                result = task.Result;
                resolved = true;
            });

            deadline = Time.realtimeSinceStartup + 60f;
            while (!resolved && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(resolved, Is.True, "host create call resolved");
            Assert.That(result.Success, Is.True, "host created relay session: " + result.Message);

            File.WriteAllText(RelayMarkers.GetCodePath(), result.JoinCode + "\n");
            Debug.Log("[PM] RELAY_HOST_READY " + result.JoinCode);

            var nm = NetworkManager.Singleton;
            bool sawClient = false;
            deadline = Time.realtimeSinceStartup + 240f;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (nm.ConnectedClients.Count >= 2)
                {
                    sawClient = true;
                }

                if (sawClient && nm.ConnectedClients.Count < 2)
                {
                    break;
                }

                yield return null;
            }

            Assert.That(sawClient, Is.True, "a client joined the relay host");
            Assert.That(NetworkGameManager.Instance.IsSpawned, Is.True, "NetworkGameManager spawned on relay host");
        }
    }

    public sealed class RelayClientSmokeTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            var load = SceneManager.LoadSceneAsync("00_Bootstrap", LoadSceneMode.Single);
            while (load != null && !load.isDone)
            {
                yield return null;
            }

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (GameBootstrap.Instance != null && GameBootstrap.Instance.Session != null)
            {
                var leaving = GameBootstrap.Instance.Session.LeaveSessionAsync();
                float deadline = Time.realtimeSinceStartup + 10f;
                while (!leaving.IsCompleted && Time.realtimeSinceStartup < deadline)
                {
                    yield return null;
                }
            }

            var nm = NetworkManager.Singleton;
            if (nm != null && nm.IsListening)
            {
                nm.Shutdown();
            }

            yield return null;

            if (nm != null)
            {
                Object.Destroy(nm.gameObject);
            }

            if (GameBootstrap.Instance != null)
            {
                Object.Destroy(GameBootstrap.Instance.gameObject);
            }

            yield return null;
        }

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator RelayClientJoinsByCode()
        {
            RelayMarkers.RequireRelayEnvironment();

            float deadline = Time.realtimeSinceStartup + 10f;
            while (GameBootstrap.Instance == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "GameBootstrap present");

            string codePath = RelayMarkers.GetCodePath();
            deadline = Time.realtimeSinceStartup + 120f;
            while (!File.Exists(codePath) && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(File.Exists(codePath), Is.True, "host published a relay join code");
            string code = File.ReadAllText(codePath).Trim();
            Assert.That(code, Is.Not.Empty, "relay join code is non-empty");

            var session = GameBootstrap.Instance.Session;
            Assert.That(session, Is.InstanceOf<MultiplayerServicesSessionService>(),
                "awaiting MultiplayerServices backend (set PM_SESSION_BACKEND=ugs)");

            bool resolved = false;
            SessionOperationResult result = default;
            session.JoinByCodeAsync(code).ContinueWith(task =>
            {
                result = task.Result;
                resolved = true;
            });

            deadline = Time.realtimeSinceStartup + 60f;
            while (!resolved && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(resolved, Is.True, "client join call resolved");
            Assert.That(result.Success, Is.True, "client joined relay session: " + result.Message);

            var nm = NetworkManager.Singleton;
            deadline = Time.realtimeSinceStartup + 60f;
            while ((!nm.IsConnectedClient || NetworkGameManager.Instance == null
                    || !NetworkGameManager.Instance.IsSpawned)
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(nm.IsConnectedClient, Is.True, "client connected over relay");
            Assert.That(NetworkGameManager.Instance.IsSpawned, Is.True, "NetworkGameManager spawned on relay client");

            var player = nm.LocalClient != null ? nm.LocalClient.PlayerObject : null;
            Assert.That(player, Is.Not.Null, "client player avatar spawned");
            var avatar = player.GetComponent<PlayerAvatar>();
            Assert.That(avatar, Is.Not.Null, "client avatar has PlayerAvatar");

            deadline = Time.realtimeSinceStartup + 30f;
            while (avatar.RoleValue == (byte)PlayerRole.Dispatcher && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Debug.Log("[PM] RELAY_CLIENT_ROLE " + avatar.RoleValue);
        }
    }
}
