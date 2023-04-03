// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Maintenance.Telerik.Steps
{
    /// <summary>Removes an extension.</summary>
    internal interface IRemoveExtensionStep : IStep
    {
        /// <summary>Gets or sets the package name.</summary>
        string PackageName { get; set; }

        /// <summary>Gets or sets a value indicating whether to delete the installed files.</summary>
        bool DeleteFiles { get; set; }
    }
}
