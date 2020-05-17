// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class PageFolderTemplate
    {
        [DataMember(Name = "key")]
        public int Key { get; set; }
        
        [DataMember(Name = "value")]
        public string Value { get; set; }        
    }
}
