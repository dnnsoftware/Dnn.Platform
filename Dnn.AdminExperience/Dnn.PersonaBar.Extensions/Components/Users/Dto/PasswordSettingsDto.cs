// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Users.Components.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Web;

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
