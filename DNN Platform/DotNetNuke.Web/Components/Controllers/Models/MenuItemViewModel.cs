// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Components.Controllers.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class MenuItemViewModel
    {
        public string ID { get; set; }

        public string Text { get; set; }

        public string Source { get; set; }

        public int Order { get; set; }
    }
}
