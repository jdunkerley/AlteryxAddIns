param($installPath, $toolsPath, $package, $project)

function setContent
{
	param ($projectItem)
	Write-Host $projectItem.FileNames(1)
	$content = (Get-Content $projectItem.FileNames(1)) -replace "PROJECT",$project.Name
	Set-Content -Path $projectItem.FileNames(1) -Value $content
	$projectItem.Properties.Item("BuildAction").Value = [int]2
	$projectItem.Properties.Item("CopyToOutputDirectory").Value = [int]2
}

Write-Host "Finding Alteryx Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -eq $null) {
	$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
}


$alteryxName = "AlteryxGuiToolkit"
$alteryxRef = $project.Object.References | Where-Object { $_.Name -eq $alteryxName }
if ($alteryxRef -eq $null) {
	if ($reg -eq $null) {
		throw "Couldn't Find Alteryx. You Need Alteryx Installed"
	}

	$dir = $reg.InstallDir64
	Write-Host "Found " $dir

	Write-Host "Adding a reference to $alteryxName Dll to the project"
	$project.Object.References.Add("$dir\$alteryxName.dll")
	$alteryxRef = $project.Object.References | Where-Object { $_.Name -eq $alteryxName }
	$alteryxRef.CopyLocal = $false
}

foreach ($projectItem in ($project.ProjectItems | Where-Object { $_.Name -eq "Install.bat" -or $_.Name -eq "Uninstall.bat" } ))
{
	setContent $projectItem
}

$scripts = $project.ProjectItems | Where-Object { $_.Name -eq "Scripts" }
foreach ($projectItem in $scripts.ProjectItems) 
{
	setContent $projectItem
}


$debug = $project.ConfigurationManager | Where-Object { $_.ConfigurationName -eq "Debug" }
if ($debug -ne $null -and $reg -ne $null) {
	$dir = $reg.InstallDir64
	$debug.Properties.Item("StartAction").Value = [int]1
	$debug.Properties.Item("StartProgram").Value = "$dir\AlteryxGui.exe"
}