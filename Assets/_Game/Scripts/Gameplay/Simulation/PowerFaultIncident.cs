using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public sealed class PowerFaultIncident : IncidentController
    {
        public const string ActionResetBreaker = "reset_breaker";
        public const string ActionReroutePower = "reroute_power";
        public const string ActionReduceEngineLoad = "reduce_engine_load";

        public float ActivateAfter = 0f;
        public float EscalateAfterSeconds = 12f;
        public float FailAfterSeconds = 35f;
        public float StableHoldSeconds = 2f;
        public float MaxEngineLoadForReset = 0.30f;
        public float ReduceLoadAmount = 0.15f;

        private static readonly string[] Actions =
        {
            ActionResetBreaker,
            ActionReroutePower,
            ActionReduceEngineLoad
        };

        private bool _clueLogged;
        private bool _hazardApplied;
        private float _stableTime;

        public PowerFaultIncident(TrainSystemModel model, MissionEventLog log, float activateAfter = 0f)
            : base("powerfault", IncidentKind.PowerFault, TrainSubsystem.Electrical, model, log)
        {
            ActivateAfter = activateAfter;
        }

        protected override float ActivateAt
        {
            get { return ActivateAfter; }
        }

        public override IEnumerable<string> AvailableActions
        {
            get { return Actions; }
        }

        protected override void OnActivate()
        {
            Model.Apply(SubsystemEffect.SetPowered(TrainSubsystem.Electrical, false));
            SetPhase(IncidentPhase.Active);
            LogBegan("cause.breaker_overload");
        }

        protected override void OnTick(float dt, float missionTime)
        {
            if (IsTerminal)
            {
                return;
            }

            bool powered = Model.Get(TrainSubsystem.Electrical).Powered;
            SetSeverity(powered ? 0.2f : 1f);

            if (!_clueLogged && !powered)
            {
                _clueLogged = true;
                LogClue("clue.power_fluctuation");
            }

            if (!_hazardApplied && Elapsed >= EscalateAfterSeconds && Phase == IncidentPhase.Active)
            {
                _hazardApplied = true;
                SetPhase(IncidentPhase.Escalating);
                Model.Apply(new SubsystemEffect(TrainSubsystem.Electrical, healthDelta: -0.05f));
                RaiseChanged();
            }

            if (powered)
            {
                _stableTime += dt;
                if (Phase != IncidentPhase.Stabilized && Phase != IncidentPhase.Resolved)
                {
                    SetPhase(IncidentPhase.Stabilized);
                    RaiseChanged();
                }

                if (_stableTime >= StableHoldSeconds && Phase != IncidentPhase.Resolved)
                {
                    SetPhase(IncidentPhase.Resolved);
                    RaiseChanged();
                }
            }
            else
            {
                _stableTime = 0f;
                if (Elapsed >= FailAfterSeconds)
                {
                    Model.Apply(new SubsystemEffect(TrainSubsystem.Electrical, healthDelta: -1f));
                    SetPhase(IncidentPhase.Failed);
                    RaiseChanged();
                }
            }
        }

        protected override bool IsActionAvailable(string actionId)
        {
            for (int i = 0; i < Actions.Length; i++)
            {
                if (Actions[i] == actionId)
                {
                    return true;
                }
            }

            return false;
        }

        protected override InteractionResult ApplyAction(string actionId, PlayerInteractionContext context)
        {
            switch (actionId)
            {
                case ActionResetBreaker:
                    if (Model.Get(TrainSubsystem.Electrical).Powered)
                    {
                        return InteractionResult.Rejected(
                            InteractionResultCode.PreconditionsNotMet, "powerfault.already_powered", Model.Version);
                    }

                    if (Model.Get(TrainSubsystem.Engine).Load > MaxEngineLoadForReset)
                    {
                        return InteractionResult.Rejected(
                            InteractionResultCode.PreconditionsNotMet, "powerfault.load_too_high", Model.Version);
                    }

                    Model.Apply(SubsystemEffect.SetPowered(TrainSubsystem.Electrical, true));
                    return InteractionResult.Ok("powerfault.breaker_reset", Model.Version);

                case ActionReroutePower:
                    Model.Apply(SubsystemEffect.SetPowered(TrainSubsystem.Electrical, true));
                    return InteractionResult.Ok("powerfault.power_rerouted", Model.Version);

                case ActionReduceEngineLoad:
                    Model.Apply(new SubsystemEffect(TrainSubsystem.Engine, loadDelta: -ReduceLoadAmount));
                    return InteractionResult.Ok("powerfault.load_reduced", Model.Version);

                default:
                    return InteractionResult.Rejected(
                        InteractionResultCode.InvalidAction, "interaction.invalid_action", Model.Version);
            }
        }

        public override void Reset()
        {
            base.Reset();
            _clueLogged = false;
            _hazardApplied = false;
            _stableTime = 0f;
        }
    }
}
