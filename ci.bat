@ECHO OFF
powershell Set-ExecutionPolicy Unrestricted
powershell Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
powershell Unblock-File .\Build\CI\build.ps1
pushd .\Build\CI\

ECHO QuickDeploy CI Tool

powershell .\build.ps1 -Verbosity Minimal -Command="%1"

popd
popd
popd
popd

@ECHO ON