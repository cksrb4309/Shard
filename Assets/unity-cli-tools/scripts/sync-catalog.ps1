param(
    [string]$Project,
    [int]$Port = 0,
    [string]$UnityCli = "unity-cli",
    [string]$OutputPath = "catalog/tools.json"
)

$ErrorActionPreference = "Stop"

$argsList = @()
if (-not [string]::IsNullOrWhiteSpace($Project)) {
    $argsList += @("--project", $Project)
}
if ($Port -gt 0) {
    $argsList += @("--port", "$Port")
}
$argsList += "list"

$raw = & $UnityCli @argsList
if ($LASTEXITCODE -ne 0) {
    throw "unity-cli list failed with exit code $LASTEXITCODE."
}

$parsed = $raw | ConvertFrom-Json
$tools = $null
if ($parsed.PSObject.Properties.Name -contains "tools") {
    $tools = $parsed.tools
} else {
    $tools = $parsed
}

$catalog = [ordered]@{
    version = 1
    generatedAt = (Get-Date).ToUniversalTime().ToString("o")
    source = "unity-cli list"
    project = $Project
    port = $(if ($Port -gt 0) { $Port } else { $null })
    tools = $tools
}

$fullOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
$directory = [System.IO.Path]::GetDirectoryName($fullOutputPath)
if (-not [System.IO.Directory]::Exists($directory)) {
    [System.IO.Directory]::CreateDirectory($directory) | Out-Null
}

$catalog | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath $fullOutputPath -Encoding UTF8
Write-Host "Wrote $fullOutputPath"
