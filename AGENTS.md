# Repository instructions

- Read `README.md`, `docs/TAVOITE-JA-SUUNNITELMA.md`, and the task-relevant
  documentation before changing behavior.
- Treat project documentation and tests as maintained product contracts. Every
  accepted user-visible behavior, limitation, settings-schema, architecture, or
  workflow change requires matching documentation and relevant tests in the same
  change.
- Add a regression test for every reproducible bug fix. Do not delete, skip,
  loosen, or rewrite a test merely to make an implementation pass. If a
  requirement changes, document the accepted change and its reason before
  updating the test.
- These rules apply to every contributor, including human developers and agents.
  Hand off work only after relevant tests have run, documentation matches the
  result, and any known limitation is recorded.
- Preserve the source applications' behavior unless
  `docs/TAVOITE-JA-SUUNNITELMA.md` explicitly changes it. Use the original
  SuperLighter, Aimoro, and SoundDirectionVisualizer repositories as read-only
  parity references unless the user separately asks to modify them.
- Keep the imported SoundDirectionVisualizer core logic platform-independent and
  deterministic. Preserve its existing test coverage, explicit stereo
  front/back ambiguity, and full multichannel handling.
- Preserve overlay click-through, no-activation, and topmost behavior. Preserve
  deterministic cleanup of display state, audio capture, global hotkeys,
  background watchers, tray resources, and overlay windows.
- Centralize global hotkey registration in the host. All UI surfaces that expose
  the same binding must edit the same settings state.
- Run the full Release build and automated tests before handing off changes that
  can affect runnable or test output. Documentation-only and repository-policy-
  only changes do not require a build solely for validation.
- On this development machine, use `scripts\build-and-publish.ps1` for every
  build that produces runnable output. It runs the complete Release verification,
  creates the canonical single-file publish, and synchronizes the desktop
  shortcut required by `AGENTS.local.md`.
- If `AGENTS.local.md` exists, read and obey it before any build or publish task.
  It contains machine-specific workflow requirements and is intentionally not
  committed.
- Do not commit generated output under `bin`, `obj`, `artifacts`, `TestResults`,
  or IDE-specific state.
