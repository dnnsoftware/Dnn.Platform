namespace DotNetNuke.Entities.Portals
{
    public interface IPortalSettingHandlers
    {
        void PortalSettingUpdated(object sender, PortalSettingUpdatedEventArgs args);
    }
}
