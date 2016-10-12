using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library
{
    public abstract class PersonaBarApiController : DnnApiController
    {
        public int PortalId => PortalSettings.PortalId;
    }
}
