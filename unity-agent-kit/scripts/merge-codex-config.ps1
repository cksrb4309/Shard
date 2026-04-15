param(
    [string]$ConfigPath = "$HOME\.codex\config.toml",
    [string]$ProjectRoot,
    [string]$UnityCliPath,
    [string]$UnityPrefabParserPath,
    [string]$OllamaVisionPython
)

if (-not (Test-Path $ConfigPath)) {
    New-Item -ItemType File -Path $ConfigPath -Force | Out-Null
}

$content = Get-Content -Path $ConfigPath -Raw -Encoding utf8

function Ensure-SectionValue {
    param(
        [string]$Section,
        [string]$Key,
        [string]$ValueLine
    )

    $script:content = $script:content
    if ($script:content -match "(?ms)^\[$([regex]::Escape($Section))\]\s*.*?^\s*$") {
        if ($script:content -match "(?ms)^\[$([regex]::Escape($Section))\]\s*.*?^\s*$" -and $script:content -match "(?m)^\s*$([regex]::Escape($Key))\s*=") {
            return
        }
        $script:content = [regex]::Replace(
            $script:content,
            "(?ms)(^\[$([regex]::Escape($Section))\]\s*\r?\n)",
            "`$1$ValueLine`r`n",
            1
        )
        return
    }

    $script:content = ($script:content.TrimEnd() + "`r`n`r`n[$Section]`r`n$ValueLine`r`n")
}

function Ensure-Block {
    param(
        [string]$Section,
        [string[]]$Lines
    )

    if ($script:content -match "(?m)^\[$([regex]::Escape($Section))\]") {
        return
    }
    $block = ($Lines -join "`r`n")
    $script:content = ($script:content.TrimEnd() + "`r`n`r`n[$Section]`r`n$block`r`n")
}

Ensure-SectionValue -Section "features" -Key "multi_agent" -ValueLine 'multi_agent = true'

if ($UnityCliPath) {
    Ensure-Block -Section "mcp_servers.unity_cli" -Lines @(
        'command = "node"',
        ('args = ["{0}"]' -f $UnityCliPath.Replace('\', '\\'))
    )
}

if ($UnityPrefabParserPath) {
    Ensure-Block -Section "mcp_servers.unity_prefab_parser" -Lines @(
        'command = "node"',
        ('args = ["{0}"]' -f ($UnityPrefabParserPath.Replace('\', '\\') + '\\dist\\index.js'))
    )
}

if ($OllamaVisionPython) {
    Ensure-Block -Section "mcp_servers.ollama_vision" -Lines @(
        ('command = "{0}"' -f $OllamaVisionPython.Replace('\', '\\')),
        'args = ["-m", "src.server"]'
    )
}

if ($ProjectRoot) {
    Ensure-Block -Section ("projects.'{0}'" -f $ProjectRoot.Replace('\', '\\')) -Lines @(
        'trust_level = "trusted"'
    )
}

Set-Content -Path $ConfigPath -Value $content -Encoding utf8
Write-Output "Updated $ConfigPath"
