[CmdletBinding(SupportsShouldProcess)]
param(
    [string]$TaskName = "HorseEntryDiscordNotifier",
    [string]$ProjectRoot = "",
    [string]$ThursdayTime = "16:10",
    [string]$FridayTime = "10:10",
    [string]$WeekendTime = "10:10",
    [string]$RaceDayMorningTime = "07:00"
)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Split-Path -Parent $PSScriptRoot
}
$ProjectRoot = [System.IO.Path]::GetFullPath($ProjectRoot)
$runScript = Join-Path $ProjectRoot "scripts\run-check.ps1"

if (-not (Test-Path -LiteralPath $runScript)) {
    throw "Runner script was not found: $runScript"
}

function Convert-ToTime([string]$Value) {
    return [DateTime]::ParseExact($Value, "HH:mm", [Globalization.CultureInfo]::InvariantCulture)
}

$actionArguments = "-NoProfile -ExecutionPolicy Bypass -File `"$runScript`" -ProjectRoot `"$ProjectRoot`""

if ($PSCmdlet.ShouldProcess($TaskName, "Register Windows scheduled task")) {
    $action = New-ScheduledTaskAction `
        -Execute "powershell.exe" `
        -Argument $actionArguments `
        -WorkingDirectory $ProjectRoot

    $triggers = @(
        New-ScheduledTaskTrigger -Weekly -WeeksInterval 1 -DaysOfWeek Thursday -At (Convert-ToTime $ThursdayTime)
        New-ScheduledTaskTrigger -Weekly -WeeksInterval 1 -DaysOfWeek Friday -At (Convert-ToTime $FridayTime)
        New-ScheduledTaskTrigger -Weekly -WeeksInterval 1 -DaysOfWeek Saturday, Sunday -At (Convert-ToTime $WeekendTime)
        New-ScheduledTaskTrigger -Weekly -WeeksInterval 1 -DaysOfWeek Saturday, Sunday -At (Convert-ToTime $RaceDayMorningTime)
    )

    $settings = New-ScheduledTaskSettingsSet `
        -StartWhenAvailable `
        -MultipleInstances IgnoreNew `
        -ExecutionTimeLimit (New-TimeSpan -Minutes 30)

    Register-ScheduledTask `
        -TaskName $TaskName `
        -Action $action `
        -Trigger $triggers `
        -Settings $settings `
        -Description "Checks JRA-VAN race entries and notifies Discord for registered horses." `
        -Force | Out-Null

    Write-Host "Registered scheduled task '$TaskName'."
    Write-Host "Verify: Get-ScheduledTask -TaskName '$TaskName'"
}
