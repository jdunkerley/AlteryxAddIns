param($fileName)
#Uninstaller for OmniBus.XmlTools

$bins = @()

Write-Host "Finding Alteryx Admin Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}

Write-Host "Finding Alteryx User Install Location..."
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}

foreach ($dir in $bins) {
    Write-Host "Uninstalled " $dir
	$target = "$dir\..\Settings\AdditionalPlugins\$fileName.ini"
    Remove-Item -Path $target
}
