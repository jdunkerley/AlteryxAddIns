@echo off
powershell "Start-Process -FilePath powershell.exe -ArgumentList '%~fs0\..\Scripts\Installer.ps1', 'OmniBus.Reporting', 'OmniBus.Reporting' -verb RunAs"
