using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Missions;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class MissionBuilderTests
    {
        [Test]
        public void BuildsDirectorWithConfiguredIncidents()
        {
            var config = new MissionConfig
            {
                Id = "m1",
                Players = 3,
                TargetDurationSeconds = 120f,
                Incidents =
                {
                    IncidentConfig.Overheat(),
                    IncidentConfig.PowerFault("powerfault", 10f),
                    IncidentConfig.BrakeLeak("brakeleak", 20f)
                }
            };

            var director = MissionBuilder.Build(config);

            Assert.That(director.MissionId, Is.EqualTo("m1"));
            Assert.That(director.Players, Is.EqualTo(3));
            Assert.That(director.TargetDuration, Is.EqualTo(120f));
            Assert.That(director.Incidents.Count, Is.EqualTo(3));
        }

        [Test]
        public void DifficultyScalesHeatGain()
        {
            var baseConfig = new MissionConfig { Players = 2, Incidents = { IncidentConfig.Overheat() } };
            var hardConfig = new MissionConfig { Players = 4, Incidents = { IncidentConfig.Overheat() } };

            var easy = MissionBuilder.Build(baseConfig);
            var hard = MissionBuilder.Build(hardConfig);

            Assert.That(hard.Model.HeatGainPerSecond, Is.GreaterThan(easy.Model.HeatGainPerSecond));
        }

        [Test]
        public void NullConfigThrows()
        {
            Assert.Throws<System.ArgumentNullException>(() => MissionBuilder.Build(null));
        }

        [Test]
        public void BuiltMissionCanRunToCompletion()
        {
            var config = new MissionConfig { Players = 2, TargetDurationSeconds = 2f };
            config.Incidents.Add(IncidentConfig.Overheat());
            var director = MissionBuilder.Build(config);

            director.EnterLobby();
            director.StartMission();
            for (int i = 0; i < 40 && director.Phase == GamePhase.Running; i++)
            {
                director.Tick(0.1f);
            }

            Assert.That(director.Result, Is.Not.Null);
            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Success));
        }
    }
}
