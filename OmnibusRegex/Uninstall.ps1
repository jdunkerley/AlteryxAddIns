$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root

Write-Host "Finding Alteryx Admin Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bin = $reg.InstallDir64 + '\HtmlPlugins\OmnibusRegex'
    $cmd = "/c rmdir ""$bin"""
    Start-Process cmd -ArgumentList $cmd -verb RunAs -wait
}

Write-Host "Finding Alteryx User Install Location..."
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bin = $reg.InstallDir64 + '\HtmlPlugins\OmnibusRegex'
    cmd /c rmdir "$bin"
}

Pop-Location