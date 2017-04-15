param($folder)
$ErrorActionPreference = "Stop"

$name = Split-Path $folder -Leaf

Push-Location $folder

gci . | Unblock-File

$bins = @()
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}

foreach ($dir in $bins) {
    $bin = Join-Path $dir "HtmlPlugins\$name"
    if (Test-PAth $bin) {
        Write-Host "Removing Existing Link $bin"
        $cmd = "/c rmdir ""$bin"""
        Start-Process cmd -ArgumentList $cmd -verb RunAs -wait
    }
    
    Write-Host "Creating Link $bin"
    $cmd = "/c mklink /J ""$bin"" ""$folder"""
    Start-Process cmd -ArgumentList $cmd -verb RunAs -wait
}

Pop-Location