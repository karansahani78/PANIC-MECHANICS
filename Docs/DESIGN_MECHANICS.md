# PANIC MECHANICS — Implemented Mechanics (as coded)

This documents the **currently implemented** pure-C# mechanics under
`Assets/_Game/Scripts/Gameplay/`, which are covered by the 57 passing tests in
`Tools/SimVerify`. It is descriptive, not aspirational — tune by editing the
numbers here and in the configs.

## Train system model (`TrainSystemModel`)

Six subsystems: Engine, Electrical, Cooling, Brakes, Track, Communication.
Each carries normalized (0..1) `Health`, `Temperature`, `Pressure`, `Load`, plus
`Enabled`/`Powered` flags and a monotonic `Version` (bumped on every change).

Initial state:

- Engine: health 1, temperature 0.25, load 0.35, enabled, powered
- Electrical: health 1, load 0.40, enabled, powered
- Cooling: health 1, load 0.50, **disabled**, powered
- Brakes: health 1, pressure 1.0
- Track / Communication: health 1

Per tick (dt):

```
pumpRunning  = electrical.Powered && cooling.Enabled && cooling.Health > 0.1
coolingFlow  = pumpRunning ? cooling.Health : 0
heat         = engine.Load * HeatGainPerSecond          (0.06)
coolingPower = coolingFlow * cooling.Load * CoolRatePerSecond  (0.08)
temperature += (heat - coolingPower) * dt               (clamped 0..1)
brakeLeak    = brakes.Load * BrakeLeakPerSecond         (0.03)
pressure    -= brakeLeak * dt                           (clamped 0..1)
```

Key coupling: cutting `Electrical.Powered` (power fault) or `Cooling.Enabled`
(closed valve) stops cooling, so the engine heats up.

## Incidents

All incidents share `IncidentController`: phases Dormant → Active → Escalating →
Stabilized → Resolved/Failed; they log `MissionEvent`s, track attempts and clues,
and validate repairs. Max interaction distance is **3.0** units.

### Overheat (`overheat`, Engine)

- Activates: sets `Cooling.Enabled = false`; cause `cause.coolant_valve_closed`.
- Clue at temp ≥ 0.55 (`clue.engine_hot`).
- Escalates at temp ≥ 0.70 (−0.05 health to Engine and Cooling).
- Fails at temp ≥ 0.98 (Engine health → 0).
- Actions: `open_cooling_valve`, `restore_pump_power`, `reduce_engine_load`.
- Resolves after temperature ≤ 0.30 held for 2s with the valve open.

### Power fault (`powerfault`, Electrical)

- Activates: sets `Electrical.Powered = false`; cause `cause.breaker_overload`.
- Clue immediately (`clue.power_fluctuation`).
- Escalates after 12s unresolved (−0.05 Electrical health).
- Fails after 35s unresolved (Electrical health → 0).
- Actions: `reset_breaker` (rejected while `Engine.Load > 0.30` —
  `powerfault.load_too_high`), `reroute_power`, `reduce_engine_load`.
- Resolves after power restored and held for 2s.

### Brake leak (`brakeleak`, Brakes)

- Activates: adds +0.5 Brakes load so pressure leaks each tick; cause
  `cause.brake_pressure_leak`.
- Clue at pressure ≤ 0.60 (`clue.brake_soft`).
- Escalates at pressure ≤ 0.35.
- Fails at pressure ≤ 0.10 (pressure and load → 0).
- Actions: `close_brake_valve` (sets load to 0), `recharge_brakes` (+0.2
  pressure), `pump_brakes` (+0.05 pressure, −0.25 load).
- Resolves after the valve is closed and pressure ≥ 0.50 held for 2s.

## Mission flow (`MissionDirector`)

Phases: Boot → Lobby → Countdown → Running → Debrief. On each Running tick it
ticks the model and all incidents, then evaluates outcome:

- Any incident `Failed` → mission **Failure**.
- Brakes pressure ≤ 0 → mission **Failure**.
- Elapsed ≥ target duration → mission **Success**.

`EndMission` builds a `MissionResult` (outcome, duration, resolved/failed
incidents, score, debrief summary lines).

## Difficulty (`DifficultyScaler`)

By player count (clamped 2–4):

```
heatMultiplier        = 0.90 + 0.10 * (players - 2)
timePressureMultiplier= 0.85 + 0.075 * (players - 2)
targetDurationSeconds = 300 + 30 * (players - 2)
```

`MissionBuilder` multiplies the model's `HeatGainPerSecond` by
`heatMultiplier * DifficultyScale` and `BrakeLeakPerSecond` by
`timePressureMultiplier * DifficultyScale`.

## Scoring (`MissionScorer`)

- +100 per resolved incident, −150 per failed incident.
- +200 success / +100 partial bonus.
- Time bonus = `round((target - duration) * 2)` on success.

Debrief rating: S (success, score ≥ 300), A (success), B (partial), C otherwise.

## Roles (`RoleInformationService`, `RoleAssignmentService`)

Diagnostic visibility by role:

- Engineer → Engine, Cooling
- Electrician → Electrical
- Driver → Brakes, Track
- Dispatcher → all subsystems

`RoleAssignmentService.Assign` gives 2–4 clients distinct roles in
Engineer/Electrician/Driver/Dispatcher order; more clients than roles throws.

**Authoritative binding (`NetworkGameManager`, host only):** roles are assigned by
the host in join order (host → Engineer, first client → Electrician, …) on every
connect/disconnect, using `RoleAssignmentService`. The interaction relay uses the
host-assigned role — clients never send a role, so a client cannot claim a
privileged role. Each `PlayerAvatar._role` (server-writable) replicates the
assigned role to its owner.

**Small advantage, not exclusive access (`RoleExpertise` + `InteractionService`):**
an "expert" role matches the incident it specializes in
(Engineer→overheat, Electrician→powerfault, Driver→brakeleak; Dispatcher is
expert for every target — the fallback role). Expert roles pay a reduced
interaction cooldown (`InteractionService.DefaultCooldownSeconds *
ExpertCooldownMultiplier`; runtime wiring: 0.8s × 0.25 = 0.2s); non-experts pay
the full 0.8s. This enforces a role advantage without making any incident
unsolvable when a role is absent. Unknown targets are permissive for every role.

## Interaction validation (`InteractionService`)

Host-only pipeline for a client action request:

1. Target must exist → else `InvalidTarget`.
2. Phase must equal `RequiredPhase` (Running) → else `WrongPhase`.
3. `Distance ≤ MaxDistance` → else `TooFar`.
4. Per (player, target, action) cooldown → else `Cooldown`. The cooldown is
   scaled by `ExpertCooldownMultiplier` when `IsExpertFor(role, targetId)` is
   true (expert advantage step).
5. Target `CanInteract` precondition → else its reason.
6. Apply, then return the target's `IInteractionResultSource.LastResult` (the
   incident's real feedback key), setting a cooldown only if accepted.

Clients never mutate authoritative state (see `Docs/CONTRACTS.md` §5).
