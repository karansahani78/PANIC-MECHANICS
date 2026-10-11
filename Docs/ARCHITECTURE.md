# PANIC MECHANICS — Architecture

**Status:** Scaffolded. Unity-free gameplay core implemented and verified
(35/35 standalone NUnit tests). Unity Editor `6000.3.26f1` installed but not yet
opened — `.meta` generation, package restore and the two-client smoke test are
pending license activation (see `README.md`).

## 1. Repository shape

This repository root is the Unity project root. The editor scaffold
(`Packages/`, `ProjectSettings/ProjectVersion.txt`), assembly definitions and the
Unity-free gameplay core already exist.

Implemented today:

- `Assets/_Game/Scripts/Core/` — `PanicMechanics.Core.asmdef` (pure; no engine refs)
- `Assets/_Game/Scripts/Gameplay/` — `PanicMechanics.Gameplay.asmdef` (pure; no engine refs)
  - `Simulation/` — train model, mission director, incidents (overheat/powerfault/brakeleak), scoring, event log, report
  - `Interaction/` — `InteractionService` (host validation routing), `IncidentInteractionTarget`, `RoleInformationService`
  - `Missions/` — `MissionConfig`/`IncidentConfig` + `MissionBuilder` (content → director, applies difficulty)
  - `Lobby/` — `RoleAssignmentService`
- `Assets/_Game/Scripts/Data/` — `PanicMechanics.Data.asmdef` (engine; ScriptableObject defs)
- `Assets/_Game/Scripts/Runtime/` — **engine/network layer, no asmdef (Assembly-CSharp), UNVERIFIED**: `GameBootstrap`, `LocalGameSessionService`, `NetworkGameManager`, `PlayerAvatar`, `InteractableStation`, `HudController`, `SessionMenuController`
- `Assets/_Game/Tests/EditMode/` — `PanicMechanics.Tests.EditMode.asmdef`
- `Tools/SimVerify/` — standalone .NET harness compiling `Core` + `Gameplay` for CI-less verification
- `Docs/RUNTIME_SETUP.md` — editor scene/prefab wiring + smoke-test steps

Deferred until after the two-client smoke test (per `AGENTS.md` and §3):
Relay/`MultiplayerServicesSessionService`, `Player`/`Networking`/`UI` asmdefs,
scenes and prefabs. Target layout:

```text
game/                               # repo root == Unity project root (after bootstrap)
├── Assets/_Game/
│   ├── Art/{Characters,Environment,Props,VFX}
│   ├── Audio/{Music,SFX,Mixers}
│   ├── Data/{Incidents,Missions,Roles}
│   ├── Prefabs/{Characters,Interactables,Train,Network}
│   ├── Scenes/{00_Bootstrap,01_MainMenu,02_Lobby,10_EngineCarPrototype,90_Results}.unity
│   ├── Scripts/{Core,Gameplay,Player,Interaction,Networking,UI,Audio,Persistence}
│   ├── Settings/
│   └── Tests/{EditMode,PlayMode}
├── Packages/{manifest.json,packages-lock.json}
├── ProjectSettings/
├── Docs/
├── .opencode/skills/
├── .gitattributes
├── .gitignore
└── AGENTS.md
```

Use Unity-generated `.meta` files only. Do not move/rename imported assets after
other agents reference them.

## 2. Technology baseline (verified against Unity 6.3 LTS docs, 2026-10-11)

**Editor:** Unity 6.3 LTS **installed as `6000.3.26f1`** (Apple Silicon), recorded
in `README.md` + `ProjectSettings/ProjectVersion.txt`. Unity 6.0 LTS reaches EOL
2026-10-16, so it is not selected.

| Package | Pinned version | Notes |
|---|---|---|
| `com.unity.netcode.gameobjects` | 2.13.3 | conservative 2.x line; NGO 3.x needs Unity 6000.7 |
| `com.unity.services.multiplayer` | 2.3.3 | editor-bundled (docs list 2.4.0; pinned to bundled) |
| `com.unity.inputsystem` | 1.20.0 | editor-bundled (docs list 1.20.1; pinned to bundled) |
| `com.unity.multiplayer.playmode` | 2.0.2 | local multi-client testing |
| `com.unity.multiplayer.tools` | 2.2.12 | network profiler/debug tools |
| `com.unity.test-framework` | 1.4.2 | editor project-template value |
| `com.unity.render-pipelines.universal` | 17.0.1 | editor project-template value (URP) |
| `com.unity.ugui` | 2.0.0 | editor project-template value (uGUI + TextMeshPro) |

These are the versions shipped/validated by `6000.3.26f1` itself (read from the
editor's package cache and URP template). This supersedes the earlier
docs-page figures, which advertised slightly newer patches.

Sources (verified 2026-10-11):
- Editor archive/LTS: https://unity.com/releases/editor/archive
- NGO 2.13 manual (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.netcode.gameobjects.html
- Multiplayer Services (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.services.multiplayer.html
- Input System (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.inputsystem.html
- Multiplayer Play Mode (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.multiplayer.playmode.html
- Multiplayer Tools (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.multiplayer.tools.html
- Test Framework (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.test-framework.html
- URP (6000.3): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.render-pipelines.universal.html

**Version rule:** commit `manifest.json` and `packages-lock.json`. No parallel
agent upgrades packages independently. Verify a two-client smoke test on the
pinned set before feature work.

**Approval status (2026-10-11):** technology decisions approved. Editor =
`6000.3.26f1` (installed). Package versions are pinned to the editor-bundled set
(see table). NGO 2.13.3 must not move to 3.x without a documented reason. Final
compatibility verification of the pinned packages against the editor, and the
two-client smoke test, remain **pending** until the editor license is activated
and the project is opened. No package version may be guessed or auto-upgraded.

## 3. Assembly boundaries

Reference direction is strictly one-way:

```
PanicMechanics.Core            (enums, contracts, models — noEngineReferences)
        ▲
        │
PanicMechanics.Gameplay        (MissionDirector, TrainSystemModel, incidents,
        ▲                       InteractionService, RoleInformationService — pure)
        ├── PanicMechanics.Data    (ScriptableObject definitions; engine-facing)
        └── PanicMechanics.Tests.EditMode   (references Core + Gameplay)
```

**Created now:** `Core`, `Gameplay`, `Data`, `Tests.EditMode` (`Core`/`Gameplay`
declare `"noEngineReferences": true` to enforce purity).

**Deferred until after the smoke test:** `Player`, `Networking`, `UI` asmdefs. In
the interim, engine/network scripts placed under `Assets/_Game/Scripts/` outside an
asmdef compile into `Assembly-CSharp`, which automatically references the asmdef
assemblies and all package assemblies — this avoids package-reference churn before
the stack is verified.

- Core must not reference Gameplay/Data/Tests or `UnityEngine` scene APIs.
- UI must not reference Networking internals; it binds to Core service interfaces.
- Integrator owns the final asmdef decision.

## 3a. Verification harness

`Tools/SimVerify/` is a standalone .NET NUnit project (`net10.0`, `LangVersion 9.0`)
that links `Core` + `Gameplay` source and the `Tests/EditMode` files directly, so
the pure gameplay rules compile and test without the Unity Editor. Latest actual
result: **59 passed / 0 failed**. This is the fallback test path until the editor
is licensed; the same test files also run under Unity Test Framework (EditMode).

## 4. Runtime services & composition

- Scene `00_Bootstrap` initializes services and persistent objects (`SessionService`,
  `MissionService`, `TrainSystemService`, `InteractionService`, `MissionEventLog`)
  and prevents duplicate persistent managers.
- Services expose interfaces from `Docs/CONTRACTS.md`; consumers depend on
  interfaces, never on concrete implementations.
- Networking is encapsulated behind `IGameSessionService`; host-authority is the
  MVP topology. Steam/other providers can be added later without touching gameplay.

## 5. Networking topology

Listen-host authoritative: host owns mission/incident/train/score state; clients
send interaction requests; host validates and replicates results. See
`unity-multiplayer-ngo` skill and `Docs/CONTRACTS.md` §5.

## 6. Quality gates (Milestone 0 exit)

1. Unity project opens and all scripts compile with zero errors.
2. Two clients can connect (host + client) via the session adapter.
3. Up to four players can be simulated locally (Multiplayer Play Mode).
4. Player roles/interactions synchronize.
5. Host validates gameplay-critical actions (invalid request is rejected).
6. Mission state is consistent for every connected player.
7. Disconnects / failed joins produce understandable errors.
8. Automated tests run and **actual** results are recorded.

A phase is not done while a P0/P1 defect blocks its acceptance criteria.
