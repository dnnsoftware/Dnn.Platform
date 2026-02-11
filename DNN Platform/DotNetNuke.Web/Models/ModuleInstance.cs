// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with details about an instance of a module.</summary>
    [DataContract]
    public class ModuleInstance
    {
        /// <summary>Gets or sets the name of the page/tab the module is on.</summary>
        [DataMember]
        public string PageName { get; set; }

        /// <summary>Gets or sets the tab path of the page/tab the module is on.</summary>
        [DataMember]
        public string PagePath { get; set; }

        /// <summary>Gets or sets the ID of the page/tab the module is on.</summary>
        [DataMember]
        public int TabId { get; set; }

        /// <summary>Gets or sets the module's ID.</summary>
        [DataMember]
        public int ModuleId { get; set; }
    }
}
