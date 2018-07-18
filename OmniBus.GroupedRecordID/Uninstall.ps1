$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root
..\Scripts\UninstallerHTML.ps1 $root
Pop-Location