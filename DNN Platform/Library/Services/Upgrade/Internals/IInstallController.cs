// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade.Internals
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   Interface for InstallController. This Interface is meant for Internal use only.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public interface IInstallController
    {
        string InstallerLogName { get; }

        bool IsValidSqlServerVersion(string connectionString);

        bool IsAbleToPerformDatabaseActions(string connectionString);

        bool IsValidDotNetVersion();

        bool IsSqlServerDbo();

        bool IsAvailableLanguagePack(string cultureCode);

        /// <summary>
        /// GetInstallConfig - Returns configuration stored in DotNetNuke.Install.Config.
        /// </summary>
        /// <returns>ConnectionConfig object. Null if information is not present in the config file.</returns>
        InstallConfig GetInstallConfig();

        /// <summary>
        /// SetInstallConfig - Saves configuration n DotNetNuke.Install.Config.
        /// </summary>
        void SetInstallConfig(InstallConfig installConfig);

        /// <summary>
        /// RemoveFromInstallConfig - Removes the specified XML Node from the InstallConfig.
        /// </summary>
        /// <param name="xmlNodePath"></param>
        void RemoveFromInstallConfig(string xmlNodePath);

        /// <summary>
        /// GetConnectionFromWebConfig - Returns Connection Configuration in web.config file.
        /// </summary>
        /// <returns>ConnectionConfig object. Null if information is not present in the config file.</returns>
        ConnectionConfig GetConnectionFromWebConfig();

        CultureInfo GetCurrentLanguage();

        string TestDatabaseConnection(ConnectionConfig connectionConfig);

        CultureInfo GetCultureFromCookie();
    }
}
