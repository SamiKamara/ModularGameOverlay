[CmdletBinding()]
param(
    [string]$PublishDirectory,

    [switch]$SkipDesktopShortcut
)

$ErrorActionPreference = 'Stop'
$workspace = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$solution = Join-Path $workspace 'ModularGameOverlay.sln'
$project = Join-Path $workspace 'src\ModularGameOverlay.App\ModularGameOverlay.App.csproj'
$canonicalPublishDirectory = Join-Path $workspace 'artifacts\publish\win-x64'
if ([string]::IsNullOrWhiteSpace($PublishDirectory)) {
    $PublishDirectory = $canonicalPublishDirectory
}
$publishDirectory = [IO.Path]::GetFullPath($PublishDirectory)
$publishedExecutable = Join-Path $publishDirectory 'ModularGameOverlay.exe'
$shortcutPath = 'C:\Users\samin\Desktop\ModularGameOverlay.lnk'

if (-not $SkipDesktopShortcut -and -not [string]::Equals(
        $publishDirectory,
        [IO.Path]::GetFullPath($canonicalPublishDirectory),
        [StringComparison]::OrdinalIgnoreCase)) {
    throw 'Desktop integration is allowed only for the canonical local publish directory.'
}

$runningPublishedInstances = @()
if (-not $SkipDesktopShortcut) {
    $runningPublishedInstances = @(
        Get-CimInstance Win32_Process -Filter "Name = 'ModularGameOverlay.exe'" -ErrorAction SilentlyContinue |
            Where-Object {
                $_.ExecutablePath -and
                [string]::Equals(
                    [IO.Path]::GetFullPath($_.ExecutablePath),
                    $publishedExecutable,
                    [StringComparison]::OrdinalIgnoreCase)
            }
    )
}
$restartAfterPublish = $runningPublishedInstances.Count -gt 0

dotnet restore $solution
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

dotnet format $solution --verify-no-changes --no-restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet format verification failed.' }

dotnet build $solution --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw 'Release build failed.' }

dotnet test $solution --configuration Release --no-build
if ($LASTEXITCODE -ne 0) { throw 'Release tests failed.' }

foreach ($instance in $runningPublishedInstances) {
    Stop-Process -Id $instance.ProcessId -Force
}
if ($runningPublishedInstances.Count -gt 0) {
    Wait-Process -Id $runningPublishedInstances.ProcessId -ErrorAction SilentlyContinue
}

$publishRoot = [IO.Path]::GetFullPath((Join-Path $workspace 'artifacts'))
$resolvedPublishDirectory = [IO.Path]::GetFullPath($publishDirectory)
if (-not $resolvedPublishDirectory.StartsWith(
        $publishRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Unsafe publish directory: $resolvedPublishDirectory"
}
if (Test-Path -LiteralPath $resolvedPublishDirectory) {
    $removed = $false
    for ($attempt = 1; $attempt -le 40; $attempt++) {
        try {
            Remove-Item -LiteralPath $resolvedPublishDirectory -Recurse -Force -ErrorAction Stop
            $removed = $true
            break
        }
        catch [System.IO.IOException], [System.UnauthorizedAccessException] {
            if ($attempt -eq 40) { throw }
            Start-Sleep -Milliseconds 250
        }
    }
    if (-not $removed) { throw "Could not clear publish directory: $resolvedPublishDirectory" }
}

dotnet publish $project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --no-build `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    --output $publishDirectory
if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
if (-not (Test-Path -LiteralPath $publishedExecutable)) {
    throw "Published executable was not created: $publishedExecutable"
}

$hash = (Get-FileHash -LiteralPath $publishedExecutable -Algorithm SHA256).Hash
$timestamp = (Get-Item -LiteralPath $publishedExecutable).LastWriteTime
Write-Host "Published: $publishedExecutable"
Write-Host "SHA256: $hash"
Write-Host "Timestamp: $timestamp"

if (-not $SkipDesktopShortcut) {
    $shell = New-Object -ComObject WScript.Shell
    $shortcut = $shell.CreateShortcut($shortcutPath)
    $shortcut.TargetPath = $publishedExecutable
    $shortcut.WorkingDirectory = $publishDirectory
    $shortcut.IconLocation = "$publishedExecutable,0"
    $shortcut.Description = 'Launch the latest ModularGameOverlay local build'
    $shortcut.Save()

    $verifiedShortcut = $shell.CreateShortcut($shortcutPath)
    if (-not [string]::Equals(
            [IO.Path]::GetFullPath($verifiedShortcut.TargetPath),
            $publishedExecutable,
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "Desktop shortcut target verification failed: $($verifiedShortcut.TargetPath)"
    }

    Write-Host "Shortcut: $shortcutPath -> $($verifiedShortcut.TargetPath)"
}

if ($restartAfterPublish) {
    Start-Process -FilePath $shortcutPath
}
