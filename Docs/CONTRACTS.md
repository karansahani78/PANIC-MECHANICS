# PANIC MECHANICS — Shared Interface Contracts (FROZEN)

**Status:** Frozen by Agent 0 (Integrator). Version `contracts-v1`.
**Owner:** Integrator only. No agent may edit these types or enums without a
written proposal approved by the integrator.

These types are the single source of truth. Do not create competing versions.
When the Unity project exists they live under `Assets/_Game/Scripts/Core/`
(namespace `PanicMechanics.Core`). Until then this document is authoritative.

---

## 1. Enums (`PanicMechanics.Core`)

```csharp
public enum GamePhase
{
    Boot, MainMenu, Lobby, Countdown, Running, Debrief, Results
}

public enum PlayerRole
{
    Engineer, Electrician, Driver, Dispatcher
}

public enum TrainSubsystem
{
    Engine, Electrical, Cooling, Brakes, Track, Communication
}

public enum IncidentPhase
{
    Dormant, Active, Escalating, Stabilized, Resolved, Failed
}

public enum MissionOutcome
{
    Success, PartialSuccess, Failure, Aborted
}

public enum InteractionResultCode
{
    Accepted, InvalidTarget, TooFar, WrongPhase, InvalidAction,
    PreconditionsNotMet, Cooldown, AlreadyResolved, NotAuthorized
}
```

Rules: use serialized enums or stable strings for authored content IDs. Never use
display names as unique IDs.

---

## 2. Shared models (`PanicMechanics.Core`)

| Type | Kind | Owns |
|---|---|---|
| `MissionDefinition` | ScriptableObject | ID, title, description, target duration, allowed players, incident sequence, modifiers |
| `IncidentDefinition` | ScriptableObject | ID, root cause, subsystem, severity, activation conditions, initial symptoms, escalation steps, repair actions, state effects, debrief templates |
| `IncidentRuntimeState` | struct/class | incident ID, phase, severity, elapsed time, discovered clues, attempts, resolved timestamp |
| `TrainSubsystemState` | struct | subsystem ID, normalized health/pressure/temperature/load, enabled/disabled flags, state version |
| `InteractionRequest` | readonly struct | target network object ID, action ID, optional payload, client sequence number |
| `InteractionResult` | readonly struct | result code, user-readable feedback key, authoritative state version |
| `MissionEvent` | readonly struct | event ID, tick/time, event type, actor ID, incident ID, subsystem ID, payload keys |
| `MissionResult` | class | outcome, duration, resolved/failed incidents, optional objectives, score breakdown, event summary |
| `PlayerRoleProfile` | ScriptableObject | role, diagnostic permissions, info display config, optional action modifiers |

---

## 3. Interfaces (`PanicMechanics.Core`)

```csharp
public interface IGameSessionService
{
    Task<SessionOperationResult> CreatePrivateSessionAsync(int maxPlayers);
    Task<SessionOperationResult> JoinByCodeAsync(string joinCode);
    Task LeaveSessionAsync();
    string GetLocalPlayerId();
    event Action<SessionStatus> StatusChanged;
}

public interface IInteractionTarget
{
    string TargetId { get; }
    bool CanInteract(PlayerInteractionContext context, string actionId,
                     out InteractionResultCode rejectionReason);
    void HandleAcceptedInteraction(PlayerInteractionContext context, string actionId);
}

public interface IMissionService
{
    GamePhase CurrentPhase { get; }
    MissionDefinition CurrentMission { get; }
    event Action<GamePhase> PhaseChanged;
    event Action<MissionResult> MissionEnded;
}

public interface ITrainSystemService
{
    TrainSubsystemState GetSubsystemState(TrainSubsystem subsystem);
    event Action<TrainSubsystemState> SubsystemStateChanged;
    InteractionResult ApplyRepairAction(InteractionRequest request,
                                        PlayerInteractionContext context);
}
```

Supporting types referenced above (`SessionOperationResult`, `SessionStatus`,
`PlayerInteractionContext`) are defined by the integrator in the same folder and
follow the same no-fork rule.

These are **contracts**, not a demand that all logic lives in interface methods.
The integrator may adapt signatures before freezing a `contracts-vN`, but must
document the change and update every dependent agent before merging.

---

## 4. State ownership

| Owner | Owns |
|---|---|
| `MissionDirector` | phase, clock, incident schedule, mission outcome |
| `TrainSystemModel` | canonical state/dependency rules per subsystem |
| `IncidentController` | one incident's runtime progression; requests system changes |
| `InteractionService` | validates and routes action requests |
| `RoleInformationService` | which diagnostics each role can access |
| `MissionEventLog` | records important events for results/debrief |
| `SessionService` | create/leave sessions, exposes session status |
| UI components | render models and submit user intents; own **no** authoritative rules |

---

## 5. Networking contract

1. Client presses interact → local UI shows a **pending** indicator (no local state mutation).
2. Client sends target ID, action ID and small payload over NGO.
3. Host validates session phase, object existence, distance, allowed action, cooldown, incident preconditions.
4. Host applies the action to the canonical model.
5. Host appends a `MissionEvent` and updates replicated state.
6. All clients update animation/audio/UI/incident feedback from the authoritative result.

**Never** allow a client to set health, resolve an incident, adjust mission time,
or award score directly. Keep messages small and derived data local when possible.

---

## 6. Change process

1. Propose the exact signature/behavior change and the reason.
2. Integrator approves and bumps to `contracts-vN`.
3. Integrator updates this document and notifies every dependent agent.
4. Only then may dependent code change.

Any undocumented divergence is a P1 integration defect.
