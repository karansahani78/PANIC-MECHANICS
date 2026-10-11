using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;
using PanicMechanics.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PanicMechanics.Tests.PlayMode
{
    public static class GameplayMarkers
    {
        public static void RequireGameplayEnvironment()
        {
            string env = System.Environment.GetEnvironmentVariable("PM_GAMEPLAY");
            if (env != "1")
            {
                Assert.Ignore("Full gameplay-loop test; run with PM_GAMEPLAY=1 (accelerated via Time.timeScale).");
            }
        }
    }

    public sealed class GameplayLoopTests
    {
        private const float TimeScale = 10f;
        private const float WallStep = 45f;

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
            Time.timeScale = 1f;

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

        private static IncidentController Find(MissionDirector director, string id)
        {
            for (int i = 0; i < director.Incidents.Count; i++)
            {
                if (director.Incidents[i].Id == id)
                {
                    return director.Incidents[i];
                }
            }

            return null;
        }

        private static IEnumerator WaitForHostAndRunning(MissionDirector[] directorOut)
        {
            float deadline = Time.realtimeSinceStartup + 20f;
            while (GameBootstrap.Instance == null && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance, Is.Not.Null, "GameBootstrap present");

            var nm = NetworkManager.Singleton;
            Assert.That(nm, Is.Not.Null, "NetworkManager present");
            Assert.That(nm.StartHost(), Is.True, "host started");

            deadline = Time.realtimeSinceStartup + 20f;
            while ((NetworkGameManager.Instance == null || !NetworkGameManager.Instance.IsSpawned)
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(NetworkGameManager.Instance, Is.Not.Null, "NetworkGameManager present");

            deadline = Time.realtimeSinceStartup + 20f;
            while (GameBootstrap.Instance.Director.Phase != GamePhase.Running
                   && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(GameBootstrap.Instance.Director.Phase, Is.EqualTo(GamePhase.Running), "mission Running");
            directorOut[0] = GameBootstrap.Instance.Director;
        }

        private static IEnumerator WaitForActive(MissionDirector director, string id)
        {
            float deadline = Time.realtimeSinceStartup + WallStep;
            IncidentController incident = null;
            while (Time.realtimeSinceStartup < deadline)
            {
                incident = Find(director, id);
                if (incident != null && incident.Phase != IncidentPhase.Dormant)
                {
                    yield break;
                }

                yield return null;
            }

            Assert.Fail("incident '" + id + "' never activated (elapsed " + director.Elapsed + ")");
        }

        private static IEnumerator WaitForResolved(MissionDirector director, string id)
        {
            float deadline = Time.realtimeSinceStartup + WallStep;
            while (Time.realtimeSinceStartup < deadline)
            {
                var incident = Find(director, id);
                if (incident != null && incident.Phase == IncidentPhase.Resolved)
                {
                    yield break;
                }

                if (director.Phase == GamePhase.Debrief)
                {
                    Assert.Fail("mission ended before '" + id + "' resolved");
                }

                yield return null;
            }

            Assert.Fail("incident '" + id + "' never resolved");
        }

        [UnityTest]
        [Timeout(60000)]
        public IEnumerator SceneExposesStationForEveryRequiredIncidentAction()
        {
            yield return null;

            var director = GameBootstrap.Instance.Director;
            var stations = Object.FindObjectsByType<InteractableStation>(FindObjectsSortMode.None);

            var required = new Dictionary<string, string[]>
            {
                { "overheat", new[] { "open_cooling_valve" } },
                { "powerfault", new[] { "reduce_engine_load", "reset_breaker" } },
                { "brakeleak", new[] { "close_brake_valve" } },
            };

            foreach (var pair in required)
            {
                var incident = Find(director, pair.Key);
                Assert.That(incident, Is.Not.Null, "mission defines incident " + pair.Key);
                foreach (string action in pair.Value)
                {
                    Assert.That(
                        stations.Any(s => s.TargetId == pair.Key && s.ActionId == action),
                        Is.True,
                        "scene has a station for " + pair.Key + "/" + action);
                }
            }

            foreach (var station in stations)
            {
                var incident = Find(director, station.TargetId);
                Assert.That(incident, Is.Not.Null, "station target incident exists: " + station.TargetId);
                Assert.That(incident.AvailableActions, Does.Contain(station.ActionId),
                    "station action is valid for incident: " + station.TargetId + "/" + station.ActionId);
            }
        }

        [UnityTest]
        [Timeout(240000)]
        public IEnumerator FullMissionCanBeCompletedByResolvingAllIncidents()
        {
            GameplayMarkers.RequireGameplayEnvironment();
            Time.timeScale = TimeScale;

            var directorOut = new MissionDirector[1];
            yield return WaitForHostAndRunning(directorOut);
            var director = directorOut[0];
            var manager = NetworkGameManager.Instance;

            yield return WaitForActive(director, "overheat");
            manager.SubmitInteractionServerRpc("overheat", "open_cooling_valve", 1f);
            yield return WaitForResolved(director, "overheat");

            yield return WaitForActive(director, "powerfault");
            manager.SubmitInteractionServerRpc("powerfault", "reduce_engine_load", 1f);
            manager.SubmitInteractionServerRpc("powerfault", "reset_breaker", 1f);
            yield return WaitForResolved(director, "powerfault");

            yield return WaitForActive(director, "brakeleak");
            manager.SubmitInteractionServerRpc("brakeleak", "close_brake_valve", 1f);
            yield return WaitForResolved(director, "brakeleak");

            float deadline = Time.realtimeSinceStartup + WallStep;
            while (director.Phase != GamePhase.Debrief && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(director.Phase, Is.EqualTo(GamePhase.Debrief), "mission reaches Debrief");
            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Success), "all incidents resolved => Success");
            Assert.That(director.Result.FailedIncidents, Is.Empty, "no failed incidents");

            yield return new WaitForSeconds(0.5f);
            Assert.That(manager.Outcome.Value, Is.EqualTo((byte)MissionOutcome.Success),
                "outcome replicated to clients");
        }

        [UnityTest]
        [Timeout(120000)]
        public IEnumerator NeglectedMissionFailsWithReplicatedOutcome()
        {
            GameplayMarkers.RequireGameplayEnvironment();
            Time.timeScale = TimeScale;

            var directorOut = new MissionDirector[1];
            yield return WaitForHostAndRunning(directorOut);
            var director = directorOut[0];
            var manager = NetworkGameManager.Instance;

            float deadline = Time.realtimeSinceStartup + WallStep;
            while (director.Phase != GamePhase.Debrief && Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            Assert.That(director.Phase, Is.EqualTo(GamePhase.Debrief), "mission reaches Debrief");
            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Failure), "neglect => Failure");
            Assert.That(director.Result.FailedIncidents, Does.Contain("overheat"), "overheat failed");

            yield return new WaitForSeconds(0.5f);
            Assert.That(manager.Outcome.Value, Is.EqualTo((byte)MissionOutcome.Failure),
                "failure replicated to clients");
        }
    }
}
