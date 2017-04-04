param($installPath, $toolsPath, $package, $project)

$alteryxName = "AlteryxRecordInfo.Net"
$alteryxRef = $project.Object.References | Where-Object { $_.Name -eq $alteryxName }
if ($alteryxRef -eq $null) {
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

	Write-Host "Adding a reference to $alteryxName Dll to the project"
	$project.Object.References.Add("$dir\$alteryxName.dll")
	$alteryxRef = $project.Object.References | Where-Object { $_.Name -eq $alteryxName }
	$alteryxRef.CopyLocal = $false
}

# Force to x64 for all modes...
