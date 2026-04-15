$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) {
    exit 0
}

try {
    $payload = $raw | ConvertFrom-Json -Depth 20
} catch {
    exit 0
}

function Get-StringValues {
    param([object]$Value)

    if ($null -eq $Value) {
        return @()
    }
    if ($Value -is [string]) {
        return @($Value)
    }
    if ($Value -is [System.Collections.IDictionary]) {
        $items = @()
        foreach ($entry in $Value.GetEnumerator()) {
            $items += Get-StringValues -Value $entry.Value
        }
        return $items
    }
    if ($Value -is [System.Collections.IEnumerable] -and -not ($Value -is [string])) {
        $items = @()
        foreach ($item in $Value) {
            $items += Get-StringValues -Value $item
        }
        return $items
    }

    $props = $Value.PSObject.Properties
    if ($props.Count -eq 0) {
        return @()
    }

    $items = @()
    foreach ($prop in $props) {
        $items += Get-StringValues -Value $prop.Value
    }
    return $items
}

$dangerousPatterns = @(
    '(?i)\brm(\.exe)?\b.*-(?:r|rf|fr)\b',
    '(?i)\bRemove-Item\b.*\b-Recurse\b',
    '(?i)\bRemove-Item\b.*\b-Force\b',
    '(?i)\bgit\s+reset\s+--hard\b',
    '(?i)\bgit\s+clean\s+-fdx?\b',
    '(?i)\bdel(\.exe)?\b.*\/[sqf]+\b',
    '(?i)\brmdir(\.exe)?\b.*\/[sqf]+\b',
    '(?i)\b(Set-Content|Add-Content|Out-File|New-Item|Copy-Item|Move-Item|Remove-Item)\b.*graphify-out[\\/]',
    '(?i)\b(Set-Content|Add-Content|Out-File|New-Item|Copy-Item|Move-Item|Remove-Item)\b.*Assets[\\/].*\.(md|txt|markdown)\b',
    '(?i)\b(Set-Content|Add-Content|Out-File|New-Item|Copy-Item|Move-Item|Remove-Item)\b.*(?:Library|Temp|Logs|Obj)[\\/]'
)

$strings = Get-StringValues -Value $payload
foreach ($candidate in $strings) {
    foreach ($pattern in $dangerousPatterns) {
        if ($candidate -match $pattern) {
            Write-Error "Blocked dangerous shell command by Claude hook: $candidate"
            Write-Error "Use RTK for noisy output and keep destructive or generated-artifact shell writes out of tool calls."
            exit 2
        }
    }
}

exit 0
