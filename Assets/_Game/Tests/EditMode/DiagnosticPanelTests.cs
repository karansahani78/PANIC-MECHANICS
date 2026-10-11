using System.Collections.Generic;
using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Diagnostics;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class DiagnosticPanelTests
    {
        private static List<TrainSubsystemState> DefaultStates()
        {
            var states = new List<TrainSubsystemState>();
            foreach (TrainSubsystem subsystem in System.Enum.GetValues(typeof(TrainSubsystem)))
            {
                states.Add(new TrainSubsystemState(subsystem, 1f, 0.2f, 0.9f, 0.3f, true, true, 0));
            }

            return states;
        }

        [Test]
        public void EngineerPanelShowsEngineAndCoolingOnly()
        {
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Engineer);

            var text = DiagnosticPanel.Format(profile, DefaultStates());

            Assert.That(text, Does.Contain("Engine"));
            Assert.That(text, Does.Contain("Cooling"));
            Assert.That(text, Does.Not.Contain("Electrical"));
            Assert.That(text, Does.Not.Contain("Brakes"));
        }

        [Test]
        public void ElectricianPanelShowsElectricalOnly()
        {
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Electrician);

            var text = DiagnosticPanel.Format(profile, DefaultStates());

            Assert.That(text, Does.Contain("Electrical"));
            Assert.That(text, Does.Not.Contain("Engine"));
        }

        [Test]
        public void DispatcherPanelShowsEverySubsystem()
        {
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Dispatcher);

            var text = DiagnosticPanel.Format(profile, DefaultStates());

            foreach (TrainSubsystem subsystem in System.Enum.GetValues(typeof(TrainSubsystem)))
            {
                Assert.That(text, Does.Contain(subsystem.ToString()));
            }
        }

        [Test]
        public void PanelReusesDiagnosticDescriptions()
        {
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Electrician);
            var states = new List<TrainSubsystemState>
            {
                new TrainSubsystemState(TrainSubsystem.Electrical, 1f, 0f, 0f, 0.4f, true, false, 0)
            };

            var text = DiagnosticPanel.Format(profile, states);

            Assert.That(text, Does.Contain("NO POWER"));
        }

        [Test]
        public void VisibleSubsystemsFollowsEnumOrder()
        {
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Dispatcher);

            var visible = DiagnosticPanel.VisibleSubsystems(profile);

            Assert.That(visible.Count, Is.EqualTo(System.Enum.GetValues(typeof(TrainSubsystem)).Length));
            Assert.That(visible[0], Is.EqualTo(TrainSubsystem.Engine));
            Assert.That(visible[visible.Count - 1], Is.EqualTo(TrainSubsystem.Communication));
        }

        [Test]
        public void EmptyStatesProducesEmptyPanel()
        {
            var profile = PlayerRoleProfile.CreateDefault(PlayerRole.Driver);

            var text = DiagnosticPanel.Format(profile, new List<TrainSubsystemState>());

            Assert.That(text, Is.Empty);
        }
    }
}
