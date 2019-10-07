using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class BulkPageResponse
    {
        [DataMember(Name = "overallStatus")]
        public int OverallStatus { get; set; }

        [DataMember(Name = "pages")]
        public IEnumerable<BulkPageResponseItem> Pages { get; set; }
    }
}