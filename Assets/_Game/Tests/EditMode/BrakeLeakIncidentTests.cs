using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class BrakeLeakIncidentTests
    {
        private MissionDirector _director;
        private BrakeLeakIncident _brake;

        private static PlayerInteractionContext Actor()
        {
            return new PlayerInteractionContext(3UL, PlayerRole.Driver, 1f, 0f);
        }

        private static InteractionRequest Request(string action)
        {
            return new InteractionRequest("brakeleak", action, 3UL, 1u);
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
            _brake = new BrakeLeakIncident(_director.Model, _director.Log);
            _director.AddIncident(_brake);
            _director.EnterLobby();
            _director.StartMission();
            _director.Tick(0.1f);
        }

        [Test]
        public void ActivationStartsLeak()
        {
            Assert.That(_brake.Phase, Is.EqualTo(IncidentPhase.Active));
            Assert.That(_director.Model.Get(TrainSubsystem.Brakes).Load, Is.GreaterThan(0.4f));
        }

        [Test]
        public void PressureDropsOverTime()
        {
            Advance(5f);
            Assert.That(_director.Model.Get(TrainSubsystem.Brakes).Pressure, Is.LessThan(1f));
        }

        [Test]
        public void CloseValveThenRechargeResolves()
        {
            _brake.TryRepair(Request(BrakeLeakIncident.ActionCloseBrakeValve), Actor());
            _brake.TryRepair(Request(BrakeLeakIncident.ActionRechargeBrakes), Actor());

            Assert.That(_director.Model.Get(TrainSubsystem.Brakes).Load, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(_director.Model.Get(TrainSubsystem.Brakes).Pressure, Is.GreaterThanOrEqualTo(0.5f));

            Advance(2.5f);
            Assert.That(_brake.Phase, Is.EqualTo(IncidentPhase.Resolved));
        }

        [Test]
        public void ClosingValveTwiceIsRejected()
        {
            _brake.TryRepair(Request(BrakeLeakIncident.ActionCloseBrakeValve), Actor());
            var again = _brake.TryRepair(Request(BrakeLeakIncident.ActionCloseBrakeValve), Actor());

            Assert.That(again.Code, Is.EqualTo(InteractionResultCode.PreconditionsNotMet));
        }

        [Test]
        public void EscalatesThenFailsWhenIgnored()
        {
            Advance(45f);
            Assert.That(_brake.Phase == IncidentPhase.Escalating || _brake.Phase == IncidentPhase.Failed, Is.True,
                "Expected Escalating or Failed but was " + _brake.Phase);

            Advance(20f);
            Assert.That(_brake.Phase, Is.EqualTo(IncidentPhase.Failed));
            Assert.That(_director.Result.Outcome, Is.EqualTo(MissionOutcome.Failure));
        }
    }
}
