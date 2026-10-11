# PANIC MECHANICS — AGENTS.md

Project-level instructions for all coding agents. Read this, `Docs/CONTRACTS.md`,
`Docs/ARCHITECTURE.md` and `Docs/AGENT_TASKS.md` before changing anything.

## Project

2–4 player online co-op 3D game. Unity **6.3 LTS (6000.3.x)** + C#. Netcode for
GameObjects (host-authoritative), Unity Multiplayer Services (sessions/Relay).
See `PANIC_MECHANICS_BLUEPRINT.md` for the full brief.

## Hard rules (do not violate)

1. Only the **integrator** edits `Packages/`, `ProjectSettings/`, scenes, build
   settings, assembly definitions and shared contracts.
2. Never create competing versions of shared enums/models/interfaces in
   `Docs/CONTRACTS.md`. Propose changes to the integrator first.
3. Gameplay rules are plain C# (no `MonoBehaviour`, no NGO/UGS/UI/scene deps).
4. Clients **request** actions; the **host validates and applies**. No client may
   mutate authoritative state.
5. Never hand-author, edit or delete `.meta` files. Let Unity own them.
6. Never upgrade Unity packages or edit `manifest.json` without approval.
7. One agent owns a shared contract/config file at a time.
8. Return the standard report: files changed, APIs, setup, **actual** test
   results, limitations, integration notes. Never claim success you did not verify.
9. Stay within your owned folders (see `Docs/AGENT_TASKS.md`).
10. No comments unless requested; follow existing code conventions.

## Build / test request

Unity is not yet installed on this machine, so the project cannot be opened or
compiled yet. Once bootstrapped, agents must run:

- Compile: open the project in Unity 6.3 LTS (or Unity batchmode `-quit -batchmode -logFile`).
- Tests: Unity Test Framework EditMode + PlayMode via Test Runner / `-runTests`.
- Record **actual** pass/fail counts; if a suite cannot run, say so.

## Skills

Project skills live in `.opencode/skills/`. Load the relevant one before working:

- `unity-csharp-engineering` — code layout, namespaces, asmdefs, conventions
- `unity-multiplayer-ngo` — NGO, sessions, host authority, Relay
- `cooperative-game-design` — roles, asymmetry, fail-forward, balancing
- `gameplay-systems-simulation` — train model, incidents, event log
- `unity-ui-audio-feedback` — uGUI/TMP, HUD, debrief, audio, accessibility
- `unity-testing-profiling` — tests, multiplayer QA, performance
- `parallel-agent-integration` — ownership, branches, merge order
- `steam-pc-release` — build/distribution, Steamworks (deferred)

## Ownership map (summary)

Agent 0 Integrator / Agent 1 Gameplay / Agent 2 Player+Interaction /
Agent 3 Networking / Agent 4 UI / Agent 5 Content+QA / Agent 6 Art+Audio.
Full write perms: `Docs/AGENT_TASKS.md`.

## Status

Foundation only. Do not start full parallel implementation until the integrator
has bootstrapped the project and the two-client smoke test passes.
