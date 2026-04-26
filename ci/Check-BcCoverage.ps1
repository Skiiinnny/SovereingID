param(
    [Parameter(Mandatory = $true)]
    [string]$CoberturaPath,
    [int]$Threshold = 70
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-CoberturaFiles {
    param([string]$PathPattern)

    if ($PathPattern -match '[\*\?]') {
        $matches = @(Get-ChildItem -Path . -Recurse -File -Filter (Split-Path -Path $PathPattern -Leaf) -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -like ("*" + ($PathPattern -replace '/', '\')) })
        
        if ($matches.Count -eq 0) {
            throw "Cobertura report not found at '$PathPattern'."
        }
        return @($matches | ForEach-Object { $_.FullName })
    }

    if (Test-Path -Path $PathPattern -PathType Leaf) {
        return @((Resolve-Path -Path $PathPattern).Path)
    }

    throw "Cobertura report not found at '$PathPattern'."
}

function Resolve-BoundedContext {
    param([string]$AssemblyName)

    if ([string]::IsNullOrWhiteSpace($AssemblyName)) { return $null }

    if ($AssemblyName.StartsWith("SovereignID.Auth.")) { return "bc-auth" }
    if ($AssemblyName.StartsWith("SovereignID.Issuer.")) { return "bc-issuer" }
    if ($AssemblyName.StartsWith("SovereignID.Verifier.")) { return "bc-verifier" }
    if ($AssemblyName.StartsWith("SovereignID.SharedKernel.")) { return "shared" }

    if (
        $AssemblyName.StartsWith("SovereignID.Crypto.") -or
        $AssemblyName.StartsWith("SovereignID.Chain.") -or
        $AssemblyName.StartsWith("SovereignID.Demo.Phase1")
    ) {
        return "legacy"
    }

    return $null
}

$expectedBcs = @("bc-auth", "bc-issuer", "bc-verifier", "shared")
$coverageByBc = @{}
$lineCoverageByBc = @{}
foreach ($bc in $expectedBcs) {
    $coverageByBc[$bc] = [ordered]@{
        Covered = 0.0
        Coverable = 0.0
    }
    $lineCoverageByBc[$bc] = @{}
}

$files = Resolve-CoberturaFiles -PathPattern $CoberturaPath
if ($files.Count -eq 0) {
    throw "No Cobertura files match '$CoberturaPath'."
}

foreach ($file in $files) {
    [xml]$xml = Get-Content -LiteralPath $file -Raw
    $packages = $xml.SelectNodes("//coverage/packages/package")
    foreach ($package in $packages) {
        $assemblyName = [string]$package.name
        $bc = Resolve-BoundedContext -AssemblyName $assemblyName
        if ($null -eq $bc -or $bc -eq "legacy") { continue }

        $classes = @($package.SelectNodes("./classes/class"))
        foreach ($classNode in $classes) {
            $lines = @($classNode.SelectNodes("./lines/line"))
            if ($lines.Count -eq 0) { continue }
            $fileName = [string]$classNode.filename
            foreach ($line in $lines) {
                $lineNumber = [string]$line.number
                $lineKey = "$fileName::$lineNumber"
                $hasHit = ([int]$line.hits -gt 0)

                if (-not $lineCoverageByBc[$bc].ContainsKey($lineKey)) {
                    $lineCoverageByBc[$bc][$lineKey] = $hasHit
                }
                elseif ($hasHit) {
                    # A line is considered covered if at least one report marks it as covered.
                    $lineCoverageByBc[$bc][$lineKey] = $true
                }
            }
        }
    }
}

$table = @()
$failing = @()

foreach ($bc in $expectedBcs) {
    $coverable = [double]$lineCoverageByBc[$bc].Count
    $covered = [double](@($lineCoverageByBc[$bc].Values | Where-Object { $_ }).Count)
    $coverageByBc[$bc].Covered = $covered
    $coverageByBc[$bc].Coverable = $coverable

    $covered = [math]::Round($covered, 2)
    $coverable = [math]::Round($coverable, 2)
    $pct = if ($coverable -gt 0) { [math]::Round(($covered / $coverable) * 100, 2) } else { 0.0 }

    if ($coverable -eq 0) {
        Write-Warning "$bc has no coverable lines."
        $status = "warning:no-coverable-lines"
    }
    elseif ($pct -lt $Threshold) {
        $status = "fail"
        $failing += "$bc (${pct}% < ${Threshold}%)"
    }
    else {
        $status = "pass"
    }

    $table += [pscustomobject]@{
        BC = $bc
        "covered / coverable" = "{0} / {1}" -f $covered, $coverable
        "%" = $pct
        status = $status
    }
}

Write-Host ""
Write-Host "BC coverage summary"
$table | Format-Table -AutoSize | Out-String | Write-Host

if ($failing.Count -gt 0) {
    Write-Error ("Coverage below threshold: " + ($failing -join ", "))
    exit 1
}

exit 0
