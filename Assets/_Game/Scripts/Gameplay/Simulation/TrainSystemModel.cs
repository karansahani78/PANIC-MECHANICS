using System;
using System.Collections.Generic;
using PanicMechanics.Core;

namespace PanicMechanics.Gameplay.Simulation
{
    public readonly struct SubsystemEffect
    {
        public readonly TrainSubsystem Subsystem;
        public readonly float HealthDelta;
        public readonly float TemperatureDelta;
        public readonly float PressureDelta;
        public readonly float LoadDelta;
        public readonly bool HasEnabledFlag;
        public readonly bool Enabled;
        public readonly bool HasPoweredFlag;
        public readonly bool Powered;

        public SubsystemEffect(
            TrainSubsystem subsystem,
            float healthDelta = 0f,
            float temperatureDelta = 0f,
            float pressureDelta = 0f,
            float loadDelta = 0f,
            bool? enabled = null,
            bool? powered = null)
        {
            Subsystem = subsystem;
            HealthDelta = healthDelta;
            TemperatureDelta = temperatureDelta;
            PressureDelta = pressureDelta;
            LoadDelta = loadDelta;
            HasEnabledFlag = enabled.HasValue;
            Enabled = enabled ?? false;
            HasPoweredFlag = powered.HasValue;
            Powered = powered ?? false;
        }

        public static SubsystemEffect SetEnabled(TrainSubsystem subsystem, bool enabled)
        {
            return new SubsystemEffect(subsystem, enabled: enabled);
        }

        public static SubsystemEffect SetPowered(TrainSubsystem subsystem, bool powered)
        {
            return new SubsystemEffect(subsystem, powered: powered);
        }
    }

    public sealed class TrainSystemModel
    {
        public event Action<TrainSubsystemState> StateChanged;

        private readonly Dictionary<TrainSubsystem, TrainSubsystemState> _states =
            new Dictionary<TrainSubsystem, TrainSubsystemState>();

        private int _version;

        public float HeatGainPerSecond = 0.06f;
        public float CoolRatePerSecond = 0.08f;
        public float BrakeLeakPerSecond = 0.03f;

        public int Version
        {
            get { return _version; }
        }

        public TrainSystemModel()
        {
            Reset();
        }

        public void Reset()
        {
            _version = 0;
            _states.Clear();
            SetInitial(TrainSubsystem.Engine, health: 1f, temperature: 0.25f, load: 0.35f, enabled: true, powered: true);
            SetInitial(TrainSubsystem.Electrical, health: 1f, load: 0.40f, enabled: true, powered: true);
            SetInitial(TrainSubsystem.Cooling, health: 1f, load: 0.50f, enabled: false, powered: true);
            SetInitial(TrainSubsystem.Brakes, health: 1f, pressure: 1f, load: 0f, enabled: true, powered: true);
            SetInitial(TrainSubsystem.Track, health: 1f, enabled: true, powered: true);
            SetInitial(TrainSubsystem.Communication, health: 1f, enabled: true, powered: true);
        }

        private void SetInitial(
            TrainSubsystem subsystem,
            float health,
            float temperature = 0f,
            float pressure = 0f,
            float load = 0f,
            bool enabled = true,
            bool powered = true)
        {
            _states[subsystem] = new TrainSubsystemState(
                subsystem, health, temperature, pressure, load, enabled, powered, _version);
        }

        public TrainSubsystemState Get(TrainSubsystem subsystem)
        {
            return _states[subsystem];
        }

        public TrainSubsystemState Apply(SubsystemEffect effect)
        {
            var current = _states[effect.Subsystem];
            var next = current.With(
                health: Clamp01(current.Health + effect.HealthDelta),
                temperature: Clamp01(current.Temperature + effect.TemperatureDelta),
                pressure: Clamp01(current.Pressure + effect.PressureDelta),
                load: Clamp01(current.Load + effect.LoadDelta),
                enabled: effect.HasEnabledFlag ? effect.Enabled : (bool?)null,
                powered: effect.HasPoweredFlag ? effect.Powered : (bool?)null);
            Store(next);
            return next;
        }

        public void Tick(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            var engine = _states[TrainSubsystem.Engine];
            var cooling = _states[TrainSubsystem.Cooling];
            var electrical = _states[TrainSubsystem.Electrical];
            var brakes = _states[TrainSubsystem.Brakes];

            bool pumpRunning = electrical.Powered && cooling.Enabled && cooling.Health > 0.1f;
            float coolingFlow = pumpRunning ? cooling.Health : 0f;

            float heat = engine.Load * HeatGainPerSecond;
            float coolingPower = coolingFlow * cooling.Load * CoolRatePerSecond;
            float newTemp = Clamp01(engine.Temperature + (heat - coolingPower) * dt);
            Store(engine.With(temperature: newTemp));

            float leak = brakes.Load * BrakeLeakPerSecond;
            if (leak > 0f)
            {
                Store(brakes.With(pressure: Clamp01(brakes.Pressure - leak * dt)));
            }
        }

        private void Store(TrainSubsystemState state)
        {
            _version++;
            state = state.With(version: _version);
            _states[state.Subsystem] = state;
            var handler = StateChanged;
            if (handler != null)
            {
                handler(state);
            }
        }

        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            return value > 1f ? 1f : value;
        }
    }
}
