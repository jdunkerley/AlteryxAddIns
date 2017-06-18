param($folder, $category, $description)
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSCommandPath
Push-Location $root

$configFile = Join-Path $root "Config.xml"



Set-Content $configFile -Value '<?xml version="1.0"?>'
Add-Content $configFile -Value '<AlteryxJavaScriptPlugin>'
Add-Content $configFile -Value '    <Properties>'
Add-Content $configFile -Value '        <MetaInfo>'
Add-Content $configFile -Value "            <Name>$folder</Name>"
Add-Content $configFile -Value "            <Description>$description</Description>"
Add-Content $configFile -Value '            <ToolVersion>1.0.0.1</ToolVersion>'
Add-Content $configFile -Value "            <CategoryName>$category</CategoryName>"
Add-Content $configFile -Value '            <Author>James Dunkerley - Alteryx OmniBus</Author>'
Add-Content $configFile -Value '            <Icon>AlteryxOmnibusLogo.png</Icon>'
Add-Content $configFile -Value '        </MetaInfo>'
Add-Content $configFile -Value '    </Properties>'
Add-Content $configFile -Value '</AlteryxJavaScriptPlugin>'


Compress-Archive -Path ("$root\AlteryxOmnibusLogo.png", "$root\Config.xml", "$root\$folder") -DestinationPath "$folder.zip" -Verbose -Update
Remove-Item "$root\Config.xml"

Rename-Item "$folder.zip" "$folder.yxi"