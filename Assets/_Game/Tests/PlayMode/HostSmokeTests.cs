using System.Collections;
using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PanicMechanics.Tests.PlayMode
{
    public sealed class HostSmokeTests
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
        public IEnumerator HostStartsMissionAndAdvances()
        {
            float timeout = Time.realtimeSinceStartup + 10f;
            while (GameBootstrap.Instance == null && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "GameBootstrap present after scene load");

            var nm = NetworkManager.Singleton;
            Assert.That(nm, Is.Not.Null, "NetworkManager present in scene");
            Assert.That(nm.StartHost(), Is.True, "host started");

            timeout = Time.realtimeSinceStartup + 10f;
            while ((NetworkGameManager.Instance == null || !NetworkGameManager.Instance.IsSpawned)
                   && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            var manager = NetworkGameManager.Instance;
            Assert.That(manager, Is.Not.Null, "NetworkGameManager present");
            Assert.That(manager.IsSpawned, Is.True, "NetworkGameManager spawned on host");

            timeout = Time.realtimeSinceStartup + 5f;
            while (manager.Phase.Value != GamePhase.Running && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(manager.Phase.Value, Is.EqualTo(GamePhase.Running), "mission reaches Running");

            float before = manager.Elapsed.Value;
            yield return new WaitForSeconds(1f);
            Assert.That(manager.Elapsed.Value, Is.GreaterThan(before), "mission clock advances");

            Assert.That(manager.Subsystems.Count, Is.EqualTo(NetworkGameManager.SubsystemCount),
                "subsystems replicated to clients");

            var avatar = nm.LocalClient != null && nm.LocalClient.PlayerObject != null
                ? nm.LocalClient.PlayerObject.GetComponent<PlayerAvatar>()
                : null;
            Assert.That(avatar, Is.Not.Null, "host player avatar spawned");

            timeout = Time.realtimeSinceStartup + 20f;
            while (avatar.RoleValue != (byte)PlayerRole.Engineer &&
                   Time.realtimeSinceStartup < timeout)
            {
                yield return null;
            }

            Assert.That(avatar.RoleValue, Is.EqualTo((byte)PlayerRole.Engineer),
                "host avatar assigned Engineer (first in join order)");
        }
    }
}
