// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// This interface allow the page to interact with his modules to delete/rollback or publish a specific version.
    /// The module that wants support page versioning need to implement it in the Bussiness controller.
    /// </summary>
    public interface IVersionable
    {
        /// <summary>
        /// This method deletes a concrete version of the module.
        /// </summary>
        /// <param name="moduleId">ModuleId.</param>
        /// <param name="version">Version number.</param>
        void DeleteVersion(int moduleId, int version);

        /// <summary>
        /// This method performs a rollback of a concrete version of the module.
        /// </summary>
        /// <param name="moduleId">Module Id.</param>
        /// <param name="version">Version number that need to be rollback.</param>
        /// <returns>New version number created after the rollback process.</returns>
        int RollBackVersion(int moduleId, int version);

        /// <summary>
        /// This method publishes a version of the module.
        /// </summary>
        /// <param name="moduleId">Module Id.</param>
        /// <param name="version">Version number.</param>
        void PublishVersion(int moduleId, int version);

        /// <summary>
        /// This method returns the version number of the current published module version.
        /// </summary>
        /// <param name="moduleId">Module Id.</param>
        /// <returns>Version number of the current published content version.</returns>
        int GetPublishedVersion(int moduleId);

        /// <summary>
        /// This method returns the latest version number of the current module.
        /// </summary>
        /// <param name="moduleId">Module Id.</param>
        /// <returns>Version number of the current published content version.</returns>
        int GetLatestVersion(int moduleId);
    }
}
