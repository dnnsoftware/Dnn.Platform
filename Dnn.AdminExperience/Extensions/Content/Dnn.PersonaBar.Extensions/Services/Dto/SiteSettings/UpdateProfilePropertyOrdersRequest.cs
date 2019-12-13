#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateProfilePropertyOrdersRequest
    {
        public int? PortalId { get; set; }

        public UpdateProfilePropertyRequest[] Properties { get; set; }
    }
}
