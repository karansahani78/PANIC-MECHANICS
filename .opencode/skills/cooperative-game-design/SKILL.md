---
name: cooperative-game-design
description: Use when designing or balancing PANIC MECHANICS co-op mechanics, roles, incidents, asymmetry, fail-forward rules and the incident report. Triggers on roles, Engineer/Electrician/Driver/Dispatcher, player-count scaling, co-op loop, comedy, fairness, asymmetric information, mission briefing/debrief, or 2-4 player balancing.
---

# Cooperative Game Design (PANIC MECHANICS)

## Core hook

Four players, four different views of one disaster. Nobody knows enough alone.
Short sessions (~5–7 min vertical slice), readable cause-and-effect, funny
failure, voluntary rematch.

## Product pillars (never violate)

1. Cooperative chaos — players need each other; failure is funny, not punishing.
2. Readable cause and effect — a player can explain *why* a system failed.
3. Asymmetric information — each role sees different diagnostics; no role is a
   mandatory bottleneck.
4. Short sessions.
5. Learning through play (systems thinking, root cause, communication, priority,
   experimentation) — no classroom quizzes.
6. Socially shareable outcomes (compact incident report).
7. Respectful retention — cosmetic only, no streak punishment.

## Role design rules

- Roles grant **privileged information and a small advantage**, not exclusive
  access to critical actions. 2-player teams must never be blocked by a missing
  role.
- Engineer: temperature/pressure/mechanical symptoms; engine/cooling/repair.
- Electrician: power routing, fuse health, load/voltage; breakers/relays/fuses.
- Driver: speed, braking, route; forward hazards; buys time, not root fixes.
- Dispatcher: warning summaries, logs, cross-system clues; marks suspected
  subsystem, recommends order, limited diagnostic scan; cannot repair all.
- 2 players: two information panels each, shared interactions.
- 3 players: Dispatcher summary becomes a shared lightweight panel.
- 4 players: all roles. Always allow duplicate/blended roles and lobby edits.
- **Never create a required task only one role can perform unless a fallback
  exists.**

## Fail-forward rule

A mistake makes the mission harder or funnier; it should not instantly end the
run. Reserve instant failure for clearly communicated catastrophic states.
Most errors are recoverable through teamwork.

## Incident/comedy rules

- Every consequence has a visible cause and a recovery window.
- Prefer player-created comedy over scripted cutscenes.
- Serious failure may look ridiculous, but state changes must stay readable.
- Use a small set of reusable expressive reactions, not one-off animations.

## Incident report

Generate deterministically from the event log (`MissionEvent`): top cause, best
recovery, funniest chain reaction, team rating, one learning point. No AI text in
the MVP; templates only.

## Balancing & validation

- Balance is **not proven** until playtested. State this explicitly.
- Verify at 2, 3 and 4 players that no one waits long without a meaningful action.
- Verify each role has something to do and that the mission director scales
  incident overlap/timing by active player count.

## Non-goals (first release)

No PvP, no open world, no campaign, no level editor, no procedural train, no
economy/power-ups, no real-engineering claims, no required in-game voice, no daily
streak punishment, no backend beyond sessions + local settings.

## Official references

- Blueprint §4 Roles, §5 Vertical slice, §6 Emergencies & comedy, §7 Retention:
  `PANIC_MECHANICS_BLUEPRINT.md`
- Unity Multiplayer Center (scaling guidance): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.multiplayer.center.html

## Incompatible implementations to avoid

- One-role-only critical tasks without a fallback.
- Punishing instant failure with no visible cause.
- Randomness without a readable cause.
- Feature creep into PvP/campaign/procedural before the loop is validated.
