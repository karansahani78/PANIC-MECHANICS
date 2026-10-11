using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Missions;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class MissionIntegrationTests
    {
        private static IncidentController FindIncident(MissionDirector director, string id)
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

        private static PlayerInteractionContext Actor(ulong id, PlayerRole role)
        {
            return new PlayerInteractionContext(id, role, 1f, 0f);
        }

        private static InteractionRequest Req(string incidentId, string action)
        {
            return new InteractionRequest(incidentId, action, 1u, 0u);
        }

        private static void Advance(MissionDirector director, float seconds)
        {
            int steps = (int)(seconds / 0.1f) + 1;
            for (int i = 0; i < steps && director.Phase == GamePhase.Running; i++)
            {
                director.Tick(0.1f);
            }
        }

        [Test]
        public void ThreeIncidentMissionSucceedsWhenCrewCooperates()
        {
            var config = new MissionConfig { Id = "coop", Players = 3, TargetDurationSeconds = 12f };
            config.Incidents.Add(IncidentConfig.Overheat("overheat", 0f));
            config.Incidents.Add(IncidentConfig.PowerFault("powerfault", 2f));
            config.Incidents.Add(IncidentConfig.BrakeLeak("brakeleak", 4f));
            var director = MissionBuilder.Build(config);
            director.EnterLobby();
            director.StartMission();

            Advance(director, 0.2f);
            var overheat = FindIncident(director, "overheat");
            overheat.TryRepair(Req("overheat", OverheatIncident.ActionOpenCoolingValve),
                Actor(1UL, PlayerRole.Engineer));

            Advance(director, 2.1f);
            var power = FindIncident(director, "powerfault");
            power.TryRepair(Req("powerfault", PowerFaultIncident.ActionReduceEngineLoad),
                Actor(2UL, PlayerRole.Electrician));
            power.TryRepair(Req("powerfault", PowerFaultIncident.ActionResetBreaker),
                Actor(2UL, PlayerRole.Electrician));

            Advance(director, 4.1f);
            var brake = FindIncident(director, "brakeleak");
            brake.TryRepair(Req("brakeleak", BrakeLeakIncident.ActionCloseBrakeValve),
                Actor(3UL, PlayerRole.Driver));
            brake.TryRepair(Req("brakeleak", BrakeLeakIncident.ActionRechargeBrakes),
                Actor(3UL, PlayerRole.Driver));

            Advance(director, 10f);

            Assert.That(director.Phase, Is.EqualTo(GamePhase.Debrief));
            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Success));
            Assert.That(director.Result.FailedIncidents, Is.Empty);
            Assert.That(director.Result.ResolvedIncidents, Does.Contain("overheat"));
            Assert.That(director.Result.ResolvedIncidents, Does.Contain("powerfault"));
            Assert.That(director.Result.ResolvedIncidents, Does.Contain("brakeleak"));
        }

        [Test]
        public void NeglectedCrewFailsTheMission()
        {
            var config = new MissionConfig { Id = "neglect", Players = 2, TargetDurationSeconds = 120f };
            config.Incidents.Add(IncidentConfig.Overheat("overheat", 0f));
            var director = MissionBuilder.Build(config);
            director.EnterLobby();
            director.StartMission();

            Advance(director, 60f);

            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Failure));
            Assert.That(director.Result.FailedIncidents, Does.Contain("overheat"));
            Assert.That(director.Result.Summary, Is.Not.Empty);
        }
    }
}
