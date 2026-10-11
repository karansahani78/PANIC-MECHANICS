using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Diagnostics;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class DiagnosticServiceTests
    {
        [Test]
        public void EngineerReadsOnlyVisibleSubsystems()
        {
            var model = new TrainSystemModel();
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Engineer);

            var readings = DiagnosticService.Read(model, profile);

            Assert.That(readings.Count, Is.EqualTo(2));
            bool hasEngine = false;
            bool hasCooling = false;
            foreach (var r in readings)
            {
                if (r.Subsystem == TrainSubsystem.Engine) hasEngine = true;
                if (r.Subsystem == TrainSubsystem.Cooling) hasCooling = true;
                Assert.That(r.Subsystem, Is.Not.EqualTo(TrainSubsystem.Brakes));
            }

            Assert.That(hasEngine, Is.True);
            Assert.That(hasCooling, Is.True);
        }

        [Test]
        public void EnginePrimaryValueIsTemperature()
        {
            var model = new TrainSystemModel();
            var state = model.Get(TrainSubsystem.Engine);

            Assert.That(DiagnosticService.PrimaryValue(TrainSubsystem.Engine, state),
                Is.EqualTo(state.Temperature).Within(0.0001f));
        }

        [Test]
        public void BrakesPrimaryValueIsPressure()
        {
            var model = new TrainSystemModel();
            var state = model.Get(TrainSubsystem.Brakes);

            Assert.That(DiagnosticService.PrimaryValue(TrainSubsystem.Brakes, state),
                Is.EqualTo(state.Pressure).Within(0.0001f));
        }

        [Test]
        public void UnpoweredElectricalIsReportedClearly()
        {
            var state = new TrainSubsystemState(
                TrainSubsystem.Electrical, 1f, 0f, 0f, 0.4f, true, false, 0);

            Assert.That(DiagnosticService.Describe(TrainSubsystem.Electrical, state), Is.EqualTo("NO POWER"));
        }

        [Test]
        public void LowBrakePressureIsCritical()
        {
            var state = new TrainSubsystemState(
                TrainSubsystem.Brakes, 1f, 0f, 0.2f, 0f, true, true, 0);

            Assert.That(DiagnosticService.Describe(TrainSubsystem.Brakes, state), Is.EqualTo("CRITICAL"));
        }

        [Test]
        public void HotEngineIsReported()
        {
            var state = new TrainSubsystemState(
                TrainSubsystem.Engine, 1f, 0.75f, 0f, 0.35f, true, true, 0);

            Assert.That(DiagnosticService.Describe(TrainSubsystem.Engine, state), Is.EqualTo("OVERHEATING"));
        }
    }
}
