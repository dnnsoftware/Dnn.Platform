using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Entities.Urls;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class SeoUrl
    {
        [DataMember(Name = "tabId")]
        public int TabId { get; set; }

        [DataMember(Name = "saveUrl")]
        public SaveUrlDto SaveUrl { get; set; }
    }
}