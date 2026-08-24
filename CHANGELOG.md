# Changelog

## Unreleased

- Centered the shared hotkey field text vertically in both the main and global
  hotkey windows.
- Clarified the main Light Enhancement field label by naming it explicitly as a
  hotkey.
- Aligned the main window's hotkey field and dark secondary action buttons on a
  consistent right-hand grid.
- Simplified the main heading to `Modules:` and unified all settings-window
  titles and icons under ModularGameOverlay.
- Added a complete dark renderer, including checked-item glyphs, to the host
  tray menu.

## 0.1.0 - 2026-08-23

- Added the unified ModularGameOverlay WinForms and tray host.
- Imported SuperLighter, Aimoro, and SoundDirectionVisualizer as in-process modules.
- Added one settings file with first-run migration from all three legacy apps.
- Added documented module defaults and unbound-by-default hotkeys except `Ctrl+Alt+B`.
- Added the simple dark control panel and centralized eight-action hotkey editor.
- Preserved the three detailed settings windows and SoundDirectionVisualizer test suites.
- Added a four-color SVG/PNG/ICO application icon.
- Added a verified single-file Windows x64 build, test, publish, and desktop-shortcut workflow.
