using System.Collections.Generic;
using System.Text;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Diagnostics
{
    public static class DiagnosticPanel
    {
        public static IReadOnlyList<TrainSubsystem> VisibleSubsystems(PlayerRoleProfile profile)
        {
            var result = new List<TrainSubsystem>();
            foreach (TrainSubsystem subsystem in System.Enum.GetValues(typeof(TrainSubsystem)))
            {
                if (profile.CanSeeDiagnosticsFor(subsystem))
                {
                    result.Add(subsystem);
                }
            }

            return result;
        }

        public static string Format(PlayerRoleProfile profile, IReadOnlyList<TrainSubsystemState> states)
        {
            var builder = new StringBuilder();
            for (int i = 0; i < states.Count; i++)
            {
                var state = states[i];
                if (!profile.CanSeeDiagnosticsFor(state.Subsystem))
                {
                    continue;
                }

                builder.Append(state.Subsystem)
                    .Append("  ")
                    .Append(DiagnosticService.PrimaryValue(state.Subsystem, state).ToString("0.00"))
                    .Append("  ")
                    .Append(DiagnosticService.Describe(state.Subsystem, state))
                    .Append('\n');
            }

            return builder.ToString();
        }
    }
}
