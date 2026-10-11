# PANIC MECHANICS

2–4 player online co-op 3D game — keep a failing train alive until it reaches the
safe station. Unity 6.3 LTS + C#, Netcode for GameObjects (host-authoritative),
Unity Multiplayer Services (sessions/Relay).

> Full brief: [`PANIC_MECHANICS_BLUEPRINT.md`](PANIC_MECHANICS_BLUEPRINT.md)

## Repository status

**Scaffolded — awaiting Unity Editor license activation.**

- Unity Editor **6000.3.26f1 (Apple Silicon)** is installed via Unity Hub.
- The Unity project scaffold exists: `Packages/manifest.json`,
  `ProjectSettings/ProjectVersion.txt`, assembly definitions, and content folders.
- The gameplay core (enums/contracts/models + simulation + interaction validation)
  is **Unity-free** and verified: `dotnet test` → **49 passed / 0 failed**.
- A host-authoritative runtime scaffold exists under
  `Assets/_Game/Scripts/Runtime/` (session service, network replication,
  interaction relay, player, HUD) but is **unverified** — see
  [`Docs/RUNTIME_SETUP.md`](Docs/RUNTIME_SETUP.md).
- The editor has **not** been opened yet, so `.meta` files, package restore, scene
  wiring, Unity compile and the two-client smoke test are still pending. The single
  blocker is Unity license activation, which requires an interactive sign-in.

See [`Docs/ARCHITECTURE.md`](Docs/ARCHITECTURE.md) and
[`Docs/AGENT_TASKS.md`](Docs/AGENT_TASKS.md).

## Pinned baseline

Editor pinned to **6000.3.26f1** (`ProjectVersion.txt`). Package versions below
are the set **bundled/validated by that exact editor** (taken from the editor's
own package cache and URP project template, not guessed):

| Item | Version | Source |
|---|---|---|
| Unity Editor | `6000.3.26f1` | Unity Hub / installed |
| `com.unity.netcode.gameobjects` | 2.13.3 | editor package cache |
| `com.unity.services.multiplayer` | 2.3.3 | editor package cache |
| `com.unity.inputsystem` | 1.20.0 | editor package cache |
| `com.unity.multiplayer.playmode` | 2.0.2 | editor package cache |
| `com.unity.multiplayer.tools` | 2.2.12 | editor package cache |
| `com.unity.render-pipelines.universal` | 17.0.1 | URP project template |
| `com.unity.test-framework` | 1.4.2 | URP project template |
| `com.unity.ugui` | 2.0.0 | URP project template (incl. TextMeshPro) |

> The Unity docs pages advertise slightly newer patches (e.g. MPS 2.4.0, Input
> System 1.20.1). The versions above are what `6000.3.26f1` itself ships/caches
> for offline use, so they are the safest pin. Do not upgrade packages without
> integrator approval. NGO 3.x targets Unity 6000.7.

## Verification (actual results, 2026-10-11)

The pure simulation is compiled and tested standalone — no Unity required — via
`Tools/SimVerify/` (a .NET NUnit project that includes the `Core` + `Gameplay`
source directly, mirroring Unity C# 9 and the NUnit constraint API).

```text
$ dotnet test Tools/SimVerify/SimVerify.csproj
Passed!  - Failed: 0, Passed: 49, Skipped: 0, Total: 49
```

Covered: `TrainSystemModel`, the three incidents (`OverheatIncident`,
`PowerFaultIncident`, `BrakeLeakIncident`), `MissionDirector`,
`MissionScorer`/report, `DifficultyScaler`, `InteractionService`
(phase/distance/cooldown/rejection routing + result-source passthrough),
`IncidentInteractionTarget`, and `RoleInformationService` role visibility.

**Not yet verified:** Unity compile of `Data` + `Runtime`, package restore, and
the two-client multiplayer smoke test. These require opening the licensed editor.

## Next steps

1. **Activate a Unity license** (one interactive step; see below).
2. Open the project in Unity 6.3 LTS `6000.3.26f1`; let it generate `.meta` files,
   resolve packages, and write `Packages/packages-lock.json`.
3. Build the `00_Bootstrap` scene + engine-car prototype (see
   [`Docs/RUNTIME_SETUP.md`](Docs/RUNTIME_SETUP.md)).
4. Implement the Relay/`MultiplayerServicesSessionService` adapter (loopback
   session service already exists), then run the **two-client smoke test** before
   feature work.

### Activating the license (required, interactive)

The editor refuses batchmode/headless work until a license exists
(`No valid Unity Editor license found`). To unblock:

1. Open **Unity Hub** → **Sign in** with a Unity account.
2. **Preferences → Licenses → Add → Get a free personal license** (or a Pro seat).
3. Re-run headless builds/tests, or just open the project in the editor.

## Agent foundation

- [`AGENTS.md`](AGENTS.md) — project rules for all agents
- `Docs/CONTRACTS.md` — frozen shared enums/models/interfaces (`contracts-v1`)
- `Docs/ARCHITECTURE.md` — folder layout, asmdef boundaries, tech baseline
- `Docs/AGENT_TASKS.md` — ownership map, dependency graph, first task
- `Docs/QA_CHECKLIST.md` — quality gates
- `.opencode/skills/` — 8 project skills (see `AGENTS.md`)
- `Tools/SimVerify/` — standalone Unity-free test harness
