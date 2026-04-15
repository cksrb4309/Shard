[CmdletBinding()]
param(
    [string]$ConfigPath = "$env:LOCALAPPDATA\\Packages\\Claude_pzs8sxrjxfjjc\\LocalCache\\Roaming\\Claude\\claude_desktop_config.json",
    [string]$UnityCliPath = "$env:USERPROFILE\\.local\\bin\\unity-cli-mcp.js",
    [string]$UnityPrefabParserPath = "$env:USERPROFILE\\.local\\share\\unity-prefab-parser-mcp",
    [string]$ImageToolsPath = "$env:USERPROFILE\\.local\\bin\\image-tools-mcp.exe",
    [string]$OllamaVisionPython = "$env:USERPROFILE\\.local\\share\\ollama-vision-mcp\\.venv\\Scripts\\python.exe",
    [switch]$IncludeUnityCli,
    [switch]$IncludeUnityParser,
    [switch]$IncludeImageTools,
    [switch]$IncludeOllamaVision,
    [switch]$Force
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Ensure-ObjectProperty {
    param(
        [Parameter(Mandatory = $true)] $Object,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)] $Value
    )

    if (-not $Object.PSObject.Properties[$Name]) {
        $Object | Add-Member -NotePropertyName $Name -NotePropertyValue $Value
    }
}

function Set-McpServer {
    param(
        [Parameter(Mandatory = $true)] $McpServers,
        [Parameter(Mandatory = $true)][string]$Name,
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][string[]]$Args,
        [switch]$ForceUpdate
    )

    $existing = $McpServers.PSObject.Properties[$Name]
    if ($existing -and -not $ForceUpdate) {
        Write-Host "Keeping existing MCP server '$Name'." -ForegroundColor Yellow
        return
    }

    $server = [pscustomobject]@{
        command = $Command
        args    = $Args
    }

    if ($existing) {
        $McpServers.$Name = $server
        Write-Host "Updated MCP server '$Name'." -ForegroundColor Cyan
    }
    else {
        $McpServers | Add-Member -NotePropertyName $Name -NotePropertyValue $server
        Write-Host "Added MCP server '$Name'." -ForegroundColor Green
    }
}

if (Test-Path $ConfigPath) {
    $raw = Get-Content -Raw -Encoding UTF8 $ConfigPath
    if ([string]::IsNullOrWhiteSpace($raw)) {
        $config = [pscustomobject]@{}
    }
    else {
        $config = $raw | ConvertFrom-Json
    }
}
else {
    $config = [pscustomobject]@{}
}

Ensure-ObjectProperty -Object $config -Name "mcpServers" -Value ([pscustomobject]@{})

if ($IncludeUnityParser) {
    if (-not $UnityPrefabParserPath) {
        throw "UnityPrefabParserPath is required when IncludeUnityParser is set."
    }

    Set-McpServer -McpServers $config.mcpServers `
        -Name "unity-parser" `
        -Command "node" `
        -Args @("$UnityPrefabParserPath/dist/index.js") `
        -ForceUpdate:$Force
}

if ($IncludeUnityCli) {
    if (-not (Test-Path $UnityCliPath)) {
        throw "UnityCliPath not found: $UnityCliPath"
    }

    Set-McpServer -McpServers $config.mcpServers `
        -Name "unity-cli" `
        -Command "node" `
        -Args @($UnityCliPath) `
        -ForceUpdate:$Force
}

if ($IncludeImageTools) {
    if (-not (Test-Path $ImageToolsPath)) {
        throw "ImageToolsPath not found: $ImageToolsPath"
    }

    Set-McpServer -McpServers $config.mcpServers `
        -Name "image-tools" `
        -Command $ImageToolsPath `
        -Args @() `
        -ForceUpdate:$Force
}

if ($IncludeOllamaVision) {
    if (-not (Test-Path $OllamaVisionPython)) {
        throw "OllamaVisionPython not found: $OllamaVisionPython"
    }

    Set-McpServer -McpServers $config.mcpServers `
        -Name "ollama-vision" `
        -Command $OllamaVisionPython `
        -Args @("-m", "src.server") `
        -ForceUpdate:$Force
}

$parent = Split-Path -Parent $ConfigPath
if (-not (Test-Path $parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}

$json = $config | ConvertTo-Json -Depth 20
Set-Content -Path $ConfigPath -Value $json -Encoding UTF8

Write-Host "Claude Desktop config updated: $ConfigPath" -ForegroundColor Green
