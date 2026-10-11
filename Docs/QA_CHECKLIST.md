# PANIC MECHANICS — QA Checklist (MVP vertical slice)

Owner: Agent 5 (Content + QA). Executed by humans/agents per milestone.

## Automated (record ACTUAL results + date; never claim unverified passes)

- [x] Unity project opens in the pinned 6.3 LTS patch, zero compile errors. —
      2026-10-12, batchmode `-quit`: 0 `error CS` (all assemblies).
- [x] EditMode: full suite green. — 2026-10-12, **93/93** (includes the session
      error-mapping tests `SessionErrorTextTests` and the role-filtered
      `DiagnosticPanelTests`).
- [ ] EditMode: system dependency calculations.
- [ ] EditMode: fault escalation thresholds.
- [ ] EditMode: repair precondition validation.
- [ ] EditMode: mission phase transitions.
- [ ] EditMode: 2/3/4-player difficulty scaling.
- [ ] EditMode: outcome/score calculation.
- [ ] EditMode: incident report generated from the event log.
- [ ] EditMode: deterministic mission seed behavior.
- [ ] PlayMode: start → trigger → repair → resolve.
- [ ] PlayMode: replay resets all faults/timers.
- [x] PlayMode: two clients see the same incident state. — 2026-10-11,
      `Tools/two-client-smoke.sh` (host on clone + client, loopback): phase/subsystems
      replicated; HOST 1/1 + CLIENT 1/1. (Re-run 2026-10-12 after the Relay adapter:
      still PASS.)
- [x] PlayMode: invalid client interaction rejected by host. — 2026-10-11, same run:
      client `SubmitInteractionServerRpc(no_such_target,…)` → `InvalidTarget`
      returned and asserted by the client.
- [x] PlayMode: roles are host-assigned and enforced. — 2026-10-11, same run: host
      avatar Engineer + joining client Electrician asserted on both sides; relay
      uses the host-assigned role; expert cooldown advantage verified in EditMode
      (`RoleExpertise` + `InteractionRoleAdvantage`).
- [ ] PlayMode: Relay (Sessions) host+join. — adapter + gated fixtures exist
      (`Tools/relay-smoke.sh`, `PM_RELAY=1`); **NOT RUN** — needs a linked Unity
      Cloud project. Not a pass until the script is green.
- [ ] PlayMode: HUD reacts to state changes. — Client HUD shows replicated values;
      local role + role-filtered subsystem diagnostics via `DiagnosticPanel` (pure
      text covered by EditMode `DiagnosticPanelTests`; not GUI-asserted). Input-driven
      HUD polish pending.

## Manual multiplayer QA (real remote required for final validation)

- [ ] 2 players.  [ ] 3 players.  [ ] 4 players.
- [ ] High latency + packet loss simulation.
- [ ] Host leaves in lobby → clear message, no freeze.
- [ ] Host leaves during mission → clean end, clients to menu. — adapter maps
      `Deleted`/`RemovedFromSession` to `Disconnected`; UI surfacing pending.
- [ ] Client leaves mid-interaction → no stale objects.
- [ ] Invalid / expired join code → actionable error + retry. — `SessionErrorText`
      maps `SessionNotFound`/`InvalidSessionIdentifier` to "invalid code" (unit-tested);
      round-trip pending live.
- [ ] Session service unavailable → actionable error. — maps `MissingAssembly` /
      init failure to "unavailable"; pending live.
- [ ] Client spams interaction requests → host rate-limits.
- [ ] Two clients interact with the same panel simultaneously.
- [ ] Two clients repair the same incident simultaneously.
- [ ] Mission ends while a client is joining.
- [ ] Hosting twice / duplicate session → `SessionConflict` / `AllocationAlreadyExists`
      mapped to "already hosting" (unit-tested).

## Player-experience checks

- [ ] Objective understood within ~60 seconds.
- [ ] Each failure has a visible, explainable cause.
- [ ] Every role has meaningful work at 2/3/4 players.
- [ ] Incident report matches what actually happened.
- [ ] No sustained period where a role has nothing to do.
- [ ] Players voluntarily ask to replay.

## Severity

P0 crash/corruption/exploit/data loss · P1 cannot join/finish, desync, softlock ·
P2 broken cosmetic/UI · P3 polish. A phase is not done while a P0/P1 blocks it.
