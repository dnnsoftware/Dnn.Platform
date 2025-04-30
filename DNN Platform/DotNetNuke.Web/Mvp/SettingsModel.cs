﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvp
{
    using System.Collections.Generic;

    using DotNetNuke.Internal.SourceGenerators;

    /// <summary>The model for a settings view.</summary>
    [DnnDeprecated(9, 2, 0, "Replace WebFormsMvp and DotNetNuke.Web.Mvp with MVC or SPA patterns instead")]
    public partial class SettingsModel
    {
        public Dictionary<string, string> ModuleSettings { get; set; }

        public Dictionary<string, string> TabModuleSettings { get; set; }
    }
}
