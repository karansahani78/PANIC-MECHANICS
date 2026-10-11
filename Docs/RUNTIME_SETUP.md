# PANIC MECHANICS — Runtime Setup (Unity Editor steps)

**Status: UNVERIFIED.** Everything under `Assets/_Game/Scripts/Runtime/` was
written without a licensed editor, so it has **not** been compiled. Treat it as
scaffolding: open the project, fix any compile errors, then wire the scene per the
steps below. The pure core (`Core`, `Gameplay`) is separately verified — see
`README.md`.

## What the runtime layer contains

Assembly: `Assembly-CSharp` (no asmdef on purpose, so it auto-references the
`Core`/`Gameplay`/`Data` asmdefs and all package assemblies without
package-reference churn). Split into asmdefs after the smoke test passes.

| File | Role |
|---|---|
| `Runtime/Bootstrap/GameBootstrap.cs` | Builds `MissionDirector` + `InteractionService`, registers incident interaction targets, owns `LocalGameSessionService` |
| `Runtime/Networking/LocalGameSessionService.cs` | `IGameSessionService` over NGO loopback (host/client on `127.0.0.1`) — dev/smoke-test only |
| `Runtime/Networking/NetworkGameManager.cs` | Host-authoritative: ticks the sim, replicates phase/elapsed/subsystem states, validates interaction `ServerRpc`s |
| `Runtime/Networking/SubsystemNetState.cs` | `INetworkSerializable` snapshot of a `TrainSubsystemState` |
| `Runtime/Player/PlayerAvatar.cs` | Owner-driven CharacterController + Input System move/look/interact |
| `Runtime/Interaction/InteractableStation.cs` | Marker with `targetId`/`actionId`/`prompt` on a collider |
| `Runtime/UI/HudController.cs` | Reads replicated state into TMP labels |
| `Runtime/UI/SessionMenuController.cs` | Host/Join buttons → `IGameSessionService` |

## Scene: `00_Bootstrap`

1. Create an empty scene `Assets/_Game/Scenes/00_Bootstrap.unity`.
2. Add an empty GameObject `Bootstrap` with `GameBootstrap`.
3. Add a GameObject `NetworkManager` with the NGO **NetworkManager** component.
   - **Network Prefabs** → add the player prefab (below).
   - Leave transport as `UnityTransport` (defaults to `127.0.0.1:7777`).
4. Add a GameObject `NetworkGameManager` with `NetworkGameManager` **and** a
   `NetworkObject` component.
5. Add a directional light, a ground plane, and a Canvas with a `HudController`
   bound to four TMP labels (`Phase`, `Timer`, `Subsystems`, `Feedback`).
6. Optionally add a second Canvas/menu with `SessionMenuController` + two Buttons.
7. Add to **Build Settings → Scenes In Build**.

## Player prefab: `NetworkPlayer`

1. Create a capsule; add `CharacterController`.
2. Child: a `Camera` named `View` (tag `MainCamera` optional).
3. Add `PlayerAvatar`; assign `viewCamera` = the child camera.
4. Add a `NetworkObject`. Save as `Assets/_Game/Prefabs/NetworkPlayer.prefab`.
5. Assign it as the NetworkManager **Player Prefab** and enable
   **Auto-spawn Player Prefab** (or spawn manually in `NetworkGameManager`).

## Interaction stations

For each incident, place a station near the relevant machinery:

1. Add a collider + `InteractableStation`.
2. Set `targetId` to the incident id (`overheat`, `powerfault`, `brakeleak`).
3. Set `actionId` to one of the incident's actions, e.g. `open_cooling_valve`.
4. Optionally put a `NetworkObject` on it if you want it networked later.

The host maps `targetId` → `IncidentInteractionTarget`; the client only sends the
id + action + distance and never mutates state (see `Docs/CONTRACTS.md` §5).

## Two-client smoke test

1. Open the project in Unity **6000.3.26f1**; fix any compile errors.
2. Use **Multiplayer Play Mode** (Window → Multiplayer Play Mode) with one virtual
   player, or build two instances.
3. Instance A → **Host**; Instance B → **Join** (`LOCAL`).
4. Expected: both see the same `Phase`/timer/subsystem values; a client walking to
   a station and pressing **E** triggers a host-validated interaction and the
   feedback key appears on the client HUD.
5. Record actual pass/fail in `Docs/QA_CHECKLIST.md`.

## Deferred (not in this scaffold)

- Relay/`MultiplayerServicesSessionService` (internet join) — loopback only for now.
- Host migration, dedicated servers, Steam.
- Full locomotion/animation, audio, accessibility.
