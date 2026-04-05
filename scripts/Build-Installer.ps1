param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
$setupProject = Join-Path $repoRoot "installer\ThommyKalkulator.Setup\ThommyKalkulator.Setup.wixproj"

Write-Host "Baue MSI-Installer: $setupProject"
& dotnet build $setupProject -c $Configuration

if ($LASTEXITCODE -ne 0)
{
    throw "Der Installer-Build ist fehlgeschlagen."
}

$msiDirectory = Join-Path $repoRoot "installer\ThommyKalkulator.Setup\bin\$Configuration"
Write-Host "Fertig. Ausgabeordner: $msiDirectory"
