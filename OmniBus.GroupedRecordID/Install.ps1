$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root
../Scripts/InstallerHTML.ps1 $root
Pop-Location