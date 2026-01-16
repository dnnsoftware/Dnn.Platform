// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with details about a site/portal.</summary>
    [DataContract]
    public class SiteDetail
    {
        /// <summary>Initializes a new instance of the <see cref="SiteDetail"/> class.</summary>
        public SiteDetail()
        {
            this.Modules = new List<ModuleDetail>();
        }

        /// <summary>Gets or sets the version of DNN the site is running.</summary>
        [DataMember]
        public string DnnVersion { get; set; }

        /// <summary>Gets or sets the name of the site/portal.</summary>
        [DataMember]
        public string SiteName { get; set; }

        /// <summary>Gets or sets a value indicating whether the current user is a super user.</summary>
        [DataMember]
        public bool IsHost { get; set; }

        /// <summary>Gets or sets a value indicating whether the current user is a site administrator.</summary>
        [DataMember]
        public bool IsAdmin { get; set; }

        /// <summary>Gets or sets the list of modules.</summary>
        [DataMember]
        public IList<ModuleDetail> Modules { get; set; }
    }
}
