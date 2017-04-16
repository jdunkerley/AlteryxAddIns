param($target)
$ErrorActionPreference = "Stop"

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
    $bin = Join-Path $dir "HtmlPlugins\$target"
    if (Test-PAth $bin) {
        Write-Host "Removing Existing Link $bin"
        $cmd = "/c rmdir ""$bin"""
        Start-Process cmd -ArgumentList $cmd -verb RunAs -wait
    }
}