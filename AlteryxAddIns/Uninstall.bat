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
        `reg query "HKLM\SOFTWARE\Wow6432Node\SRC\Alteryx" /v InstallDir64`
    ) DO SET alteryxPath=%%M

    if '%alteryxPath%' NEQ '' (
        pushd "%alteryxPath%\..\Settings\AdditionalPlugins\"
        del JDTools.ini /Q
        echo Deleted installed config from "%alteryxPath%\..\Settings\AdditionalPlugins\JDTools.ini"
        del RoslynPlugIn.ini /Q
        echo Deleted installed config from "%alteryxPath%\..\Settings\AdditionalPlugins\RoslynPlugIn.ini"
    ) else (
        echo Please delete "JDTools.ini" and "RoslynPlugIn.ini" from <AlteryxInstallDir>\Settings\AdditionalPlugins
    )

    popd
    pause