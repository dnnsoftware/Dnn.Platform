// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Tabs
{
    public interface ITabChangeTracker
    {
        /// <summary>
        /// Tracks a change when a module is added to a page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="userId">User Id who provokes the change</param>        
        void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is modified on a page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is deleted from a page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is copied from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);

        /// <summary>
        /// Tracks a change when a copied module is deleted from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the change</param>
        /// <param name="moduleVersion">Version number corresponding to the change</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>       
        /// <param name="userId">User Id who provokes the change</param>
        void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);
    }
}
