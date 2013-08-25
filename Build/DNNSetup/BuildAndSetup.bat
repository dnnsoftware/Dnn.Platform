SET SRC_FOLDER=C:\Git\Dnn.Platform
devenv /build release C:\Git\Dnn.Build\DNNSetup\DNNSetup.sln
devenv /build release %SRC_FOLDER%\DNN_Platform.sln

attrib -r %SRC_FOLDER%\Website\Install\Config\07.00.06.config
attrib -r %SRC_FOLDER%\Website\Install\DotNetNuke.install.config.resources

C:\Git\Dnn.Build\DNNSetup\bin\DNNSetup.exe C:\Git\Dnn.Build\DNNSetup\bin all
