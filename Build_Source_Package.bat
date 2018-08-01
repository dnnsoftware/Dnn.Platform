@echo off

set builder=MSBuild.exe
where /q "%builder%"
if %ERRORLEVEL% == 0 goto build

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
@echo . Creating source package using:
@echo .    "%builder%"
@echo .
@echo .-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-
@echo .

if not exist Artifacts mkdir Artifacts

pushd Artifacts
del /f /q *Source.zip
popd
pushd "DNN Platform"
git clean -xdf
popd
pushd Website
git clean -xdf
popd

set BUILD_NUMBER=9.2.1
"%builder%" /t:CreateSource /v:n Build/BuildScripts/CreateCommunityPackages.build

:finish
set builder=
set BUILD_NUMBER=
pause
