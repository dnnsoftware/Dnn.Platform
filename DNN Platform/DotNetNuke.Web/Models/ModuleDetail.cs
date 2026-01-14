// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    /// <summary>A data transfer object with details about a module.</summary>
    [DataContract]
    public class ModuleDetail
    {
        /// <summary>Initializes a new instance of the <see cref="ModuleDetail"/> class.</summary>
        public ModuleDetail()
        {
            this.ModuleInstances = new List<ModuleInstance>();
        }

        /// <summary>Gets or sets the version of the desktop module.</summary>
        [DataMember]
        public string ModuleVersion { get; set; }

        /// <summary>Gets or sets the module name.</summary>
        [DataMember]
        public string ModuleName { get; set; }

        /// <summary>Gets or sets a list of module instances.</summary>
        [DataMember]
        public IList<ModuleInstance> ModuleInstances { get; set; }
    }
}
