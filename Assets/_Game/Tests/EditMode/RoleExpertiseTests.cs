using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Interaction;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class RoleExpertiseTests
    {
        [Test]
        public void EngineerIsExpertForOverheatOnly()
        {
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Engineer, "overheat"), Is.True);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Engineer, "powerfault"), Is.False);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Engineer, "brakeleak"), Is.False);
        }

        [Test]
        public void ElectricianIsExpertForPowerFaultOnly()
        {
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Electrician, "powerfault"), Is.True);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Electrician, "overheat"), Is.False);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Electrician, "brakeleak"), Is.False);
        }

        [Test]
        public void DriverIsExpertForBrakeLeakOnly()
        {
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Driver, "brakeleak"), Is.True);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Driver, "overheat"), Is.False);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Driver, "powerfault"), Is.False);
        }

        [Test]
        public void DispatcherIsExpertForEveryTarget()
        {
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Dispatcher, "overheat"), Is.True);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Dispatcher, "powerfault"), Is.True);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Dispatcher, "brakeleak"), Is.True);
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Dispatcher, "unknown"), Is.True);
        }

        [Test]
        public void UnknownTargetIsPermissiveForEveryRole()
        {
            Assert.That(RoleExpertise.IsExpert(PlayerRole.Engineer, "mystery_panel"), Is.True);
            Assert.That(RoleExpertise.IsExpert((PlayerRole)999, "mystery_panel"), Is.True);
        }

        [Test]
        public void UnknownRoleIsNotExpertForKnownTargets()
        {
            Assert.That(RoleExpertise.IsExpert((PlayerRole)999, "overheat"), Is.False);
            Assert.That(RoleExpertise.IsExpert((PlayerRole)999, "powerfault"), Is.False);
        }
    }

    public sealed class InteractionRoleAdvantageTests
    {
        private GamePhase _phase;
        private InteractionService _service;
        private int _acceptedCount;

        private sealed class FakeTarget : IInteractionTarget
        {
            public string TargetId { get; private set; }
            public int AcceptedCount;

            public FakeTarget(string id)
            {
                TargetId = id;
            }

            public bool CanInteract(
                PlayerInteractionContext context, string actionId, out InteractionResultCode rejectionReason)
            {
                rejectionReason = InteractionResultCode.Accepted;
                return true;
            }

            public void HandleAcceptedInteraction(PlayerInteractionContext context, string actionId)
            {
                AcceptedCount++;
            }
        }

        private FakeTarget _target;

        [SetUp]
        public void SetUp()
        {
            _phase = GamePhase.Running;
            _service = new InteractionService(() => _phase)
            {
                MaxDistance = 3f,
                DefaultCooldownSeconds = 2f,
                ExpertCooldownMultiplier = 0.5f,
                IsExpertFor = (role, targetId) => role == PlayerRole.Engineer
            };
            _target = new FakeTarget("pump");
            _service.RegisterTarget(_target);
            _acceptedCount = 0;
        }

        private static InteractionResult Submit(
            InteractionService service, ulong playerId, PlayerRole role, float time)
        {
            return service.Submit(
                new InteractionRequest("pump", "act", playerId, 0u),
                new PlayerInteractionContext(playerId, role, 1f, time));
        }

        [Test]
        public void ExpertRolePaysReducedCooldown()
        {
            var first = Submit(_service, 1UL, PlayerRole.Engineer, 10f);
            var second = Submit(_service, 1UL, PlayerRole.Engineer, 11f);

            Assert.That(first.Accepted, Is.True);
            Assert.That(second.Accepted, Is.True);
        }

        [Test]
        public void NonExpertRolePaysFullCooldown()
        {
            var first = Submit(_service, 2UL, PlayerRole.Electrician, 10f);
            var second = Submit(_service, 2UL, PlayerRole.Electrician, 11f);
            var third = Submit(_service, 2UL, PlayerRole.Electrician, 13f);

            Assert.That(first.Accepted, Is.True);
            Assert.That(second.Code, Is.EqualTo(InteractionResultCode.Cooldown));
            Assert.That(third.Accepted, Is.True);
        }

        [Test]
        public void CooldownIsWellKnownAndAcceptedCountTracksAll()
        {
            var a = Submit(_service, 1UL, PlayerRole.Engineer, 10f);
            var b = Submit(_service, 2UL, PlayerRole.Electrician, 10f);
            var c = Submit(_service, 1UL, PlayerRole.Engineer, 11f);
            var d = Submit(_service, 2UL, PlayerRole.Electrician, 13.5f);

            Assert.That(a.Accepted, Is.True);
            Assert.That(b.Accepted, Is.True);
            Assert.That(c.Accepted, Is.True);
            Assert.That(d.Accepted, Is.True);
            Assert.That(_target.AcceptedCount, Is.EqualTo(4));
        }
    }
}