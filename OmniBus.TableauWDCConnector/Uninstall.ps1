$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
..\Scripts\UninstallerHTML.ps1 .
Pop-Location