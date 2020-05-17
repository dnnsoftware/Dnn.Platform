// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Modules
{
    public interface IShareable
    {
        /// <summary>Does this module support Module Sharing (i.e., sharing modules between sites within a SiteGroup)?</summary>
        ModuleSharing SharingSupport { get; set; }
    }
}
