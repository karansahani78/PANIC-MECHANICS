# PANIC MECHANICS — Architecture

**Status:** Foundation / pre-implementation. No gameplay code yet.

## 1. Repository shape

This repository root currently holds the blueprint, the OpenCode agent
foundation (`.opencode/`, `AGENTS.md`, `Docs/`). The Unity project will live at
the repository root once the editor is installed (see `README.md`).

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

**Editor:** Unity 6.3 LTS, line `6000.3.x` (current supported LTS, supported to
Dec 2027). The exact patch is pinned at install time from Unity Hub and recorded
in `README.md` + `ProjectSettings/ProjectVersion.txt`. Unity 6.0 LTS reaches EOL
2026-10-16, so it is not selected.

| Package | Pinned version | Notes |
|---|---|---|
| `com.unity.netcode.gameobjects` | 2.13.3 | conservative 2.x line; NGO 3.x needs Unity 6000.7 |
| `com.unity.services.multiplayer` | 2.4.0 | unified Sessions/Relay/Lobby SDK |
| `com.unity.inputsystem` | 1.20.1 | new Input System |
| `com.unity.multiplayer.playmode` | 2.0.2 | local multi-client testing |
| `com.unity.multiplayer.tools` | 2.2.12 | network profiler/debug tools |
| `com.unity.test-framework` | 1.6.x | core package, editor-matched |
| `com.unity.render-pipelines.universal` | 17.3.x | core package, editor-matched (URP) |
| `com.unity.ugui` | editor-matched | uGUI + TextMeshPro for the prototype UI |

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

**Approval status (2026-10-11):** technology decisions approved. Editor = latest
stable Unity 6.3 LTS `6000.3.x` in Unity Hub (record exact patch at install).
NGO 2.13.3 is pinned and must not move to 3.x without a documented reason; final
compatibility verification of NGO 2.13.3 against the selected editor and
Multiplayer Services 2.4.0 is **pending** until the editor is installed and the
smoke test runs. No package version may be guessed or auto-upgraded.

## 3. Assembly boundaries (created after the smoke test)

Reference direction is strictly one-way:

```
PanicMechanics.Core            (enums, contracts, models, events — no Unity scene deps)
        ▲
        │
PanicMechanics.Gameplay        (MissionDirector, TrainSystemModel, incidents, scoring)
        ▲
        ├── PanicMechanics.Player        (movement, look, role views)
        ├── PanicMechanics.Networking    (NGO adapters, session service)
        └── PanicMechanics.UI            (menus, HUD, results)
PanicMechanics.Tests           (references all, not referenced by any)
```

- Core must not reference Gameplay/Player/Networking/UI or `UnityEngine` scene APIs.
- UI must not reference Networking internals; it binds to Core service interfaces.
- Integrator owns the final asmdef decision; create them only after the smoke test
  passes to avoid package-reference churn.

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
