# Architecture

## Process and lifecycle

`ModularGameOverlay.App` is the .NET 9 Windows Forms host. It owns:

- the single-instance mutex and existing-instance activation event;
- the visible notification-area icon and menu;
- the simple main window and centralized hotkey window;
- the single `%AppData%\ModularGameOverlay\settings.json` file;
- the single Win32 `RegisterHotKey` registry;
- startup, persistence callbacks, and deterministic shutdown for all three
  modules.

The ported application-context classes run in embedded mode. Their own
notification-area icons are hidden, their own global hotkey registrations are
disabled, and settings persistence is delegated to the host. Existing module
services, overlay forms, and detailed settings forms remain in their module
assemblies.

```text
ModularGameOverlay.App
  ├─ SuperLighter.Module
  ├─ Aimoro.Module
  └─ SoundDirectionVisualizer.Module
       └─ SoundDirectionVisualizer.Core
```

`SoundDirectionVisualizer.Core` remains a platform-independent `net9.0`
project without Windows Forms or Windows dependencies. Its direction, audio,
calibration, and visualization models remain deterministically testable.

## Settings and migration

`ModularGameOverlaySettings` contains the schema version, the three existing
module `AppSettings` models, and the canonical `HotkeyConfiguration` section.
Module hotkey properties are mirrored from the canonical section so the legacy
settings forms and the centralized editor always modify the same final state.

When the combined settings file does not exist, `SettingsStore` attempts to
deserialize each legacy settings file independently. A missing or invalid
section falls back only to that module's defaults. Migration never writes to
the old files, and the presence of the combined file prevents repeated
migration.

Persistence writes a temporary file first and then moves it atomically to the
final path. I/O or permission failures do not crash the overlay host.

## Hotkeys

`GlobalHotkeyManager` owns one hidden message window. Empty bindings are not
registered. Before saving, the application validates keys and detects duplicate
assignments across all eight actions. Windows registration failures are shown
through a notification-area balloon while other successfully registered
hotkeys remain active.

Opening a detailed module settings window suspends global hotkeys while the
hotkey fields receive input. When the window closes, module settings are merged
into the canonical configuration, cross-module duplicates are rejected, and
hotkeys are registered again.

## Shared window and menu language

The host's owner-drawn hotkey control centers a binding or `Not set` both
horizontally and vertically. The main window's hotkey field, every **Detailed
settings** button, and the centralized hotkey button share a common width and
right edge. Host and module settings title bars use ModularGameOverlay naming
and the running host executable's icon.

The notification-area menu uses the host's own
`ToolStripProfessionalRenderer`. Backgrounds, selection states, separators,
check boxes, and check marks all use the shared dark palette, including nested
menus.

## Module deactivation

- SuperLighter hides the brightness overlay and restores gamma and color-matrix
  effects when enhancement is disabled or the host exits.
- Aimoro hides the reticle and stops its 25 ms input polling in embedded mode
  while the overlay is disabled.
- SoundDirectionVisualizer hides its overlay and stops audio capture, render
  timers, and target-refresh timers. They restart with the module.

## Local publication

`scripts\build-and-publish.ps1` is the canonical build path on the configured
development machine. It verifies formatting, the Release build, and all tests
before replacing the verified published instance. It cleans only a validated
subdirectory under `artifacts`, creates one self-contained executable, updates
the desktop shortcut, and restarts the application only when that exact
published executable was running before the update.

## GitHub Release flow

`.github\workflows\release.yml` runs on immutable semantic-version tags. It
checks that the tag, application project version, and dated changelog entry
agree, then invokes `scripts\build-release.ps1`. The workflow publishes only the
named Windows x64 executable, application license, third-party notices, and
checksum manifest. The workflow token is limited to `contents: write`.

`scripts\create-release.ps1` is the maintainer entry point. It accepts releases
only from a clean local `main` that exactly matches `origin/main`, rejects
existing tags, performs the complete local release build, and creates an
annotated tag. Pushing that tag delegates final release construction to GitHub
Actions. The complete contract is documented in [RELEASING.md](RELEASING.md).
