using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Interaction;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class InteractionServiceTests
    {
        private GamePhase _phase;
        private InteractionService _service;

        private sealed class FakeTarget : IInteractionTarget
        {
            public string TargetId { get; private set; }
            public bool Allow = true;
            public InteractionResultCode BlockReason = InteractionResultCode.PreconditionsNotMet;
            public int AcceptedCount;

            public FakeTarget(string id)
            {
                TargetId = id;
            }

            public bool CanInteract(PlayerInteractionContext context, string actionId, out InteractionResultCode rejectionReason)
            {
                rejectionReason = BlockReason;
                return Allow;
            }

            public void HandleAcceptedInteraction(PlayerInteractionContext context, string actionId)
            {
                AcceptedCount++;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _phase = GamePhase.Running;
            _service = new InteractionService(() => _phase)
            {
                MaxDistance = 3f
            };
        }

        private static InteractionRequest Request(string target, string action)
        {
            return new InteractionRequest(target, action, 1UL, 0u);
        }

        private static PlayerInteractionContext Context(float distance, float time)
        {
            return new PlayerInteractionContext(1UL, PlayerRole.Engineer, distance, time);
        }

        [Test]
        public void UnknownTargetIsRejected()
        {
            var result = _service.Submit(Request("missing", "act"), Context(1f, 0f));

            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.InvalidTarget));
        }

        [Test]
        public void WrongPhaseIsRejected()
        {
            var target = new FakeTarget("pump");
            _service.RegisterTarget(target);
            _phase = GamePhase.Lobby;

            var result = _service.Submit(Request("pump", "act"), Context(1f, 0f));

            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.WrongPhase));
            Assert.That(target.AcceptedCount, Is.EqualTo(0));
        }

        [Test]
        public void TooFarIsRejected()
        {
            var target = new FakeTarget("pump");
            _service.RegisterTarget(target);

            var result = _service.Submit(Request("pump", "act"), Context(5f, 0f));

            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.TooFar));
        }

        [Test]
        public void TargetPreconditionRejectionIsPropagated()
        {
            var target = new FakeTarget("pump") { Allow = false };
            _service.RegisterTarget(target);

            var result = _service.Submit(Request("pump", "act"), Context(1f, 0f));

            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.PreconditionsNotMet));
            Assert.That(result.FeedbackKey, Is.EqualTo("interaction.preconditions"));
        }

        [Test]
        public void AcceptedInteractionCallsTarget()
        {
            var target = new FakeTarget("pump");
            _service.RegisterTarget(target);

            var result = _service.Submit(Request("pump", "act"), Context(1f, 0f));

            Assert.That(result.Accepted, Is.True);
            Assert.That(target.AcceptedCount, Is.EqualTo(1));
        }

        [Test]
        public void CooldownBlocksRepeatUntilTimeAdvances()
        {
            var target = new FakeTarget("pump");
            _service.RegisterTarget(target);
            _service.DefaultCooldownSeconds = 2f;

            var first = _service.Submit(Request("pump", "act"), Context(1f, 10f));
            var second = _service.Submit(Request("pump", "act"), Context(1f, 11f));
            var third = _service.Submit(Request("pump", "act"), Context(1f, 13f));

            Assert.That(first.Accepted, Is.True);
            Assert.That(second.Code, Is.EqualTo(InteractionResultCode.Cooldown));
            Assert.That(third.Accepted, Is.True);
            Assert.That(target.AcceptedCount, Is.EqualTo(2));
        }

        [Test]
        public void UnregisterRemovesTarget()
        {
            var target = new FakeTarget("pump");
            _service.RegisterTarget(target);

            Assert.That(_service.TargetCount, Is.EqualTo(1));
            Assert.That(_service.UnregisterTarget("pump"), Is.True);

            var result = _service.Submit(Request("pump", "act"), Context(1f, 0f));
            Assert.That(result.Code, Is.EqualTo(InteractionResultCode.InvalidTarget));
        }
    }
}
