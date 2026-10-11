using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Missions
{
    public sealed class IncidentConfig
    {
        public string Id = "overheat";
        public IncidentKind Kind = IncidentKind.Overheat;
        public TrainSubsystem Subsystem = TrainSubsystem.Engine;
        public float ActivateAfter = 0f;
        public float SeverityWeight = 1f;

        public float SymptomTemperature = 0.55f;
        public float EscalateTemperature = 0.70f;
        public float FailTemperature = 0.98f;
        public float StabilizeTemperature = 0.30f;
        public float StableHoldSeconds = 2f;
        public float ReduceLoadAmount = 0.15f;

        public static IncidentConfig Overheat(string id = "overheat", float activateAfter = 0f)
        {
            return new IncidentConfig
            {
                Id = id,
                Kind = IncidentKind.Overheat,
                Subsystem = TrainSubsystem.Engine,
                ActivateAfter = activateAfter
            };
        }

        public static IncidentConfig PowerFault(string id = "powerfault", float activateAfter = 0f)
        {
            return new IncidentConfig
            {
                Id = id,
                Kind = IncidentKind.PowerFault,
                Subsystem = TrainSubsystem.Electrical,
                ActivateAfter = activateAfter
            };
        }

        public static IncidentConfig BrakeLeak(string id = "brakeleak", float activateAfter = 0f)
        {
            return new IncidentConfig
            {
                Id = id,
                Kind = IncidentKind.BrakeLeak,
                Subsystem = TrainSubsystem.Brakes,
                ActivateAfter = activateAfter
            };
        }
    }

    public sealed class MissionConfig
    {
        public string Id = "mission.default";
        public float TargetDurationSeconds = 300f;
        public int Players = 2;
        public float DifficultyScale = 1f;
        public List<IncidentConfig> Incidents = new List<IncidentConfig>();
    }
}
