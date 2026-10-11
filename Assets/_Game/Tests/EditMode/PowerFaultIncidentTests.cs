using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class PowerFaultIncidentTests
    {
        private MissionDirector _director;
        private PowerFaultIncident _power;

        private static PlayerInteractionContext Actor()
        {
            return new PlayerInteractionContext(2UL, PlayerRole.Electrician, 1f, 0f);
        }

        private static InteractionRequest Request(string action)
        {
            return new InteractionRequest("powerfault", action, 2UL, 1u);
        }

        private void Advance(float seconds)
        {
            int steps = (int)(seconds / 0.1f);
            for (int i = 0; i < steps && _director.Phase == GamePhase.Running; i++)
            {
                _director.Tick(0.1f);
            }
        }

        [SetUp]
        public void SetUp()
        {
            _director = new MissionDirector("m", 3, 300f);
            _power = new PowerFaultIncident(_director.Model, _director.Log);
            _director.AddIncident(_power);
            _director.EnterLobby();
            _director.StartMission();
            _director.Tick(0.1f);
        }

        [Test]
        public void ActivationCutsPowerAndLogsClue()
        {
            Assert.That(_power.Phase, Is.EqualTo(IncidentPhase.Active));
            Assert.That(_director.Model.Get(TrainSubsystem.Electrical).Powered, Is.False);
            Assert.That(_director.Log.CountOf(MissionEventType.SymptomDiscovered), Is.GreaterThan(0));
        }

        [Test]
        public void ResetBreakerRejectedWhileEngineLoadHigh()
        {
            var result = _power.TryRepair(Request(PowerFaultIncident.ActionResetBreaker), Actor());

            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.PreconditionsNotMet));
            Assert.That(result.FeedbackKey, Is.EqualTo("powerfault.load_too_high"));
            Assert.That(_director.Model.Get(TrainSubsystem.Electrical).Powered, Is.False);
        }

        [Test]
        public void ReduceLoadThenResetBreakerResolves()
        {
            _power.TryRepair(Request(PowerFaultIncident.ActionReduceEngineLoad), Actor());
            var reset = _power.TryRepair(Request(PowerFaultIncident.ActionResetBreaker), Actor());

            Assert.That(reset.Accepted, Is.True);
            Assert.That(_director.Model.Get(TrainSubsystem.Electrical).Powered, Is.True);

            Advance(2.5f);
            Assert.That(_power.Phase, Is.EqualTo(IncidentPhase.Resolved));
        }

        [Test]
        public void EscalatesWhenIgnored()
        {
            Advance(12.5f);
            Assert.That(_power.Phase, Is.EqualTo(IncidentPhase.Escalating));
        }

        [Test]
        public void FailsMissionWhenIgnoredTooLong()
        {
            Advance(40f);

            Assert.That(_power.Phase, Is.EqualTo(IncidentPhase.Failed));
            Assert.That(_director.Result.Outcome, Is.EqualTo(MissionOutcome.Failure));
            Assert.That(_director.Result.FailedIncidents, Does.Contain("powerfault"));
        }
    }
}
