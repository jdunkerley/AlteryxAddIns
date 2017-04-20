@echo off
powershell "gci . -Recurse | Unblock-File"
powershell "Start-Process -FilePath powershell.exe -ArgumentList '%~fs0\..\Scripts\Install.ps1', 'PROJECT', 'PROJECT' -verb RunAs -Wait"