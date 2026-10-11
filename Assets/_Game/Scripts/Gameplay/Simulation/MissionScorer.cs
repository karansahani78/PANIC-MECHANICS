using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public static class MissionScorer
    {
        public const int ResolvedIncidentPoints = 100;
        public const int FailedIncidentPoints = -150;
        public const int SuccessBonus = 200;
        public const int PartialBonus = 100;

        public static ScoreBreakdown Score(
            MissionOutcome outcome,
            IReadOnlyList<IncidentController> incidents,
            float duration,
            float targetDuration)
        {
            var breakdown = new ScoreBreakdown();

            for (int i = 0; i < incidents.Count; i++)
            {
                switch (incidents[i].Phase)
                {
                    case IncidentPhase.Resolved:
                        breakdown.IncidentsResolved++;
                        break;
                    case IncidentPhase.Failed:
                        breakdown.IncidentsFailed++;
                        break;
                }
            }

            int timeBonus = 0;
            if (outcome == MissionOutcome.Success && targetDuration > duration)
            {
                timeBonus = (int)Math.Round((targetDuration - duration) * 2f);
            }

            breakdown.TimeBonus = timeBonus;
            breakdown.Total =
                breakdown.IncidentsResolved * ResolvedIncidentPoints +
                breakdown.IncidentsFailed * FailedIncidentPoints +
                timeBonus +
                (outcome == MissionOutcome.Success
                    ? SuccessBonus
                    : outcome == MissionOutcome.PartialSuccess ? PartialBonus : 0);

            return breakdown;
        }
    }
}
