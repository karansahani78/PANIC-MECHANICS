using System;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Simulation;

namespace PanicMechanics.Gameplay.Missions
{
    public static class MissionBuilder
    {
        public static MissionDirector Build(MissionConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var profile = DifficultyScaler.ForPlayers(config.Players);
            var director = new MissionDirector(config.Id, profile.Players, config.TargetDurationSeconds);

            float scale = config.DifficultyScale <= 0f ? 1f : config.DifficultyScale;
            director.Model.HeatGainPerSecond *= profile.HeatGainMultiplier * scale;
            director.Model.BrakeLeakPerSecond *= profile.TimePressureMultiplier * scale;

            if (config.Incidents != null)
            {
                for (int i = 0; i < config.Incidents.Count; i++)
                {
                    var incident = CreateIncident(config.Incidents[i], director.Model, director.Log);
                    if (incident != null)
                    {
                        director.AddIncident(incident);
                    }
                }
            }

            return director;
        }

        private static IncidentController CreateIncident(
            IncidentConfig config, TrainSystemModel model, MissionEventLog log)
        {
            switch (config.Kind)
            {
                case IncidentKind.Overheat:
                    return new OverheatIncident(model, log, config.ActivateAfter)
                    {
                        SymptomTemperature = config.SymptomTemperature,
                        EscalateTemperature = config.EscalateTemperature,
                        FailTemperature = config.FailTemperature,
                        StabilizeTemperature = config.StabilizeTemperature,
                        StableHoldSeconds = config.StableHoldSeconds,
                        ReduceLoadAmount = config.ReduceLoadAmount
                    };

                case IncidentKind.PowerFault:
                    return new PowerFaultIncident(model, log, config.ActivateAfter);

                case IncidentKind.BrakeLeak:
                    return new BrakeLeakIncident(model, log, config.ActivateAfter);

                default:
                    return null;
            }
        }
    }
}
