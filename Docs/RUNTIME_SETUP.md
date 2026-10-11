# PANIC MECHANICS — Runtime Setup (Unity Editor steps)

**Status: VERIFIED (2026-10-12).** The project compiles in batchmode with zero
errors; EditMode 93/93, PlayMode 1 passed / 4 gated-skip, SimVerify 93/93, and the
two-client loopback smoke test pass. The runtime layer now lives in its own
`PanicMechanics.Runtime` assembly; `HudController` renders the local player's
host-assigned role and only that role's subsystem diagnostics. Live Relay is
implemented and unit-covered but **not yet verified** (no Cloud project linked).

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
| `Runtime/Bootstrap/GameBootstrap.cs` | Builds `MissionDirector` + `InteractionService`, registers incident interaction targets, selects the `IGameSessionService` backend (`Local` by default, `MultiplayerServices` when requested) |
| `Runtime/Networking/LocalGameSessionService.cs` | `IGameSessionService` over NGO loopback (host/client on `127.0.0.1`) — offline dev + automated smoke tests |
| `Runtime/Networking/MultiplayerServicesSessionService.cs` | `IGameSessionService` over Unity Multiplayer Services **Sessions + Relay** (unified `com.unity.services.multiplayer` API); needs a linked Unity Cloud project |
| `Runtime/Networking/NetworkGameManager.cs` | Host-authoritative: ticks the sim, replicates phase/elapsed/subsystem states, assigns roles (join order), validates interaction `ServerRpc`s with the assigned role |
| `Runtime/Networking/SubsystemNetState.cs` | `INetworkSerializable` snapshot of a `TrainSubsystemState` |
| `Runtime/Player/PlayerAvatar.cs` | Owner-driven CharacterController + Input System move/look/interact; `_role` server-authorized on spawn |
| `Runtime/Interaction/InteractableStation.cs` | Marker with `targetId`/`actionId`/`prompt` on a collider |
| `Runtime/UI/HudController.cs` | Reads replicated state into TMP labels; role label + role-filtered diagnostics via `DiagnosticPanel` (creates `RoleLabel` at runtime if unwired) |
| `Runtime/UI/SessionMenuController.cs` | Host/Join buttons → `IGameSessionService`; shows a runtime status/join-code label |

## Session backends

`GameBootstrap` exposes a serialized `SessionBackend { Local, MultiplayerServices }`
(default **Local**). The environment variable `PM_SESSION_BACKEND=ugs` overrides it
to `MultiplayerServices` at runtime, so automated runs and the shipped default stay
on loopback while a dev can test Relay without editing the scene.

`MultiplayerServicesSessionService` (`IGameSessionService`) performs, on first use:

1. `UnityServices.InitializeAsync()` (idempotent; `Unity.Services.Core`).
2. `AuthenticationService.Instance.SignInAnonymouslyAsync()` when not signed in.
3. Host: `MultiplayerService.Instance.CreateSessionAsync(new SessionOptions {
   Name, MaxPlayers, IsPrivate = true }.WithRelayNetwork())` → the MPS SDK allocates
   Relay, points the scene `UnityTransport` at it, and starts NGO **Host** on
   `NetworkManager.Singleton` (the same in-scene `NetworkManager`). `session.Code` is
   the shareable join code.
4. Join: `MultiplayerService.Instance.JoinSessionByCodeAsync(code)`; the SDK reads the
   session's network property and connects NGO **Client** over Relay.

Host-authoritative roles (`NetworkGameManager`) and the `InteractionService`
expertise/cooldown rules are unchanged — the adapter only swaps how the transport
connects. Errors are mapped to user text by `PanicMechanics.Core.SessionErrorText`
(pure, unit-tested). Failed creates/joins clean up any leaked session and shut down
the `NetworkManager`.

### Unity Cloud project linkage (required for live Relay)

Live Relay/Sessions require the Unity project to be linked to a Unity Cloud project.
This is an editor/Dashboard step and must be done once by a human:

1. Open the project in Unity **6000.3.26f1**.
2. **Edit → Project Settings → Services** (or **Window → Unity Services → General**)
   and **Link** the project to a Unity Cloud organization/project. Create a free
   project if needed.
3. In the [Unity Cloud Dashboard](https://cloud.unity.com) → your project →
   **Services**, enable **Authentication**, **Relay**, and **Multiplayer / Sessions**.
4. Confirm anonymous sign-in is allowed (Authentication → Sign-in providers →
   **Anonymous** enabled).
5. Verify linkage: `ProjectSettings/ProjectSettings.asset` should show a non-empty
   `cloudProjectId` (currently empty in this repo). Do **not** commit secrets;
   the Cloud project id lives in `ProjectSettings.asset` and is not a secret.

Without a linked project, `InitializeAsync`/sign-in/Relay will fail; the adapter
returns a failed `SessionOperationResult` with a readable message and the app stays
on the loopback backend. `LocalGameSessionService` remains the offline/test path.

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

## Relay smoke test (requires a linked Cloud project)

`Tools/relay-smoke.sh` mirrors the loopback script but drives the Relay adapter:
the host (on an APFS clone) creates a Relay session with
`PM_SESSION_BACKEND=ugs` and writes `session.Code` to a marker file; the client
(on the repo) reads it and joins by code. Both fixtures
(`RelayHostSmokeTests`, `RelayClientSmokeTests`) `Assert.Ignore` unless
`PM_RELAY=1`, so ordinary PlayMode runs skip them.

```text
$ ./Tools/relay-smoke.sh .     # after linking a Unity Cloud project
RELAY_SMOKE_PASS
```

**Not yet verified** — no Unity Cloud project is linked in this repo yet
(`ProjectSettings.asset` `cloudProjectId` is empty), so Relay has not been run
against live clients. Do not claim live Relay works until this script passes.

## Deferred (not in this scaffold)

- Live Relay verification (adapter + gated smoke test exist; blocked on Cloud
  project linkage above).
- Host migration, dedicated servers, Steam.
- Full locomotion/animation, audio, accessibility.
