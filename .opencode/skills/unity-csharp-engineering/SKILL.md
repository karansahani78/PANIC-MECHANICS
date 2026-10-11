---
name: unity-csharp-engineering
description: Use when writing or refactoring C# in the PANIC MECHANICS Unity project. Covers project layout, namespaces, assembly definitions, plain-C#-vs-MonoBehaviour separation, ScriptableObject data, serialization and Unity coding conventions. Triggers on Unity scripts, asmdefs, MonoBehaviours, ScriptableObjects, namespaces, .meta files, or C# style questions in this repo.
---

# Unity C# Engineering (PANIC MECHANICS)

## Purpose

Keep gameplay rules testable and framework-free; keep Unity types at the edges.

- **Rules live in plain C# classes** (no `MonoBehaviour`, no `UnityEngine` scene singletons).
- **MonoBehaviours** handle lifecycle, input, presentation and networking glue only.
- **ScriptableObjects** carry authored data, not runtime state.

## Canonical project layout

```text
Assets/_Game/
  Scripts/
    Core/          # PanicMechanics.Core: enums, contracts, models, events
    Gameplay/      # TrainSystems, Incidents, Mission, Scoring
    Player/        # movement, look, role views
    Interaction/   # targeting, prompts, request submission
    Networking/    # NGO + session adapters
    UI/            # uGUI/TextMeshPro panels consuming interfaces
    Audio/         # audio feedback
    Persistence/   # local settings
  Data/            # Incidents/, Missions/, Roles/ ScriptableObjects
  Prefabs/         # Characters/, Interactables/, Train/, Network/
  Scenes/
  Tests/EditMode/  Tests/PlayMode/
```

## Namespaces

Root namespace `PanicMechanics`. Match namespace to folder: `PanicMechanics.Core`,
`PanicMechanics.Gameplay.Mission`, `PanicMechanics.Networking`, `PanicMechanics.UI`.
Do **not** invent parallel namespaces or duplicate types.

## Assembly definitions (do not create before smoke test)

The integrator creates asmdefs **only after** the two-client package smoke test
passes, to avoid package-reference churn. Planned assemblies:
`PanicMechanics.Core`, `.Gameplay`, `.Player`, `.Networking`, `.UI`, `.Tests`.
Reference direction is one-way: Core ← Gameplay ← {Player, Networking, UI}.
Never reference UI from Core/Gameplay.

## Conventions

- One public type per file; file name matches the type.
- Use `[SerializeField] private` with `[Tooltip]`; avoid public fields.
- Prefer `readonly struct` for value records (e.g. `InteractionResult`).
- Prefer `TryGet...` + `out InteractionResultCode` over exceptions for validation.
- No `FindObjectOfType`, no `GameObject.Find`, no `Resources.Load` in gameplay.
- No per-frame allocations in gameplay loops; cache collections, avoid LINQ in
  hot paths, avoid closures in `Update`.
- Use `UnityEngine.Debug` only for diagnostics; route player-facing feedback
  through the UI/audio layer.
- Do not use `async void` except at UI event entry points; use `Task`/`UniTask`
  patterns already present in the project.

## Data contracts

Types in `Explained in Docs/CONTRACTS.md` are **shared contracts**. Never create a
competing `GamePhase`, `PlayerRole`, `TrainSubsystem`, `IncidentPhase`,
`MissionOutcome`, `InteractionResultCode`, `MissionEvent`, `MissionResult`,
`TrainSubsystemState`, `InteractionRequest` or `InteractionResult`. Propose changes
to the integrator first.

## .meta and asset rules

- Let Unity generate and own `.meta` files. Never create, edit, delete or
  hand-author `.meta` files.
- Do not move or rename imported assets after other agents reference them.
- Commit `Packages/manifest.json` and `Packages/packages-lock.json`; never edit
  `manifest.json` or `ProjectSettings/` without integrator approval.

## Validation requirements

- The project compiles with zero errors and no new warnings in changed files.
- Changed gameplay logic has an EditMode test or an explicit note why not.
- No new `UnityEngine` dependency added to Core rules.
- Report files changed, tests run, and known limitations.

## Official references

- Unity 6.3 LTS Manual: https://docs.unity3d.com/6000.3/Documentation/Manual/index.html
- Scripting/using assembly definitions: https://docs.unity3d.com/6000.3/Documentation/Manual/ScriptCompilationAssemblyDefinitionFiles.html
- ScriptableObject: https://docs.unity3d.com/6000.3/Documentation/Manual/class-ScriptableObject.html
- C# style guide (Unity): https://unity.com/resources/c-sharp-style-guide-unity

## Incompatible implementations to avoid

- God-object `GameManager` holding mission, networking, UI and save state.
- Business rules inside MonoBehaviours or `NetworkBehaviour` handlers.
- UI or Networking code mutating authoritative gameplay state.
- Adding NuGet packages or third-party libraries without integrator approval.
