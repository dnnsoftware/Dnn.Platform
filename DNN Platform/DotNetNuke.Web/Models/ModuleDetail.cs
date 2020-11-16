// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class ModuleDetail
    {
        public ModuleDetail()
        {
            this.ModuleInstances = new List<ModuleInstance>();
        }

        [DataMember]
        public string ModuleVersion { get; set; }

        [DataMember]
        public string ModuleName { get; set; }

        [DataMember]
        public IList<ModuleInstance> ModuleInstances { get; set; }
    }
}
