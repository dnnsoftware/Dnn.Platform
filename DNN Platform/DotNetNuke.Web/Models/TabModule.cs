// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Web.Models
{
    [Serializable]
    public class TabModule
    {
        public TabInfo TabInfo { get; set; }

        public ModuleInfo ModuleInfo { get; set; }

        public string ModuleVersion { get; set; }
    }
}
