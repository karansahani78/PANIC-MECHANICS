---
name: unity-multiplayer-ngo
description: Use when implementing networking, sessions, Relay or host-authoritative state for PANIC MECHANICS with Netcode for GameObjects and the Unity Multiplayer Services SDK. Triggers on NGO, NetworkBehaviour, NetworkVariable, ServerRpc, ClientRpc, Relay, join code, sessions, lobby, host authority, Multiplayer Play Mode, or desync issues.
---

# Unity Multiplayer / Netcode for GameObjects (PANIC MECHANICS)

## Pinned versions (Unity 6.3 LTS / 6000.3)

| Package | Version |
|---|---|
| `com.unity.netcode.gameobjects` | 2.13.3 |
| `com.unity.services.multiplayer` | 2.4.0 |
| `com.unity.multiplayer.playmode` | 2.0.2 |
| `com.unity.multiplayer.tools` | 2.2.12 |

Do **not** upgrade packages. NGO 3.x requires Unity 6000.7 and makes Netcode for
Entities a hard dependency; stay on the 2.13.x line for this project.

## Topology

Listen-host authoritative multiplayer for the first co-op prototype:

- Host owns mission phase, incident/fault state, timers, train movement, scoring
  and outcomes.
- Clients send **interaction requests**, never state mutations.
- Host validates phase, object existence, distance, allowed action, cooldown and
  incident preconditions; then applies and replicates the result.
- Critical networked physics props stay host-driven in the MVP.

## Required interaction flow

1. Client presses interact → local UI shows a *pending* indicator only.
2. Client sends target ID + action ID (+ small payload) via NGO.
3. Host validates everything server-side.
4. Host applies to canonical model (`ITrainSystemService` / `IMissionService`).
5. Host appends a `MissionEvent` and updates replicated state.
6. All clients play animation/audio/UI from the authoritative result.

## Session abstraction

Gameplay must never call UGS/Relay/Lobby APIs directly. Use
`IGameSessionService` (`CreatePrivateSessionAsync`, `JoinByCodeAsync`,
`LeaveSessionAsync`, `GetLocalPlayerId`, `StatusChanged`). The UGS adapter lives
in `Scripts/Networking/**` only. This keeps a future Steam provider swappable.

## Host-authoritative rules

- Never let a client set health, resolve an incident, adjust time, or award score.
- Guard every RPC handler with `if (!IsServer) return;` (or server/host check).
- Rate-limit and distance-check interaction requests on the host.
- Do not network-spawn trivial particles; replay cosmetic VFX locally on
  authoritative events.
- Give each interactable a stable string ID and explicit action list.
- Don't replicate derivable data unless consistency requires it.

## Disconnect policy (MVP)

- Client leaves → removed from crew; mission continues with difficulty adjustment.
- Host leaves/crashes → clean run end; remaining clients show a message and
  return to menu. No host migration in the MVP.
- Never leave stale NetworkObjects alive across retry/return-to-lobby. Replaying
  must reset all networked fault/timer state.

## Testing

- Use Multiplayer Play Mode for local 2–4 client simulation.
- The host-authority path must be verified with a bogus/out-of-range request and
  by two clients acting on the same target simultaneously.
- Real remote/relay validation cannot be claimed from local tests alone; note it
  as pending.

## Official references

- NGO 2.13 docs: https://docs.unity3d.com/Packages/com.unity.netcode.gameobjects@2.13/manual/index.html
- NGO releases/changelog: https://github.com/Unity-Technologies/com.unity.netcode.gameobjects/releases
- Multiplayer Services sessions: https://docs.unity.com/en-us/mps-sdk/sessions
- MPS get started: https://docs.unity.com/en-us/mps-sdk/get-started
- Network connection management: https://docs.unity.com/en-us/mps-sdk/manage-session-network-connection
- Multiplayer Play Mode: https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@2.0/manual/index.html

## Incompatible implementations to avoid

- Client-authoritative movement of critical gameplay/physics state.
- Business/mission logic duplicated inside NetworkBehaviours instead of calling
  service interfaces.
- Direct Relay/Lobby SDK calls sprinkled through gameplay or UI code.
- Editing `Packages/manifest.json`, `ProjectSettings/` or scene build settings
  without integrator approval.
- Assuming NGO 3.x / Netcode-for-Entities APIs when the project pins 2.13.3.
