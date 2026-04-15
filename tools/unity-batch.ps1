[CmdletBinding()]
param(
    [ValidateSet("compile", "smoke", "editmode", "playmode", "validate")]
    [string]$Task = "validate",
    [string]$ProjectPath,
    [string]$UnityExe,
    [string]$LogDirectory,
    [string]$ResultsDirectory,
    [switch]$IncludeEditModeTests,
    [switch]$IncludePlayModeTests,
    [switch]$StrictBuildSettings
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-ProjectVersion {
    param([string]$ResolvedProjectPath)

    $versionFile = Join-Path $ResolvedProjectPath "ProjectSettings\ProjectVersion.txt"
    if (-not (Test-Path $versionFile)) {
        throw "ProjectVersion.txt was not found at $versionFile"
    }

    $line = Get-Content $versionFile | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
    if (-not $line) {
        throw "m_EditorVersion was not found in $versionFile"
    }

    return ($line -split ":", 2)[1].Trim()
}

function Get-UnityHubSecondaryRoot {
    $pathFile = Join-Path $env:APPDATA "UnityHub\secondaryInstallPath.json"
    if (-not (Test-Path $pathFile)) {
        return $null
    }

    try {
        $raw = Get-Content $pathFile -Raw
        $parsed = $raw | ConvertFrom-Json
        if ($parsed) {
            return [string]$parsed
        }
    }
    catch {
        $trimmed = (Get-Content $pathFile -Raw).Trim().Trim('"')
        if ($trimmed) {
            return $trimmed
        }
    }

    return $null
}

function Resolve-UnityEditorPath {
    param(
        [string]$ResolvedProjectPath,
        [string]$RequestedUnityExe
    )

    $version = Get-ProjectVersion -ResolvedProjectPath $ResolvedProjectPath
    $candidates = New-Object System.Collections.Generic.List[string]

    foreach ($directPath in @($RequestedUnityExe, $env:UNITY_EXE)) {
        if ($directPath) {
            $candidates.Add($directPath)
        }
    }

    $roots = New-Object System.Collections.Generic.List[string]
    foreach ($root in @(
        $env:UNITY_EDITOR_ROOT,
        (Get-UnityHubSecondaryRoot),
        "C:\01_Program\UnityEditor",
        "C:\Program Files\Unity\Hub\Editor",
        "C:\Program Files\Unity\Editor"
    )) {
        if ($root -and (Test-Path $root)) {
            $roots.Add($root)
        }
    }

    foreach ($root in ($roots | Select-Object -Unique)) {
        if ((Split-Path $root -Leaf) -eq $version) {
            $candidates.Add((Join-Path $root "Editor\Unity.exe"))
        }

        $candidates.Add((Join-Path $root "$version\Editor\Unity.exe"))
    }

    foreach ($candidate in ($candidates | Select-Object -Unique)) {
        if ($candidate -and (Test-Path $candidate)) {
            return (Resolve-Path $candidate).Path
        }
    }

    throw "Unity editor for version $version could not be found. Pass -UnityExe or set UNITY_EXE."
}

function Ensure-Directory {
    param([string]$Path)

    if (-not (Test-Path $Path)) {
        New-Item -ItemType Directory -Path $Path | Out-Null
    }
}

function Invoke-UnityBatch {
    param(
        [string]$ResolvedUnityExe,
        [string[]]$Arguments
    )

    Write-Host "Unity: $ResolvedUnityExe"
    Write-Host "Args : $($Arguments -join ' ')"

    $process = Start-Process -FilePath $ResolvedUnityExe -ArgumentList $Arguments -PassThru -Wait -NoNewWindow
    if ($process.ExitCode -ne 0) {
        throw "Unity exited with code $($process.ExitCode)"
    }
}

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

if (-not $ProjectPath) {
    $ProjectPath = Join-Path $scriptRoot ".."
}

$resolvedProjectPath = (Resolve-Path $ProjectPath).Path
$resolvedUnityExe = Resolve-UnityEditorPath -ResolvedProjectPath $resolvedProjectPath -RequestedUnityExe $UnityExe

$resolvedLogDirectory = if ($LogDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($LogDirectory)
}
else {
    Join-Path $resolvedProjectPath "Logs\validation"
}

$resolvedResultsDirectory = if ($ResultsDirectory) {
    $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($ResultsDirectory)
}
else {
    Join-Path $resolvedLogDirectory "results"
}

Ensure-Directory -Path $resolvedLogDirectory
Ensure-Directory -Path $resolvedResultsDirectory

function Invoke-Compile {
    $logFile = Join-Path $resolvedLogDirectory "compile.log"
    Invoke-UnityBatch -ResolvedUnityExe $resolvedUnityExe -Arguments @(
        "-batchmode",
        "-quit",
        "-projectPath", $resolvedProjectPath,
        "-logFile", $logFile
    )
}

function Invoke-SmokeValidation {
    $logFile = Join-Path $resolvedLogDirectory "smoke.log"
    $method = if ($StrictBuildSettings) {
        "BatchValidationRunner.RunProjectSmokeValidationStrict"
    }
    else {
        "BatchValidationRunner.RunProjectSmokeValidation"
    }

    Invoke-UnityBatch -ResolvedUnityExe $resolvedUnityExe -Arguments @(
        "-batchmode",
        "-quit",
        "-projectPath", $resolvedProjectPath,
        "-executeMethod", $method,
        "-logFile", $logFile
    )
}

function Invoke-EditModeTests {
    $logFile = Join-Path $resolvedLogDirectory "editmode.log"
    Invoke-UnityBatch -ResolvedUnityExe $resolvedUnityExe -Arguments @(
        "-batchmode",
        "-quit",
        "-projectPath", $resolvedProjectPath,
        "-executeMethod", "BatchValidationRunner.RunProjectEditModeSmokeTests",
        "-logFile", $logFile
    )
}

function Invoke-PlayModeTests {
    $logFile = Join-Path $resolvedLogDirectory "playmode.log"
    $resultFile = Join-Path $resolvedResultsDirectory "playmode-results.xml"
    Invoke-UnityBatch -ResolvedUnityExe $resolvedUnityExe -Arguments @(
        "-batchmode",
        "-quit",
        "-projectPath", $resolvedProjectPath,
        "-runTests",
        "-testPlatform", "PlayMode",
        "-testResults", $resultFile,
        "-logFile", $logFile
    )
}

switch ($Task) {
    "compile" {
        Invoke-Compile
    }
    "smoke" {
        Invoke-SmokeValidation
    }
    "editmode" {
        Invoke-EditModeTests
    }
    "playmode" {
        Invoke-PlayModeTests
    }
    "validate" {
        Invoke-Compile
        Invoke-SmokeValidation

        if ($IncludeEditModeTests) {
            Invoke-EditModeTests
        }

        if ($IncludePlayModeTests) {
            Invoke-PlayModeTests
        }
    }
}
