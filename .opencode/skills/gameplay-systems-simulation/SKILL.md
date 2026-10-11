---
name: gameplay-systems-simulation
description: Use when implementing the PANIC MECHANICS train simulation, MissionDirector, TrainSystemModel, incident runtime, dependency graph and deterministic event logging. Triggers on subsystems, overheating, power fault, brake pressure leak, escalation curves, repair preconditions, mission state, scoring, replay reset, or data-driven incidents.
---

# Gameplay Systems & Simulation (PANIC MECHANICS)

## Goal

A deterministic, data-driven, **plain-C#** simulation. No NGO, no UGS, no UI, no
scene singletons inside the model.

## Owner state

| Component | Owns |
|---|---|
| `MissionDirector` | phase, clock, incident schedule, outcome |
| `TrainSystemModel` | canonical subsystem state + dependency rules |
| `IncidentController` | one incident's runtime progression; requests changes via model |
| `InteractionService` | validates and routes action requests |
| `RoleInformationService` | which diagnostics each role can see |
| `MissionEventLog` | ordered `MissionEvent` records for debrief/scoring |

Incidents **never** mutate unrelated objects directly. They request validated
state changes through `TrainSystemModel` / `MissionDirector`.

## Subsystems & dependency chain

`TrainSubsystem`: Engine, Electrical, Cooling, Brakes, Track, Communication.

Example overheating chain (the reference model):
1. Pump power drops → 2. coolant flow decreases → 3. engine temp rises →
4. alarm + steam VFX → 5. over-threshold pressure leaks steam (hazard) →
6. crew restores pump power, opens valve, stabilizes → 7. repeatedly resetting
the breaker without reducing load trips it again.

MVP incidents (must be connected, not three unrelated minigames):
- **Overheat** — closed cooling valve / electrical fault stops pump.
- **Power fault** — overload trips breaker; restoring power before reducing load
  re-trips it.
- **Brake pressure leak** — pressure drops; speeding makes a hazard section
  harder; emergency brake buys time at a cost.

## Data contracts

Define incidents as `IncidentDefinition` ScriptableObjects with: ID, root cause,
subsystem, severity, activation conditions, initial symptoms, escalation steps,
repair actions, state effects, debrief templates. Runtime uses `IncidentRuntimeState`
(ID, phase, severity, elapsed, discovered clues, attempts, resolved timestamp).
`TrainSubsystemState` exposes normalized health/pressure/temperature/load, flags
and a state version.

## Rules

- Use serialized enums/stable string IDs for content IDs; never display names.
- Fixed-timestep deterministic update; derive `MissionResult` from the event log.
- `ApplyRepairAction` returns `InteractionResult` with a stable result code.
- Replaying resets **all** subsystem, incident, timer and score state.
- Difficulty scales by active player count (2/3/4).

## Validation requirements

- EditMode tests: dependency calculations, escalation thresholds, repair
  preconditions, phase transitions, player-count scaling, outcome/score,
  report generation from event log, replay reset, deterministic seed behavior.
- No `UnityEngine` reference in Core/Gameplay rules (ScriptableObject *definitions*
  may use `UnityEngine` types for authoring only).
- Report assumptions and any contract changes before merging.

## Official references

- Blueprint §6 failure model, §9 architecture, §10 data contracts, §16 testing:
  `PANIC_MECHANICS_BLUEPRINT.md`
- ScriptableObject: https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html

## Incompatible implementations to avoid

- Incident scripts that directly mutate many unrelated objects.
- Non-deterministic outcomes, hidden causes, or unscaled incident difficulty.
- Calling networking/UI/scene singletons from core rules.
- Creating competing copies of shared enums/models.
