$root = Split-Path -Parent $PSCommandPath
Push-Location $root
.\Installer.ps1 "OmniBus" "OmniBus" "..\AddIns"
.\Installer.ps1 "OmniBus.Roslyn" "OmniBus" "..\Roslyn"
.\InstallerHTML.ps1 "OmniBusRegex"
.\Uninstaller.ps1 "JDTools"
.\Uninstaller.ps1 "RoslynPlugIn"
Pop-Location