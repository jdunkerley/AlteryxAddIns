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
    $path = $_.FullName
    Write-Host $path
    $content = (Get-Content $path) -Replace "<HintPath>.*?\\(AlteryxGuiToolkit\.dll)</HintPath>",'<HintPath>%%DIR%%\$1</HintPath>'
    $content = $content -Replace "<HintPath>.*?\\(AlteryxRecordInfo\.Net\.dll)</HintPath>",'<HintPath>%%DIR%%\$1</HintPath>'
    $content = $content -Replace "%%DIR%%","$dir"
    Set-Content -Path $path -Value $content -Force
    Write-Host $content
}

Pop-Location