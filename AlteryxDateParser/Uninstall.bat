@echo off

>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

if '%errorlevel%' NEQ '0' (
	echo Requesting administrative privileges...
	goto UACPrompt
) else (
	goto gotAdmin
)

:UACPrompt
	echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
	echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
	"%temp%\getadmin.vbs"
	exit /B

:gotAdmin
	FOR /F "usebackq tokens=2,* skip=2" %%L IN (
		`reg query "HKCU\SOFTWARE\SRC\Alteryx" /v LastInstallDir`
	) DO SET alteryxPath=%%M

	pushd "%alteryxPath%\..\Settings\AdditionalPlugins\"
    del JDTools.ini /Q
	popd

	echo Deleted installed config from "%alteryxPath%\..\Settings\AdditionalPlugins\JDTools.ini"
	pause