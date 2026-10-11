using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;
using UnityEngine;

namespace PanicMechanics.Data
{
    [CreateAssetMenu(menuName = "Panic Mechanics/Incident Definition", fileName = "IncidentDefinition")]
    public sealed class IncidentDefinitionSO : ScriptableObject
    {
        public string Id = "overheat";
        public IncidentKind Kind = IncidentKind.Overheat;
        public TrainSubsystem Subsystem = TrainSubsystem.Engine;
        public float ActivateAfterSeconds = 0f;
        public float SeverityWeight = 1f;

        [Header("Overheat thresholds")]
        public float SymptomTemperature = 0.55f;
        public float EscalateTemperature = 0.70f;
        public float FailTemperature = 0.98f;
        public float StabilizeTemperature = 0.30f;
        public float StableHoldSeconds = 2f;
        public float ReduceLoadAmount = 0.15f;

        public IncidentController Build(TrainSystemModel model, MissionEventLog log)
        {
            switch (Kind)
            {
                case IncidentKind.Overheat:
                    return new OverheatIncident(model, log, ActivateAfterSeconds)
                    {
                        SymptomTemperature = SymptomTemperature,
                        EscalateTemperature = EscalateTemperature,
                        FailTemperature = FailTemperature,
                        StabilizeTemperature = StabilizeTemperature,
                        StableHoldSeconds = StableHoldSeconds,
                        ReduceLoadAmount = ReduceLoadAmount
                    };
                default:
                    Debug.LogError("Incident kind not yet implemented: " + Kind +
                                   " (" + Id + ")");
                    return null;
            }
        }
    }
}
