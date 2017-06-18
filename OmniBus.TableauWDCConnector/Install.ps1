$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
../Scripts/InstallerHTML.ps1 .
Pop-Location