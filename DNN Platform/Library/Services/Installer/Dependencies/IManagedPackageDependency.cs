using DotNetNuke.Services.Installer.Packages;

namespace DotNetNuke.Services.Installer.Dependencies
{
    public interface IManagedPackageDependency
    {
        PackageDependencyInfo PackageDependency { get; set; }
    }
}
