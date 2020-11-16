// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Security.Services.Dto
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Runtime.Serialization;

    using DotNetNuke.ComponentModel.DataAnnotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class TabDto
    {
        public string Name { get; set; }

        public string TabId { get; set; }

        public int ParentTabId { get; set; }

        public IList<TabDto> ChildTabs { get; set; }
    }
}
