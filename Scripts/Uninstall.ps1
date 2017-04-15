$root = Split-Path -Parent $PSCommandPath
Push-Location $root
.\Uninstaller.ps1 "OmniBus"
.\Uninstaller.ps1 "OmniBus.XmlTools"
.\Uninstaller.ps1 "OmniBus.Roslyn"
.\UninstallerHTML.ps1 "OmniBusRegex"
.\Uninstaller.ps1 "JDTools"
.\Uninstaller.ps1 "RoslynPlugIn"
Pop-Location