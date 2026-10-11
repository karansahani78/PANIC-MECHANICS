# PANIC MECHANICS

2–4 player online co-op 3D game — keep a failing train alive until it reaches the
safe station. Unity 6.3 LTS + C#, Netcode for GameObjects (host-authoritative),
Unity Multiplayer Services (sessions/Relay).

> Full brief: [`PANIC_MECHANICS_BLUEPRINT.md`](PANIC_MECHANICS_BLUEPRINT.md)

## Repository status

**Foundation only — no Unity project yet.** This machine does **not** currently
have Unity Hub/Editor, `git-lfs`, or the .NET SDK installed, so the Unity project
cannot be created or compiled here. The agent foundation (skills, contracts,
architecture, ownership) is in place and ready for bootstrap.

See [`Docs/ARCHITECTURE.md`](Docs/ARCHITECTURE.md) for the assessment and
[`Docs/AGENT_TASKS.md`](Docs/AGENT_TASKS.md) for agent ownership.

## Pinned baseline (verified against Unity 6.3 LTS docs, 2026-10-11)

| Item | Version |
|---|---|
| Unity Editor | 6.3 LTS, `6000.3.x` (pin exact patch at install) |
| `com.unity.netcode.gameobjects` | 2.13.3 |
| `com.unity.services.multiplayer` | 2.4.0 |
| `com.unity.inputsystem` | 1.20.1 |
| `com.unity.multiplayer.playmode` | 2.0.2 |
| `com.unity.multiplayer.tools` | 2.2.12 |
| `com.unity.test-framework` | 1.6.x (core, editor-matched) |
| `com.unity.render-pipelines.universal` | 17.3.x (core, editor-matched) |
| `com.unity.ugui` | editor-matched (uGUI + TextMeshPro) |

Do not upgrade packages without integrator approval. NGO 3.x targets Unity 6000.7.

## Bootstrap steps (needs Unity Hub + git-lfs installed)

1. Install **Unity Hub** and the latest **Unity 6.3 LTS (`6000.3.x`)** editor.
2. Install **Git LFS** (`brew install git-lfs && git lfs install`).
3. Create a new 3D (URP) project at this repository root, or open it so Unity
   generates `Packages/`, `ProjectSettings/`, `Assets/` and `.meta` files.
4. Add the pinned packages above via Package Manager, then commit
   `Packages/manifest.json` + `Packages/packages-lock.json`.
5. Record the exact editor patch in `ProjectSettings/ProjectVersion.txt` and here.
6. Run the **two-client smoke test** (host + client) before feature work.
7. Freeze contracts from [`Docs/CONTRACTS.md`](Docs/CONTRACTS.md) into
   `Assets/_Game/Scripts/Core/`.

## Agent foundation

- [`AGENTS.md`](AGENTS.md) — project rules for all agents
- `Docs/CONTRACTS.md` — frozen shared enums/models/interfaces
- `Docs/ARCHITECTURE.md` — folder layout, asmdef boundaries, tech baseline
- `Docs/AGENT_TASKS.md` — ownership map, dependency graph, first task
- `Docs/QA_CHECKLIST.md` — quality gates
- `.opencode/skills/` — 8 project skills (see `AGENTS.md`)
