using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public sealed class OverheatIncident : IncidentController
    {
        public const string ActionOpenCoolingValve = "open_cooling_valve";
        public const string ActionRestorePumpPower = "restore_pump_power";
        public const string ActionReduceEngineLoad = "reduce_engine_load";

        public float ActivateAfter = 0f;
        public float SymptomTemperature = 0.55f;
        public float EscalateTemperature = 0.70f;
        public float FailTemperature = 0.98f;
        public float StabilizeTemperature = 0.30f;
        public float StableHoldSeconds = 2f;
        public float ReduceLoadAmount = 0.15f;

        private static readonly string[] Actions =
        {
            ActionOpenCoolingValve,
            ActionRestorePumpPower,
            ActionReduceEngineLoad
        };

        private bool _symptomLogged;
        private bool _hazardApplied;
        private float _stableTime;

        public OverheatIncident(TrainSystemModel model, MissionEventLog log, float activateAfter = 0f)
            : base("overheat", IncidentKind.Overheat, TrainSubsystem.Engine, model, log)
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
            Model.Apply(SubsystemEffect.SetEnabled(TrainSubsystem.Cooling, false));
            SetPhase(IncidentPhase.Active);
            LogBegan("cause.coolant_valve_closed");
        }

        protected override void OnTick(float dt, float missionTime)
        {
            if (IsTerminal)
            {
                return;
            }

            var engine = Model.Get(TrainSubsystem.Engine);
            float temp = engine.Temperature;
            SetSeverity(temp);

            if (!_symptomLogged && temp >= SymptomTemperature)
            {
                _symptomLogged = true;
                LogClue("clue.engine_hot");
            }

            if (!_hazardApplied && temp >= EscalateTemperature && Phase == IncidentPhase.Active)
            {
                _hazardApplied = true;
                SetPhase(IncidentPhase.Escalating);
                Model.Apply(new SubsystemEffect(TrainSubsystem.Engine, healthDelta: -0.05f));
                Model.Apply(new SubsystemEffect(TrainSubsystem.Cooling, healthDelta: -0.05f));
            }

            if (temp >= FailTemperature)
            {
                Model.Apply(new SubsystemEffect(TrainSubsystem.Engine, healthDelta: -1f));
                SetPhase(IncidentPhase.Failed);
                RaiseChanged();
                return;
            }

            bool pumpRunning = Model.Get(TrainSubsystem.Electrical).Powered &&
                               Model.Get(TrainSubsystem.Cooling).Enabled;
            bool cooling = pumpRunning && temp <= StabilizeTemperature;

            if (cooling)
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
                case ActionOpenCoolingValve:
                    if (Model.Get(TrainSubsystem.Cooling).Enabled)
                    {
                        return InteractionResult.Rejected(
                            InteractionResultCode.PreconditionsNotMet, "overheat.valve_already_open", Model.Version);
                    }
                    Model.Apply(SubsystemEffect.SetEnabled(TrainSubsystem.Cooling, true));
                    return InteractionResult.Ok("overheat.valve_opened", Model.Version);

                case ActionRestorePumpPower:
                    if (Model.Get(TrainSubsystem.Electrical).Powered)
                    {
                        return InteractionResult.Rejected(
                            InteractionResultCode.PreconditionsNotMet, "overheat.pump_already_powered", Model.Version);
                    }
                    Model.Apply(SubsystemEffect.SetPowered(TrainSubsystem.Electrical, true));
                    return InteractionResult.Ok("overheat.pump_restored", Model.Version);

                case ActionReduceEngineLoad:
                    Model.Apply(new SubsystemEffect(TrainSubsystem.Engine, loadDelta: -ReduceLoadAmount));
                    return InteractionResult.Ok("overheat.load_reduced", Model.Version);

                default:
                    return InteractionResult.Rejected(
                        InteractionResultCode.InvalidAction, "interaction.invalid_action", Model.Version);
            }
        }

        public override void Reset()
        {
            base.Reset();
            _symptomLogged = false;
            _hazardApplied = false;
            _stableTime = 0f;
        }
    }
}
