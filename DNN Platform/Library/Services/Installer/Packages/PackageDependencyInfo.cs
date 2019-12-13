using System;

namespace DotNetNuke.Services.Installer.Packages
{
    [Serializable]
    public class PackageDependencyInfo
    {
	    public int PackageDependencyId { get; set; }
	    public int PackageId { get; set; }
	    public string PackageName { get; set; }
	    public Version Version { get; set; }
    }
}
