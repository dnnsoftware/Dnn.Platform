// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Runtime.Serialization;

namespace Dnn.PersonaBar.Pages.Services.Dto
{
    [DataContract]
    public class Theme
    {
        [DataMember(Name = "skinSrc")]
        public string SkinSrc { get; set; }

        [DataMember(Name = "containerSrc")]
        public string ContainerSrc { get; set; }
    }
}
