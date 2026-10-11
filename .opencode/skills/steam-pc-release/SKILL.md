---
name: steam-pc-release
description: Use when planning PANIC MECHANICS PC/Steam build, distribution, Steamworks integration, store assets or playtest builds. Triggers on Steam, Steamworks, Steam Direct, build pipeline, IL2CPP, Windows build, store page, capsule, trailer, crash reporting, analytics, or release checklist.
---

# Steam / PC Release (PANIC MECHANICS)

## Timing rule

Do **not** build the Steam marketing package before a stable playable loop
exists. Steam integration happens after the vertical slice (Milestone 6), not
during the first code sprint.

## Platform baseline

- Target: Windows PC first, Steam distribution.
- Build with Unity 6.3 LTS, IL2CPP, x86_64, Universal Windows build support.
- Use Git + Git LFS for large binary assets (install `git-lfs` first; it is not
  currently installed on this machine).

## Deferred until demonstrated need

- Steamworks integration (Steam invites, lobbies, networking sockets, cloud).
- Paid third-party assets, external backends, dedicated servers.
- Crash reporting and analytics require an explicit opt-in decision.

Keep networking behind `IGameSessionService` so a Steam provider can be added
without rewriting gameplay.

## Pre-release checklist (after vertical slice)

- 30–60s clip with clear escalation, funny repair mistake, team recovery.
- One-sentence multiplayer hook; ≥3 real-player moments on the store page.
- Invite a few co-op creators to test; never buy fake wishlists/reviews.
- Use a demo/playtest for qualitative feedback and store interest.
- Re-check current Steam Direct rules/fees and Steamworks networking/invite
  options at release time (platform rules change).
- Never claim the game is educational, therapeutic, or simulating real machinery.

## Build/validation requirements

- Reproducible build from a clean checkout with locked package versions.
- Documented known limitations for any external playtest build.
- `Docs/QA_CHECKLIST.md` (host/join, disconnect, results consistency) executed on
  a real remote connection, not only local.

## Official references

- Blueprint §22 Steam/distribution, §19 Milestone 6:
  `PANIC_MECHANICS_BLUEPRINT.md`
- Steam Networking Sockets (future integration): https://partner.steamgames.com/doc/api/ISteamNetworkingSockets
- Unity Windows build / IL2CPP: https://docs.unity3d.com/6000.3/Documentation/Manual/WindowsBuild.html
- Git LFS: https://git-lfs.com/

## Incompatible implementations to avoid

- Building store/marketing assets before the loop is proven fun.
- Adding Steamworks or external backend services before documented need.
- Trusting player-hosted results for competitive global leaderboards.
- Shipping without locked package versions or documented known limitations.
