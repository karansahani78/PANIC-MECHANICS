---
name: unity-ui-audio-feedback
description: Use when building PANIC MECHANICS menus, lobby, HUD, role diagnostic panels, results/debrief screens, interaction prompts, or audio/VFX feedback with uGUI and TextMeshPro. Triggers on UI, HUD, TextMeshPro, TMP, Canvas, lobby screen, results screen, warning alarm, SFX, VFX, accessibility, subtitles, or UI scale.
---

# Unity UI / Audio Feedback (PANIC MECHANICS)

## UI stack (pinned)

- Unity UI (uGUI): `com.unity.ugui` (core package, editor-matched).
- TextMeshPro for HUD/labels (included via uGUI).
- UI Test Framework is available in Unity 6.3 for UI Toolkit testing if needed.

UI consumes **service interfaces/events only**. UI never owns authoritative
gameplay state and never mutates train/incident state directly.

## Screens

- **01 Main menu:** Play with Friends, Join by Code, Settings, Credits, Exit.
  Actionable error + retry on connection failure. Mouse/keyboard first.
- **02 Lobby:** 4 slots, names/status, role assignment (editable, duplicates
  allowed), ready state, copy join code, mission select, host-only Start, clear
  connection/loading states.
- **10 Engine car HUD:** mission timer/progress, emergency banner, context prompt,
  interaction status/errors, role diagnostic panel, crew status/ping, pause/settings.
- **90 Results/debrief:** outcome, time, resolved vs unresolved incidents, root
  cause (from event log), best recovery, costly mistake, one learning point,
  rematch / change role / return to lobby.

## Interaction feedback rules

- Subtle highlight on valid target.
- Prompt names the action (e.g. "Hold E: Reset Breaker").
- Invalid actions explain **why** ("Reduce electrical load first") using
  `InteractionResult` feedback keys.
- Timed interactions show progress and interrupt consistently.
- Every panel interaction produces both state feedback and a sound/animation cue.

## Audio priorities

1. Distinguishable warning alarms. 2. Physical interaction SFX (lever, breaker,
   valve, tool, clank). 3. State-change SFX (pump starts, pressure stabilizes,
   trip). 4. Sparse character reactions. 5. Danger-adaptive music that never masks
   essential cues.

**Audio rule:** every critical gameplay state has a visual equivalent.

## Accessibility baseline

- Master/music/SFX volume; subtitles for authored lines and critical cues.
- Remappable interaction/movement/tool controls where feasible; UI scale option.
- Status uses icon/shape/text **plus** color — never color alone.
- Adjustable camera motion / screen shake.

## Validation requirements

- UI binds to `IGameSessionService`, `IMissionService`, `ITrainSystemService`,
  `RoleInformationService` events; does not read `NetworkVariable` internals.
- Handle loading, retry, no-session, invalid-code, disconnected, mission-ended.
- Manual test plan covering each state; screenshots or hierarchy notes returned.
- No edits to `Packages/`, gameplay/network scripts, or reserved scenes.

## Official references

- uGUI: https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.ugui.html
- TextMeshPro (uGUI): https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.textmeshpro.html
- Input System (remapping): https://docs.unity3d.com/Packages/com.unity.inputsystem@1.20/manual/index.html

## Incompatible implementations to avoid

- UI reading or writing authoritative gameplay/network state directly.
- Color-only status indicators.
- HUD logic depending on networking internals instead of service interfaces.
- Editing reserved scenes or the package manifest.
