using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Components
{
    public class UrlRewriterUtilsWrapper : IUrlRewriterUtilsWrapper
    {        
        public FriendlyUrlOptions GetExtendOptionsForURLs(int portalId)
        {
            return UrlRewriterUtils.ExtendOptionsForCustomURLs(UrlRewriterUtils.GetOptionsFromSettings(new FriendlyUrlSettings(portalId)));
        }
    }
}