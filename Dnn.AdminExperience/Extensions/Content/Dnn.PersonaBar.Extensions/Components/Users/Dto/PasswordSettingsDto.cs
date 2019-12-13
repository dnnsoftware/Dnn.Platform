// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Dnn.PersonaBar.Users.Components.Dto
{
    [DataContract]
    public class PasswordSettingsDto
    {
        [DataMember(Name = "minLength")]
        public int MinLength { get; set; }

        [DataMember(Name = "minNumberOfSpecialChars")]
        public int MinNumberOfSpecialChars { get; set; }

        [DataMember(Name = "validationExpression")]
        public string ValidationExpression { get; set; }
    }
}
