// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class SiteDetail
    {
        public SiteDetail()
        {
            this.Modules = new List<ModuleDetail>();
        }

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
    }
}
