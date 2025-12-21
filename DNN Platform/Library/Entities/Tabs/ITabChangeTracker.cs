// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System.Diagnostics.CodeAnalysis;

    using DotNetNuke.Entities.Modules;

    public interface ITabChangeTracker
    {
        /// <summary>Tracks a change when a module is added to a page.</summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="userId">User ID who provokes the change.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>Tracks a change when a module is modified on a page.</summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="userId">User ID who provokes the change.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>Tracks a change when a module is deleted from a page.</summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="userId">User ID who provokes the change.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId);

        /// <summary>Tracks a change when a module is copied from an existing page.</summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="originalTabId">Tab ID where the module originally is.</param>
        /// <param name="userId">User ID who provokes the change.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);

        /// <summary>Tracks a change when a copied module is deleted from an existing page.</summary>
        /// <param name="module">Module which tracks the change.</param>
        /// <param name="moduleVersion">Version number corresponding to the change.</param>
        /// <param name="originalTabId">Tab ID where the module originally is.</param>
        /// <param name="userId">User ID who provokes the change.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId);
    }
}
