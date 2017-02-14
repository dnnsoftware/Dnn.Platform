@echo off

set builder=%ProgramFiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe
if exist "%builder%" goto build

set builder=%ProgramFiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe
if exist "%builder%" goto build

set builder=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe
if exist "%builder%" goto build

set builder=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\MSBuild.exe
if exist "%builder%" goto build

set builder=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe
if exist "%builder%" goto build

@echo .
@echo .-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-
@echo .
@echo .     Couldn't find proper "MSBuild" to build the application
@echo .     You must have VS 2015 or higher installed to build DNN.
@echo .
@echo .-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-
@echo .

goto finish

:build
@echo .
@echo .-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-
@echo .
@echo . Building application using
@echo .    "%builder%"
@echo .
@echo .-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-
@echo .

.nuget\NuGet.exe restore DNN_Platform.sln
"%builder%" /t:CreateInstall /v:m Build/BuildScripts/DNN_Package.build

:finish
set builder=
pause
