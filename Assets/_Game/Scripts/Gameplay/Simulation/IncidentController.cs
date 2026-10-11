using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public abstract class IncidentController
    {
        public const float MaxInteractionDistance = 3.0f;

        public string Id { get; private set; }
        public IncidentKind Kind { get; private set; }
        public TrainSubsystem Subsystem { get; private set; }
        public IncidentPhase Phase { get; private set; }
        public float Severity { get; private set; }
        public float Elapsed { get; protected set; }
        public int Attempts { get; private set; }
        public int DiscoveredClues { get; private set; }
        public float ResolvedAt { get; private set; }

        protected readonly TrainSystemModel Model;
        protected readonly MissionEventLog Log;
        protected float MissionTime;

        private bool _activated;

        public event Action<IncidentController> Changed;

        protected IncidentController(
            string id,
            IncidentKind kind,
            TrainSubsystem subsystem,
            TrainSystemModel model,
            MissionEventLog log)
        {
            Id = id;
            Kind = kind;
            Subsystem = subsystem;
            Model = model;
            Log = log;
            ResolvedAt = -1f;
            Reset();
        }

        public IncidentRuntimeState RuntimeState
        {
            get
            {
                return new IncidentRuntimeState(
                    Id, Phase, Severity, Elapsed, Attempts, ResolvedAt, DiscoveredClues);
            }
        }

        public bool IsTerminal
        {
            get { return Phase == IncidentPhase.Resolved || Phase == IncidentPhase.Failed; }
        }

        public bool CanAttempt(
            string actionId, PlayerInteractionContext context, out InteractionResultCode reason)
        {
            if (IsTerminal)
            {
                reason = InteractionResultCode.AlreadyResolved;
                return false;
            }

            if (Phase == IncidentPhase.Dormant)
            {
                reason = InteractionResultCode.WrongPhase;
                return false;
            }

            return CanPerform(actionId, context, out reason);
        }

        protected virtual float ActivateAt
        {
            get { return 0f; }
        }

        public abstract IEnumerable<string> AvailableActions { get; }

        public void Tick(float dt, float missionTime)
        {
            MissionTime = missionTime;

            if (!_activated)
            {
                if (missionTime < ActivateAt)
                {
                    return;
                }

                _activated = true;
                Elapsed = 0f;
                OnActivate();
                RaiseChanged();
            }

            if (dt > 0f)
            {
                Elapsed += dt;
            }

            OnTick(dt, missionTime);
        }

        public InteractionResult TryRepair(InteractionRequest request, PlayerInteractionContext context)
        {            Attempts++;
            Log.Append(MissionTime, 0, MissionEventType.ActionAttempted,
                context.PlayerId, Id, Subsystem, request.ActionId);

            if (IsTerminal)
            {
                LogRejected(request.ActionId, InteractionResultCode.AlreadyResolved);
                return InteractionResult.Rejected(
                    InteractionResultCode.AlreadyResolved, "incident.done", Model.Version);
            }

            if (Phase == IncidentPhase.Dormant)
            {
                LogRejected(request.ActionId, InteractionResultCode.WrongPhase);
                return InteractionResult.Rejected(
                    InteractionResultCode.WrongPhase, "incident.not_active", Model.Version);
            }

            InteractionResultCode reason;
            if (!CanPerform(request.ActionId, context, out reason))
            {
                LogRejected(request.ActionId, reason);
                return InteractionResult.Rejected(reason, FeedbackKeyFor(reason), Model.Version);
            }

            var result = ApplyAction(request.ActionId, context);
            if (result.Accepted)
            {
                Log.Append(MissionTime, 0, MissionEventType.ActionAccepted,
                    context.PlayerId, Id, Subsystem, request.ActionId);
            }
            else
            {
                LogRejected(request.ActionId, result.Code);
            }

            RaiseChanged();
            return new InteractionResult(result.Code, result.FeedbackKey, Model.Version);
        }

        protected virtual bool CanPerform(
            string actionId, PlayerInteractionContext context, out InteractionResultCode reason)
        {
            if (!IsActionAvailable(actionId))
            {
                reason = InteractionResultCode.InvalidAction;
                return false;
            }

            if (context.Distance > MaxInteractionDistance)
            {
                reason = InteractionResultCode.TooFar;
                return false;
            }

            reason = InteractionResultCode.Accepted;
            return true;
        }

        protected abstract bool IsActionAvailable(string actionId);

        protected abstract InteractionResult ApplyAction(string actionId, PlayerInteractionContext context);

        protected abstract void OnActivate();

        protected abstract void OnTick(float dt, float missionTime);

        protected void SetPhase(IncidentPhase phase)
        {
            if (Phase == phase)
            {
                return;
            }

            Phase = phase;

            switch (phase)
            {
                case IncidentPhase.Escalating:
                    Log.Append(MissionTime, 0, MissionEventType.IncidentEscalated, 0UL, Id, Subsystem);
                    break;
                case IncidentPhase.Stabilized:
                    Log.Append(MissionTime, 0, MissionEventType.IncidentStabilized, 0UL, Id, Subsystem);
                    break;
                case IncidentPhase.Resolved:
                    ResolvedAt = MissionTime;
                    Log.Append(MissionTime, 0, MissionEventType.IncidentResolved, 0UL, Id, Subsystem);
                    break;
                case IncidentPhase.Failed:
                    Log.Append(MissionTime, 0, MissionEventType.IncidentFailed, 0UL, Id, Subsystem);
                    break;
            }
        }

        protected void SetSeverity(float severity)
        {
            Severity = TrainSystemModel.Clamp01(severity);
        }

        protected void LogBegan(string causeKey)
        {
            Log.Append(MissionTime, 0, MissionEventType.IncidentBegan, 0UL, Id, Subsystem, causeKey);
        }

        protected void LogClue(string clueKey)
        {
            DiscoveredClues++;
            Log.Append(MissionTime, 0, MissionEventType.SymptomDiscovered, 0UL, Id, Subsystem, clueKey);
        }

        private void LogRejected(string actionId, InteractionResultCode code)
        {
            Log.Append(MissionTime, 0, MissionEventType.ActionRejected, 0UL, Id, Subsystem,
                actionId, code.ToString());
        }

        protected void RaiseChanged()
        {
            var handler = Changed;
            if (handler != null)
            {
                handler(this);
            }
        }

        public virtual void Reset()
        {
            _activated = false;
            Phase = IncidentPhase.Dormant;
            Severity = 0f;
            Elapsed = 0f;
            Attempts = 0;
            DiscoveredClues = 0;
            ResolvedAt = -1f;
        }

        private static string FeedbackKeyFor(InteractionResultCode code)
        {
            switch (code)
            {
                case InteractionResultCode.InvalidAction:
                    return "interaction.invalid_action";
                case InteractionResultCode.TooFar:
                    return "interaction.too_far";
                case InteractionResultCode.WrongPhase:
                    return "interaction.wrong_phase";
                case InteractionResultCode.AlreadyResolved:
                    return "interaction.already_resolved";
                case InteractionResultCode.PreconditionsNotMet:
                    return "interaction.preconditions";
                case InteractionResultCode.NotAuthorized:
                    return "interaction.not_authorized";
                default:
                    return "interaction.rejected";
            }
        }
    }
}
