namespace DotNetNuke.Entities.Modules
{
    public interface IUpgradeable
    {
        string UpgradeModule(string Version);
    }
}
