using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.Modules.DynamicContentManager.Components.Entities
{

    public class DCCSettings
    {
        [DataMember(Name = "contentTypePageSize")]
        public int ContentTypePageSize { get; set; }

        [DataMember(Name = "dataTypePageSize")]
        public int DataTypePageSize { get; set; }

        [DataMember(Name = "templatePageSize")]
        public int TemplatePageSize { get; set; }
    }
}