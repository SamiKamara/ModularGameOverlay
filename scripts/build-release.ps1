[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$projectPath = Join-Path $repositoryRoot 'src\ModularGameOverlay.App\ModularGameOverlay.App.csproj'
$artifactsRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts'))
$releaseRoot = [IO.Path]::GetFullPath((Join-Path $artifactsRoot 'release'))
$releaseDirectory = [IO.Path]::GetFullPath((Join-Path $releaseRoot "v$Version"))
$workflowPublishDirectory = Join-Path $releaseDirectory 'publish'
$releaseExecutable = Join-Path $releaseDirectory 'ModularGameOverlay-win-x64.exe'
$releaseLicense = Join-Path $releaseDirectory 'LICENSE.txt'
$thirdPartyNotices = Join-Path $releaseDirectory 'THIRD-PARTY-NOTICES.txt'
$checksumFile = Join-Path $releaseDirectory 'SHA256SUMS.txt'

if (-not $releaseDirectory.StartsWith(
        $releaseRoot + [IO.Path]::DirectorySeparatorChar,
        [StringComparison]::OrdinalIgnoreCase)) {
    throw "Unsafe release directory: $releaseDirectory"
}

[xml]$project = Get-Content -LiteralPath $projectPath
$projectVersion = $project.SelectSingleNode('//Version').InnerText
if ($projectVersion -ne $Version) {
    throw "Requested version '$Version' does not match project version '$projectVersion'."
}

if (Test-Path -LiteralPath $releaseDirectory) {
    Remove-Item -LiteralPath $releaseDirectory -Recurse -Force
}
New-Item -ItemType Directory -Path $releaseDirectory -Force | Out-Null

$isContinuousIntegration =
    [string]::Equals($env:CI, 'true', [StringComparison]::OrdinalIgnoreCase) -or
    [string]::Equals($env:GITHUB_ACTIONS, 'true', [StringComparison]::OrdinalIgnoreCase)

if ($isContinuousIntegration) {
    & (Join-Path $PSScriptRoot 'build-and-publish.ps1') `
        -PublishDirectory $workflowPublishDirectory `
        -SkipDesktopShortcut
    $publishedExecutable = Join-Path $workflowPublishDirectory 'ModularGameOverlay.exe'
}
else {
    & (Join-Path $PSScriptRoot 'build-and-publish.ps1')
    $publishedExecutable = Join-Path $repositoryRoot 'artifacts\publish\win-x64\ModularGameOverlay.exe'
}

if (-not (Test-Path -LiteralPath $publishedExecutable -PathType Leaf)) {
    throw "The verified publish did not produce '$publishedExecutable'."
}

Copy-Item -LiteralPath $publishedExecutable -Destination $releaseExecutable -Force
Copy-Item -LiteralPath (Join-Path $repositoryRoot 'LICENSE') -Destination $releaseLicense -Force

$projectAssetsPath = Join-Path $repositoryRoot 'src\ModularGameOverlay.App\obj\project.assets.json'
$projectAssets = Get-Content -LiteralPath $projectAssetsPath -Raw | ConvertFrom-Json
$targetFramework = $projectAssets.project.frameworks.PSObject.Properties |
    Select-Object -First 1 -ExpandProperty Value
$runtimeDependency = @($targetFramework.downloadDependencies) |
    Where-Object { $_.name -eq 'Microsoft.NETCore.App.Runtime.win-x64' } |
    Select-Object -First 1
if (-not $runtimeDependency) {
    throw 'Unable to resolve the bundled .NET runtime version from project.assets.json.'
}

$runtimeVersion = ($runtimeDependency.version.Trim('[', ']') -split ',' | Select-Object -First 1).Trim()
$runtimePackageRoot = Join-Path $env:USERPROFILE `
    ".nuget\packages\microsoft.netcore.app.runtime.win-x64\$runtimeVersion"
$runtimeNoticesPath = Join-Path $runtimePackageRoot 'THIRD-PARTY-NOTICES.TXT'
$runtimeLicensePath = Join-Path $runtimePackageRoot 'LICENSE.TXT'
if (-not (Test-Path -LiteralPath $runtimeNoticesPath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $runtimeLicensePath -PathType Leaf)) {
    throw "Bundled .NET runtime licensing files were not found under '$runtimePackageRoot'."
}

function Get-PackageVersion([string]$PackageId) {
    $prefix = "$PackageId/"
    $entry = $projectAssets.libraries.PSObject.Properties.Name |
        Where-Object { $_.StartsWith($prefix, [StringComparison]::OrdinalIgnoreCase) } |
        Select-Object -First 1
    if (-not $entry) {
        throw "Unable to resolve package '$PackageId' from project.assets.json."
    }

    return $entry.Substring($prefix.Length)
}

$naudioCoreVersion = Get-PackageVersion 'NAudio.Core'
$naudioWasapiVersion = Get-PackageVersion 'NAudio.Wasapi'
$tensorsVersion = Get-PackageVersion 'System.Numerics.Tensors'
$tensorsPackageRoot = Join-Path $env:USERPROFILE `
    ".nuget\packages\system.numerics.tensors\$tensorsVersion"
$tensorsLicensePath = Join-Path $tensorsPackageRoot 'LICENSE.TXT'
$tensorsNoticesPath = Join-Path $tensorsPackageRoot 'THIRD-PARTY-NOTICES.TXT'
if (-not (Test-Path -LiteralPath $tensorsLicensePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $tensorsNoticesPath -PathType Leaf)) {
    throw "System.Numerics.Tensors licensing files were not found under '$tensorsPackageRoot'."
}

$mitLicense = @'
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
'@

$noticeText = @"
Third-party notices for ModularGameOverlay $Version

================================================================================
.NET runtime $runtimeVersion
================================================================================

$([IO.File]::ReadAllText($runtimeLicensePath).TrimEnd())

$([IO.File]::ReadAllText($runtimeNoticesPath).TrimEnd())

================================================================================
NAudio.Core $naudioCoreVersion and NAudio.Wasapi $naudioWasapiVersion
================================================================================

Copyright (c) 2026 Mark Heath

$($mitLicense.TrimEnd())

================================================================================
System.Numerics.Tensors $tensorsVersion
================================================================================

$([IO.File]::ReadAllText($tensorsLicensePath).TrimEnd())

$([IO.File]::ReadAllText($tensorsNoticesPath).TrimEnd())
"@

$utf8WithoutBom = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText($thirdPartyNotices, $noticeText, $utf8WithoutBom)

$hashedAssets = @($releaseExecutable, $releaseLicense, $thirdPartyNotices)
$checksumLines = foreach ($asset in $hashedAssets) {
    $hash = (Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant()
    "$hash  $([IO.Path]::GetFileName($asset))"
}
$checksumLines | Set-Content -LiteralPath $checksumFile -Encoding Ascii

foreach ($asset in $hashedAssets) {
    $assetName = [IO.Path]::GetFileName($asset)
    $checksumLine = Get-Content -LiteralPath $checksumFile |
        Where-Object { $_ -match "  $([regex]::Escape($assetName))$" } |
        Select-Object -First 1
    if (-not $checksumLine) {
        throw "Generated checksum file does not contain '$assetName'."
    }

    $expectedHash = $checksumLine.Split(' ')[0].Trim()
    $actualHash = (Get-FileHash -LiteralPath $asset -Algorithm SHA256).Hash.ToLowerInvariant()
    if ($actualHash -ne $expectedHash) {
        throw "Generated release checksum verification failed for '$assetName'."
    }
}

if ($isContinuousIntegration -and (Test-Path -LiteralPath $workflowPublishDirectory)) {
    Remove-Item -LiteralPath $workflowPublishDirectory -Recurse -Force
}

Write-Host "Release assets verified in $releaseDirectory"
Get-Item -LiteralPath $releaseExecutable, $releaseLicense, $thirdPartyNotices, $checksumFile
