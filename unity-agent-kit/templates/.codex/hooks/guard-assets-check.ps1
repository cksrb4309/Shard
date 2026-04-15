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

$guardedPatterns = @(
    '(?i)\.unity$',
    '(?i)\.prefab$',
    '(?i)ProjectSettings[\\/]'
)

$strings = Get-StringValues -Value $payload
foreach ($candidate in $strings) {
    foreach ($pattern in $guardedPatterns) {
        if ($candidate -match $pattern) {
            Write-Error "Blocked guarded asset edit by Codex hook: $candidate"
            Write-Error "Use explicit approval and project guard rules before editing scenes, prefabs, or ProjectSettings."
            exit 2
        }
    }
}

exit 0
