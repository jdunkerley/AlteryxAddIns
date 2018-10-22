param($fileName, $toolGroup)
#Installer for OmniBus.Reporting

$root = Split-Path -Parent $PSCommandPath
$target = Split-Path -Parent $root
$ini = "$root\$fileName.ini"

Get-ChildItem $target | Unblock-File

"[Settings]" >> $ini
"x64Path=$target" >> $ini
"x86Path=$target" >> $ini
"ToolGroup=$toolGroup" >> $ini

Write-Host "Finding Alteryx Admin Install Location..."
$bins = @()

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
    Write-Host "Installing to " $dir
	$target = "$dir\..\Settings\AdditionalPlugins\$fileName.ini"
    Copy-Item $ini -Destination $target
}

Remove-Item $ini
