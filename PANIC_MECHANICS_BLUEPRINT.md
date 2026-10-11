# PANIC MECHANICS — Agent-Ready Game Blueprint

**Document type:** Product brief + game design document + technical architecture + parallel-agent execution plan  
**Status:** Initial production blueprint; validate through playtests before expanding scope  
**Working title:** PANIC MECHANICS  
**Subtitle used in this document:** The Last Train  
**Target platform:** Windows PC first; Steam distribution target  
**Players:** 2–4 players online co-op  
**Engine/language:** Unity 6 LTS + C#  
**Networking direction:** Netcode for GameObjects (NGO), host-authoritative gameplay, Unity Multiplayer Services sessions/Relay for the first playable online prototype  
**Genre:** Cooperative chaos / task prioritization / communication / physics comedy  

> The goal is not to build a huge game first. The goal is to build one short session that makes a group laugh, communicate, learn one mechanic, and voluntarily ask for another round.

---

## 1. Executive summary

PANIC MECHANICS is a 2–4 player cooperative game about keeping a malfunctioning train moving while interconnected systems fail. Every player gets a different perspective on the situation and has different information, but nobody is allowed to solve the entire emergency alone. Players must communicate, diagnose root causes, prioritize repairs, and deal with funny physical consequences when they make mistakes.

The core hook is:

**Four players. Four different views of one disaster. Nobody knows enough alone.**

The train, tools, panels and physics create the comedy. The systems model creates depth. Short missions, team improvement, shareable incident reports, cosmetics and eventually a daily challenge create reasons to return.

### Product pillars

1. **Cooperative chaos:** players need each other, but failure should be funny rather than punishing.
2. **Readable cause and effect:** the player can understand why a system failed, even if they did not predict it.
3. **Asymmetric information:** each role sees different diagnostics; no role is a mandatory bottleneck in small groups.
4. **Short sessions:** first vertical slice should be playable in roughly 5–7 minutes; future full missions can run 8–12 minutes.
5. **Learning through play:** players learn systems thinking, troubleshooting, communication, prioritization and experimentation through feedback—not through classroom quizzes.
6. **Socially shareable outcomes:** each round produces a funny story and a compact incident report.
7. **Respectful retention:** daily challenges and progression invite players back without punishing them for missing a day.

### Main success criterion

A new group can start a mission with minimal explanation, understand the objective within one minute, experience at least one funny failure, complete or fail the mission fairly, understand one thing they learned, and voluntarily replay.

---

## 2. Audience and product positioning

### Primary audience

- Friends who play co-op games together on PC.
- Players who enjoy communication, silly mistakes, fast team decisions and physics comedy.
- Groups of 2–4 who want a session that does not require a 30-minute commitment.
- Streamers and creators looking for unexpected co-op moments.

### Secondary audience

- Couples and small friend groups.
- Players who enjoy puzzle/problem-solving co-op games.
- Players who prefer teamwork over competitive PvP.

### Positioning statement

“PANIC MECHANICS is the co-op train repair game where each player sees a different part of the disaster. Diagnose the fault, argue about the fix, and try to keep the train alive.”

### Non-goals for the first release

- No PvP mode.
- No open world.
- No large campaign.
- No user-generated level editor.
- No procedural infinite train generation.
- No complex economy or paid power-ups.
- No real-world engineering or safety-training claims.
- No built-in voice-chat requirement for the first prototype; playtests can use an external voice app. Add in-game voice only after validating the loop and moderation requirements.
- No daily streak punishment or limited-time access to core abilities.
- No dedicated backend beyond what the initial session system and local settings require.

---

## 3. Core gameplay loop

### Moment-to-moment loop

1. Receive a clear emergency cue.
2. Each player inspects their role-specific information and the shared physical space.
3. Players communicate observations and form a diagnosis.
4. The team decides which problem to solve first.
5. Players perform interactions: flip breakers, open/close valves, replace fuses, cool a component, set brake pressure, or operate the train controls.
6. The train model recalculates system state.
7. Success reduces danger; a bad action can create another symptom or escalate a different system.
8. The crew sees immediate, readable feedback and continues.

### Round loop

1. **Briefing (20–30 seconds):** show the destination, mission rule and one visible objective.
2. **Active mission (4–6 minutes):** 2–3 major incidents, connected system effects, time pressure and a few optional bonus tasks.
3. **Outcome (15–30 seconds):** survive, arrive, or fail in a way the player understands.
4. **Incident report (30–45 seconds):** show top cause, best recovery, funny mistake, team rating and one short lesson.
5. **Rematch/leave (immediate):** clear choice—play another mission, replay with a modifier, or return to lobby.

### Fail-forward rule

A mistake should usually make the mission harder or funnier rather than instantly ending it. Reserve instant failure for clearly communicated catastrophic states. Players should be able to recover from most errors through teamwork.

---

## 4. Role design

Roles provide privileged information and a small advantage, not exclusive access to every critical action. This prevents a two-player team from becoming impossible because one specific role is missing.

### Engineer

- Sees engine temperature, pressure and mechanical symptoms.
- Can identify mechanical causes more quickly.
- Can interact with engine, cooling and mechanical repair stations.
- Learns the difference between symptoms and root causes.

### Electrician

- Sees power routing, fuse health, load and voltage warnings.
- Can quickly diagnose circuit faults and reset electrical components.
- Can interact with breakers, fuse panels and power relays.
- Learns dependency chains: powering one system can affect another.

### Driver

- Controls speed, braking and route/track decisions.
- Sees forward hazards and train handling indicators.
- Can make temporary control decisions that buy time but may not fix root causes.
- Learns prioritization under time pressure.

### Dispatcher

- Sees warning summaries, partial logs, route notes and cross-system clues.
- Can mark a suspected subsystem, recommend an order of operations and trigger a limited diagnostic scan.
- Cannot directly repair all systems.
- Learns synthesis and communication.

### Two-player and three-player scaling

- In a 2-player session, each player receives two information panels and repair interactions are shared.
- In a 3-player session, one player receives the Dispatcher summary as a lightweight shared panel rather than a mandatory separate role.
- In a 4-player session, all roles are available.
- Role assignment must be editable in the lobby; allow duplicate or blended roles if needed.
- The mission director scales incident overlap, timing and required actions based on active player count.
- Never create a required task that only one specific role can perform unless a fallback route exists.

---

## 5. First vertical slice scope

### One environment

**Engine Car 01:** a compact, stylized 3D train compartment containing:

- Central engine module.
- Electrical breaker panel.
- Cooling valve and coolant reservoir.
- Brake pressure control.
- Driver console with speed/brake levers.
- Warning lights and gauges.
- A small tool rack.
- Loose physical props used for comedy.
- One window showing the moving environment, hazards and progress.

### One mission

**Mission name:** The Last Express  
**Objective:** Keep the train running until it reaches the safe station.  
**Target duration:** 5–7 minutes for the first vertical slice.  
**Players:** 2–4.  
**Primary incidents:**

1. **Overheat:** engine temperature rises. Cooling flow is reduced by a closed valve; an electrical fault may prevent the pump from operating.
2. **Power fault:** overloaded circuit trips the breaker. Restoring power before reducing load can trip it again.
3. **Brake pressure leak:** brake pressure drops. Driving too fast makes a dangerous section harder to handle; an emergency brake buys time but has a cost.

The three incidents must be connected through a small dependency graph—not just three unrelated minigames.

### MVP acceptance criteria

- Host can create a private session and see a join code.
- Two remote players can join the same session and spawn in the train car.
- A group can complete or fail the mission from start to finish.
- Clients can move, look around and interact with the same physical panels.
- Each role sees distinct diagnostic information.
- The host validates interactions and owns the authoritative train-system state.
- At least three failure types cause understandable, observable effects.
- Mission result is consistent for all clients.
- The incident report is generated from actual mission events—not invented after the fact.
- Replaying the mission resets all relevant state.
- No known P0/P1 defect prevents a complete round.

---

## 6. Design rules for emergencies and comedy

### Failure system model

Use data-driven incidents with explicit:

- Root cause.
- Symptoms.
- Conditions for activation.
- Severity.
- Escalation curve.
- Related systems.
- Repair actions.
- Preconditions and consequences.
- Success/failure event.
- Debrief facts.

Avoid writing incidents as independent scripts that directly manipulate many unrelated objects. Incident logic should make requests through the TrainSystemModel/MissionDirector, which applies validated state changes.

### Example dependency chain: overheating

1. Pump power drops.
2. Coolant flow decreases.
3. Engine temperature rises.
4. A warning alarm and visual steam effect become active.
5. If pressure exceeds a threshold, a nearby pipe begins leaking steam and creates a temporary visibility/physics hazard.
6. The crew can restore pump power, open the coolant valve and stabilize temperature.
7. If someone repeatedly resets the breaker without reducing the load, the breaker trips again.

### Comedy principles

- The cause of an accident should be readable after the event.
- Use physical timing, sounds, animation and consequences—not random noise alone.
- A serious system failure can have a ridiculous presentation, but the player must still understand what changed.
- Use a small number of expressive, reusable reactions rather than hundreds of one-off animations.
- Prefer player-created comedy over scripted cutscenes.
- Failure should create a story the team can retell.

### Incident report data

Track event records such as:

- Incident began.
- Symptom discovered.
- Player action attempted.
- System state changed.
- Incident stabilized/escalated.
- Emergency brake used.
- Train reached destination or failed.

From these events, generate the report deterministically. AI-generated text is not required for the MVP; templated humor is easier to control, cheaper and testable.

---

## 7. Retention and learning design

Do not optimize for compulsive play. Optimize for meaningful replay, team mastery and social return.

### Core retention features

**Crew progression**
- Earn soft progression from missions played and objectives completed.
- Unlock cosmetic uniforms, paint, nameplates, emotes, tool skins, train decorations and victory poses.
- No paid power, required grind or stat upgrades that create unfairness.

**Incident report**
- Best repair decision.
- Most avoidable mistake.
- Funniest chain reaction.
- Team survival rating.
- One short explanation of a dependency discovered.
- Shareable image/card later; for MVP, a structured results screen is enough.

**Daily Disaster (post-MVP)**
- A shared scenario seed or fixed daily configuration.
- Everybody can attempt it; missing a day does not remove progress.
- Cosmetic or profile rewards only.
- No core content locked behind consecutive logins.
- Daily global leaderboard should not be trusted for high-stakes competition while using player-hosted simulation.

**Weekly challenge (post-MVP)**
- More complex connected faults.
- A modifier that changes decisions, not only difficulty.
- Crew best score and time.

**Social loop**
- Invite code and private lobby first.
- Rejoin last crew later.
- Rematch option at mission end.
- Shareable incident report later.

### Learning design

The game should reinforce:
- Systems thinking: changing one subsystem can affect another.
- Root-cause analysis: fix the cause, not just the symptom.
- Communication: report observations instead of only issuing commands.
- Prioritization: decide which failure is most dangerous.
- Experimentation: test a hypothesis and read the outcome.

After a mission, provide one optional prompt: “What caused the failure?”, “Which action bought the most time?” or “What should the crew do differently?” Keep debriefs short and grounded in the event log. Do not claim that players have earned real engineering qualifications.

---

## 8. Technology and network architecture

### Proposed stack

- Unity 6 LTS.
- C#.
- Universal Render Pipeline (URP), low-poly/stylized art.
- Netcode for GameObjects (NGO) for networked GameObject gameplay.
- Unity Multiplayer Services SDK for sessions and Relay in the first internet-playable prototype.
- Unity Input System.
- Unity Test Framework.
- TextMeshPro or the currently recommended Unity text UI package for HUD text.
- ScriptableObjects for authored mission and incident definitions.
- Git + Git LFS for large binary assets as needed.

**Version rule:** At project bootstrap, choose one Unity Editor patch and one compatible package set, commit `Packages/manifest.json` and `Packages/packages-lock.json`, and do not let parallel agents upgrade packages independently. The NGO repository listed both 2.x and 3.x lines in October 2026. Prefer a conservative, tested package combination over adopting a major networking-package line during feature development. Verify the chosen Editor/package compatibility and build a two-client smoke test before feature work begins.

Unity Multiplayer Services now unifies session management across the functionality previously exposed through Lobby, Relay and Matchmaker SDKs. Use the current unified SDK rather than starting a new project on deprecated standalone service SDKs. Official docs: https://docs.unity.com/en-us/mps-sdk/sessions and https://docs.unity.com/en-us/mps-sdk/get-started

### Network topology

Use a listen-host authoritative model for the first casual co-op prototype:

- Host owns mission state, fault states, timers, train movement state, scoring and incident outcomes.
- Client sends interaction requests, not direct state mutations.
- Host validates request, player distance, interaction target, current state, preconditions and action cooldown.
- Host applies the change, then replicates the resulting state/event to all clients.
- Networked physical props should have a clear authority rule; avoid client-driven critical physics state in the MVP.

### Why not a dedicated server yet?

The game has 2–4 players and no PvP economy. A player-hosted session reduces early operational complexity. The trade-off is host advantage, host disconnect handling and weaker leaderboard trust. If public ranked competition or host-independent persistence becomes important, reassess dedicated servers or server-authoritative hosting later.

### Session abstraction

Gameplay code must not call Unity lobby/relay APIs directly. Define an `IGameSessionService` adapter so UI and game flow can use:

- `CreatePrivateSessionAsync(maxPlayers)`
- `JoinByCodeAsync(code)`
- `LeaveSessionAsync()`
- `GetLocalPlayerId()`
- Session status/connect/disconnect events

The Unity Multiplayer Services adapter implements the interface. A future Steam integration can add Steam invites or a platform-specific provider without rewriting game simulation. Do not build Steamworks integration in the first code sprint unless Steam invitation flow is required for the first external playtest.

### Voice chat

- MVP: external voice app during playtests; provide a clear in-game onboarding instruction.
- Post-validation: investigate proximity voice or walkie-talkie-style voice, including mute/report options and player safety. Do not make a voice-off daily challenge until in-game voice exists or the rules can account for external voice.

### Host disconnect policy

For the MVP, if the host leaves or crashes, show a clear message and return the remaining clients to the lobby/menu. Do not attempt host migration until the basic round is stable.

---

## 9. Code architecture and module boundaries

### Guiding principle

Keep gameplay rules testable in plain C# classes, keep MonoBehaviours focused on Unity lifecycle/input/presentation, and keep network transport calls behind adapters. Avoid a giant `GameManager` class.

### Suggested repository structure

```text
PanicMechanics/
├── Assets/
│   ├── _Game/
│   │   ├── Art/
│   │   │   ├── Characters/
│   │   │   ├── Environment/
│   │   │   ├── Props/
│   │   │   └── VFX/
│   │   ├── Audio/
│   │   │   ├── Music/
│   │   │   ├── SFX/
│   │   │   └── Mixers/
│   │   ├── Data/
│   │   │   ├── Incidents/
│   │   │   ├── Missions/
│   │   │   └── Roles/
│   │   ├── Prefabs/
│   │   │   ├── Characters/
│   │   │   ├── Interactables/
│   │   │   ├── Train/
│   │   │   └── Network/
│   │   ├── Scenes/
│   │   │   ├── 00_Bootstrap.unity
│   │   │   ├── 01_MainMenu.unity
│   │   │   ├── 02_Lobby.unity
│   │   │   ├── 10_EngineCarPrototype.unity
│   │   │   └── 90_Results.unity
│   │   ├── Scripts/
│   │   │   ├── Core/
│   │   │   ├── Gameplay/
│   │   │   │   ├── Incidents/
│   │   │   │   ├── TrainSystems/
│   │   │   │   ├── Mission/
│   │   │   │   └── Scoring/
│   │   │   ├── Player/
│   │   │   ├── Interaction/
│   │   │   ├── Networking/
│   │   │   ├── UI/
│   │   │   ├── Audio/
│   │   │   └── Persistence/
│   │   ├── Settings/
│   │   └── Tests/
│   │       ├── EditMode/
│   │       └── PlayMode/
│   └── ...
├── Packages/
│   ├── manifest.json
│   └── packages-lock.json
├── ProjectSettings/
├── .gitattributes
├── .gitignore
├── CONTRIBUTING.md
├── README.md
└── Docs/
    ├── GAME_DESIGN.md
    ├── ARCHITECTURE.md
    ├── NETWORKING.md
    └── AGENT_TASKS.md
```

Use Unity `.meta` files from Unity; do not generate or remove them manually. Avoid moving/renaming imported assets after other agents have referenced them.

### Assembly definitions

Create assembly definitions only after the initial package smoke test. Suggested assemblies:
- `PanicMechanics.Core`
- `PanicMechanics.Gameplay`
- `PanicMechanics.Player`
- `PanicMechanics.Networking`
- `PanicMechanics.UI`
- `PanicMechanics.Tests`

Do not create unnecessary asmdefs if they cause package-reference churn during the first sprint; the integrator owns the final decision.

---

## 10. Core data contracts

Define these types early and treat them as shared contracts. Other agents must not create competing versions of these classes.

### Enums

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

### Shared models

- `MissionDefinition` (ScriptableObject): ID, title, description, target duration, allowed players, incident sequence, optional modifiers.
- `IncidentDefinition` (ScriptableObject): ID, root cause, subsystem, severity, activation conditions, initial symptoms, escalation steps, repair actions, state effects and debrief templates.
- `IncidentRuntimeState`: incident ID, phase, severity, elapsed time, discovered clues, attempts, resolved timestamp.
- `TrainSubsystemState`: subsystem ID, normalized health/pressure/temperature/load as applicable, enabled/disabled flags and latest state version.
- `InteractionRequest`: target network object ID, action ID, optional payload, client sequence number.
- `InteractionResult`: result code, user-readable feedback key, authoritative state version.
- `MissionEvent`: event ID, tick/time, event type, actor ID (if relevant), incident ID, subsystem ID, payload keys.
- `MissionResult`: outcome, duration, resolved incidents, failed incidents, optional objectives, score breakdown and event summary.
- `PlayerRoleProfile`: role, diagnostic permissions, info display configuration and optional action modifiers.

Use serialized enums/IDs or stable strings for authored content IDs. Do not use display names as unique IDs.

### Interfaces

```csharp
public interface IGameSessionService
{
    Task<SessionOperationResult> CreatePrivateSessionAsync(int maxPlayers);
    Task<SessionOperationResult> JoinByCodeAsync(string joinCode);
    Task LeaveSessionAsync();
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

These are intended contracts, not a demand to implement all business logic inside interface methods. The integrator can adapt signatures before freezing them, but must document any change and update dependent agents before merging.

### State ownership

- `MissionDirector`: phase, clock, incident schedule and mission outcome.
- `TrainSystemModel`: canonical state/dependency rules for each subsystem.
- `IncidentController`: manages one incident's runtime progression and requests system changes.
- `InteractionService`: validates and routes action requests.
- `RoleInformationService`: decides which diagnostics are available to each role.
- `MissionEventLog`: records important events for results/debrief.
- `SessionService`: creates/leaves sessions and exposes session status.
- UI components: render models and submit user intents; they do not own authoritative gameplay rules.

---

## 11. Networking contract

### Required flow

1. Client presses interact.
2. Local UI shows a pending indicator but does not mutate authoritative state.
3. Client sends target ID, action ID and any small action payload through NGO.
4. Host validates session phase, object existence, distance, allowed action, cooldown and incident preconditions.
5. Host applies the action to the canonical model.
6. Host appends a `MissionEvent` and updates replicated state.
7. All clients update animation, audio, UI and incident feedback from the authoritative result.

### Rules

- Never allow a client to set `health = 100`, resolve an incident, adjust mission time or award score directly.
- Don't replicate data that can be derived locally unless consistency requires it.
- Keep network messages small; do not send full diagnostic logs every frame.
- Use server/host authority checks inside every critical RPC handler.
- Validate action rate and distance server-side.
- Avoid network-spawning trivial particles; play cosmetic effects locally in response to authoritative events where possible.
- Give each interactable a stable ID and clear action list.
- If `NetworkVariable` or RPC APIs change across the selected NGO version, the networking agent must confirm against that exact package version.

### Reconnect and disconnection behavior

MVP:
- A client disconnecting is removed from the active crew; mission continues with difficulty adjustment if practical.
- Host disconnect ends that run cleanly; remaining clients see a message and return to menu.
- All clients see a consistent result if the host completes the mission.
- Retry and return-to-lobby flows must not leave stale networked objects alive.

Post-MVP:
- Steam friends invite integration.
- Reconnect flow.
- Host migration or dedicated server investigation.
- In-game voice chat.

---

## 12. Scene and UI specification

### Scene 00 — Bootstrap

Responsibilities:
- Initialize services and input.
- Initialize logging/configuration.
- Load main menu safely even if optional services are offline.
- Own persistent service objects; prevent duplicated persistent managers.

### Scene 01 — Main menu

Buttons:
- Play with Friends.
- Join by Code.
- Settings.
- Credits.
- Exit.

Rules:
- Session connection errors show an actionable message and retry option.
- Main menu must be usable with mouse/keyboard; controller navigation is later scope.

### Scene 02 — Lobby

Show:
- Four player slots.
- Player names/status.
- Role assignment.
- Ready/not-ready state.
- Copy join code/invite code.
- Mission selection (one mission in MVP).
- Start button visible to host only.
- Clear connection and loading states.

### Scene 10 — Engine car prototype

HUD:
- Mission timer/progress.
- Current emergency banner.
- Context interaction prompt.
- Interaction status and error feedback.
- Role-specific diagnostic panel.
- Crew status and ping indicator.
- Pause/settings menu.

World cues:
- Lighting changes, alarm tones, steam, sparks, gauge movement and readable warning labels.
- Use color plus icon/shape/text to communicate status; do not rely on color alone.

### Scene 90 — Results/debrief

Show:
- Result/outcome.
- Time survived/arrival time.
- Incidents resolved vs unresolved.
- Major root cause, based on event log.
- Best recovery and costly mistake.
- One learning point.
- Rematch, change role, return to lobby.

### Accessibility baseline

- Adjustable master/music/SFX volume.
- Subtitles for authored voice lines and critical audio cues.
- Remappable keys if feasible; at minimum separate interaction, movement and tool controls.
- UI scale option.
- Status indicators use icons/text in addition to color.
- Camera motion and screen shake are adjustable.

---

## 13. Input and interaction rules

Keyboard/mouse defaults (adjustable later):
- WASD: move.
- Mouse: look.
- E: interact/confirm context action.
- Q: ping/mark object.
- Tab: show crew/role information panel.
- Escape: pause/settings.
- Number keys or wheel: select tool if a tool-selection system is enabled.

Interaction requirements:
- A subtle highlight indicates a valid target.
- The prompt names the action (e.g. “Hold E: Reset Breaker”).
- Invalid actions explain why (e.g. “Reduce electrical load first”).
- Interactions that take time show clear progress and can be interrupted consistently.
- Every panel interaction should produce both state feedback and a sound/animation cue.

Player controller requirements:
- Stable movement on moving train floor.
- No camera shaking from replicated physics noise.
- Fixed camera sensitivity settings.
- Player cannot leave the play space without a clear invisible barrier or recover mechanic.
- Add a simple return-to-safe-position command during development to recover from physics traps.

---

## 14. Art, animation and audio direction

### Visual direction

- Stylized, low-poly, colorful industrial train.
- Strong silhouettes and readable control panels.
- Warm, slightly exaggerated animation; avoid realism that makes hazards hard to read.
- Moderate detail budget; no unnecessary high-resolution assets for the first prototype.
- Use modular panels and props so new failures can reuse the same assets.

### Required placeholder art

- One train interior modular kit.
- One simple character with four color-coded role accessories.
- 6–10 interactable props: breaker, valve, gauge, fuse, lever, tool box, pressure pipe, coolant reservoir, warning light, control console.
- 3 fault VFX: steam, electrical spark, brake smoke/pressure vent.
- 3 environment signs/hazards visible through the train window.

### Audio priorities

1. Warning alarms that remain distinguishable.
2. Physical interaction sounds: lever, breaker, valve, tool, metal clank.
3. State change sounds: pump starts, pressure stabilizes, electrical trip.
4. Character exertion/reaction sounds—short, sparse, not intrusive.
5. Background music that grows with danger but never masks essential cues.

### Audio rule

Critical gameplay states must have a visual equivalent so players who cannot hear a cue can still respond.

---

## 15. Save data and progression

MVP local settings:
- Audio volumes.
- Mouse sensitivity.
- UI scale.
- Accessibility toggles.
- Tutorial tips completed.

MVP progression can be ephemeral during the prototype. Persist cosmetics/preferences only once a repeatable mission loop is proven. Avoid designing a database/backend before the retention loop is validated.

Later persistence:
- Player profile and unlocked cosmetics.
- Crew best results.
- Daily/weekly challenge history.
- Steam Cloud or a small service, depending on final architecture.

Do not trust player-hosted client data for a competitive global leaderboard. Either label results as community/friends scores or move ranked validation to server-authoritative infrastructure.

---

## 16. Testing and quality gates

### EditMode tests

- Train system dependency calculations.
- Fault escalation thresholds.
- Repair precondition validation.
- Mission phase transitions.
- Difficulty scaling for 2/3/4 players.
- Mission outcome and score calculation.
- Incident report generation from event logs.
- Deterministic mission seed behavior where applicable.

### PlayMode tests

- Start mission, trigger incident, repair, resolve.
- Repeat mission reset.
- Player movement and interaction range.
- HUD reacts to state changes.
- Two networked clients see the same incident state.
- Invalid client interaction is rejected by host.
- Disconnect and host shutdown do not freeze the menu.

### Manual multiplayer QA

Use two separate builds/devices whenever possible. Unity's Multiplayer Play Mode can be used for local multi-player simulation, but final online validation needs a real remote connection and network variability.

Test cases:
- 2, 3 and 4 players.
- High latency and packet loss simulation.
- Host leaves while in lobby.
- Host leaves during mission.
- Client leaves during an interaction.
- Join code is invalid/expired.
- Session service unavailable.
- A client spams interaction requests.
- Two clients interact with the same panel simultaneously.
- Two clients try to repair the same incident at the same time.
- Mission ends while a client is joining.
- Replaying does not preserve old faults/timers.

### Performance targets for first playable slice

- Stable 60 FPS target on a mid-range PC at prototype quality; allow reduced target hardware to be finalized after measurement.
- Avoid per-frame allocations in core gameplay loops.
- No unnecessary network updates for static props.
- Load into a mission quickly enough that the lobby does not feel like a waiting room.
- Profile before optimizing; do not prematurely build a custom ECS architecture.

### Severity rules

- **P0:** crash, corrupted project, exploit that prevents the game from working, data loss affecting saves.
- **P1:** cannot join/finish a mission; critical multiplayer desync; repeated softlock.
- **P2:** broken cosmetic/UI behavior, rare non-blocking issue.
- **P3:** polish request.

Do not mark a phase done while a known P0/P1 blocks its acceptance criteria.

---

## 17. Parallel coding-agent operating model

### Non-negotiable integration rules

1. **One integration owner** controls the main branch, package versions, project settings, scene/build settings, assembly definitions and shared contracts.
2. Each agent owns an isolated folder and preferably a separate branch/worktree.
3. Agents must not edit the same scene or prefab simultaneously.
4. No agent may change a public interface or shared enum silently. Propose the change to the integrator first.
5. Every agent must return: files changed, APIs added, setup steps, tests run, known limitations and integration notes.
6. No agent may upgrade Unity packages or edit `Packages/manifest.json` without integrator approval.
7. Avoid large binary assets in the first pass; use primitives/placeholders and let the integrator create final prefab wiring.
8. Merge in dependency order, not whenever an agent finishes.
9. After each merge, open Unity, let imports complete, run compilation and the relevant test suite before merging the next feature.
10. A coding agent must not claim a scene works unless it was actually opened/tested or clearly identify the missing verification.

### Workstream map

| Agent | Ownership | Must not own | Deliverable |
|---|---|---|---|
| Agent 0: Integrator/Tech Lead | Unity project, packages, shared contracts, scene/build settings, integration, merges | Feature implementation beyond glue and smoke-test fixes | Compiling skeleton and integration builds |
| Agent 1: Gameplay Simulation | `Scripts/Gameplay/**`, `Data/Incidents/**`, `Data/Missions/**`, EditMode tests for model | Networking package, scene/prefab wiring, UI | MissionDirector, TrainSystemModel, incident logic, event log |
| Agent 2: Player + Interactions | `Scripts/Player/**`, `Scripts/Interaction/**`, player prefab draft if isolated | Mission rules, lobby UI, package manifest | Movement, interaction targeting, validated intent/request model |
| Agent 3: Networking + Sessions | `Scripts/Networking/**` and session test harness | Gameplay rules, UI scenes, mission content | Session adapter, NGO RPC adapters, host validation, connection flow |
| Agent 4: UI/UX | `Scripts/UI/**`, UI prefab assets; scene only if reserved by integrator | Gameplay truth/state ownership, networking internals | Main menu/lobby/HUD/results panels consuming agreed interfaces |
| Agent 5: Mission Content + QA | ScriptableObject definitions and tests under reserved content/test paths; `Docs/QA_CHECKLIST.md` | Shared runtime scripts, package settings, production scenes | Three data-driven incidents, first mission configuration, test plan |
| Optional Agent 6: Art/Audio Prototype | `Art/**`, `Audio/**`, art-only prefab assets under named folders | Runtime scripts and shared scenes | Placeholder visual/audio package with import notes |

If fewer agents are available, combine Agent 5 with Agent 1, and Agent 6 with Agent 4. Never split the integrator role.

### Merge order

1. Agent 0 creates skeleton and commits contracts.
2. Agent 1 implements plain-C# model and tests without Unity scene dependencies where possible.
3. Agent 2 implements player movement/interactions against the request contract.
4. Agent 3 integrates session/multiplayer transport against the request contract.
5. Agent 4 binds UI to service interfaces and data events.
6. Agent 5 supplies ScriptableObject data and test scenarios after data schemas are frozen.
7. Integrator wires one scene and runs end-to-end tests.
8. Art/audio merge after gameplay loop is stable.

---

## 18. Copy-paste prompts for each coding agent

### Prompt for Agent 0 — Integrator / Tech Lead

> You are the technical integrator for PANIC MECHANICS, a Unity 6 LTS 2–4 player online co-op game. Establish the Unity project skeleton, select and pin compatible package versions, set up Git ignore/LFS guidance, create initial scenes/build settings, and author the shared contracts in `Scripts/Core` and `Docs/ARCHITECTURE.md`. Use NGO for networking and the unified Unity Multiplayer Services SDK for sessions/Relay, but first verify a two-client smoke test on the chosen package versions. Do not implement the full game. Freeze enums/models/interfaces listed in this blueprint and publish them for other agents. Own `Packages/`, `ProjectSettings/`, shared contracts, scene build settings and all merges. Prevent other agents from editing these paths. Deliver a clean compiling project and a README with exact editor/package setup. Report actual tests run; do not claim success without verification.

### Prompt for Agent 1 — Gameplay Simulation

> Implement the pure gameplay simulation for PANIC MECHANICS using the shared contracts from `Scripts/Core`. Own only `Scripts/Gameplay/**`, `Data/Incidents/**`, `Data/Missions/**` and your assigned EditMode tests. Build `MissionDirector`, `TrainSystemModel`, incident runtime logic, a small event log and result/debrief calculation. Implement the three MVP incident concepts: Overheat, Power Fault and Brake Pressure Leak. They must interact through explicit subsystem dependencies and produce understandable causal consequences. Do not import or call NGO, UGS, UI or scene singletons from core gameplay rules. Keep logic testable and data-driven. Write tests for preconditions, escalation, resolution, outcome, replay reset and player-count scaling. If the shared contract is insufficient, propose the exact change before modifying shared code. Return changed files, tests, assumptions and integration instructions.

### Prompt for Agent 2 — Player Controller + Interactions

> Implement the local player controller and interaction system for PANIC MECHANICS. Own only `Scripts/Player/**`, `Scripts/Interaction/**` and an isolated player prefab draft if agreed. Implement basic WASD movement, mouse look, stable movement inside a moving train car, an interaction ray/targeting mechanism, valid-target highlight, prompt state and a request object that uses the shared `InteractionRequest` contract. Keep network APIs out of player input code: submit intents through an agreed interface so the networking adapter can relay them to the host. Never mutate authoritative train/incident state locally. Include tests or a manual verification checklist, sensible input defaults, recovery behavior for stuck players, and clear integration notes. Do not modify shared contracts, scene settings or package manifest.

### Prompt for Agent 3 — Networking + Session Service

> Implement the online session and networking layer for PANIC MECHANICS. Own only `Scripts/Networking/**` and an isolated multiplayer test harness. Use the project’s pinned NGO version and the agreed session abstraction. Implement create private session, join by code, leave session, player join/leave state, host-authoritative interaction requests and authoritative result replication. Host must validate game phase, target existence, player distance, allowed action, action cooldown and repair preconditions. Do not duplicate mission logic; call `IMissionService`/`ITrainSystemService` contracts. Handle invalid join code, service failure, client disconnect and host shutdown with clear statuses. Do not edit UI scenes or gameplay simulation. Verify with at least two local clients and document any test that still needs real remote validation. No package upgrades without integrator approval.

### Prompt for Agent 4 — UI/UX

> Implement the UI layer for PANIC MECHANICS, owning only `Scripts/UI/**` and the UI prefabs assigned by the integrator. Build main menu, private lobby/join-code screens, 2–4 player roster/role assignment, mission HUD, role-specific diagnostic panel, interaction prompts, pause/settings panel and results/debrief screen. UI must consume service interfaces/events and must not own authoritative game state. Include loading, retry, no-session, invalid-code, disconnected and mission-ended states. Keep the visual style bold, playful, readable and low-cost; use placeholders when final art is absent. Use icons/text as well as color for alerts. Do not modify package manifest, gameplay/networking code or reserved scenes. Return hierarchy details, binding requirements, and a short manual test plan.

### Prompt for Agent 5 — Mission Content + QA

> Create the data-driven MVP content for PANIC MECHANICS using the agreed ScriptableObject schemas only. Own only `Data/Incidents/**`, `Data/Missions/**` and the QA documentation/test cases assigned by the integrator. Configure The Last Express mission with three connected incidents (Overheat, Power Fault, Brake Pressure Leak), clear symptoms and readable consequences. Tune the first run for 2–4 players, with difficulty adjustments so no role is mandatory. Write a mission beat sheet, edge-case checklist and playtest questions focused on fun, clarity, funny emergent moments and voluntary rematches. Do not create competing runtime scripts or change shared schemas without approval. Do not claim game balance is proven until playtested.

### Prompt for Agent 6 — Art/Audio Prototype (optional)

> Produce a cohesive placeholder art/audio set for PANIC MECHANICS: a stylized low-poly engine room, one modular breaker panel, valve, gauge, control console, coolant tank, a few loose tools, three readable fault VFX and core interaction SFX. Own only the agreed `Art/**`, `Audio/**` and isolated art prefabs. Do not alter runtime scripts or shared scenes. Optimize for clarity, readable silhouettes and reuse; do not produce a large environment. Provide asset import settings and performance notes. Avoid copyrighted assets; document any asset license. The first priority is legibility and feedback, not fidelity.

### Prompt for QA/Test Agent (if separate)

> Test PANIC MECHANICS from the player perspective against `Docs/QA_CHECKLIST.md`. Cover 2, 3 and 4-player sessions, invalid interactions, double interactions, incident escalations, resolution, replay reset, client disconnect, host disconnect, UI feedback, readable cause/effect and results accuracy. Record exact reproduction steps, expected/actual behavior and severity. Prioritize P0/P1 blockers. Do not change runtime code unless explicitly assigned; create separate tickets or a concise bug report with screenshots/logs. Never state a bug is fixed without retesting the reproduction case.

---

## 19. Sprint plan and milestones

Use outcome-based milestones, not promises tied to a fixed calendar. A solo developer plus agents may move at different speeds depending on the Unity experience and integration friction.

### Milestone 0 — Project bootstrap

Deliverables:
- Unity project opens and compiles.
- Package versions are pinned.
- One local player can move in a graybox car.
- Two-client host/join smoke test succeeds.
- Shared contracts and module ownership documented.

Exit gate: no package/version uncertainty blocks feature work.

### Milestone 1 — Single-player simulation graybox

Deliverables:
- One train compartment.
- Basic player movement and interactions.
- Overheat incident and one repair action.
- Mission timer and simple win/fail state.
- EditMode tests for system logic.

Exit gate: a developer can play and understand one incident without network code.

### Milestone 2 — First multiplayer loop

Deliverables:
- Private session create/join by code.
- 2–4 player presence.
- Replicated player movement and world interactions.
- Host-authoritative state change for one incident.
- Consistent result state on all clients.

Exit gate: at least two real builds can complete the loop repeatedly.

### Milestone 3 — Vertical slice

Deliverables:
- Three connected incidents.
- Four diagnostic roles or role blends.
- 5–7 minute mission.
- Readable VFX/SFX feedback.
- Incident report generated from event log.
- Rematch flow.

Exit gate: an unfamiliar group can play with minimal coaching.

### Milestone 4 — Playtest and fun pass

Deliverables:
- Record several group playtests.
- Fix confusion, downtime, unfair role bottlenecks and networking bugs.
- Tune failure escalation and recovery windows.
- Collect player quotes and voluntary replay behavior (with consent).

Exit gate: groups understand the game and voluntarily replay; if not, improve core mechanics before adding content.

### Milestone 5 — Retention slice

Only after the core loop passes:
- Cosmetic progression.
- Daily Disaster seed/configuration.
- Friends' best records or a non-ranked leaderboard.
- Shareable incident card.

Exit gate: the system adds reasons to replay without disguising a weak core loop.

### Milestone 6 — Steam playtest build

- Steam build pipeline.
- Store capsule/screenshots/trailer plan.
- Friends invite/lobby integration decision.
- Crash reporting and opt-in analytics decision.
- Accessibility and onboarding pass.
- External playtest build with known limitations documented.

---

## 20. First-playtest questions

Do not only ask “Did you like it?” Observe behavior and ask specific questions:

- Did you understand the objective within 60 seconds?
- What was the funniest moment, and why?
- Did you understand why a failure happened?
- Did each player have something meaningful to do?
- Did anyone wait too long without an action?
- Did role-specific information cause useful communication, or just confusion?
- Did a wrong repair feel funny/fair or random/unfair?
- Did you ask to replay without being prompted?
- Which mechanic would make you invite another friend?
- What part was repetitive after the second round?
- Did the incident report accurately reflect what happened?
- Would you play the game again next week? Why or why not?

### Early product signals

Use as directional signals, not universal industry benchmarks:
- Most playtest groups finish the mission or clearly understand why they failed.
- Players can explain at least one causal chain after the round.
- A majority of groups choose a rematch without being prompted.
- There are no sustained periods in which one role has nothing meaningful to do.
- Players naturally retell at least one moment from the round.
- People invite additional friends or ask how to play again.

If players laugh once but do not want to replay, fix the moment-to-moment loop before adding progression.

---

## 21. Risks and mitigations

| Risk | Why it matters | Mitigation |
|---|---|---|
| Too similar to existing co-op repair/train games | Generic repair tasks are not a defensible hook | Make asymmetric diagnosis + connected system failures central and test it against simpler variants |
| Network complexity overwhelms gameplay | Online synchronization can consume production time | Validate two-client networking early; keep model logic separate; host-authoritative interactions |
| Too much chaos becomes random/unfair | Players stop feeling responsible for outcomes | Every consequence must have a visible cause and recovery window |
| One role is required or boring | Partial crews cannot play well | Shared interactions, flexible role assignment, player-count scaling |
| Daily challenges feel like chores | Retention becomes pressure instead of enjoyment | No streak punishment; rewards are cosmetic; let players miss days |
| AI features create cost or unpredictability | Not necessary to deliver the core loop | Use deterministic logic and templates first; AI is optional and post-validation |
| Player-hosted leaderboards are exploited | Scores are not independently verified | Friends/crew comparisons first; server authority for later ranked modes |
| Content demand grows too soon | Small team spends months making assets before fun is proven | One room, three incidents, one mission until the loop works |
| Competitors already have similar mechanics | The concept alone is not unique | Play the closest games, maintain a feature comparison, prototype the exact differentiator |
| Voice moderation/support burden | Voice adds safety and implementation needs | External voice during test; investigate built-in voice only after core loop validation |

---

## 22. Steam and distribution plan

Do not build the Steam marketing package before a stable playable loop exists.

After the vertical slice:
- Make a 30–60 second clip with a clear escalation, funny repair mistake and team recovery.
- Explain the multiplayer hook in one sentence.
- Capture at least three real-player moments rather than only cinematic footage.
- Invite a small number of co-op creators to test; avoid buying fake wishlists/reviews or paying for vanity metrics.
- Use a demo/playtest to collect qualitative feedback and store-page interest.
- Check the official Steam Direct rules/fees and Steamworks networking/invite options when the product is ready; platform rules can change.
- Do not claim the game is educational, therapeutic or professionally simulating real machinery.

---

## 23. Immediate next actions

1. Create the Unity project and pin Unity/package versions.
2. Create a Git repository with an explicit agent ownership map.
3. Freeze the shared contracts in `Scripts/Core`.
4. Have the integrator run the two-client network smoke test before parallel feature development.
5. Assign Agents 1–5 from the prompts in Section 18.
6. Merge by the prescribed order and keep the main branch compiling after every integration.
7. Build one playable graybox incident before building all three incidents.
8. Playtest with actual friend groups and only then commit to the retention/viral feature set.

---

## 24. Official technical references

Confirm the exact package APIs against the installed project versions before coding; online docs may describe a newer version than the project lockfile.

- Unity Multiplayer Services SDK / sessions: https://docs.unity.com/en-us/mps-sdk/sessions
- Unity Multiplayer Services getting started: https://docs.unity.com/en-us/mps-sdk/get-started
- Unity Multiplayer Services billing/subservices: https://docs.unity.com/en-us/mps-sdk/mps-billing
- Unity Netcode for GameObjects repository/releases: https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases
- Unity Netcode for GameObjects basics/tutorial: https://learn.unity.com/tutorial/get-started-with-netcode-for-gameobjects
- Steam Networking Sockets (consider for future Steam integration): https://partner.steamgames.com/doc/api/ISteamNetworkingSockets

---

## Definition of done for the entire first vertical slice

The first vertical slice is done only when a group can reliably create/join a private session, select or receive roles, play the complete mission, see understandable system failures, repair the train through host-validated interactions, complete or fail, review an accurate incident report, and immediately replay. The project must compile from a clean checkout with locked package versions, contain automated tests for the core simulation, and have no unresolved P0/P1 defects that block the experience.

Everything beyond that is scope until the vertical slice proves fun.
