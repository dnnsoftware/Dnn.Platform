using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Components
{
    public interface IUrlRewriterUtilsWrapper
    {
        FriendlyUrlOptions GetExtendOptionsForURLs(int portalId);
    }
}