$root = Split-Path -Parent $PSCommandPath
Push-Location $root

Get-ChildItem *.nupkg | Remove-Item

.\nuget.exe pack .\Framework.Shared\OmniBus.Framework.Shared.csproj -Build -Symbols -IncludeReferencedProjects -ForceEnglishOutput -Properties Configuration=Release
.\nuget.exe pack .\Framework\OmniBus.Framework.csproj -Build -Symbols -IncludeReferencedProjects -ForceEnglishOutput -Properties Configuration=Release
.\nuget.exe pack .\Framework.GUI\OmniBus.Framework.GUI.csproj -Build -Symbols -IncludeReferencedProjects -ForceEnglishOutput -Properties Configuration=Release

Pop-Location