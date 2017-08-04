param($folder, [string]$version = "1.0.0", [string[]]$folders, [string]$imagePath = "")

$ErrorActionPreference = "Stop"

$dom = $env:userdomain
$usr = $env:username
$fullName = ([adsi]"WinNT://$dom/$usr,user").fullname

if (Test-Path $folder) {
    $folder = Resolve-Path $folder
    $name = Split-Path $folder -Leaf
    $parent = Split-Path $folder -Parent
} else {
    $name = $folder
    $parent = Get-Location
}

if (Test-Path $imagePath) {
    $imagePath = Resolve-Path $imagePath
}

Write-Host "Creating $name version $version... ($parent)"

$temp = $([System.IO.Path]::GetTempFileName())
Remove-Item $temp
New-Item $temp -type directory | Out-Null
push-location $temp

$configSourceFile = "$folder\${name}Config.xml"
if (Test-Path $configSourceFile) {
    $folders += $folder
    Write-Host "Including $folder"
} else {
    $configSourceFile = "$folder.xml"
}

$configFile = Join-Path "$temp" "Config.xml"

Set-Content $configFile -Value '<?xml version="1.0"?>'
Add-Content $configFile -Value '<AlteryxJavaScriptPlugin>'
Add-Content $configFile -Value '    <Properties>'
Add-Content $configFile -Value '        <MetaInfo>'
Add-Content $configFile -Value "            <Name>$name</Name>"

if (Test-Path $configSourceFile) {
    $config = Get-Content $configSourceFile 
    $category = @(@($config -split "\n") -match '.*CategoryName.*')[0] -replace ".*<CategoryName>(.*)</CategoryName>.*",'$1'
    Add-Content $configFile -Value "            <CategoryName>$category</CategoryName>"
    $description = @(@($config -split "\n") -match '.*Description.*')[0] -replace ".*<Description>(.*)</Description>.*",'$1'
    Add-Content $configFile -Value "            <Description>$description</Description>"
} 

Add-Content $configFile -Value "            <ToolVersion>$version</ToolVersion>"
Add-Content $configFile -Value "            <Author>$fullName</Author>"
Add-Content $configFile -Value "           <Icon>$name.png</Icon>"
Add-Content $configFile -Value '        </MetaInfo>'
Add-Content $configFile -Value '    </Properties>'
Add-Content $configFile -Value '</AlteryxJavaScriptPlugin>'

if (Test-Path $imagePath) {
    Write-Host "Copying $imagePath"
    Copy-Item -Destination "$temp\$name.png" -Path "$imagePath"
}
else  {
    if (Test-Path "$folder\$name.png") {
        Write-Host "Copying $folder\$name.png"
        Copy-Item -Destination "$temp" -Path "$folder\$name.png"
    } else {
        if (Test-Path "$folder.png") {
            Write-Host "Copying $folder.png"
            Copy-Item -Destination "$temp" -Path "$folder.png"
        }
    }
}

foreach ($sourceFolder in $folders) {
    if (!(Test-Path $sourceFolder)) {
        $sourceFolder = Join-Path $parent $sourceFolder
    }
    Write-Host "Copying $sourceFolder"
    $leaf = Split-Path $sourceFolder -Leaf
    New-Item $leaf -type directory | Out-Null
    Get-ChildItem $sourceFolder -Exclude '*.ts', '*.map', 'node_modules', '*.bak', 'yarn.lock', 'package.json', 'ts*.json', '*Install.ps1' | Copy-Item -Destination "$temp\$leaf" -Recurse
}

Compress-Archive -Path "$temp\*" -DestinationPath "$temp\$name.zip" -Verbose -Update

$target = "$parent\${name}_$version.yxi"
if (Test-Path $target) {
    Remove-Item $target
}

Move-Item "$temp\$name.zip" $target
Pop-Location
Remove-Item $temp -Recurse -Force
Write-Host "Created $target"