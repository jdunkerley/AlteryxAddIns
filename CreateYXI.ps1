param($folder, $version)
$ErrorActionPreference = "Stop"

$dom = $env:userdomain
$usr = $env:username
$fullName = ([adsi]"WinNT://$dom/$usr,user").fullname

$folder = Resolve-Path $folder
$name = Split-Path $folder -Leaf
$parent = Split-Path $folder -Parent

$temp = $([System.IO.Path]::GetTempFileName())
Remove-Item $temp
New-Item $temp -type directory
push-location $temp
New-Item $name -type directory

$config = Get-Content "$folder\${name}Config.xml"
$category = @(@($config -split "\n") -match '.*CategoryName.*')[0] -replace ".*<CategoryName>(.*)</CategoryName>.*",'$1'
$description = @(@($config -split "\n") -match '.*Description.*')[0] -replace ".*<Description>(.*)</Description>.*",'$1'

$configFile = Join-Path $temp "Config.xml"

Set-Content $configFile -Value '<?xml version="1.0"?>'
Add-Content $configFile -Value '<AlteryxJavaScriptPlugin>'
Add-Content $configFile -Value '    <Properties>'
Add-Content $configFile -Value '        <MetaInfo>'
Add-Content $configFile -Value "            <Name>$name</Name>"
Add-Content $configFile -Value "            <Description>$description</Description>"
Add-Content $configFile -Value "            <ToolVersion>$version</ToolVersion>"
Add-Content $configFile -Value "            <CategoryName>$category</CategoryName>"
Add-Content $configFile -Value "            <Author>$fullName</Author>"
Add-Content $configFile -Value "           <Icon>$name.png</Icon>"
Add-Content $configFile -Value '        </MetaInfo>'
Add-Content $configFile -Value '    </Properties>'
Add-Content $configFile -Value '</AlteryxJavaScriptPlugin>'

Get-ChildItem $folder -Exclude '*.ts', '*.map', 'node_modules', '*.bak', 'yarn.lock', 'package.json', 'ts*.json' | Copy-Item -Destination "$temp\$name"
Compress-Archive -Path ("$folder\$name.png", $configFile, "$temp\$name") -DestinationPath "$temp\$name.zip" -Verbose -Update

$target = "$parent\${name}_$version.yxi"
if (Test-Path $target) {
    Remove-Item $target
}

Move-Item "$temp\$name.zip" $target
Pop-Location
Remove-Item $temp -Recurse -Force
