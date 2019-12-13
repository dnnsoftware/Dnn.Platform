namespace DotNetNuke.Entities.Portals
{
    public interface IPortalEventHandlers
    {
        void PortalCreated(object sender, PortalCreatedEventArgs args);
    }
}
