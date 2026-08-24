# Changelog

All notable ModularGameOverlay changes are documented here. Versions follow
semantic versioning.

## [Unreleased]

### Fixed

- Pinned local and GitHub builds to the .NET 9 SDK family so newer major SDKs
  cannot silently select a different C# language version while .NET 9 servicing
  updates remain eligible.
- Made release changelog validation accept Windows CRLF line endings.
- Avoided a C# 14 contextual `field` keyword collision in the hotkey layout
  diagnostic property.

## [0.1.0] - 2026-08-24

### Added

- Unified ModularGameOverlay WinForms and notification-area host.
- In-process SuperLighter, Aimoro, and SoundDirectionVisualizer modules.
- One settings file with first-run migration from all three legacy apps.
- Documented module defaults and unbound-by-default hotkeys except `Ctrl+Alt+B`.
- Simple dark control panel and centralized eight-action hotkey editor.
- Preserved detailed module settings windows and SoundDirectionVisualizer test
  suites.
- Four-color SVG, PNG, and ICO application assets.
- Verified single-file Windows x64 build, test, publish, and desktop-shortcut
  workflow.
- Tag-driven GitHub Release tooling with release assets, licensing notices, and
  SHA-256 checksums.

### Changed

- Centered the shared hotkey field text vertically in both the main and global
  hotkey windows.
- Clarified the main Light Enhancement field label by naming it explicitly as a
  hotkey.
- Aligned the main window's hotkey field and dark secondary action buttons on a
  consistent right-hand grid.
- Simplified the main heading to `Modules:` and unified all settings-window
  titles and icons under ModularGameOverlay.
- Added a complete dark renderer, including checked-item glyphs, to the host
  notification-area menu.

[Unreleased]: https://github.com/SamiKamara/ModularGameOverlay/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/SamiKamara/ModularGameOverlay/releases/tag/v0.1.0
