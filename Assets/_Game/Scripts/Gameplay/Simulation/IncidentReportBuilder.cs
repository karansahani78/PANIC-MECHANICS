using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public sealed class IncidentReport
    {
        public string TopCause = string.Empty;
        public string BestRecovery = string.Empty;
        public string CostlyMistake = string.Empty;
        public string Rating = string.Empty;
        public string Lesson = string.Empty;
        public readonly List<string> Lines = new List<string>();
    }

    public static class IncidentReportBuilder
    {
        private static readonly Dictionary<string, string> CauseText =
            new Dictionary<string, string>
            {
                { "cause.coolant_valve_closed", "The coolant valve was closed, so heat never left the engine." },
                { "cause.breaker_overload", "An overloaded circuit tripped the main breaker." },
                { "cause.brake_pressure_leak", "A seal failed and brake pressure bled away." },
                { "cause.unknown", "The root cause could not be determined during the run." }
            };

        private static readonly Dictionary<string, string> LessonText =
            new Dictionary<string, string>
            {
                { "overheat", "Fix the cause, not the alarm: opening the valve let the pump actually cool the engine." },
                { "powerfault", "Reduce the load before restoring power, or the breaker trips again." },
                { "brakeleak", "Pressure is a resource: slow down early so the leak does not become a crash." }
            };

        private static readonly Dictionary<string, string> ActionText =
            new Dictionary<string, string>
            {
                { "open_cooling_valve", "opening the coolant valve" },
                { "restore_pump_power", "restoring pump power" },
                { "reduce_engine_load", "easing the engine load" }
            };

        public static IncidentReport Build(MissionResult result, MissionEventLog log)
        {
            var report = new IncidentReport();
            var events = log.Events;

            string causeKey = FindFirstValue(events, MissionEventType.IncidentBegan);
            string topCauseText;
            if (!CauseText.TryGetValue(causeKey, out topCauseText))
            {
                topCauseText = CauseText["cause.unknown"];
            }

            report.TopCause = topCauseText;

            string recoveryAction = FindFirstValue(events, MissionEventType.ActionAccepted);
            string recoveryText;
            if (!ActionText.TryGetValue(recoveryAction, out recoveryText))
            {
                recoveryText = "No successful repair was recorded.";
            }

            report.BestRecovery = recoveryText;

            string costlyAction = MostFrequentRejected(events);
            if (costlyAction == null)
            {
                report.CostlyMistake = "No major mistakes recorded.";
            }
            else
            {
                string actionLabel;
                if (!ActionText.TryGetValue(costlyAction, out actionLabel))
                {
                    actionLabel = costlyAction;
                }

                report.CostlyMistake = string.Format(
                    "The crew repeatedly tried {0} while it was not allowed.", actionLabel);
            }

            report.Rating = RatingFor(result);

            string lesson;
            if (!LessonText.TryGetValue(FirstIncidentKind(result, log), out lesson))
            {
                lesson = "Read each symptom before reacting; the train rewards diagnosis over speed.";
            }

            report.Lesson = lesson;

            report.Lines.Add("Outcome: " + result.Outcome);
            report.Lines.Add("Top cause: " + report.TopCause);
            report.Lines.Add("Best recovery: " + report.BestRecovery);
            report.Lines.Add("Costly mistake: " + report.CostlyMistake);
            report.Lines.Add("Rating: " + report.Rating);
            report.Lines.Add("Lesson: " + report.Lesson);

            return report;
        }

        private static string FirstIncidentKind(MissionResult result, MissionEventLog log)
        {
            foreach (var e in log.Events)
            {
                if (e.Type == MissionEventType.IncidentBegan)
                {
                    return e.IncidentId;
                }
            }

            return string.Empty;
        }

        private static string RatingFor(MissionResult result)
        {
            if (result.Outcome == MissionOutcome.Success && result.Score.Total >= 300)
            {
                return "S";
            }

            if (result.Outcome == MissionOutcome.Success)
            {
                return "A";
            }

            if (result.Outcome == MissionOutcome.PartialSuccess)
            {
                return "B";
            }

            return "C";
        }

        private static string FindFirstValue(IReadOnlyList<MissionEvent> events, MissionEventType type)
        {
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].Type == type)
                {
                    return events[i].Value;
                }
            }

            return string.Empty;
        }

        private static string MostFrequentRejected(IReadOnlyList<MissionEvent> events)
        {
            var counts = new Dictionary<string, int>();
            string best = null;
            int bestCount = 0;

            for (int i = 0; i < events.Count; i++)
            {
                var e = events[i];
                if (e.Type != MissionEventType.ActionRejected || string.IsNullOrEmpty(e.Key))
                {
                    continue;
                }

                int count;
                counts.TryGetValue(e.Key, out count);
                count++;
                counts[e.Key] = count;

                if (count > bestCount)
                {
                    bestCount = count;
                    best = e.Key;
                }
            }

            return best;
        }
    }
}
