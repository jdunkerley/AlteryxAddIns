param($installPath, $toolsPath, $package, $project)

foreach ($reference in $project.Object.References) 
{
	if ($reference.Name -eq "AlteryxRecordInfo.Net") 
	{
		Write-Host "AlteryxRecordInfo.Net Found"
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
$project.Object.References.Add($dir + "\AlteryxRecordInfo.Net.dll")
foreach ($reference in $project.Object.References) 
{
	if ($reference.Name -eq "AlteryxRecordInfo.Net") 
	{
		$reference.CopyLocal = $false;
	}
}