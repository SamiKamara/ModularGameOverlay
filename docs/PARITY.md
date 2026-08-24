# Module parity matrix

This matrix defines the behavior retained from the verified source application
baseline. The main window exposes only common toggles and the important Light
Enhancement hotkey; every setting listed below remains available in the
module's detailed settings window.

## SuperLighter

- Enhancement enabled and live preview
- gamma from 0.50 to 6.00
- contrast from 50% to 200%
- saturation from 0% to 300%
- brightness overlay from 0% to 60%
- physical brightness for DDC/CI-capable displays
- per-monitor settings persistence
- Reset display and neutral values
- Toggle enhancement and Open settings hotkeys
- restoration of gamma, color-matrix, and overlay effects on deactivation and
  Exit

Intentional host differences: Open settings starts unbound, and the toggle can
also be cleared in the detailed module window. Light Enhancement retains the
`Ctrl+Alt+B` default for new installations.

## Aimoro

- reticle enable and disable
- automatic Steam game display detection and manual display targeting
- display cycling
- hold-to-show with a selectable mouse button
- primary and outline colors
- opacity, scale, arm length, gap, and thickness
- center-dot visibility and size
- live persistence and reticle updates
- Toggle reticle, Cycle displays, and Open settings hotkeys

Intentional host differences: all three hotkeys start unbound. While the
overlay is disabled, the embedded module does not poll the mouse button.

## SoundDirectionVisualizer

- default and selected audio endpoints
- best-available verified 7.1/5.1 process probing with stereo fallback
- optional detected-game process capture
- automatic game-process fallback for sustained centered endpoint output
- automatic calibration and manual threshold, smoothing, and balance controls
- loud-sound emphasis and its threshold
- automatic Steam game display targeting and manual display selection
- overlay color, opacity, height, thickness, marker size, and offsets
- independent ambient and loud marker size, opacity, and color controls
- loud marker outline controls
- ring, ticks, current rays, current markers, listener dot, trail, and labels
- trail duration
- Status tab, capture event history, and debug channel meter
- Toggle overlay, Cycle displays, and Open settings hotkeys

The SoundDirectionVisualizer.Core tests preserve explicit stereo front/back
ambiguity, multichannel masks, all-channel analysis, calibration, loudness,
trail behavior, and marker rendering. Intentional host differences are the
three unbound hotkey defaults and stopping audio capture while the module is
disabled.

## Shared host changes

- one visible notification-area icon and one process;
- one settings file with atomic persistence;
- one centralized hotkey registry and an eight-action editor;
- one Light Enhancement binding shared by the main window, centralized editor,
  and detailed SuperLighter window;
- a compatible dark visual language and new four-color SVG/ICO product brand;
- module-specific executable entry points are not included in the release.
