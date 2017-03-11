$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root

$bins = @()

Write-Host "Finding Alteryx Admin Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\AlteryxZ -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}

Write-Host "Finding Alteryx User Install Location..."
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\AlteryxZ -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}

if ($bins.Count -eq 0) {
    Write-Host "Failed to find Alteryx Install"
    Pop-Location
    exit -1
}

