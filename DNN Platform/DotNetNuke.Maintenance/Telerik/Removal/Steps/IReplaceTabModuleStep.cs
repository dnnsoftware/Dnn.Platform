// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    using System;
    using System.Collections;

    /// <summary>Replaces a given module in a page with another module.</summary>
    internal interface IReplaceTabModuleStep : IStep
    {
        /// <summary>Gets or sets the name of the module to replace.</summary>
        string OldModuleName { get; set; }

        /// <summary>Gets or sets the name of the module that will replace the old module.</summary>
        string NewModuleName { get; set; }

        /// <summary>Gets or sets callback function to migrate old settings.</summary>
        Func<Hashtable, Hashtable> MigrateSettings { get; set; }
    }
}
