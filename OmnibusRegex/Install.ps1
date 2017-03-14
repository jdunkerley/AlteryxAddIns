$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root

gci . | Unblock-File

Write-Host "Finding Alteryx Admin Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bin = $reg.InstallDir64 + '\HtmlPlugins\OmnibusRegex'
    $cmd = "/c mklink /J ""$bin"" ""$root"""
    Start-Process cmd -ArgumentList $cmd -verb RunAs -wait
}

Write-Host "Finding Alteryx User Install Location..."
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bin = $reg.InstallDir64 + '\HtmlPlugins\OmnibusRegex'
    $cmd = "/c mklink /J ""$bin"" ""$root"""
    Start-Process cmd -ArgumentList $cmd -wait
}

Pop-Location