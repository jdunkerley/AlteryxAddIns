@echo off
powershell "Start-Process -FilePath powershell.exe -ArgumentList '%~fs0\Scripts\Uninstall.ps1', 'PROJECT' -verb RunAs"