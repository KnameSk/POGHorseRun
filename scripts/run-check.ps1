[CmdletBinding()]
param(
    [string]$ProjectRoot = "",
    [string]$DotnetPath = "",
    [string]$From = "",
    [string]$To = "",
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Split-Path -Parent $PSScriptRoot
}
$ProjectRoot = [System.IO.Path]::GetFullPath($ProjectRoot)
$projectFile = Join-Path $ProjectRoot "src\HorseEntryNotifier.Console\HorseEntryNotifier.Console.csproj"
$logsDirectory = Join-Path $ProjectRoot "logs"

if (-not (Test-Path -LiteralPath $projectFile)) {
    throw "Console project was not found: $projectFile"
}

if ([string]::IsNullOrWhiteSpace($DotnetPath)) {
    $localDotnet = Join-Path $ProjectRoot ".dotnet\dotnet.exe"
    $DotnetPath = if (Test-Path -LiteralPath $localDotnet) { $localDotnet } else { "dotnet" }
}

New-Item -ItemType Directory -Path $logsDirectory -Force | Out-Null
$arguments = @(
    "run",
    "--project", $projectFile,
    "--configuration", "Release",
    "--no-restore",
    "-p:NuGetAudit=false",
    "--",
    "check"
)

if (-not [string]::IsNullOrWhiteSpace($From)) { $arguments += @("--from", $From) }
if (-not [string]::IsNullOrWhiteSpace($To)) { $arguments += @("--to", $To) }
if ($DryRun) { $arguments += "--dry-run" }

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$logPath = Join-Path $logsDirectory "check-$timestamp.log"

Push-Location $ProjectRoot
try {
    $previousErrorActionPreference = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    & $DotnetPath @arguments 2>&1 | Tee-Object -FilePath $logPath
    $exitCode = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorActionPreference
    if ($exitCode -ne 0) {
        throw "Race entry check failed with exit code $exitCode. Log: $logPath"
    }
}
finally {
    if ($null -ne $previousErrorActionPreference) {
        $ErrorActionPreference = $previousErrorActionPreference
    }
    Pop-Location
}
