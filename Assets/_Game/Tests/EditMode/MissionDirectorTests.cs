using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    [TestFixture]
    public class MissionDirectorTests
    {
        [Test]
        public void PhaseFlowsFromLobbyToRunningToDebrief()
        {
            var director = new MissionDirector("test", 4, 5f);
            director.EnterLobby();
            Assert.That(director.Phase, Is.EqualTo(GamePhase.Lobby));

            director.StartMission();
            Assert.That(director.Phase, Is.EqualTo(GamePhase.Running));

            for (int i = 0; i < 60; i++)
            {
                director.Tick(0.1f);
            }

            Assert.That(director.Phase, Is.EqualTo(GamePhase.Debrief));
            Assert.That(director.Result, Is.Not.Null);
            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Success));
        }

        [Test]
        public void MissionFailsWhenIncidentFails()
        {
            var director = new MissionDirector("test", 4, 300f);
            var overheat = new OverheatIncident(director.Model, director.Log);
            director.AddIncident(overheat);
            director.EnterLobby();
            director.StartMission();

            for (int i = 0; i < 600; i++)
            {
                director.Tick(0.1f);
                if (director.Phase != GamePhase.Running)
                {
                    break;
                }
            }

            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Failure));
            Assert.That(director.Result.FailedIncidents, Does.Contain("overheat"));
        }

        [Test]
        public void ResetAllowsCleanReplay()
        {
            var director = new MissionDirector("test", 4, 5f);
            var overheat = new OverheatIncident(director.Model, director.Log);
            director.AddIncident(overheat);
            director.EnterLobby();
            director.StartMission();

            for (int i = 0; i < 60; i++)
            {
                director.Tick(0.1f);
            }

            director.Reset();
            Assert.That(director.Phase, Is.EqualTo(GamePhase.Boot));
            Assert.That(director.Elapsed, Is.EqualTo(0f));
            Assert.That(director.Result, Is.Null);
            Assert.That(overheat.Phase, Is.EqualTo(IncidentPhase.Dormant));
            Assert.That(director.Log.Count, Is.EqualTo(0));
            Assert.That(director.Model.Get(TrainSubsystem.Engine).Temperature, Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void PhaseChangedEventFires()
        {
            var director = new MissionDirector("test", 2, 5f);
            GamePhase observed = GamePhase.Boot;
            director.PhaseChanged += phase => observed = phase;
            director.EnterLobby();
            Assert.That(observed, Is.EqualTo(GamePhase.Lobby));
        }
    }

    [TestFixture]
    public class DifficultyScalerTests
    {
        [Test]
        public void DifficultyIsMonotonicAcrossPlayerCounts()
        {
            var two = DifficultyScaler.ForPlayers(2);
            var three = DifficultyScaler.ForPlayers(3);
            var four = DifficultyScaler.ForPlayers(4);

            Assert.That(three.HeatGainMultiplier, Is.GreaterThan(two.HeatGainMultiplier));
            Assert.That(four.HeatGainMultiplier, Is.GreaterThan(three.HeatGainMultiplier));
            Assert.That(three.TimePressureMultiplier, Is.GreaterThan(two.TimePressureMultiplier));
            Assert.That(four.TimePressureMultiplier, Is.GreaterThan(three.TimePressureMultiplier));
            Assert.That(four.TargetDurationSeconds, Is.GreaterThan(two.TargetDurationSeconds));
        }

        [Test]
        public void PlayerCountIsClamped()
        {
            Assert.That(DifficultyScaler.ForPlayers(0).Players, Is.EqualTo(2));
            Assert.That(DifficultyScaler.ForPlayers(9).Players, Is.EqualTo(4));
        }
    }

    [TestFixture]
    public class ScoringAndReportTests
    {
        [Test]
        public void ResolvingIncidentAddsScoreAndReportCause()
        {
            var director = new MissionDirector("m", 4, 5f);
            var overheat = new OverheatIncident(director.Model, director.Log);
            director.AddIncident(overheat);
            director.EnterLobby();
            director.StartMission();

            director.Tick(0.1f);
            overheat.TryRepair(
                new InteractionRequest("overheat", OverheatIncident.ActionOpenCoolingValve, 1UL, 1u),
                new PlayerInteractionContext(1UL, PlayerRole.Engineer, 1f, 0f));

            for (int i = 0; i < 600 && director.Phase == GamePhase.Running; i++)
            {
                director.Tick(0.1f);
            }

            Assert.That(director.Result.Outcome, Is.EqualTo(MissionOutcome.Success));
            Assert.That(director.Result.ResolvedIncidents, Does.Contain("overheat"));
            Assert.That(director.Result.Score.IncidentsResolved, Is.EqualTo(1));
            Assert.That(director.Result.Score.Total, Is.GreaterThan(0));
            Assert.That(director.Result.Summary, Is.Not.Empty);
            Assert.That(string.Join("\n", director.Result.Summary), Does.Contain("Top cause"));
        }

        [Test]
        public void ReportCountsRepeatedMistakes()
        {
            var director = new MissionDirector("m", 4, 1f);
            var overheat = new OverheatIncident(director.Model, director.Log);
            director.AddIncident(overheat);
            director.EnterLobby();
            director.StartMission();
            director.Tick(0.1f);

            var badRequest = new InteractionRequest("overheat", "invalid_action", 1UL, 1u);
            var actor = new PlayerInteractionContext(1UL, PlayerRole.Engineer, 1f, 0f);
            for (int i = 0; i < 3; i++)
            {
                overheat.TryRepair(badRequest, actor);
            }

            for (int i = 0; i < 40 && director.Phase == GamePhase.Running; i++)
            {
                director.Tick(0.1f);
            }

            Assert.That(director.Result.Summary, Is.Not.Empty);
            Assert.That(string.Join("\n", director.Result.Summary), Does.Contain("Costly mistake"));
        }
    }
}
