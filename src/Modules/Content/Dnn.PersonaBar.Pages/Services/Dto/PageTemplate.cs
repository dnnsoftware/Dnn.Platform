using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class PageTemplate
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "folder")]
        public PageFolderTemplate Folder { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "includeContent")]
        public bool IncludeContent { get; set; }

        [DataMember(Name = "tabId")]
        public int TabId { get; set; }
    }
}