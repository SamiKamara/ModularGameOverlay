# Source application baseline

The baseline was verified on 2026-08-23 before porting the modules. The legacy
repositories were not modified.

| Application | Commit | Verification | Result |
| --- | --- | --- | --- |
| SuperLighter | `71638eb` / `v1.3.0` | `dotnet build .\SuperLighter.sln -c Release` and `--self-test` | 0 warnings, 0 errors, self-test passed |
| Aimoro | `fc734ba` | `dotnet build .\Aimoro.sln -c Release` | 0 warnings, 0 errors; the source had no test project |
| SoundDirectionVisualizer | `cfbcf28` | `dotnet test .\SoundDirectionVisualizer.sln --configuration Release` | Core 62/62 and App 76/76 tests passed |

The ported source is under this repository's `src` directory. Original entry
points remain as source references but are excluded from module compilation.
`ModularGameOverlay.App` is the solution's only executable entry point.

All 138 SoundDirectionVisualizer automated tests were moved into the combined
solution without deleting or skipping tests. The SuperLighter self-test also
runs from the host test suite on an STA thread. Host tests protect Aimoro's
critical new unbound-hotkey and migration behavior.
