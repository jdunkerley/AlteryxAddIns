@echo off
powershell "Start-Process -FilePath powershell.exe -ArgumentList '%~fs0\..\Scripts\Installer.ps1', 'PROJECT', 'PROJECT' -verb RunAs"