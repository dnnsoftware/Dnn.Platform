namespace DotNetNuke.Services.Installer.Packages
{
    public interface IPackageEditor
    {
        int PackageID { get; set; }
        bool IsWizard { get; set; }

        void Initialize();

        void UpdatePackage();
    }
}
