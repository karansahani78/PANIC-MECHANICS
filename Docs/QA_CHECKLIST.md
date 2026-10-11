# PANIC MECHANICS — QA Checklist (MVP vertical slice)

Owner: Agent 5 (Content + QA). Executed by humans/agents per milestone.

## Automated (record ACTUAL results + date; never claim unverified passes)

- [ ] Unity project opens in the pinned 6.3 LTS patch, zero compile errors.
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
- [ ] PlayMode: HUD reacts to state changes.
- [ ] PlayMode: two clients see the same incident state.
- [ ] PlayMode: invalid client interaction rejected by host.

## Manual multiplayer QA (real remote required for final validation)

- [ ] 2 players.  [ ] 3 players.  [ ] 4 players.
- [ ] High latency + packet loss simulation.
- [ ] Host leaves in lobby → clear message, no freeze.
- [ ] Host leaves during mission → clean end, clients to menu.
- [ ] Client leaves mid-interaction → no stale objects.
- [ ] Invalid / expired join code → actionable error + retry.
- [ ] Session service unavailable → actionable error.
- [ ] Client spams interaction requests → host rate-limits.
- [ ] Two clients interact with the same panel simultaneously.
- [ ] Two clients repair the same incident simultaneously.
- [ ] Mission ends while a client is joining.
- [ ] Replaying does not preserve old faults/timers.

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
