param($folder)
$ErrorActionPreference = "Stop"

$folder = Resolve-Path $folder
$name = Split-Path $folder -Leaf

Push-Location $folder

gci . | Unblock-File

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
    $bin = Join-Path $dir "HtmlPlugins\$name"
    if (Test-PAth $bin) {
        Write-Host "Removing Existing Link $bin"
        $cmd = "$cmd rmdir ""$bin"" &"
    }

    Write-Host "Creating Link $bin"
    $cmd = "$cmd mklink /J ""$bin"" ""$folder"" &"
}

if ($cmd -ne "") {
    Start-Process cmd -ArgumentList "/c $cmd" -verb RunAs -wait
}

Pop-Location
