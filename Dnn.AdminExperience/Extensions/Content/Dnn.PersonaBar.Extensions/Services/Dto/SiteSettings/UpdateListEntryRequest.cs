#region Usings

using Newtonsoft.Json;

#endregion

namespace Dnn.PersonaBar.SiteSettings.Services.Dto
{
    public class UpdateListEntryRequest
    {
        public int? PortalId { get; set; }

        public int? EntryId { get; set; }

        public bool EnableSortOrder { get; set; }

        public string ListName { get; set; }

        public string Text { get; set; }

        public string Value { get; set; }

        public int? SortOrder { get; set; }
    }
}
