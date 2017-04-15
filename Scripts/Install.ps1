$root = Split-Path -Parent $PSCommandPath
Push-Location $root
.\Installer.ps1 "OmniBus" "OmniBus" "..\OmniBus"
.\Installer.ps1 "OmniBus.XmlTools" "OmniBus" "..\OmniBus.XmlTools"
.\Installer.ps1 "OmniBus.Roslyn" "OmniBus" "..\OmniBus.Roslyn"
.\InstallerHTML.ps1 "..\OmniBusRegex"
.\Uninstaller.ps1 "JDTools"
.\Uninstaller.ps1 "RoslynPlugIn"
Pop-Location