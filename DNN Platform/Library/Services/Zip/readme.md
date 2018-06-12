# Zip Services in DNN

DNN uses SharpZipLib as the zip system of choice. Because of this, it is bundled with DNN. 

This folder here contains a fix for a breaking upgrade in DNN 9.2 regarding the ZIP. 
The class in this folder will be used by .net when an assembly is missing. 
It will then check if .net was looking for SharpZipLib, and if necessary, 
redirect it to the correct (new) assembly. 

Once this remapping has been completed, this code will not be used again until the next
restart of the DNN application. 