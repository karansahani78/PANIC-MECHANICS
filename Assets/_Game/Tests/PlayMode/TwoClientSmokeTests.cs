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
    public static class TwoClientMarkers
    {
        public static string GetMarkerPath()
        {
            string env = System.Environment.GetEnvironmentVariable("PM_MARKER");
            return string.IsNullOrEmpty(env) ? "/tmp/pm-host-ready" : env;
        }

        public static void RequireTwoClientEnvironment()
        {
            string env = System.Environment.GetEnvironmentVariable("PM_TWOCLIENT");
            if (env != "1")
            {
                Assert.Ignore("Two-client tests run via Tools/two-client-smoke.sh (set PM_TWOCLIENT=1).");
            }
        }
    }

    public sealed class HostProcessSmokeTests
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
        public IEnumerator HostServesClientJoinAndTicks()
        {
            TwoClientMarkers.RequireTwoClientEnvironment();

            float deadline = Time.realtimeSinceStartup + 10f;
            while (GameBootstrap.Instance == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "GameBootstrap present");

            var nm = NetworkManager.Singleton;
            Assert.That(nm, Is.Not.Null, "NetworkManager present");
            Assert.That(nm.StartHost(), Is.True, "host started");

            deadline = Time.realtimeSinceStartup + 10f;
            while ((NetworkGameManager.Instance == null || !NetworkGameManager.Instance.IsSpawned)
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(NetworkGameManager.Instance, Is.Not.Null, "NetworkGameManager present on host");
            Assert.That(NetworkGameManager.Instance.IsSpawned, Is.True, "NetworkGameManager spawned on host");

            File.WriteAllText(TwoClientMarkers.GetMarkerPath(), "ready\n");
            Debug.Log("[PM] HOST_READY");

            bool sawClient = false;
            int? joinedClientRole = null;
            deadline = Time.realtimeSinceStartup + 240f;
            while (Time.realtimeSinceStartup < deadline)
            {
                if (nm.ConnectedClients.Count >= 2)
                {
                    sawClient = true;
                    if (joinedClientRole == null)
                    {
                        foreach (var pair in nm.ConnectedClients)
                        {
                            if (pair.Key == nm.LocalClientId)
                            {
                                continue;
                            }

                            var avatar = pair.Value.PlayerObject != null
                                ? pair.Value.PlayerObject.GetComponent<PlayerAvatar>()
                                : null;
                            if (avatar != null)
                            {
                                joinedClientRole = avatar.RoleValue;
                            }
                        }
                    }
                }

                if (sawClient && nm.ConnectedClients.Count < 2)
                {
                    break;
                }

                yield return null;
            }

            Assert.That(sawClient, Is.True, "a client joined the host");
            Assert.That(joinedClientRole, Is.EqualTo((byte)PlayerRole.Electrician),
                "host assigned Electrician to the joining client (second in join order)");

            var manager = NetworkGameManager.Instance;
            Assert.That(manager.Phase.Value, Is.EqualTo(GamePhase.Running), "mission Running on host");

            float before = manager.Elapsed.Value;
            yield return new WaitForSeconds(1f);
            Assert.That(manager.Elapsed.Value, Is.GreaterThan(before), "mission clock advances on host");

            Assert.That(manager.Subsystems.Count, Is.EqualTo(NetworkGameManager.SubsystemCount),
                "subsystem states replicated on host");

            var hostAvatar = nm.LocalClient != null && nm.LocalClient.PlayerObject != null
                ? nm.LocalClient.PlayerObject.GetComponent<PlayerAvatar>()
                : null;
            Assert.That(hostAvatar, Is.Not.Null, "host player avatar spawned");

            deadline = Time.realtimeSinceStartup + 20f;
            while (hostAvatar.RoleValue != (byte)PlayerRole.Engineer &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(hostAvatar.RoleValue, Is.EqualTo((byte)PlayerRole.Engineer),
                "host avatar assigned Engineer (first in join order)");
        }
    }

    public sealed class ClientProcessSmokeTests
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
        [Timeout(120000)]
        public IEnumerator ClientJoinsAndSeesReplicatedState()
        {
            TwoClientMarkers.RequireTwoClientEnvironment();

            float deadline = Time.realtimeSinceStartup + 10f;
            while (GameBootstrap.Instance == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "GameBootstrap present");

            var session = GameBootstrap.Instance.Session;
            Assert.That(session, Is.Not.Null, "Session service present");

            bool resolved = false;
            SessionOperationResult result = default;
            session.JoinByCodeAsync(LocalGameSessionService.LoopbackCode).ContinueWith(task =>
            {
                result = task.Result;
                resolved = true;
            });
            deadline = Time.realtimeSinceStartup + 30f;
            while (!resolved && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(resolved, Is.True, "join call resolved");
            Assert.That(result.Success, Is.True, "client started: " + result.Message);

            var nm = NetworkManager.Singleton;
            deadline = Time.realtimeSinceStartup + 30f;
            while ((!nm.IsConnectedClient || NetworkGameManager.Instance == null
                    || !NetworkGameManager.Instance.IsSpawned || !nm.IsListening)
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(nm.IsConnectedClient, Is.True, "client connected to host");

            var manager = NetworkGameManager.Instance;
            Assert.That(manager.IsSpawned, Is.True, "NetworkGameManager spawned on client");

            deadline = Time.realtimeSinceStartup + 20f;
            while (manager.Phase.Value != GamePhase.Running && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(manager.Phase.Value, Is.EqualTo(GamePhase.Running),
                "client sees mission Running");

            float before = manager.Elapsed.Value;
            yield return new WaitForSeconds(1f);
            Assert.That(manager.Elapsed.Value, Is.GreaterThan(before),
                "client sees mission clock advance");

            Assert.That(manager.Subsystems.Count, Is.EqualTo(NetworkGameManager.SubsystemCount),
                "client receives replicated subsystem states");

            var player = nm.LocalClient != null ? nm.LocalClient.PlayerObject : null;
            Assert.That(player, Is.Not.Null, "client player avatar spawned");
            var avatar = player.GetComponent<PlayerAvatar>();

            deadline = Time.realtimeSinceStartup + 20f;
            while (avatar.RoleValue != (byte)PlayerRole.Electrician &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(avatar.RoleValue, Is.EqualTo((byte)PlayerRole.Electrician),
                "client assigned Electrician by host (second in join order)");

            InteractionResultCode roundTrip = InteractionResultCode.WrongPhase;
            bool gotRejection = false;
            manager.InteractionResolved += (code, key) =>
            {
                roundTrip = code;
                gotRejection = true;
            };

            manager.SubmitInteractionServerRpc("does_not_exist", "no_such_action", 0.1f);
            deadline = Time.realtimeSinceStartup + 10f;
            while (!gotRejection && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(gotRejection, Is.True, "host-validated rejection returned to client");
            Assert.That(roundTrip, Is.EqualTo(InteractionResultCode.InvalidTarget),
                "client received InvalidTarget for unknown target");

            yield return new WaitForSeconds(3f);
        }
    }
}