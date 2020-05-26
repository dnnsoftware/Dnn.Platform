// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.Models
{
    [DataContract]
    public class SiteDetail
    {
        [DataMember]
        public string DnnVersion { get; set; }

        [DataMember]
        public string SiteName { get; set; }

        [DataMember]
        public bool IsHost { get; set; }

        [DataMember]
        public bool IsAdmin { get; set; }

        [DataMember]
        public IList<ModuleDetail> Modules { get; set; }

        public SiteDetail()
        {
            Modules = new List<ModuleDetail>();
        }
    }
}
