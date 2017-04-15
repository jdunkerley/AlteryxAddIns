param($fileName)

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
	$target = Join-Path $dir "..\Settings\AdditionalPlugins\$fileName.ini"
    if (Test-Path $target) {
        Write-Host "Uninstalled $target from $dir"
        Remove-Item -Path $target
    }
}
