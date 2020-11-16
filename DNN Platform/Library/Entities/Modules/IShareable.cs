// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    public interface IShareable
    {
        /// <summary>Gets or sets does this module support Module Sharing (i.e., sharing modules between sites within a SiteGroup)?.</summary>
        ModuleSharing SharingSupport { get; set; }
    }
}
