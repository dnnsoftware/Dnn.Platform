using DotNetNuke.Web.Api;

namespace Dnn.PersonaBar.Library
{
    /// <summary>
    /// The base class for persona bar api.
    /// </summary>
    public abstract class PersonaBarApiController : DnnApiController
    {
        public int PortalId => PortalSettings.PortalId;
    }
}
