# PANIC MECHANICS — Runtime Setup (Unity Editor steps)

**Status: VERIFIED (2026-10-11).** The project compiles in batchmode with zero
errors and the host smoke test passes. The runtime layer now lives in its own
`PanicMechanics.Runtime` assembly.

## Generated already (no manual steps required)

The editor tool `PanicSceneBuilder`
(`Assets/_Game/Editor/PanicSceneBuilder.cs`, menu **Panic Mechanics → Build
Playable Slice**) generates and commits:

- `Assets/_Game/Content/Incidents/Incident_{overheat,powerfault,brakeleak}.asset`
- `Assets/_Game/Content/Missions/Mission_Derail.asset`
- `Assets/_Game/Prefabs/Player.prefab`
- `Assets/_Game/Scenes/00_Bootstrap.unity` (added to Build Settings)
- TextMeshPro Essential Resources (via **Panic Mechanics → Import TMP Essentials**)

Re-run it any time with:

```text
Unity -batchmode -nographics -quit -projectPath <repo> \
  -executeMethod PanicMechanics.EditorTools.PanicSceneBuilder.BuildAll
```

Automated checks (batchmode):

```text
# EditMode + PlayMode
Unity -batchmode -nographics -projectPath <repo> -runTests \
  -testPlatform EditMode -testResults /tmp/editmode.xml
Unity -batchmode -nographics -projectPath <repo> -runTests \
  -testPlatform PlayMode -testResults /tmp/playmode.xml
```

The manual steps below document what the tool builds and how to smoke-test it.

## What the runtime layer contains

Assembly: `PanicMechanics.Runtime` (refs `Core`/`Gameplay`/`Data` + NGO, Input
System, TextMeshPro, uGUI). Split from `Assembly-CSharp` once the stack was
verified.

| File | Role |
|---|---|
| `Runtime/Bootstrap/GameBootstrap.cs` | Builds `MissionDirector` + `InteractionService`, registers incident interaction targets, owns `LocalGameSessionService` |
| `Runtime/Networking/LocalGameSessionService.cs` | `IGameSessionService` over NGO loopback (host/client on `127.0.0.1`) — dev/smoke-test only |
| `Runtime/Networking/NetworkGameManager.cs` | Host-authoritative: ticks the sim, replicates phase/elapsed/subsystem states, assigns roles (join order), validates interaction `ServerRpc`s with the assigned role |
| `Runtime/Networking/SubsystemNetState.cs` | `INetworkSerializable` snapshot of a `TrainSubsystemState` |
| `Runtime/Player/PlayerAvatar.cs` | Owner-driven CharacterController + Input System move/look/interact; `_role` server-authorized on spawn |
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

**Automated and passing (2026-10-11).** Unity cannot open the same project twice,
so `Tools/two-client-smoke.sh` runs the **host** on an APFS clone of the project
(`cp -cR`) and the **client** on the repo, then parses both NUnit XML results:

```text
$ ./Tools/two-client-smoke.sh .
TWO_CLIENT_SMOKE_PASS    # HOST 1/1 + CLIENT 1/1 over 127.0.0.1:7777
```

- Both fixtures (`HostProcessSmokeTests.HostServesClientJoinAndTicks`,
  `ClientProcessSmokeTests.ClientJoinsAndSeesReplicatedState`) `Assert.Ignore`
  unless `PM_TWOCLIENT=1`, so ordinary EditMode/PlayMode runs don't time out.
  The script exports it before launching either editor.
- Host fixture writes a marker, waits for a real client join **and** its
  departure, then asserts the mission reached `Running`, the clock advanced,
  subsystems replicated, and host-authoritative roles: the host avatar is
  **Engineer** and the joining client is assigned **Electrician**. The client
  fixture joins via `IGameSessionService.JoinByCodeAsync("LOCAL")`, asserts the
  replicated phase/clock/subsystem count and its own assigned role
  (Electrician), then sends an invalid interaction `ServerRpc` and asserts the
  host-validated `InvalidTarget` rejection comes back over the wire.
- Debug artifacts (logs + XML) are copied to `/tmp/pm-smoke-latest`.

Manual alternative (uses the same loopback path; not needed for the gate):

1. Open the project in Unity **6000.3.26f1**.
2. Use **Multiplayer Play Mode** (Window → Multiplayer Play Mode) with one virtual
   player, or build two instances, both loading `00_Bootstrap`.
3. Instance A → click **Host**; Instance B → click **Join** (`LOCAL`).
4. Expected: both see the same `Phase`/timer/subsystem values; a client walking to
   a station and pressing **E** triggers a host-validated interaction and the
   feedback key appears on the client HUD.
5. Record actual pass/fail in `Docs/QA_CHECKLIST.md`.

## Deferred (not in this scaffold)

- Relay/`MultiplayerServicesSessionService` (internet join) — loopback only for now.
- Host migration, dedicated servers, Steam.
- Full locomotion/animation, audio, accessibility.
