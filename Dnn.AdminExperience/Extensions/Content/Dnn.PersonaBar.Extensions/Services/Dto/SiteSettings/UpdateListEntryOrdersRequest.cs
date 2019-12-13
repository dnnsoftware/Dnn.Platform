#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateListEntryOrdersRequest
    {
        public int? PortalId { get; set; }

        public UpdateListEntryRequest[] Entries { get; set; }
    }
}
