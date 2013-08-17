Please note:

In order for this component to be used in a Medium Trust environment, the DLL has been recompiled without the strong naming attribute from AssemblyInfo.cs:

[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("../ICSharpCode.SharpZipLib.key")]

In addition, the dependency on NUnit.Framework.dll was also removed.

This was done with written notification and permission from Christoph Wille of AlphaSierraPapa ( christophw@alphasierrapapa.com ) on September 23, 2004