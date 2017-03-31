$root = Split-Path -Parent $PSCommandPath
Push-Location $root

Get-ChildItem *.nupkg | Remove-Item

.\nuget.exe pack .\Framework.Shared\OmniBus.Framework.Shared.csproj -Build -Symbols -IncludeReferencedProjects -ForceEnglishOutput -Properties Configuration=Release
.\nuget.exe pack .\Framework\OmniBus.Framework.csproj -Build -Symbols -IncludeReferencedProjects -ForceEnglishOutput -Properties Configuration=Release
.\nuget.exe pack .\Framework.GUI\OmniBus.Framework.GUI.csproj -Build -Symbols -IncludeReferencedProjects -ForceEnglishOutput -Properties Configuration=Release

Get-ChildItem *.nupkg | Where-Object { $_.Name -notmatch ".*\.symbols\.nupkg" } | foreach-object { & .\nuget.exe push $_.FullName -Source https://www.nuget.org/api/v2/package }

Pop-Location