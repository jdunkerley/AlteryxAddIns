param($mode)
$root = Split-Path -Parent $PSCommandPath
Push-Location $root
.\Scripts\Installer.ps1 "OmniBus" "OmniBus" ".\AlteryxAddIns\bin\$mode"
.\Scripts\Installer.ps1 "OmniBus.XmlTools" "OmniBus" ".\OmniBus.XmlTools\bin\$mode"
.\Scripts\Installer.ps1 "OmniBus.Roslyn" "OmniBus" ".\AlteryxAddIns.Roslyn\bin\$mode"
.\Scripts\InstallerHTML.ps1 "$root\OmniBusRegex"
.\Scripts\Uninstaller.ps1 "JDTools"
.\Scripts\Uninstaller.ps1 "RoslynPlugIn"
Pop-Location