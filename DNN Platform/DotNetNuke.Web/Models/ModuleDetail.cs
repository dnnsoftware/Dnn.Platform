// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.Models
{
    [DataContract]
    public class ModuleDetail
    {
        [DataMember]
        public string ModuleVersion { get; set; }

        [DataMember]
        public string ModuleName { get; set; }

        [DataMember]
        public IList<ModuleInstance> ModuleInstances { get; set; }

        public ModuleDetail()
        {
            ModuleInstances = new List<ModuleInstance>();
        }
    }
}
