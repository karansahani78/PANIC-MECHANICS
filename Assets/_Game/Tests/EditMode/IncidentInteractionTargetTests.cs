using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Interaction;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class IncidentInteractionTargetTests
    {
        private GamePhase _phase;
        private MissionDirector _director;
        private OverheatIncident _overheat;
        private InteractionService _service;

        [SetUp]
        public void SetUp()
        {
            _director = new MissionDirector("m", 4, 300f);
            _overheat = new OverheatIncident(_director.Model, _director.Log);
            _director.AddIncident(_overheat);
            _director.EnterLobby();
            _director.StartMission();
            _director.Tick(0.1f);

            _phase = GamePhase.Running;
            _service = new InteractionService(() => _phase)
            {
                MaxDistance = IncidentController.MaxInteractionDistance
            };
            _service.RegisterTarget(new IncidentInteractionTarget(_overheat));
        }

        private static PlayerInteractionContext Actor(float distance)
        {
            return new PlayerInteractionContext(1UL, PlayerRole.Engineer, distance, 0f);
        }

        [Test]
        public void AcceptedActionReturnsIncidentFeedbackKey()
        {
            var result = _service.Submit(
                new InteractionRequest("overheat", OverheatIncident.ActionOpenCoolingValve, 1UL, 0u),
                Actor(1f));

            Assert.That(result.Accepted, Is.True);
            Assert.That(result.FeedbackKey, Is.EqualTo("overheat.valve_opened"));
        }

        [Test]
        public void PreconditionRejectionFromIncidentIsPropagated()
        {
            _overheat.TryRepair(
                new InteractionRequest("overheat", OverheatIncident.ActionOpenCoolingValve, 1UL, 0u),
                Actor(1f));

            var result = _service.Submit(
                new InteractionRequest("overheat", OverheatIncident.ActionOpenCoolingValve, 1UL, 0u),
                Actor(1f));

            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.PreconditionsNotMet));
            Assert.That(result.FeedbackKey, Is.EqualTo("overheat.valve_already_open"));
        }

        [Test]
        public void DormantIncidentReportsWrongPhase()
        {
            var director = new MissionDirector("m2", 4, 300f);
            var incident = new OverheatIncident(director.Model, director.Log, activateAfter: 5f);
            director.AddIncident(incident);
            director.EnterLobby();
            director.StartMission();

            InteractionResultCode reason;
            bool allowed = incident.CanAttempt(OverheatIncident.ActionOpenCoolingValve, Actor(1f), out reason);

            Assert.That(allowed, Is.False);
            Assert.That(reason, Is.EqualTo(InteractionResultCode.WrongPhase));
        }

        [Test]
        public void CanAttemptReturnsTrueWhenActionIsAvailable()
        {
            InteractionResultCode reason;
            bool allowed = _overheat.CanAttempt(OverheatIncident.ActionReduceEngineLoad, Actor(1f), out reason);

            Assert.That(allowed, Is.True);
            Assert.That(reason, Is.EqualTo(InteractionResultCode.Accepted));
        }
    }
}
