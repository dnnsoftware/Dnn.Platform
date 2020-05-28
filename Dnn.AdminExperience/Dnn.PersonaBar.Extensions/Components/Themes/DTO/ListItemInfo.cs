// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
