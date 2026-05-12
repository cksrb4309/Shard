param(
    [string]$CatalogPath = "catalog/tools.json",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$fullCatalogPath = [System.IO.Path]::GetFullPath($CatalogPath)
if (-not [System.IO.File]::Exists($fullCatalogPath)) {
    throw "Catalog file not found: $fullCatalogPath"
}

$catalog = Get-Content -LiteralPath $fullCatalogPath -Raw | ConvertFrom-Json
$tools = $catalog.tools
$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("# unity-cli-tools Summary")
$lines.Add("")
$lines.Add("Generated from: $CatalogPath")
$lines.Add("")

$groups = $tools | Group-Object group | Sort-Object Name
foreach ($group in $groups) {
    $name = if ([string]::IsNullOrWhiteSpace($group.Name)) { "ungrouped" } else { $group.Name }
    $lines.Add("## $name")
    $lines.Add("")

    foreach ($tool in ($group.Group | Sort-Object name)) {
        $description = if ($tool.description) { $tool.description } else { "" }
        $lines.Add("- ``$($tool.name)``: $description")

        $parameters = $tool.parameters
        if ($parameters) {
            foreach ($parameter in $parameters) {
                $required = if ($parameter.required) { " required" } else { "" }
                $type = if ($parameter.type) { $parameter.type } else { "value" }
                $lines.Add("  - ``$($parameter.name)`` ($type$required)")
            }
        }
    }

    $lines.Add("")
}

$content = $lines -join [Environment]::NewLine
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    Write-Output $content
} else {
    $fullOutputPath = [System.IO.Path]::GetFullPath($OutputPath)
    $directory = [System.IO.Path]::GetDirectoryName($fullOutputPath)
    if (-not [System.IO.Directory]::Exists($directory)) {
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    }

    Set-Content -LiteralPath $fullOutputPath -Value $content -Encoding UTF8
    Write-Host "Wrote $fullOutputPath"
}
