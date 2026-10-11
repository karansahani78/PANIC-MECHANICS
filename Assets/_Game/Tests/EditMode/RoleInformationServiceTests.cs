using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Interaction;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class RoleInformationServiceTests
    {
        private RoleInformationService _service;

        [SetUp]
        public void SetUp()
        {
            _service = RoleInformationService.CreateDefault();
        }

        [Test]
        public void EngineerSeesEngineAndCoolingOnly()
        {
            Assert.That(_service.CanSee(PlayerRole.Engineer, TrainSubsystem.Engine), Is.True);
            Assert.That(_service.CanSee(PlayerRole.Engineer, TrainSubsystem.Cooling), Is.True);
            Assert.That(_service.CanSee(PlayerRole.Engineer, TrainSubsystem.Electrical), Is.False);
        }

        [Test]
        public void ElectricianSeesElectricalOnly()
        {
            Assert.That(_service.CanSee(PlayerRole.Electrician, TrainSubsystem.Electrical), Is.True);
            Assert.That(_service.CanSee(PlayerRole.Electrician, TrainSubsystem.Engine), Is.False);
        }

        [Test]
        public void DriverSeesBrakesAndTrack()
        {
            Assert.That(_service.CanSee(PlayerRole.Driver, TrainSubsystem.Brakes), Is.True);
            Assert.That(_service.CanSee(PlayerRole.Driver, TrainSubsystem.Track), Is.True);
        }

        [Test]
        public void DispatcherSeesAllSubsystems()
        {
            foreach (TrainSubsystem subsystem in System.Enum.GetValues(typeof(TrainSubsystem)))
            {
                Assert.That(_service.CanSee(PlayerRole.Dispatcher, subsystem), Is.True,
                    "Dispatcher should see " + subsystem);
            }
        }

        [Test]
        public void UnknownRoleFallsBackToDefaultProfile()
        {
            var fallback = _service.Get((PlayerRole)999);
            Assert.That(fallback, Is.Not.Null);
        }
    }
}
