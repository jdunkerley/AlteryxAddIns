@echo off

>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system"

if '%errorlevel%' NEQ '0' (
    if '%1' NEQ 'ELEV' (
        echo Requesting administrative privileges...
        goto UACPrompt
    ) else (
        exit /B -1
    )
) else (
    goto gotAdmin
)

:UACPrompt
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "ELEV", "", "runas", 1 >> "%temp%\getadmin.vbs"
    "%temp%\getadmin.vbs"
    exit /B

:gotAdmin
    FOR /F "usebackq tokens=2,* skip=2" %%L IN (
        `reg query "HKLM\SOFTWARE\WOW6432Node\SRC\Alteryx" /v InstallDir64`
    ) DO SET alteryxPath=%%M

    pushd "%~dp0"

    pushd Streaming
	powershell -Command "gci . | Unblock-File"
	popd

    echo [Settings]> "StreamingPlugIn.ini"
    echo x64Path=%cd%\Streaming>> "StreamingPlugIn.ini"
    echo x86Path=%cd%\Streaming>> "StreamingPlugIn.ini"
    echo ToolGroup=Streaming>> "StreamingPlugIn.ini"

    if "%alteryxPath%" NEQ "" (
        xcopy StreamingPlugIn.ini "%alteryxPath%\..\Settings\AdditionalPlugins\" /Y /Q
        del StreamingPlugIn.ini /Q
    echo Config installed to %alteryxPath%\..\Settings\AdditionalPlugins\StreamingPlugIn.ini
    ) else (
        echo Please copy "%cd$\StreamingPlugIn.ini" to <AlteryxInstallDir>\Settings\AdditionalPlugins
    )

    popd
    pause
