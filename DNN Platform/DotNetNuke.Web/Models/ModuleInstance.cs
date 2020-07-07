// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System.Runtime.Serialization;

    [DataContract]
    public class ModuleInstance
    {
        [DataMember]
        public string PageName { get; set; }

        [DataMember]
        public string PagePath { get; set; }

        [DataMember]
        public int TabId { get; set; }

        [DataMember]
        public int ModuleId { get; set; }
    }
}
