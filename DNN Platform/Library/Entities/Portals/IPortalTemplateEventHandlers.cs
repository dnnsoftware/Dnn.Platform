namespace DotNetNuke.Entities.Portals
{
    public interface IPortalTemplateEventHandlers
    {
        void TemplateCreated(object sender, PortalTemplateEventArgs args);
    }
}
