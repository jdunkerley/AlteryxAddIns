$root = Split-Path -Parent $PSCommandPath
Write-Host "Finding Alteryx Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -eq $null) {
    $reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
}

if ($reg -eq $null) {
    Write-Host "Couldn't Find Alteryx"
    exit -1
}

Write-Host "Found " $reg.InstallDir64
$dir = $reg.InstallDir64

Push-Location $root

Get-ChildItem -File -Filter *.csproj -Recurse  | ForEach {
     (Get-Content $_.FullName ).Replace("<HintPath>.*?\\(AlteryxGuiToolkit\.dll|AlteryxRecordInfo\.Net\.dll)</HintPath>", '<HintPath>$dir\$1</HintPath>') | 
     Set-Content $_.FullName
}

Pop-Location