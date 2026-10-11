using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    [TestFixture]
    public class OverheatIncidentTests
    {
        private MissionDirector _director;
        private OverheatIncident _overheat;

        private static PlayerInteractionContext Actor(float distance)
        {
            return new PlayerInteractionContext(1UL, PlayerRole.Engineer, distance, 0f);
        }

        private static InteractionRequest Request(string actionId)
        {
            return new InteractionRequest("overheat", actionId, 1UL, 1u);
        }

        [SetUp]
        public void SetUp()
        {
            _director = new MissionDirector("test", 4, 300f);
            _overheat = new OverheatIncident(_director.Model, _director.Log);
            _director.AddIncident(_overheat);
            _director.EnterLobby();
            _director.StartMission();
        }

        [Test]
        public void StaysDormantUntilActivated()
        {
            _overheat.ActivateAfter = 10f;
            _director.Tick(0.1f);
            Assert.That(_overheat.Phase, Is.EqualTo(IncidentPhase.Dormant));
        }

        [Test]
        public void ActivationClosesValveAndLogsCause()
        {
            _director.Tick(0.1f);
            Assert.That(_overheat.Phase, Is.EqualTo(IncidentPhase.Active));
            Assert.That(_director.Model.Get(TrainSubsystem.Cooling).Enabled, Is.False);
            Assert.That(_director.Log.CountOf(MissionEventType.IncidentBegan), Is.EqualTo(1));
        }

        [Test]
        public void EscalatesAboveTemperatureThreshold()
        {
            TickSeconds(24f);
            Assert.That(new[] { IncidentPhase.Escalating, IncidentPhase.Failed }, Does.Contain(_overheat.Phase));
            Assert.That(_director.Log.CountOf(MissionEventType.IncidentEscalated), Is.EqualTo(1));
        }

        [Test]
        public void OpeningCoolingValveResolvesIncident()
        {
            _director.Tick(0.1f);

            var result = _overheat.TryRepair(Request(OverheatIncident.ActionOpenCoolingValve), Actor(1f));
            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.Accepted));
            Assert.That(_director.Model.Get(TrainSubsystem.Cooling).Enabled, Is.True);

            for (int i = 0; i < 600; i++)
            {
                _director.Tick(0.1f);
                if (_overheat.Phase == IncidentPhase.Resolved)
                {
                    break;
                }
            }

            Assert.That(_overheat.Phase, Is.EqualTo(IncidentPhase.Resolved));
            Assert.That(_director.Log.CountOf(MissionEventType.IncidentResolved), Is.EqualTo(1));
        }

        [Test]
        public void FailsWhenTemperatureOverheats()
        {
            TickSeconds(45f);
            Assert.That(_overheat.Phase, Is.EqualTo(IncidentPhase.Failed));
            Assert.That(_director.Log.CountOf(MissionEventType.IncidentFailed), Is.EqualTo(1));
        }

        [Test]
        public void RejectsUnknownAction()
        {
            _director.Tick(0.1f);
            var result = _overheat.TryRepair(Request("do_a_backflip"), Actor(1f));
            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.InvalidAction));
        }

        [Test]
        public void RejectsInteractionWhenTooFar()
        {
            _director.Tick(0.1f);
            var result = _overheat.TryRepair(Request(OverheatIncident.ActionOpenCoolingValve), Actor(9f));
            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.TooFar));
        }

        [Test]
        public void RejectsRepairAfterResolution()
        {
            _director.Tick(0.1f);
            _overheat.TryRepair(Request(OverheatIncident.ActionOpenCoolingValve), Actor(1f));

            for (int i = 0; i < 600 && _overheat.Phase != IncidentPhase.Resolved; i++)
            {
                _director.Tick(0.1f);
            }

            var result = _overheat.TryRepair(Request(OverheatIncident.ActionOpenCoolingValve), Actor(1f));
            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.AlreadyResolved));
        }

        [Test]
        public void CountsAttempts()
        {
            _director.Tick(0.1f);
            _overheat.TryRepair(Request(OverheatIncident.ActionOpenCoolingValve), Actor(1f));
            Assert.That(_overheat.Attempts, Is.EqualTo(1));
        }

        [Test]
        public void ResetReturnsToDormant()
        {
            TickSeconds(5f);
            _overheat.Reset();
            Assert.That(_overheat.Phase, Is.EqualTo(IncidentPhase.Dormant));
            Assert.That(_overheat.Attempts, Is.EqualTo(0));
            Assert.That(_overheat.DiscoveredClues, Is.EqualTo(0));
        }

        private void TickSeconds(float seconds)
        {
            int steps = (int)(seconds / 0.1f);
            for (int i = 0; i < steps; i++)
            {
                _director.Tick(0.1f);
            }
        }
    }
}
