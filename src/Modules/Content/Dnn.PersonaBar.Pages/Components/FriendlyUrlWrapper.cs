using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Components
{
    public class FriendlyUrlWrapper : IFriendlyUrlWrapper
    {
        public string CleanNameForUrl(string urlPath, FriendlyUrlOptions options, out bool modified)
        {
            return FriendlyUrlController.CleanNameForUrl(urlPath, options, out modified);
        }
        
        public void ValidateUrl(string urlPath, int tabld, PortalSettings portalSettings, out bool modified)
        {
            FriendlyUrlController.ValidateUrl(urlPath, tabld, portalSettings, out modified);
        }
    }
}