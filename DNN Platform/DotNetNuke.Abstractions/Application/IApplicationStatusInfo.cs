﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Application
{
    using System;

    /// <summary>
    /// The Application Status Info, includes information about installation
    /// and database version.
    /// </summary>
    public interface IApplicationStatusInfo
    {
        /// <summary>
        /// Gets the status of application.
        /// </summary>
        UpgradeStatus Status { get; }

        /// <summary>
        /// Gets the application map path.
        /// </summary>
        /// <value>
        /// The application map path.
        /// </value>
        string ApplicationMapPath { get; }

        /// <summary>
        /// Gets the database version.
        /// </summary>
        Version DatabaseVersion { get; }

        /// <summary>
        /// IsInstalled looks at various file artifacts to determine if DotNetNuke has already been installed.
        /// </summary>
        /// <returns>true if installed else false.</returns>
        /// <remarks>
        /// If DotNetNuke has been installed, then we should treat database connection errors as real errors.
        /// If DotNetNuke has not been installed, then we should expect to have database connection problems
        /// since the connection string may not have been configured yet, which can occur during the installation
        /// wizard.
        /// </remarks>
        bool IsInstalled();

        /// <summary>
        /// Sets the status.
        /// </summary>
        /// <param name="status">The status.</param>
        void SetStatus(UpgradeStatus status);

        /// <summary>
        /// Updates the database version.
        /// </summary>
        /// <param name="version">The version.</param>
        void UpdateDatabaseVersion(Version version);

        /// <summary>
        /// Updates the database version.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="increment">The increment.</param>
        void UpdateDatabaseVersionIncrement(Version version, int increment);

        /// <summary>
        /// Needs documentation.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>true is success else false.</returns>
        bool IncrementalVersionExists(Version version);

        /// <summary>
        /// Get the last application iteration.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>The result.</returns>
        int GetLastAppliedIteration(Version version);
    }
}
