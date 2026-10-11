using System;

namespace PanicMechanics.Core
{
    public readonly struct TrainSubsystemState
    {
        public readonly TrainSubsystem Subsystem;
        public readonly float Health;
        public readonly float Temperature;
        public readonly float Pressure;
        public readonly float Load;
        public readonly bool Enabled;
        public readonly bool Powered;
        public readonly int Version;

        public TrainSubsystemState(
            TrainSubsystem subsystem,
            float health,
            float temperature,
            float pressure,
            float load,
            bool enabled,
            bool powered,
            int version)
        {
            Subsystem = subsystem;
            Health = health;
            Temperature = temperature;
            Pressure = pressure;
            Load = load;
            Enabled = enabled;
            Powered = powered;
            Version = version;
        }

        public TrainSubsystemState With(
            float? health = null,
            float? temperature = null,
            float? pressure = null,
            float? load = null,
            bool? enabled = null,
            bool? powered = null,
            int? version = null)
        {
            return new TrainSubsystemState(
                Subsystem,
                health ?? Health,
                temperature ?? Temperature,
                pressure ?? Pressure,
                load ?? Load,
                enabled ?? Enabled,
                powered ?? Powered,
                version ?? Version);
        }

        public override string ToString()
        {
            return string.Format(
                "{0}(h={1:0.00} t={2:0.00} p={3:0.00} l={4:0.00} en={5} pw={6} v={7})",
                Subsystem, Health, Temperature, Pressure, Load, Enabled, Powered, Version);
        }
    }

    public readonly struct IncidentRuntimeState
    {
        public readonly string IncidentId;
        public readonly IncidentPhase Phase;
        public readonly float Severity;
        public readonly float Elapsed;
        public readonly int Attempts;
        public readonly float ResolvedAt;
        public readonly int DiscoveredClues;

        public IncidentRuntimeState(
            string incidentId,
            IncidentPhase phase,
            float severity,
            float elapsed,
            int attempts,
            float resolvedAt,
            int discoveredClues)
        {
            IncidentId = incidentId;
            Phase = phase;
            Severity = severity;
            Elapsed = elapsed;
            Attempts = attempts;
            ResolvedAt = resolvedAt;
            DiscoveredClues = discoveredClues;
        }
    }

    public readonly struct PlayerInteractionContext
    {
        public readonly ulong PlayerId;
        public readonly PlayerRole Role;
        public readonly float Distance;
        public readonly float Time;

        public PlayerInteractionContext(ulong playerId, PlayerRole role, float distance, float time)
        {
            PlayerId = playerId;
            Role = role;
            Distance = distance;
            Time = time;
        }
    }

    public readonly struct InteractionRequest
    {
        public readonly string TargetId;
        public readonly string ActionId;
        public readonly ulong ClientId;
        public readonly uint Sequence;

        public InteractionRequest(string targetId, string actionId, ulong clientId, uint sequence)
        {
            TargetId = targetId;
            ActionId = actionId;
            ClientId = clientId;
            Sequence = sequence;
        }
    }

    public readonly struct InteractionResult
    {
        public readonly InteractionResultCode Code;
        public readonly string FeedbackKey;
        public readonly int StateVersion;

        public InteractionResult(InteractionResultCode code, string feedbackKey, int stateVersion)
        {
            Code = code;
            FeedbackKey = feedbackKey;
            StateVersion = stateVersion;
        }

        public bool Accepted
        {
            get { return Code == InteractionResultCode.Accepted; }
        }

        public static InteractionResult Rejected(InteractionResultCode code, string feedbackKey, int stateVersion)
        {
            return new InteractionResult(code, feedbackKey, stateVersion);
        }

        public static InteractionResult Ok(string feedbackKey, int stateVersion)
        {
            return new InteractionResult(InteractionResultCode.Accepted, feedbackKey, stateVersion);
        }
    }

    public readonly struct MissionEvent
    {
        public readonly long EventId;
        public readonly float Time;
        public readonly long Tick;
        public readonly MissionEventType Type;
        public readonly ulong ActorId;
        public readonly string IncidentId;
        public readonly TrainSubsystem Subsystem;
        public readonly string Key;
        public readonly string Value;

        public MissionEvent(
            long eventId,
            float time,
            long tick,
            MissionEventType type,
            ulong actorId,
            string incidentId,
            TrainSubsystem subsystem,
            string key,
            string value)
        {
            EventId = eventId;
            Time = time;
            Tick = tick;
            Type = type;
            ActorId = actorId;
            IncidentId = incidentId;
            Subsystem = subsystem;
            Key = key;
            Value = value;
        }
    }

    public sealed class ScoreBreakdown
    {
        public int IncidentsResolved;
        public int IncidentsFailed;
        public int OptionalObjectives;
        public int TimeBonus;
        public int Total;
    }

    public sealed class MissionResult
    {
        public string MissionId = string.Empty;
        public MissionOutcome Outcome;
        public float Duration;
        public int Players;
        public readonly System.Collections.Generic.List<string> ResolvedIncidents =
            new System.Collections.Generic.List<string>();
        public readonly System.Collections.Generic.List<string> FailedIncidents =
            new System.Collections.Generic.List<string>();
        public ScoreBreakdown Score = new ScoreBreakdown();
        public System.Collections.Generic.List<string> Summary =
            new System.Collections.Generic.List<string>();
    }
}
