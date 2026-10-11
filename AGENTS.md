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

Unity **6000.3.26f1 is installed and licensed**; the project compiles and tests
run. Agents must run:

- Compile: `Unity -batchmode -nographics -quit -projectPath <repo> -logFile <log>`
  (expect exit 0, zero `error CS`).
- Tests: `Unity -batchmode -nographics -projectPath <repo> -runTests
  -testPlatform EditMode|PlayMode -testResults <xml>`.
- Standalone pure-core tests: `Tools/run-tests.sh` (`dotnet test`).
- Record **actual** pass/fail counts; if a suite cannot run, say so.

Latest actual baseline (2026-10-11): compile clean; EditMode 78/78; PlayMode 1/1;
standalone 78/78; **two-client smoke test automated and passing** (host 1/1 on a
project clone, client 1/1 joining over loopback, asserting host-assigned roles
Engineer/Electrician).

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

**Milestone 0 complete (2026-10-11)** — the project compiles clean, EditMode 78/78
and PlayMode 1/1 pass, the `00_Bootstrap` playable slice is generated, the
**two-client** (host + joining client) smoke test is **automated and passing** on
the pinned package set (`Tools/two-client-smoke.sh`; both fixtures `Assert.Ignore`
unless `PM_TWOCLIENT=1`), and **roles are host-assigned and enforced**
(Engineer/Electrician verified end-to-end; `RequestRoleServerRpc` removed). Broad
parallel feature work may resume; keep changes scoped to owned folders and re-run
the smoke script before claiming network changes.
