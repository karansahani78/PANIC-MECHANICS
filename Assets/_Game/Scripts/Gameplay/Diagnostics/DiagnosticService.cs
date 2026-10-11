using System.Collections.Generic;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Gameplay.Diagnostics
{
    public readonly struct SubsystemReading
    {
        public readonly TrainSubsystem Subsystem;
        public readonly float Value;
        public readonly string Label;

        public SubsystemReading(TrainSubsystem subsystem, float value, string label)
        {
            Subsystem = subsystem;
            Value = value;
            Label = label;
        }
    }

    public static class DiagnosticService
    {
        public static IReadOnlyList<SubsystemReading> Read(TrainSystemModel model, PlayerRoleProfile profile)
        {
            var readings = new List<SubsystemReading>();
            foreach (var subsystem in profile.VisibleSubsystems)
            {
                var state = model.Get(subsystem);
                readings.Add(new SubsystemReading(subsystem, PrimaryValue(subsystem, state), Describe(subsystem, state)));
            }

            return readings;
        }

        public static float PrimaryValue(TrainSubsystem subsystem, TrainSubsystemState state)
        {
            switch (subsystem)
            {
                case TrainSubsystem.Brakes:
                    return state.Pressure;
                case TrainSubsystem.Engine:
                    return state.Temperature;
                default:
                    return state.Load;
            }
        }

        public static string Describe(TrainSubsystem subsystem, TrainSubsystemState state)
        {
            switch (subsystem)
            {
                case TrainSubsystem.Engine:
                    if (state.Temperature >= 0.70f)
                    {
                        return "OVERHEATING";
                    }

                    return state.Temperature >= 0.55f ? "running hot" : "nominal";

                case TrainSubsystem.Cooling:
                    return state.Enabled ? "pump on" : "pump off";

                case TrainSubsystem.Electrical:
                    return state.Powered ? "powered" : "NO POWER";

                case TrainSubsystem.Brakes:
                    if (state.Pressure <= 0.35f)
                    {
                        return "CRITICAL";
                    }

                    return state.Pressure <= 0.60f ? "soft" : "charged";

                default:
                    return "nominal";
            }
        }
    }
}
