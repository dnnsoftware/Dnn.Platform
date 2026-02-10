// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Models
{
    using System;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs;

    /// <summary>Information about a module instance on a specific page/tab.</summary>
    [Serializable]
    public class TabModule
    {
        /// <summary>Gets or sets the page/tab info.</summary>
        public TabInfo TabInfo { get; set; }

        /// <summary>Gets or sets the module info.</summary>
        public ModuleInfo ModuleInfo { get; set; }

        /// <summary>Gets or sets the module's desktop module version.</summary>
        public string ModuleVersion { get; set; }
    }
}
