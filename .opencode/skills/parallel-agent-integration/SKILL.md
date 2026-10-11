---
name: parallel-agent-integration
description: Use when coordinating parallel coding agents, branches/worktrees, file ownership, shared contracts, merge order and integration for PANIC MECHANICS. Triggers on agent tasks, ownership, merge conflicts, worktrees, integration, code review, contract changes, or "which agent owns X".
---

# Parallel Agent Integration (PANIC MECHANICS)

## Non-negotiable rules

1. **One integration owner** controls main branch, package versions,
   `ProjectSettings/`, scenes/build settings, assembly definitions and shared
   contracts. Feature agents never touch these.
2. Each agent owns an isolated folder and preferably a separate branch/worktree.
3. Agents must not edit the same scene or prefab simultaneously.
4. No silent changes to a public interface or shared enum — propose to the
   integrator first (see `Docs/CONTRACTS.md`).
5. Every agent returns: files changed, APIs added, setup steps, tests run, known
   limitations, integration notes.
6. No package upgrades or `Packages/manifest.json` edits without approval.
7. Avoid large binary assets in the first pass; use primitives/placeholders.
8. Merge in **dependency order**, not when an agent finishes.
9. After each merge: open Unity, let imports finish, compile, run the relevant
   suite before merging the next feature.
10. Never claim a scene works unless it was actually opened/tested — otherwise
    state the missing verification.

## Workstream ownership map

| Agent | Owns | Must not own |
|---|---|---|
| 0 Integrator/Tech Lead | project, packages, contracts, scenes/build settings, merges | feature implementation beyond glue/smoke fixes |
| 1 Gameplay Simulation | `Scripts/Gameplay/**`, `Data/Incidents/**`, `Data/Missions/**`, model EditMode tests | networking pkg, scene/prefab wiring, UI |
| 2 Player + Interactions | `Scripts/Player/**`, `Scripts/Interaction/**`, isolated player prefab draft | mission rules, lobby UI, package manifest |
| 3 Networking + Sessions | `Scripts/Networking/**`, session test harness | gameplay rules, UI scenes, mission content |
| 4 UI/UX | `Scripts/UI/**`, assigned UI prefabs | gameplay truth, networking internals |
| 5 Mission Content + QA | `Data/**`, reserved test/content paths, `Docs/QA_CHECKLIST.md` | shared runtime scripts, package settings, scenes |
| 6 Art/Audio (optional) | `Art/**`, `Audio/**`, art-only prefabs | runtime scripts, shared scenes |

If fewer agents: combine 5 with 1, and 6 with 4. **Never split the integrator role.**

## Dependency graph / merge order

```
Agent 0  ──► contracts + skeleton (first)
   │
   ├─► Agent 1 (plain-C# model + tests)      [no Unity scene deps]
   ├─► Agent 2 (player/interaction vs request contract)  → after 1
   ├─► Agent 3 (session/NGO transport)       → after 2 / parallel
   ├─► Agent 4 (UI binds service interfaces) → after contracts
   ├─► Agent 5 (SO data + test scenarios)     → after schemas frozen
   └─► Agent 6 (art/audio)                    → after loop stable
Integrator wires one scene + end-to-end tests, in dependency order.
```

Shared contracts (enums/models/interfaces in `Docs/CONTRACTS.md`) are frozen by
Agent 0 before feature work. Only one agent owns a shared contract or shared
config file at a time.

## Branch / integration protocol

- Integrate in dependency order; open Unity and run compile + tests after every
  merge; keep main compiling.
- If a contract is insufficient, propose the exact Signature/behavior change to
  the integrator; do not fork the type.
- Resolve merge conflicts in shared files by the integrator only.

## Official references

- Blueprint §17 operating model, §18 agent prompts, §19 milestones:
  `PANIC_MECHANICS_BLUEPRINT.md`
- Project ownership & contracts: `Docs/AGENT_TASKS.md`, `Docs/CONTRACTS.md`

## Incompatible implementations to avoid

- Parallel edits to one scene/prefab or shared config.
- Agents creating competing versions of shared enums/models/interfaces.
- Merging out of dependency order or without post-merge verification.
- Feature agents editing `Packages/` or `ProjectSettings/`.
