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
    pushd %~dp0

    FOR /F "usebackq tokens=2,* skip=2" %%L IN (
        `reg query "HKLM\SOFTWARE\WOW6432Node\SRC\Alteryx" /v InstallDir64`
    ) DO SET alteryxPath=%%M

    IF "%alteryxPath%" NEQ "" (
      echo [Settings] > OmniBus.ini
      echo x64Path=%~dp0OmniBus >> OmniBus.ini
      echo x86Path=%~dp0OmniBus >> OmniBus.ini
      echo ToolGroup=Omnibus >> OmniBus.ini
      move OmniBus.ini "%alteryxPath%\..\Settings\AdditionalPlugins"

      echo [Settings] > OmniBus.XmlTools.ini
      echo x64Path=%~dp0OmniBus.XmlTools >> OmniBus.XmlTools.ini
      echo x86Path=%~dp0OmniBus.XmlTools >> OmniBus.XmlTools.ini
      echo ToolGroup=Omnibus >> OmniBus.XmlTools.ini
      move OmniBus.XmlTools.ini "%alteryxPath%\..\Settings\AdditionalPlugins"

      echo [Settings] > OmniBus.Roslyn.ini
      echo x64Path=%~dp0OmniBus.Roslyn >> OmniBus.Roslyn.ini
      echo x86Path=%~dp0OmniBus.Roslyn >> OmniBus.Roslyn.ini
      echo ToolGroup=Omnibus >> OmniBus.Roslyn.ini
      move OmniBus.Roslyn.ini "%alteryxPath%\..\Settings\AdditionalPlugins"

      mklink /J "%alteryxPath%\HtmlPlugins\OmniBusRegEx" OmniBusRegEx
    )

    FOR /F "usebackq tokens=2,* skip=2" %%L IN (
        `reg query "HKCU\SOFTWARE\SRC\Alteryx" /v InstallDir64`
    ) DO SET alteryxPath=%%M

    IF "%alteryxPath%" NEQ "" (
      echo [Settings] > OmniBus.ini
      echo x64Path=%~dp0OmniBus >> OmniBus.ini
      echo x86Path=%~dp0OmniBus >> OmniBus.ini
      echo ToolGroup=Omnibus >> OmniBus.ini
      move OmniBus.ini "%alteryxPath%\..\Settings\AdditionalPlugins"

      echo [Settings] > OmniBus.XmlTools.ini
      echo x64Path=%~dp0OmniBus.XmlTools >> OmniBus.XmlTools.ini
      echo x86Path=%~dp0OmniBus.XmlTools >> OmniBus.XmlTools.ini
      echo ToolGroup=Omnibus >> OmniBus.XmlTools.ini
      move OmniBus.XmlTools.ini "%alteryxPath%\..\Settings\AdditionalPlugins"

      echo [Settings] > OmniBus.Roslyn.ini
      echo x64Path=%~dp0OmniBus.Roslyn >> OmniBus.Roslyn.ini
      echo x86Path=%~dp0OmniBus.Roslyn >> OmniBus.Roslyn.ini
      echo ToolGroup=Omnibus >> OmniBus.Roslyn.ini
      move OmniBus.Roslyn.ini "%alteryxPath%\..\Settings\AdditionalPlugins"

      mklink /J "%alteryxPath%\HtmlPlugins\OmniBusRegEx" OmniBusRegEx
    )

    popd