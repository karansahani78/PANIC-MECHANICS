using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    [TestFixture]
    public class TrainSystemModelTests
    {
        [Test]
        public void EngineTemperatureRisesWhenCoolingValveClosed()
        {
            var model = new TrainSystemModel();
            float start = model.Get(TrainSubsystem.Engine).Temperature;

            for (int i = 0; i < 100; i++)
            {
                model.Tick(0.1f);
            }

            Assert.That(model.Get(TrainSubsystem.Engine).Temperature, Is.GreaterThan(start));
        }

        [Test]
        public void EngineTemperatureFallsWhenValveOpenAndPowered()
        {
            var model = new TrainSystemModel();
            model.Apply(SubsystemEffect.SetEnabled(TrainSubsystem.Cooling, true));
            float start = model.Get(TrainSubsystem.Engine).Temperature;

            for (int i = 0; i < 100; i++)
            {
                model.Tick(0.1f);
            }

            Assert.That(model.Get(TrainSubsystem.Engine).Temperature, Is.LessThan(start));
        }

        [Test]
        public void BrakeLeakDropsPressureOverTime()
        {
            var model = new TrainSystemModel();
            model.Apply(new SubsystemEffect(TrainSubsystem.Brakes, loadDelta: 0.5f));

            for (int i = 0; i < 100; i++)
            {
                model.Tick(0.1f);
            }

            Assert.That(model.Get(TrainSubsystem.Brakes).Pressure, Is.LessThan(1f));
        }

        [Test]
        public void StateVersionIncreasesOnChange()
        {
            var model = new TrainSystemModel();
            int before = model.Version;
            model.Apply(SubsystemEffect.SetPowered(TrainSubsystem.Electrical, false));
            Assert.That(model.Version, Is.GreaterThan(before));
        }

        [Test]
        public void ResetRestoresDefaultState()
        {
            var model = new TrainSystemModel();
            model.Apply(new SubsystemEffect(TrainSubsystem.Engine, temperatureDelta: 0.5f));
            model.Reset();
            Assert.That(model.Get(TrainSubsystem.Engine).Temperature, Is.EqualTo(0.25f).Within(0.0001f));
            Assert.That(model.Get(TrainSubsystem.Cooling).Enabled, Is.False);
        }
    }
}
