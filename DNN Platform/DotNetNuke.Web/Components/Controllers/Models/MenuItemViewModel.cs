// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetNuke.Web.Components.Controllers.Models
{
    public class MenuItemViewModel
    {
        public string ID { get; set; }
        public string Text { get; set; }
        public string Source { get; set; }
        public int Order { get; set; }
    }
}
