# PANIC MECHANICS — Agent Tasks & Ownership

**Status:** Foundation frozen. Feature agents may be launched after the Unity
project is bootstrapped and the two-client smoke test passes.

Contracts are frozen in `Docs/CONTRACTS.md` (`contracts-v1`). Only the integrator
edits shared contracts, `Packages/`, `ProjectSettings/`, scenes and asmdefs.

## Ownership map

| Agent | Owns (write) | Must not own | Deliverable |
|---|---|---|---|
| 0 Integrator / Tech Lead | project, `Packages/`, `ProjectSettings/`, `Docs/CONTRACTS.md`, scenes/build settings, asmdefs, merges | feature implementation beyond glue/smoke fixes | compiling skeleton + integration builds + pinned versions |
| 1 Gameplay Simulation | `Scripts/Gameplay/**`, `Data/Incidents/**`, `Data/Missions/**`, model EditMode tests | networking pkg, scene/prefab wiring, UI | `MissionDirector`, `TrainSystemModel`, incident logic, event log, scoring |
| 2 Player + Interactions | `Scripts/Player/**`, `Scripts/Interaction/**`, isolated player prefab draft | mission rules, lobby UI, package manifest | movement, interaction targeting, validated intent model |
| 3 Networking + Sessions | `Scripts/Networking/**`, session test harness | gameplay rules, UI scenes, mission content | session adapter, NGO RPC adapters, host validation, connection flow |
| 4 UI/UX | `Scripts/UI/**`, assigned UI prefabs | gameplay truth, networking internals | main menu/lobby/HUD/results consuming interfaces |
| 5 Mission Content + QA | `Data/**`, reserved test/content paths, `Docs/QA_CHECKLIST.md` | shared runtime scripts, package settings, scenes | three data-driven incidents, mission config, test plan |
| 6 Art/Audio (optional) | `Art/**`, `Audio/**`, art-only prefabs | runtime scripts, shared scenes | placeholder visual/audio package |

If fewer agents: combine 5 with 1, 6 with 4. Never split the integrator role.

## Dependency graph / merge order

```
Agent 0  contracts + skeleton + pinned packages      (first, blocking)
   │
   ├─► Agent 1  plain-C# model + EditMode tests       (no Unity scene deps)
   │        │
   │        ├─► Agent 2  player/interaction vs request contract
   │        │        │
   │        │        └─► Agent 3  session/NGO transport + host validation
   │        │
   │        ├─► Agent 4  UI binds service interfaces   (after contracts)
   │        └─► Agent 5  ScriptableObject data + tests  (after schemas frozen)
   │
   └─► Agent 6  art/audio                             (after loop stable)

Integrator: wire one scene → run end-to-end tests → repeat per merge.
```

## Initial implementation task (foundation of Milestone 0/1)

**Task:** Integrator bootstraps the Unity 6.3 LTS project on the pinned package
set, commits contracts into `Scripts/Core`, and proves the two-client smoke test.
Then Agent 1 delivers the **plain-C# simulation + Overheat incident** alone, with
EditMode tests, before any other incident or networking feature.

**Acceptance criteria:**
- Project opens in Unity 6.3 LTS `<exact patch>` and compiles with zero errors.
- `Packages/manifest.json` + `packages-lock.json` committed with the pinned set.
- Contracts from `Docs/CONTRACTS.md` exist in `Scripts/Core` and compile.
- Host `StartHost` + client `StartClient` connect; 2 clients spawn; a join code
  (or local session) is visible.
- Agent 1: `TrainSubsystemModel` + `MissionDirector` + Overheat incident run
  headless in an EditMode test proving: escalation threshold, repair precondition,
  resolution, replay reset, and 2/3/4-player scaling.
- Actual test results recorded (suite, counts, date) in the return report.
- No P0/P1 defects; known limitations stated explicitly.

**Explicitly out of scope for the first task:** Power Fault, Brake Leak, UI
screens, art/audio, Steam, dedicated servers, host migration.

## Return template (every agent)

```
Files changed:
APIs added/changed:
Setup steps:
Tests run (actual results):
Known limitations:
Integration notes:
Contract change requests (if any):
```

## Official references

- Blueprint §17–§19: `PANIC_MECHANICS_BLUEPRINT.md`
- Ownership skills: `parallel-agent-integration`, `unity-csharp-engineering`
