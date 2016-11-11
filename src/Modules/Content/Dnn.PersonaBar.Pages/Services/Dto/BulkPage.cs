using System;
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class BulkPage
    {
        [DataMember(Name = "bulkPages")]
        public string BulkPages { get; set; }

        [DataMember(Name = "parentId")]
        public int ParentId { get; set; }

        [DataMember(Name = "keywords")]
        public string Keywords { get; set; }

        [DataMember(Name = "tags")]
        public string Tags { get; set; }

        [DataMember(Name = "includeInMenu")]
        public bool IncludeInMenu { get; set; }

        [DataMember(Name = "startDate")]
        public DateTime? StartDate { get; set; }

        [DataMember(Name = "endDate")]
        public DateTime? EndDate { get; set; }

    }
}