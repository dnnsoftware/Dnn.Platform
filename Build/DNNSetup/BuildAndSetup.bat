SET SRC_FOLDER=C:\Git\Dnn.Platform
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" /build release %SRC_FOLDER%\Build\DNNSetup\DNNSetup.sln
"C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\IDE\devenv.exe" /build release %SRC_FOLDER%\DNN_Platform.sln

attrib -r %SRC_FOLDER%\Website\Install\Config\07.00.06.config
attrib -r %SRC_FOLDER%\Website\Install\DotNetNuke.install.config.resources

"%SRC_FOLDER%\Build\DNNSetup\bin\DNNSetup.exe" "%SRC_FOLDER%\Build\DNNSetup\bin" all
