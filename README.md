# ModularGameOverlay

<p align="center">
  <img src="src/ModularGameOverlay.App/Assets/ModularGameOverlay.png" alt="ModularGameOverlay icon" width="112" height="112">
</p>

ModularGameOverlay combines SuperLighter, Aimoro, and
SoundDirectionVisualizer into one Windows game-overlay utility. A single host
process provides one notification-area icon, a simple control panel, three
detailed module settings windows, and centralized global hotkeys.

## Download

Tagged builds are published on the
[GitHub Releases page](https://github.com/SamiKamara/ModularGameOverlay/releases).
After the first release, the newest ready-to-run build is always available from
the [latest release](https://github.com/SamiKamara/ModularGameOverlay/releases/latest)
or from the stable
[Windows x64 download](https://github.com/SamiKamara/ModularGameOverlay/releases/latest/download/ModularGameOverlay-win-x64.exe).

The release executable is self-contained and does not require a separate .NET
installation. Download `SHA256SUMS.txt` from the same release to verify it.
Releases also include the application license and bundled third-party notices.
The executable is not currently code-signed, so Windows SmartScreen may show an
unknown-publisher warning.

Requirements:

- Windows 10 or Windows 11, x64;
- .NET SDK 9.0.312 only when building from source. The repository's
  `global.json` selects this version so newer installed SDKs cannot silently
  change compiler behavior.

Direct detected-game process audio capture requires Windows 10 version 2004
(build 19041) or newer. If direct activation is unavailable, the sound module
retains its selected/default-output stereo fallback.

## Usage

The main window lets you:

- enable or disable SuperLighter enhancement, the Aimoro reticle, and the sound
  direction overlay;
- edit the important Light Enhancement hotkey directly;
- open the original-style detailed settings for every module;
- edit all eight current hotkey actions in one centralized window.

The right-side hotkey and settings controls share a consistent alignment. Every
settings window uses `ModularGameOverlay - ...` titles, the application icon,
and a dark visual language; the notification-area menu and its check marks use
the same theme.

On first launch, the application migrates detailed settings from the legacy
files into:

```text
%AppData%\ModularGameOverlay\settings.json
```

The legacy files remain unchanged. Only Light Enhancement's `Ctrl+Alt+B` is
bound by default; the other seven hotkeys start unbound.

Closing the main window leaves the application running in the notification
area. Use **Exit** from that menu to stop the host and all modules cleanly.

## Local build, test, and publish

On the configured development machine, run the complete verification and local
publish pipeline with:

```powershell
.\scripts\build-and-publish.ps1
```

The script restores dependencies, verifies formatting, performs a warning-free
Release build, runs all three test suites, creates a self-contained single-file
Windows x64 publish, and verifies the configured desktop shortcut and SHA-256
hash. The canonical local executable is:

```text
artifacts\publish\win-x64\ModularGameOverlay.exe
```

Application icon assets can be regenerated from the master SVG geometry with:

```powershell
python .\scripts\generate-icon-assets.py
```

## GitHub Releases

GitHub Releases are built from immutable `vMAJOR.MINOR.PATCH` tags by GitHub
Actions, using the same release verification and asset contract as
SoundDirectionVisualizer. To verify release assets locally for the current
project version:

```powershell
.\scripts\build-release.ps1 -Version 0.1.0
```

From a clean, synchronized `main`, maintainers can validate, tag, and push a
release with:

```powershell
.\scripts\create-release.ps1 -Version 0.1.0 -Push
```

See [the release guide](docs/RELEASING.md) before creating a tag. It documents
version and changelog preparation, generated assets, workflow monitoring,
manual reruns, checksum verification, and the current signing status.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Source application baseline](docs/BASELINE.md)
- [Feature parity](docs/PARITY.md)
- [Testing and manual smoke tests](docs/TESTING.md)
- [Release process](docs/RELEASING.md)

Every human and agent contributing to the project must read
[AGENTS.md](AGENTS.md) before making changes. Documentation and tests are
maintained product contracts.

## License

ModularGameOverlay is available under the [MIT License](LICENSE).
