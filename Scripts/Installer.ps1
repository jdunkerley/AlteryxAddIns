param($fileName, $toolGroup, $target)

$bins = @()
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}
$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -ne $null) {
    $bins += $reg.InstallDir64
}

Get-ChildItem $target | Unblock-File
$fullPath = Get-Item $target | % { $_.FullName }

$root = Split-Path -Parent $PSCommandPath

Push-Location $root

$ini = "$fileName.ini"

"[Settings]" >> $ini
"x64Path=$fullPath" >> $ini
"x86Path=$fullPath" >> $ini
"ToolGroup=$toolGroup" >> $ini

foreach ($dir in $bins) {
    Write-Host "Installing to $fileName to $dir"
	$target = Join-Path $dir "..\Settings\AdditionalPlugins\$fileName.ini"
    Copy-Item $ini -Destination $target
}

Remove-Item $ini
Pop-Location