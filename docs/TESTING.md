# Testing

## One verification command

```powershell
.\scripts\build-and-publish.ps1
```

The command performs these gates in order:

1. solution restore;
2. `dotnet format` verification without modifying files;
3. warning-free Release build;
4. all automated tests without rebuilding;
5. controlled replacement of a running instance whose exact path is the
   canonical published executable;
6. clean self-contained single-file publish;
7. desktop-shortcut target, working-directory, and SHA-256 verification;
8. restart of the canonical instance only when it was running before publish.

## Automated coverage

The latest local Release run on 2026-08-24 passed 152 tests:

| Suite | Tests | Main coverage |
| --- | ---: | --- |
| SoundDirectionVisualizer.Core.Tests | 62 | stereo and multichannel analysis, calibration, loudness, trail, and visualization models |
| SoundDirectionVisualizer.App.Tests | 76 | audio source, fallback and probing, settings, UI, display detection, and process detection |
| ModularGameOverlay.Tests | 14 | defaults, migration, idempotency, partially invalid settings, canonical/unbound/duplicate hotkeys, the eight-field editor, vertically centered hotkey text, main-window right-edge alignment, shared titles/icons, dark menu rendering, and the SuperLighter self-test |

The Release build must finish with 0 warnings and 0 errors. Tests must not be
skipped, weakened, or rewritten merely to make an implementation pass.

## Verified on this development build

- [x] Baseline builds and tests passed in all three source repositories.
- [x] The combined solution built without warnings.
- [x] All automated tests passed.
- [x] First launch migrated the three local settings files into one combined
  file.
- [x] SuperLighter started disabled, Aimoro enabled, and
  SoundDirectionVisualizer enabled.
- [x] Only `Ctrl+Alt+B` remained bound; the other seven actions were unbound.
- [x] Direct-window capture showed every required main-window control.
- [x] All three detailed settings windows opened from the host and closed
  without saving.
- [x] The centralized hotkey editor rendered eight actions with the expected
  defaults.
- [x] Hotkey text was centered in both directions, and the right-side field and
  buttons shared one alignment.
- [x] The main window, centralized hotkey editor, and three detailed module
  windows used ModularGameOverlay titles and the application icon.
- [x] The notification-area check area used the dark renderer.
- [x] Aimoro and SoundDirectionVisualizer toggles were exercised off and on;
  both states persisted immediately to the shared settings file.
- [x] A second launch retained one process and activated the existing instance.
- [x] Closing the main window left the process in the notification area; a new
  launch showed the hidden main window again.
- [x] The publish directory contained one executable whose hash matched the
  desktop shortcut target.

## Manual acceptance checklist

First stop the three legacy overlay applications so their hotkeys and overlays
do not compete with the combined host. Launch `ModularGameOverlay.lnk` from the
desktop and verify:

- [ ] The main window and all four settings windows render correctly at 100%,
  125%, and 150% Windows scaling.
- [ ] Light Enhancement `Ctrl+Alt+B` works globally, updates the main-window
  state, and restores the display correctly when disabled and on Exit.
- [ ] SuperLighter gamma, contrast, saturation, brightness overlay, and
  available monitor-brightness controls match the legacy application.
- [ ] Aimoro reticle, right-mouse hold-to-show, colors, dimensions, and center
  dot match the legacy application.
- [ ] Aimoro follows a Steam game in automatic mode, and display cycling works
  with at least two displays.
- [ ] Sound Direction Visualizer shows silence, left/right, ambience, loud, and
  trail states as expected with real game audio.
- [ ] Its Status tab reports the correct endpoint, capture method, layout,
  fallback, and event history.
- [ ] 5.1/7.1 process probing and stereo fallback work on test systems where
  those sources are available.
- [ ] All eight hotkeys can be assigned, cleared, and synchronized across the
  main, centralized, and module-specific windows.
- [ ] Duplicate and externally reserved hotkeys produce understandable errors
  without damaging other bindings. After closing the reserving application,
  clicking the ModularGameOverlay shortcut again must register the binding in
  the existing host process.
- [ ] Overlays do not steal keyboard or mouse focus and retain click-through,
  no-activation, and topmost behavior.
- [ ] Display disconnect/reconnect, session unlock, and power resume do not
  crash the host.
- [ ] Notification-area **Exit** removes every overlay, audio capture, global
  hotkey, and background resource.

For failures, record the Windows version, DPI, display topology, audio endpoint
and format, game display mode, and exact reproduction steps.

## Release asset verification

`scripts\build-release.ps1 -Version <version>` runs the same gates and also
creates the exact GitHub Release assets under `artifacts\release\v<version>`.
It verifies the generated checksum manifest before reporting success. See
[RELEASING.md](RELEASING.md) for the release-specific acceptance process.

The repository pins builds to the .NET 9 SDK family in `global.json`. CI verifies
that major-version selection before compiling so a .NET 10 or later SDK installed
on a runner cannot silently change the language version or build result. Servicing
updates and newer .NET 9 feature bands remain eligible.
