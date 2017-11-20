param($target)
$ErrorActionPreference = "Stop"

$bins = @()
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null -and $reg.InstallDir64 -ne $null) {
    $bins += $reg.InstallDir64
}
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null -and $reg.InstallDir64 -ne $null) {
    $bins += $reg.InstallDir64
}

$cmd = ""
foreach ($dir in $bins) {
    $bin = Join-Path $dir "HtmlPlugins\$target"
    if (Test-Path $bin) {
        Write-Host "Removing Existing Link $bin"
        $cmd = "$cmd rmdir ""$bin"" &"
    }
}

if ($cmd -ne "") {
    Start-Process cmd -ArgumentList "/c $cmd" -verb RunAs -wait
}
