param($installPath, $toolsPath, $package, $project)

foreach ($reference in $project.Object.References) 
{
	if ($reference.Name -eq "AlteryxGuiToolkit") 
	{
		Write-Host "AlteryxGuiToolkit Found"
		exit
	}
}

Write-Host "Finding Alteryx Install Location..."
$reg = Get-ItemProperty HKLM:\SOFTWARE\WOW6432Node\SRC\Alteryx -ErrorAction SilentlyContinue
if ($reg -eq $null) {
	$reg = Get-ItemProperty HKCU:\SOFTWARE\SRC\Alteryx -ErrorAction SilentlyContinue
}

if ($reg -eq $null) {
	throw "Couldn't Find Alteryx. You Need Alteryx Installed"
}

$dir = $reg.InstallDir64
Write-Host "Found " $dir


Write-Host "Adding a reference to Alteryx Dlls to the project"
$project.Object.References.Add($dir + "\AlteryxGuiToolkit.dll")
foreach ($reference in $project.Object.References) 
{
	if ($reference.Name -eq "AlteryxGuiToolkit") 
	{
		$reference.CopyLocal = $false;
	}
}