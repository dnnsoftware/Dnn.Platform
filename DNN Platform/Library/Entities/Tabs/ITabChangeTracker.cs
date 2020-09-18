// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using DotNetNuke.Entities.Modules;

    public interface ITabChangeTracker
    {
        /// <summary>
        /// Tracks a change when a module is added to a page.
        /// </summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="userId">User Id who provokes the change.</param>
        void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is modified on a page.
        /// </summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="userId">User Id who provokes the change.</param>
        void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is deleted from a page.
        /// </summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="userId">User Id who provokes the change.</param>
        void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>
        /// Tracks a change when a module is copied from an exisitng page.
        /// </summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id who provokes the change.</param>
        void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);

        /// <summary>
        /// Tracks a change when a copied module is deleted from an exisitng page.
        /// </summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id who provokes the change.</param>
        void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);
    }
}
