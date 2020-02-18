// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Themes.Components.DTO
{
    [DataContract]
    public class ListItemInfo
    {
        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "value")]
        public string Value { get; set; }

        public ListItemInfo()
        {
            
        }

        public ListItemInfo(string text, string value)
        {
            Text = text;
            Value = value;
        }
    }
}
