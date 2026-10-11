---
name: unity-testing-profiling
description: Use when writing or running PANIC MECHANICS tests, EditMode/PlayMode suites, multiplayer QA, performance targets or profiling. Triggers on Unity Test Framework, NUnit, EditMode, PlayMode, test results, 60 FPS, Profiler, allocations, latency/packet-loss testing, exclusions, or QA checklist.
---

# Unity Testing & Profiling (PANIC MECHANICS)

## Framework

Unity Test Framework `com.unity.test-framework` @1.6 (core package, editor-matched
to Unity 6.3 LTS). Tests live under `Assets/_Game/Tests/EditMode` and
`Tests/PlayMode`. Never invent a different test runner.

## EditMode tests (pure simulation)

- Train subsystem dependency calculations.
- Fault escalation thresholds.
- Repair precondition validation.
- Mission phase transitions.
- Difficulty scaling for 2/3/4 players.
- Mission outcome and score calculation.
- Incident report generation from the event log.
- Deterministic mission-seed behavior.

## PlayMode tests

- Start mission → trigger incident → repair → resolve.
- Repeat mission resets all state (no stale faults/timers).
- Player movement and interaction range.
- HUD reacts to state changes.
- Two networked clients see the same incident state.
- Invalid client interaction is rejected by the host.
- Disconnect and host shutdown do not freeze the menu.

## Manual multiplayer QA (real remote required for final validation)

2/3/4 players; high latency + packet loss; host leaves in lobby; host leaves in
mission; client leaves mid-interaction; invalid/expired join code; session
service unavailable; interaction spamming; two clients on one panel; two clients
on one incident; mission ends during a join; replay resets faults.

Multiplayer Play Mode covers local simulation only. **Real relay/remote
connection and network variability still require a real remote test** — never
claim online validation from local only.

## Performance targets (first playable slice)

- Stable 60 FPS target on a mid-range PC at prototype quality.
- No per-frame allocations in core gameplay loops (verify in Profiler).
- No network updates for static props; no needless particle replication.
- Profile before optimizing; no premature custom ECS.

## Severity rules

- P0 crash/corruption/exploit/data loss. P1 cannot join/finish, desync, softlock.
  P2 broken cosmetic/UI. P3 polish.
- Do not mark a phase done while a P0/P1 blocks its acceptance criteria.

## Validation requirements

- Record **actual** test results (command, suite, pass/fail counts, date). If a
  test could not run, say so — do not claim success.
- Every gameplay rule change ships with an EditMode test or a written reason.
- Bugs are not "fixed" until the reproduction case is retested.

## Official references

- Test Framework: https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.test-framework.html
- Unity Test Framework manual: https://docs.unity3d.com/Packages/com.unity.test-framework@1.6/manual/index.html
- Profiler: https://docs.unity3d.com/6000.3/Documentation/Manual/Profiler.html
- Multiplayer Play Mode: https://docs.unity3d.com/Packages/com.unity.multiplayer.playmode@2.0/manual/index.html
- Multiplayer Tools (network profiler): https://docs.unity3d.com/Packages/com.unity.multiplayer.tools@2.2/manual/index.html

## Incompatible implementations to avoid

- Report claims without recorded evidence.
- Delegating EditMode gameplay tests to PlayMode/network scenes.
- Optimizing before profiling, or adding a custom ECS.
