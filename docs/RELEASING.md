# Releasing ModularGameOverlay

ModularGameOverlay releases are built by GitHub Actions from immutable semantic
version tags. The workflow publishes a ready-to-run Windows executable, so end
users do not need the source tree or the .NET SDK. Because the repository is
private, its releases and assets remain visible only to users who can access the
repository unless the repository visibility changes.

## Published assets

Every release contains these manually uploaded assets:

- `ModularGameOverlay-win-x64.exe` — self-contained, single-file Windows x64
  application;
- `LICENSE.txt` — the application's MIT license;
- `THIRD-PARTY-NOTICES.txt` — licenses and notices for the bundled .NET runtime,
  NAudio packages, and `System.Numerics.Tensors`;
- `SHA256SUMS.txt` — SHA-256 checksums for the executable, license, and notices.

GitHub also adds automatic source archives. Users who only want the application
should download the named Windows executable from the release's **Assets**
section.

The release pages are:

```text
https://github.com/SamiKamara/ModularGameOverlay/releases
https://github.com/SamiKamara/ModularGameOverlay/releases/latest
```

After the first release, the stable direct-download URL is:

```text
https://github.com/SamiKamara/ModularGameOverlay/releases/latest/download/ModularGameOverlay-win-x64.exe
```

## Release preparation

1. Complete the relevant automated and manual checks in
   [TESTING.md](TESTING.md). Do not tag a release with an unresolved
   release-blocking issue.
2. Choose a three-part semantic version such as `0.1.0` or `0.1.1`.
3. Set `<Version>` in
   `src/ModularGameOverlay.App/ModularGameOverlay.App.csproj` to that exact
   version.
4. Move release notes out of `[Unreleased]` in `CHANGELOG.md` and add the exact
   dated heading `## [VERSION] - YYYY-MM-DD`. Update the comparison links at the
   bottom of the changelog.
5. Commit and push the release preparation to `main`.
6. Confirm the working tree is clean and local `main` exactly matches
   `origin/main`.

For the first `0.1.0` release, the project version and dated changelog entry are
already prepared. A later release must repeat the version and changelog steps.

## One-command tag process

From a clean, synchronized `main`, run:

```powershell
.\scripts\create-release.ps1 -Version 0.1.0 -Push
```

The script verifies the project version, dated changelog heading, current
branch, clean worktree, remote commit, and absence of the local and remote tag.
It then runs the complete local release build, updates the configured canonical
desktop build through `build-and-publish.ps1`, creates an annotated tag, and
pushes it when `-Push` is supplied.

Without `-Push`, the command creates only the local tag and prints the exact
push command. Pushing `vMAJOR.MINOR.PATCH` starts the **Build release** workflow.

Monitor and verify it with GitHub CLI:

```powershell
gh run list --workflow release.yml --limit 5
$runId = gh run list --workflow release.yml --limit 1 --json databaseId --jq '.[0].databaseId'
gh run watch $runId --exit-status
gh release view v0.1.0
```

Do not move, delete, or recreate a published release tag. Correct a broken
release with a new patch version. Use the manual workflow rerun below only when
the tagged source is correct and the remote build or upload needs to be
repeated.

## What the scripts and workflow verify

- The version is a three-part semantic version.
- The application project, dated changelog entry, and tag versions agree.
- Local release creation starts only from clean `main` matching `origin/main`.
- Dependencies restore successfully.
- `dotnet format` reports no changes.
- The Release build has no warnings or errors and all automated tests pass.
- Publishing produces a self-contained Windows x64 executable without debug
  symbols.
- The release executable uses the expected stable asset name.
- Application licensing, bundled dependency notices, and SHA-256 checksums are
  generated and verified.
- The workflow uploads only the four named assets, never `bin`, `obj`, runtime
  packs, test output, or the full `artifacts` tree.

The workflow uses the repository-scoped `GITHUB_TOKEN` with only
`contents: write`. No personal access token or additional repository secret is
required. If organization policy blocks release creation, allow GitHub Actions
to create repository contents in the repository's Actions workflow-permission
settings; do not replace the scoped token with a personal token by default.

## Build assets without tagging

To construct and verify the exact four release assets without creating a tag:

```powershell
.\scripts\build-release.ps1 -Version 0.1.0
```

They are written under:

```text
artifacts\release\v0.1.0
```

On the configured development machine this command also uses the canonical
local build/publish pipeline, verifies the desktop shortcut, and restarts only
the exact canonical instance if it was already running. In GitHub Actions it
uses an isolated publish directory and skips machine-specific desktop
integration.

## Manual workflow rerun

If the tag exists but its release workflow needs to be rerun:

```powershell
gh workflow run release.yml --ref main -f tag=v0.1.0
```

The workflow checks out the existing tag. If the release already exists, the
four named assets are replaced with freshly verified copies; otherwise the
release is created.

## Verify a downloaded executable

Download the executable and `SHA256SUMS.txt` into the same directory, then run:

```powershell
$assetName = 'ModularGameOverlay-win-x64.exe'
$actual = (Get-FileHash ".\$assetName" -Algorithm SHA256).Hash.ToLowerInvariant()
$line = Get-Content .\SHA256SUMS.txt |
    Where-Object { $_ -match "  $([regex]::Escape($assetName))$" }
if (-not $line) { throw "Checksum entry not found for $assetName." }
$expected = $line.Split(' ')[0].Trim()
if ($actual -ne $expected) { throw 'Checksum mismatch.' }
Write-Host "Checksum verified: $actual"
```

## Signing status

The executable is not currently Authenticode-signed. Windows SmartScreen may
therefore show an unknown-publisher warning. Signing can be added later without
changing the release asset contract: sign the release executable after publish
and before calculating `SHA256SUMS.txt`.
