// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;

namespace DotNetNuke.Web.Models
{
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
