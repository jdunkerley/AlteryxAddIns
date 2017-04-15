$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root

$vsPath = .\vswhere.exe -latest -requires 'Microsoft.Component.MSBuild' -property installationPath
if ($vsPath -eq $null) {
    Write-Host "Failed to find Visual Studio Install"
    Pop-Location
    exit -1
}
$vsPath = Join-Path $vsPath 'MSBuild\15.0\Bin\MSBuild.exe'

.\nuget.exe restore .\OmniBus.AddIns.sln
& $vsPath .\OmniBus.AddIns.sln /t:Rebuild /p:Configuration=Release

Remove-Item .\Release -Recurse -Force -ErrorAction SilentlyContinue
New-Item -Path . -name "Release" -type directory

Copy-Item .\AlteryxAddIns\bin\Release .\Release -Recurse
Remove-Item .\Release\Release\Scripts -Recurse
Get-ChildItem .\Release\Release\*.bat | Remove-Item
Rename-Item .\Release\Release AddIns

Copy-Item .\AlteryxAddIns.Roslyn\bin\Release .\Release -Recurse
Remove-Item .\Release\Release\Scripts -Recurse
Get-ChildItem .\Release\Release\*.bat | Remove-Item
Rename-Item .\Release\Release Roslyn

Copy-Item .\OmniBusRegex .\Release -Recurse
Get-ChildItem .\Release\OmniBusRegex\*.bat | Remove-Item

Copy-Item .\Scripts .\Release
Get-ChildItem .\Scripts\*.bat | Move-Item .\Release

$version = Read-Host "Enter version number (e.g. 1.3.2)"
while ($version -notmatch '^\d+\.\d+\.?\d*$') {
    $version = Read-Host "Invalid Version. Enter version number (e.g. 1.3.2)"
}

Pop-Location