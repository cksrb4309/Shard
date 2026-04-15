[CmdletBinding()]
param(
    [string]$ProjectRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-ProjectRoot {
    param([string]$RequestedProjectRoot)

    if ($RequestedProjectRoot) {
        return (Resolve-Path $RequestedProjectRoot).Path
    }

    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $kitRoot = Split-Path -Parent $scriptDir
    return (Resolve-Path (Join-Path $kitRoot "..")).Path
}

function Read-UnityVersion {
    param([string]$ResolvedProjectRoot)

    $versionFile = Join-Path $ResolvedProjectRoot "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path $versionFile)) {
        return $null
    }

    $line = Get-Content $versionFile | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
    if (-not $line) {
        return $null
    }

    return ($line -split ":", 2)[1].Trim()
}

function Get-RelativePaths {
    param(
        [string]$ResolvedProjectRoot,
        [string[]]$Paths
    )

    foreach ($path in $Paths) {
        if (-not (Test-Path $path)) {
            continue
        }

        $relative = $path.Substring($ResolvedProjectRoot.Length).TrimStart('\')
        $relative
    }
}

$resolvedProjectRoot = Resolve-ProjectRoot -RequestedProjectRoot $ProjectRoot
$unityVersion = Read-UnityVersion -ResolvedProjectRoot $resolvedProjectRoot
$displayUnityVersion = if ($unityVersion) { $unityVersion } else { "unknown" }

$sceneFiles = @(Get-ChildItem -Path (Join-Path $resolvedProjectRoot "Assets") -Recurse -Filter *.unity -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty FullName)
$prefabFiles = @(Get-ChildItem -Path (Join-Path $resolvedProjectRoot "Assets") -Recurse -Filter *.prefab -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty FullName)
$assetConfigFiles = @(Get-ChildItem -Path (Join-Path $resolvedProjectRoot "Assets") -Recurse -Include *.asset -File -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match "Config|ScriptableObject|Unlock|Stats" } |
    Select-Object -ExpandProperty FullName)
$scriptFiles = @(Get-ChildItem -Path (Join-Path $resolvedProjectRoot "Assets") -Recurse -Filter *.cs -ErrorAction SilentlyContinue |
    Select-Object -ExpandProperty FullName)
$testFiles = @($scriptFiles | Where-Object { $_ -match "\\Tests\\|\\99_Tests\\" })
$rootDocs = @(
    Join-Path $resolvedProjectRoot "README.md"
    Join-Path $resolvedProjectRoot "AGENTS.md"
    Join-Path $resolvedProjectRoot "CLAUDE.md"
) | Where-Object { Test-Path $_ }

$topLevelAssetDirs = Get-ChildItem -Path (Join-Path $resolvedProjectRoot "Assets") -Directory -ErrorAction SilentlyContinue |
    Sort-Object Name |
    Select-Object -ExpandProperty FullName

Write-Output "# Unity Project Scan"
Write-Output ""
Write-Output "- Project root: $resolvedProjectRoot"
Write-Output "- Unity version: $displayUnityVersion"
Write-Output "- Top-level Assets directories: $((Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths $topLevelAssetDirs).Count)"
Write-Output "- Scene count: $($sceneFiles.Count)"
Write-Output "- Prefab count: $($prefabFiles.Count)"
Write-Output "- Candidate config asset count: $($assetConfigFiles.Count)"
Write-Output "- C# script count: $($scriptFiles.Count)"
Write-Output "- Test script count: $($testFiles.Count)"
Write-Output ""

Write-Output "## Root Docs"
foreach ($doc in (Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths $rootDocs)) {
    Write-Output "- $doc"
}
if (-not $rootDocs) {
    Write-Output "- none"
}
Write-Output ""

Write-Output "## Top-Level Assets"
foreach ($dir in (Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths $topLevelAssetDirs)) {
    Write-Output "- $dir"
}
if (-not $topLevelAssetDirs) {
    Write-Output "- none"
}
Write-Output ""

Write-Output "## Scenes"
foreach ($scene in (Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths ($sceneFiles | Select-Object -First 20))) {
    Write-Output "- $scene"
}
if (-not $sceneFiles) {
    Write-Output "- none"
}
Write-Output ""

Write-Output "## Candidate Validation Prefabs"
foreach ($prefab in (Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths ($prefabFiles | Where-Object { $_ -match "Scene|Combat|Bootstrap|Game" } | Select-Object -First 20))) {
    Write-Output "- $prefab"
}
Write-Output ""

Write-Output "## Candidate Config Assets"
foreach ($asset in (Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths ($assetConfigFiles | Select-Object -First 30))) {
    Write-Output "- $asset"
}
if (-not $assetConfigFiles) {
    Write-Output "- none"
}
Write-Output ""

Write-Output "## Existing Test Files"
foreach ($test in (Get-RelativePaths -ResolvedProjectRoot $resolvedProjectRoot -Paths ($testFiles | Select-Object -First 30))) {
    Write-Output "- $test"
}
if (-not $testFiles) {
    Write-Output "- none"
}
