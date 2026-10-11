using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public sealed class BrakeLeakIncident : IncidentController
    {
        public const string ActionCloseBrakeValve = "close_brake_valve";
        public const string ActionRechargeBrakes = "recharge_brakes";
        public const string ActionPumpBrakes = "pump_brakes";

        public float ActivateAfter = 0f;
        public float LeakLoad = 0.5f;
        public float SymptomPressure = 0.6f;
        public float EscalatePressure = 0.35f;
        public float FailPressure = 0.1f;
        public float StabilizePressure = 0.5f;
        public float StableHoldSeconds = 2f;
        public float RechargeAmount = 0.2f;
        public float PumpAmount = 0.05f;

        private static readonly string[] Actions =
        {
            ActionCloseBrakeValve,
            ActionRechargeBrakes,
            ActionPumpBrakes
        };

        private bool _clueLogged;
        private bool _hazardApplied;
        private float _stableTime;

        public BrakeLeakIncident(TrainSystemModel model, MissionEventLog log, float activateAfter = 0f)
            : base("brakeleak", IncidentKind.BrakeLeak, TrainSubsystem.Brakes, model, log)
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
            Model.Apply(new SubsystemEffect(TrainSubsystem.Brakes, loadDelta: LeakLoad));
            SetPhase(IncidentPhase.Active);
            LogBegan("cause.brake_pressure_leak");
        }

        protected override void OnTick(float dt, float missionTime)
        {
            if (IsTerminal)
            {
                return;
            }

            var brakes = Model.Get(TrainSubsystem.Brakes);
            float pressure = brakes.Pressure;
            SetSeverity(1f - pressure);

            if (!_clueLogged && pressure <= SymptomPressure)
            {
                _clueLogged = true;
                LogClue("clue.brake_soft");
            }

            if (!_hazardApplied && pressure <= EscalatePressure && Phase == IncidentPhase.Active)
            {
                _hazardApplied = true;
                SetPhase(IncidentPhase.Escalating);
                RaiseChanged();
            }

            if (pressure <= FailPressure)
            {
                Model.Apply(new SubsystemEffect(TrainSubsystem.Brakes, pressureDelta: -1f, loadDelta: -1f));
                SetPhase(IncidentPhase.Failed);
                RaiseChanged();
                return;
            }

            bool valveClosed = brakes.Load <= 0.0001f;
            if (valveClosed && pressure >= StabilizePressure)
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
                case ActionCloseBrakeValve:
                    var current = Model.Get(TrainSubsystem.Brakes);
                    if (current.Load <= 0.0001f)
                    {
                        return InteractionResult.Rejected(
                            InteractionResultCode.PreconditionsNotMet, "brakeleak.valve_already_closed", Model.Version);
                    }

                    Model.Apply(new SubsystemEffect(TrainSubsystem.Brakes, loadDelta: -current.Load));
                    return InteractionResult.Ok("brakeleak.valve_closed", Model.Version);

                case ActionRechargeBrakes:
                    Model.Apply(new SubsystemEffect(TrainSubsystem.Brakes, pressureDelta: RechargeAmount));
                    return InteractionResult.Ok("brakeleak.brakes_recharged", Model.Version);

                case ActionPumpBrakes:
                    Model.Apply(new SubsystemEffect(
                        TrainSubsystem.Brakes, pressureDelta: PumpAmount, loadDelta: -0.25f));
                    return InteractionResult.Ok("brakeleak.brakes_pumped", Model.Version);

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
