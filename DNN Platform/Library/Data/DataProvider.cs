// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
// ReSharper disable InconsistentNaming
namespace DotNetNuke.Data
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Data.SqlTypes;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Hosting;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data.PetaPoco;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Search.Entities;
    using Microsoft.ApplicationBlocks.Data;

    public abstract class DataProvider
    {
        private const int DuplicateKey = 2601;

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataProvider));

        public virtual string ConnectionString
        {
            get
            {
                // Get Connection string from web.config
                string connectionString = Config.GetConnectionString();
                if (string.IsNullOrEmpty(connectionString))
                {
                    // Use connection string specified in provider
                    connectionString = this.Settings["connectionString"];
                }

                return connectionString;
            }
        }

        public virtual string DatabaseOwner
        {
            get
            {
                string databaseOwner = this.Settings["databaseOwner"];
                if (!string.IsNullOrEmpty(databaseOwner) && databaseOwner.EndsWith(".") == false)
                {
                    databaseOwner += ".";
                }

                return databaseOwner;
            }
        }

        public string DefaultProviderName
        {
            get { return Instance().ProviderName; }
        }

        public abstract bool IsConnectionValid { get; }

        public virtual string ObjectQualifier
        {
            get
            {
                string objectQualifier = this.Settings["objectQualifier"];
                if (!string.IsNullOrEmpty(objectQualifier) && objectQualifier.EndsWith("_") == false)
                {
                    objectQualifier += "_";
                }

                return objectQualifier;
            }
        }

        public virtual string ProviderName
        {
            get { return this.Settings["providerName"]; }
        }

        public virtual string ProviderPath
        {
            get { return this.Settings["providerPath"]; }
        }

        public abstract Dictionary<string, string> Settings { get; }

        public virtual string UpgradeConnectionString
        {
            get
            {
                return !string.IsNullOrEmpty(this.Settings["upgradeConnectionString"])
                                        ? this.Settings["upgradeConnectionString"]
                                        : this.ConnectionString;
            }
        }

        public static DataProvider Instance()
        {
            return ComponentFactory.GetComponent<DataProvider>();
        }

        public abstract void ExecuteNonQuery(string procedureName, params object[] commandParameters);

        public abstract void ExecuteNonQuery(int timeoutSec, string procedureName, params object[] commandParameters);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, Dictionary<string, object> commandParameters);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable, int timeoutSec, Dictionary<string, object> commandParameters);

        public abstract IDataReader ExecuteReader(string procedureName, params object[] commandParameters);

        public abstract IDataReader ExecuteReader(int timeoutSec, string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(string procedureName, params object[] commandParameters);

        public abstract T ExecuteScalar<T>(int timeoutSec, string procedureName, params object[] commandParameters);

        public abstract IDataReader ExecuteSQL(string sql);

        public abstract IDataReader ExecuteSQL(string sql, int timeoutSec);

        public abstract string ExecuteScript(string script);

        public abstract string ExecuteScript(string script, int timeoutSec);

        public abstract string ExecuteScript(string connectionString, string sql);

        public abstract string ExecuteScript(string connectionString, string sql, int timeoutSec);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage);

        public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, int timeoutSec, out string errorMessage);

        public virtual void CommitTransaction(DbTransaction transaction)
        {
            try
            {
                transaction.Commit();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
        }

        public virtual DbTransaction GetTransaction()
        {
            var Conn = new SqlConnection(this.UpgradeConnectionString);
            Conn.Open();
            SqlTransaction transaction = Conn.BeginTransaction();
            return transaction;
        }

        public virtual void RollbackTransaction(DbTransaction transaction)
        {
            try
            {
                transaction.Rollback();
            }
            finally
            {
                if (transaction != null && transaction.Connection != null)
                {
                    transaction.Connection.Close();
                }
            }
        }

        public virtual object GetNull(object Field)
        {
            return Null.GetNull(Field, DBNull.Value);
        }

        public virtual IDataReader FindDatabaseVersion(int Major, int Minor, int Build)
        {
            return this.ExecuteReader("FindDatabaseVersion", Major, Minor, Build);
        }

        public virtual Version GetDatabaseEngineVersion()
        {
            string version = "0.0";
            IDataReader dr = null;
            try
            {
                dr = this.ExecuteReader("GetDatabaseServer");
                if (dr.Read())
                {
                    version = dr["Version"].ToString();
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return new Version(version);
        }

        public virtual IDataReader GetDatabaseVersion()
        {
            return this.ExecuteReader("GetDatabaseVersion");
        }

        public virtual IDataReader GetDatabaseInstallVersion()
        {
            return this.ExecuteReader("GetDatabaseInstallVersion");
        }

        public virtual Version GetVersion()
        {
            return this.GetVersionInternal(true);
        }

        public virtual Version GetInstallVersion()
        {
            return this.GetVersionInternal(false);
        }

        public virtual DbConnectionStringBuilder GetConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder();
        }

        public virtual string GetProviderPath()
        {
            string path = this.ProviderPath;
            if (!string.IsNullOrEmpty(path))
            {
                path = HostingEnvironment.MapPath(path);

                // ReSharper disable AssignNullToNotNullAttribute
                if (Directory.Exists(path))

                // ReSharper restore AssignNullToNotNullAttribute
                {
                    if (!this.IsConnectionValid)
                    {
                        path = "ERROR: Could not connect to database specified in connectionString for SqlDataProvider";
                    }
                }
                else
                {
                    path = "ERROR: providerPath folder " + path +
                           " specified for SqlDataProvider does not exist on web server";
                }
            }
            else
            {
                path = "ERROR: providerPath folder value not specified in web.config for SqlDataProvider";
            }

            return path;
        }

        public virtual string TestDatabaseConnection(DbConnectionStringBuilder builder, string Owner, string Qualifier)
        {
            var sqlBuilder = builder as SqlConnectionStringBuilder;
            string connectionString = Null.NullString;
            if (sqlBuilder != null)
            {
                connectionString = sqlBuilder.ToString();
                IDataReader dr = null;
                try
                {
                    dr = PetaPocoHelper.ExecuteReader(connectionString, CommandType.StoredProcedure,
                                                      Owner + Qualifier + "GetDatabaseVersion");
                }
                catch (SqlException ex)
                {
                    const string message = "ERROR:";
                    bool bError = true;
                    int i;
                    var errorMessages = new StringBuilder();
                    for (i = 0; i <= ex.Errors.Count - 1; i++)
                    {
                        SqlError sqlError = ex.Errors[i];
                        if (sqlError.Number == 2812 && sqlError.Class == 16)
                        {
                            bError = false;
                            break;
                        }

                        string filteredMessage = string.Empty;
                        switch (sqlError.Number)
                        {
                            case 17:
                                filteredMessage = "Sql server does not exist or access denied";
                                break;
                            case 4060:
                                filteredMessage = "Invalid Database";
                                break;
                            case 18456:
                                filteredMessage = "Sql login failed";
                                break;
                            case 1205:
                                filteredMessage = "Sql deadlock victim";
                                break;
                        }

                        errorMessages.Append("<b>Index #:</b> " + i + "<br/>" + "<b>Source:</b> " + sqlError.Source +
                                             "<br/>" + "<b>Class:</b> " + sqlError.Class + "<br/>" + "<b>Number:</b> " +
                                             sqlError.Number + "<br/>" + "<b>Message:</b> " + filteredMessage +
                                             "<br/><br/>");
                    }

                    if (bError)
                    {
                        connectionString = message + errorMessages;
                    }
                }
                finally
                {
                    CBO.CloseDataReader(dr, true);
                }
            }

            return connectionString;
        }

        public virtual void UpdateDatabaseVersion(int Major, int Minor, int Build, string Name)
        {
            if (Major >= 5 || (Major == 4 && Minor == 9 && Build > 0))
            {
                // If the version > 4.9.0 use the new sproc, which is added in 4.9.1 script
                this.ExecuteNonQuery("UpdateDatabaseVersionAndName", Major, Minor, Build, Name);
            }
            else
            {
                this.ExecuteNonQuery("UpdateDatabaseVersion", Major, Minor, Build);
            }
        }

        public virtual void UpdateDatabaseVersionIncrement(int Major, int Minor, int Build, int Increment, string AppName)
        {
            this.ExecuteNonQuery("UpdateDatabaseVersionIncrement", Major, Minor, Build, Increment, AppName);
        }

        public virtual int GetLastAppliedIteration(int Major, int Minor, int Build)
        {
            return this.ExecuteScalar<int>("GetLastAppliedIteration", Major, Minor, Build);
        }

        public virtual string GetUnappliedIterations(string version)
        {
            return this.ExecuteScalar<string>("GetUnappliedIterations", version);
        }

        public virtual IDataReader GetHostSetting(string SettingName)
        {
            return this.ExecuteReader("GetHostSetting", SettingName);
        }

        public virtual IDataReader GetHostSettings()
        {
            return this.ExecuteReader("GetHostSettings");
        }

        public virtual void UpdateHostSetting(string SettingName, string SettingValue, bool SettingIsSecure,
                                              int LastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateHostSetting", SettingName, SettingValue, SettingIsSecure, LastModifiedByUserID);
        }

        public virtual void DeleteServer(int ServerId)
        {
            this.ExecuteNonQuery("DeleteServer", ServerId);
        }

        public virtual IDataReader GetServers()
        {
            return this.ExecuteReader("GetServers");
        }

        public virtual void UpdateServer(int serverId, string url, string uniqueId, bool enabled, string group)
        {
            this.ExecuteNonQuery("UpdateServer", serverId, url, uniqueId, enabled, group);
        }

        public virtual int UpdateServerActivity(string serverName, string iisAppName, DateTime createdDate,
                                                 DateTime lastActivityDate, int pingFailureCount, bool enabled)
        {
            return this.ExecuteScalar<int>("UpdateServerActivity", serverName, iisAppName, createdDate, lastActivityDate, pingFailureCount, enabled);
        }

        [Obsolete("Deprecated in Platform 7.4.0, please use CreatePortal version that contain's culturecode. Scheduled removal in v10.0.0.")]
        public virtual int CreatePortal(string portalname, string currency, DateTime ExpiryDate, double HostFee,
                                        double HostSpace, int PageQuota, int UserQuota, int SiteLogHistory,
                                        string HomeDirectory, int CreatedByUserID)
        {
            return
                this.CreatePortal(
                                            PortalSecurity.Instance.InputFilter(portalname, PortalSecurity.FilterFlag.NoMarkup),
                                            currency,
                                            ExpiryDate,
                                            HostFee,
                                            HostSpace,
                                            PageQuota,
                                            UserQuota,
                                            SiteLogHistory,
                                            HomeDirectory,
                                            "en-US",
                                            CreatedByUserID);
        }

        public virtual int CreatePortal(string portalname, string currency, DateTime ExpiryDate, double HostFee,
                                        double HostSpace, int PageQuota, int UserQuota, int SiteLogHistory,
                                        string HomeDirectory, string CultureCode, int CreatedByUserID)
        {
            return
                this.ExecuteScalar<int>(
                    "AddPortalInfo",
                    PortalSecurity.Instance.InputFilter(portalname, PortalSecurity.FilterFlag.NoMarkup),
                    currency,
                    this.GetNull(ExpiryDate),
                    HostFee,
                    HostSpace,
                    PageQuota,
                    UserQuota,
                    this.GetNull(SiteLogHistory),
                    HomeDirectory,
                    CultureCode,
                    CreatedByUserID);
        }

        public virtual void DeletePortalInfo(int PortalId)
        {
            this.ExecuteNonQuery("DeletePortalInfo", PortalId);
        }

        public virtual void DeletePortalSetting(int PortalId, string SettingName, string CultureCode)
        {
            this.ExecuteNonQuery("DeletePortalSetting", PortalId, SettingName, CultureCode);
        }

        public virtual void DeletePortalSettings(int PortalId, string CultureCode)
        {
            this.ExecuteNonQuery("DeletePortalSettings", PortalId, CultureCode);
        }

        public virtual IDataReader GetExpiredPortals()
        {
            return this.ExecuteReader("GetExpiredPortals");
        }

        public virtual IDataReader GetPortals(string CultureCode)
        {
            IDataReader reader;
            if (Globals.Status == Globals.UpgradeStatus.Upgrade && Globals.DataBaseVersion < new Version(6, 1, 0))
            {
                reader = this.ExecuteReader("GetPortals");
            }
            else
            {
                reader = this.ExecuteReader("GetPortals", CultureCode);
            }

            return reader;
        }

        public virtual IDataReader GetAllPortals()
        {
            return this.ExecuteReader("GetAllPortals");
        }

        public virtual IDataReader GetPortalsByName(string nameToMatch, int pageIndex, int pageSize)
        {
            return this.ExecuteReader("GetPortalsByName", nameToMatch, pageIndex, pageSize);
        }

        public virtual IDataReader GetPortalsByUser(int userId)
        {
            return this.ExecuteReader("GetPortalsByUser", userId);
        }

        public virtual IDataReader GetPortalSettings(int PortalId, string CultureCode)
        {
            return this.ExecuteReader("GetPortalSettings", PortalId, CultureCode);
        }

        public virtual IDataReader GetPortalSpaceUsed(int PortalId)
        {
            return this.ExecuteReader("GetPortalSpaceUsed", this.GetNull(PortalId));
        }

        /// <summary>
        /// Updates the portal information.Saving basic portal settings at Admin - Site settings / Host - Portals - Edit Portal.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="portalGroupId">The portal group identifier.</param>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="logoFile">The logo file.</param>
        /// <param name="footerText">The footer text.</param>
        /// <param name="expiryDate">The expiry date.</param>
        /// <param name="userRegistration">The user registration.</param>
        /// <param name="bannerAdvertising">The banner advertising.</param>
        /// <param name="currency">The currency.</param>
        /// <param name="administratorId">The administrator identifier.</param>
        /// <param name="hostFee">The host fee.</param>
        /// <param name="hostSpace">The host space.</param>
        /// <param name="pageQuota">The page quota.</param>
        /// <param name="userQuota">The user quota.</param>
        /// <param name="paymentProcessor">The payment processor.</param>
        /// <param name="processorUserId">The processor user identifier.</param>
        /// <param name="processorPassword">The processor password.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="backgroundFile">The background file.</param>
        /// <param name="siteLogHistory">The site log history.</param>
        /// <param name="splashTabId">The splash tab identifier.</param>
        /// <param name="homeTabId">The home tab identifier.</param>
        /// <param name="loginTabId">The login tab identifier.</param>
        /// <param name="registerTabId">The register tab identifier.</param>
        /// <param name="userTabId">The user tab identifier.</param>
        /// <param name="searchTabId">The search tab identifier.</param>
        /// <param name="custom404TabId">The custom404 tab identifier.</param>
        /// <param name="custom500TabId">The custom500 tab identifier.</param>
        /// <param name="defaultLanguage">The default language.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="lastModifiedByUserID">The last modified by user identifier.</param>
        /// <param name="cultureCode">The culture code.</param>
        public virtual void UpdatePortalInfo(int portalId, int portalGroupId, string portalName, string logoFile,
                                             string footerText, DateTime expiryDate, int userRegistration,
                                             int bannerAdvertising, string currency,
                                             int administratorId, double hostFee, double hostSpace, int pageQuota,
                                             int userQuota, string paymentProcessor, string processorUserId,
                                             string processorPassword, string description, string keyWords,
                                             string backgroundFile, int siteLogHistory, int splashTabId, int homeTabId,
                                             int loginTabId,
                                             int registerTabId, int userTabId, int searchTabId, int custom404TabId, int custom500TabId,
                                             int termsTabId, int privacyTabId, string defaultLanguage,
                                             string homeDirectory, int lastModifiedByUserID, string cultureCode)
        {
            this.ExecuteNonQuery(
                "UpdatePortalInfo",
                portalId,
                portalGroupId,
                PortalSecurity.Instance.InputFilter(portalName, PortalSecurity.FilterFlag.NoMarkup),
                this.GetNull(logoFile),
                this.GetNull(footerText),
                this.GetNull(expiryDate),
                userRegistration,
                bannerAdvertising,
                currency,
                this.GetNull(administratorId),
                hostFee,
                hostSpace,
                pageQuota,
                userQuota,
                this.GetNull(paymentProcessor),
                this.GetNull(processorUserId),
                this.GetNull(processorPassword),
                this.GetNull(description),
                this.GetNull(keyWords),
                this.GetNull(backgroundFile),
                this.GetNull(siteLogHistory),
                this.GetNull(splashTabId),
                this.GetNull(homeTabId),
                this.GetNull(loginTabId),
                this.GetNull(registerTabId),
                this.GetNull(userTabId),
                this.GetNull(searchTabId),
                this.GetNull(custom404TabId),
                this.GetNull(custom500TabId),
                this.GetNull(termsTabId),
                this.GetNull(privacyTabId),
                this.GetNull(defaultLanguage),
                homeDirectory,
                lastModifiedByUserID,
                cultureCode);
        }

        public virtual void UpdatePortalSetting(int portalId, string settingName, string settingValue, int userId,
                                                string cultureCode, bool isSecure)
        {
            this.ExecuteNonQuery("UpdatePortalSetting", portalId, settingName, settingValue, userId, cultureCode, isSecure);
        }

        public virtual void UpdatePortalSetup(int portalId, int administratorId, int administratorRoleId,
                                              int registeredRoleId, int splashTabId, int homeTabId, int loginTabId,
                                              int registerTabId,
                                              int userTabId, int searchTabId, int custom404TabId, int custom500TabId,
                                              int termsTabId, int privacyTabId, int adminTabId, string cultureCode)
        {
            this.ExecuteNonQuery(
                "UpdatePortalSetup",
                portalId,
                administratorId,
                administratorRoleId,
                registeredRoleId,
                splashTabId,
                homeTabId,
                loginTabId,
                registerTabId,
                userTabId,
                searchTabId,
                custom404TabId,
                custom500TabId,
                termsTabId,
                privacyTabId,
                adminTabId,
                cultureCode);
        }

        public virtual int AddTabAfter(TabInfo tab, int afterTabId, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddTabAfter",
                afterTabId,
                tab.ContentItemId,
                this.GetNull(tab.PortalID),
                tab.UniqueId,
                tab.VersionGuid,
                this.GetNull(tab.DefaultLanguageGuid),
                tab.LocalizedVersionGuid,
                tab.TabName,
                tab.IsVisible,
                tab.DisableLink,
                this.GetNull(tab.ParentId),
                tab.IconFile,
                tab.IconFileLarge,
                tab.Title,
                tab.Description,
                tab.KeyWords,
                tab.Url,
                this.GetNull(tab.SkinSrc),
                this.GetNull(tab.ContainerSrc),
                this.GetNull(tab.StartDate),
                this.GetNull(tab.EndDate),
                this.GetNull(tab.RefreshInterval),
                this.GetNull(tab.PageHeadText),
                tab.IsSecure,
                tab.PermanentRedirect,
                tab.SiteMapPriority,
                createdByUserID,
                this.GetNull(tab.CultureCode),
                tab.IsSystem);
        }

        public virtual int AddTabBefore(TabInfo tab, int beforeTabId, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddTabBefore",
                beforeTabId,
                tab.ContentItemId,
                this.GetNull(tab.PortalID),
                tab.UniqueId,
                tab.VersionGuid,
                this.GetNull(tab.DefaultLanguageGuid),
                tab.LocalizedVersionGuid,
                tab.TabName,
                tab.IsVisible,
                tab.DisableLink,
                this.GetNull(tab.ParentId),
                tab.IconFile,
                tab.IconFileLarge,
                tab.Title,
                tab.Description,
                tab.KeyWords,
                tab.Url,
                this.GetNull(tab.SkinSrc),
                this.GetNull(tab.ContainerSrc),
                this.GetNull(tab.StartDate),
                this.GetNull(tab.EndDate),
                this.GetNull(tab.RefreshInterval),
                this.GetNull(tab.PageHeadText),
                tab.IsSecure,
                tab.PermanentRedirect,
                tab.SiteMapPriority,
                createdByUserID,
                this.GetNull(tab.CultureCode),
                tab.IsSystem);
        }

        public virtual int AddTabToEnd(TabInfo tab, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddTabToEnd",
                tab.ContentItemId,
                this.GetNull(tab.PortalID),
                tab.UniqueId,
                tab.VersionGuid,
                this.GetNull(tab.DefaultLanguageGuid),
                tab.LocalizedVersionGuid,
                tab.TabName,
                tab.IsVisible,
                tab.DisableLink,
                this.GetNull(tab.ParentId),
                tab.IconFile,
                tab.IconFileLarge,
                tab.Title,
                tab.Description,
                tab.KeyWords,
                tab.Url,
                this.GetNull(tab.SkinSrc),
                this.GetNull(tab.ContainerSrc),
                this.GetNull(tab.StartDate),
                this.GetNull(tab.EndDate),
                this.GetNull(tab.RefreshInterval),
                this.GetNull(tab.PageHeadText),
                tab.IsSecure,
                tab.PermanentRedirect,
                tab.SiteMapPriority,
                createdByUserID,
                this.GetNull(tab.CultureCode),
                tab.IsSystem);
        }

        public virtual void DeleteTab(int tabId)
        {
            this.ExecuteNonQuery("DeleteTab", tabId);
        }

        public virtual void DeleteTabSetting(int TabId, string SettingName)
        {
            this.ExecuteNonQuery("DeleteTabSetting", TabId, SettingName);
        }

        public virtual void DeleteTabSettings(int TabId)
        {
            this.ExecuteNonQuery("DeleteTabSettings", TabId);
        }

        public virtual void DeleteTabUrl(int tabId, int seqNum)
        {
            this.ExecuteNonQuery("DeleteTabUrl", tabId, seqNum);
        }

        public virtual void DeleteTabVersion(int tabVersionId)
        {
            this.ExecuteNonQuery("DeleteTabVersion", tabVersionId);
        }

        public virtual void DeleteTabVersionDetail(int tabVersionDetailId)
        {
            this.ExecuteNonQuery("DeleteTabVersionDetail", tabVersionDetailId);
        }

        public virtual void DeleteTabVersionDetailByModule(int moduleId)
        {
            this.ExecuteNonQuery("DeleteTabVersionDetailByModule", moduleId);
        }

        public virtual void DeleteTranslatedTabs(int tabId, string cultureCode)
        {
            this.ExecuteNonQuery("DeleteTranslatedTabs", tabId, cultureCode);
        }

        public virtual void EnsureNeutralLanguage(int portalId, string cultureCode)
        {
            this.ExecuteNonQuery("EnsureNeutralLanguage", portalId, cultureCode);
        }

        public virtual void ConvertTabToNeutralLanguage(int portalId, int tabId, string cultureCode)
        {
            this.ExecuteNonQuery("ConvertTabToNeutralLanguage", portalId, tabId, cultureCode);
        }

        public virtual IDataReader GetAllTabs()
        {
            return this.ExecuteReader("GetAllTabs");
        }

        public virtual IDataReader GetTab(int tabId)
        {
            return this.ExecuteReader("GetTab", tabId);
        }

        public virtual IDataReader GetTabByUniqueID(Guid uniqueId)
        {
            return this.ExecuteReader("GetTabByUniqueID", uniqueId);
        }

        public virtual IDataReader GetTabPanes(int tabId)
        {
            return this.ExecuteReader("GetTabPanes", tabId);
        }

        public virtual IDataReader GetTabPaths(int portalId, string cultureCode)
        {
            return this.ExecuteReader("GetTabPaths", this.GetNull(portalId), cultureCode);
        }

        public virtual IDataReader GetTabs(int portalId)
        {
            return this.ExecuteReader("GetTabs", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabsByModuleID(int moduleID)
        {
            return this.ExecuteReader("GetTabsByModuleID", moduleID);
        }

        public virtual IDataReader GetTabsByTabModuleID(int tabModuleID)
        {
            return this.ExecuteReader("GetTabsByTabModuleID", tabModuleID);
        }

        public virtual IDataReader GetTabsByPackageID(int portalID, int packageID, bool forHost)
        {
            return this.ExecuteReader("GetTabsByPackageID", this.GetNull(portalID), packageID, forHost);
        }

        public virtual IDataReader GetTabSetting(int TabID, string SettingName)
        {
            return this.ExecuteReader("GetTabSetting", TabID, SettingName);
        }

        public virtual IDataReader GetTabSettings(int portalId)
        {
            return this.ExecuteReader("GetTabSettings", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabAliasSkins(int portalId)
        {
            return this.ExecuteReader("GetTabAliasSkins", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabCustomAliases(int portalId)
        {
            return this.ExecuteReader("GetTabCustomAliases", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabUrls(int portalId)
        {
            return this.ExecuteReader("GetTabUrls", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabVersions(int tabId)
        {
            return this.ExecuteReader("GetTabVersions", this.GetNull(tabId));
        }

        public virtual IDataReader GetTabVersionDetails(int tabVersionId)
        {
            return this.ExecuteReader("GetTabVersionDetails", this.GetNull(tabVersionId));
        }

        public virtual IDataReader GetTabVersionDetailsHistory(int tabId, int version)
        {
            return this.ExecuteReader("GetTabVersionDetailsHistory", this.GetNull(tabId), this.GetNull(version));
        }

        public virtual IDataReader GetCustomAliasesForTabs()
        {
            return this.ExecuteReader("GetCustomAliasesForTabs");
        }

        public virtual void LocalizeTab(int tabId, string cultureCode, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("LocalizeTab", tabId, cultureCode, lastModifiedByUserID);
        }

        public virtual void MoveTabAfter(int tabId, int afterTabId, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("MoveTabAfter", tabId, afterTabId, lastModifiedByUserID);
        }

        public virtual void MoveTabBefore(int tabId, int beforeTabId, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("MoveTabBefore", tabId, beforeTabId, lastModifiedByUserID);
        }

        public virtual void MoveTabToParent(int tabId, int parentId, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("MoveTabToParent", tabId, this.GetNull(parentId), lastModifiedByUserID);
        }

        public virtual void SaveTabUrl(int tabId, int seqNum, int portalAliasId, int portalAliasUsage, string url, string queryString, string cultureCode, string httpStatus, bool isSystem, int modifiedByUserID)
        {
            this.ExecuteNonQuery("SaveTabUrl", tabId, seqNum, this.GetNull(portalAliasId), portalAliasUsage, url, queryString, cultureCode, httpStatus, isSystem, modifiedByUserID);
        }

        public virtual int SaveTabVersion(int tabVersionId, int tabId, DateTime timeStamp, int version, bool isPublished, int createdByUserID, int modifiedByUserID)
        {
            return this.ExecuteScalar<int>("SaveTabVersion", tabVersionId, tabId, timeStamp, version, isPublished, createdByUserID, modifiedByUserID);
        }

        public virtual int SaveTabVersionDetail(int tabVersionDetailId, int tabVersionId, int moduleId, int moduleVersion, string paneName, int moduleOrder, int action, int createdByUserID, int modifiedByUserID)
        {
            return this.ExecuteScalar<int>("SaveTabVersionDetail", tabVersionDetailId, tabVersionId, moduleId, moduleVersion, paneName, moduleOrder, action, createdByUserID, modifiedByUserID);
        }

        public virtual void UpdateTab(int tabId, int contentItemId, int portalId, Guid versionGuid,
                                      Guid defaultLanguageGuid, Guid localizedVersionGuid, string tabName,
                                      bool isVisible,
                                      bool disableLink, int parentId, string iconFile, string iconFileLarge,
                                      string title, string description, string keyWords, bool isDeleted, string url,
                                      string skinSrc, string containerSrc, DateTime startDate, DateTime endDate,
                                      int refreshInterval, string pageHeadText, bool isSecure,
                                      bool permanentRedirect, float siteMapPriority, int lastModifiedByuserID,
                                      string cultureCode, bool IsSystem)
        {
            this.ExecuteNonQuery(
                "UpdateTab",
                tabId,
                contentItemId,
                this.GetNull(portalId),
                versionGuid,
                this.GetNull(defaultLanguageGuid),
                localizedVersionGuid,
                tabName,
                isVisible,
                disableLink,
                this.GetNull(parentId),
                iconFile,
                iconFileLarge,
                title,
                description,
                keyWords,
                isDeleted,
                url,
                this.GetNull(skinSrc),
                this.GetNull(containerSrc),
                this.GetNull(startDate),
                this.GetNull(endDate),
                this.GetNull(refreshInterval),
                this.GetNull(pageHeadText),
                isSecure,
                permanentRedirect,
                siteMapPriority,
                lastModifiedByuserID,
                this.GetNull(cultureCode),
                IsSystem);
        }

        public virtual void UpdateTabOrder(int tabId, int tabOrder, int parentId, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateTabOrder", tabId, tabOrder, this.GetNull(parentId), lastModifiedByUserID);
        }

        public virtual void UpdateTabSetting(int TabId, string SettingName, string SettingValue,
                                             int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateTabSetting", TabId, SettingName, SettingValue, lastModifiedByUserID);
        }

        public virtual void UpdateTabTranslationStatus(int tabId, Guid localizedVersionGuid, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateTabTranslationStatus", tabId, localizedVersionGuid, lastModifiedByUserID);
        }

        public virtual void MarkAsPublished(int tabId)
        {
            this.ExecuteNonQuery("PublishTab", tabId);
        }

        public virtual void UpdateTabVersion(int tabId, Guid versionGuid)
        {
            this.ExecuteNonQuery("UpdateTabVersion", tabId, versionGuid);
        }

        public virtual int AddModule(int contentItemId, int portalId, int moduleDefId, bool allTabs, DateTime startDate,
                                     DateTime endDate, bool inheritViewPermissions, bool isShareable,
                                     bool isShareableViewOnly,
                                     bool isDeleted, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddModule",
                contentItemId,
                this.GetNull(portalId),
                moduleDefId,
                allTabs,
                this.GetNull(startDate),
                this.GetNull(endDate),
                inheritViewPermissions,
                isShareable,
                isShareableViewOnly,
                isDeleted,
                createdByUserID);
        }

        public virtual void AddTabModule(int TabId, int ModuleId, string ModuleTitle, string Header, string Footer,
                                         int ModuleOrder, string PaneName, int CacheTime, string CacheMethod,
                                         string Alignment, string Color, string Border, string IconFile, int Visibility,
                                         string ContainerSrc, bool DisplayTitle, bool DisplayPrint,
                                         bool DisplaySyndicate, bool IsWebSlice, string WebSliceTitle,
                                         DateTime WebSliceExpiryDate, int WebSliceTTL, Guid UniqueId, Guid VersionGuid,
                                         Guid DefaultLanguageGuid, Guid LocalizedVersionGuid, string CultureCode,
                                         int createdByUserID)
        {
            this.ExecuteNonQuery(
                "AddTabModule",
                TabId,
                ModuleId,
                ModuleTitle,
                this.GetNull(Header),
                this.GetNull(Footer),
                ModuleOrder,
                PaneName,
                CacheTime,
                this.GetNull(CacheMethod),
                this.GetNull(Alignment),
                this.GetNull(Color),
                this.GetNull(Border),
                this.GetNull(IconFile),
                Visibility,
                this.GetNull(ContainerSrc),
                DisplayTitle,
                DisplayPrint,
                DisplaySyndicate,
                IsWebSlice,
                WebSliceTitle,
                this.GetNull(WebSliceExpiryDate),
                WebSliceTTL,
                UniqueId,
                VersionGuid,
                this.GetNull(DefaultLanguageGuid),
                LocalizedVersionGuid,
                CultureCode,
                createdByUserID);
        }

        public virtual void DeleteModule(int moduleId)
        {
            this.ExecuteNonQuery("DeleteModule", moduleId);
        }

        public virtual void DeleteModuleSetting(int moduleId, string settingName)
        {
            this.ExecuteNonQuery("DeleteModuleSetting", moduleId, settingName);
        }

        public virtual void DeleteModuleSettings(int moduleId)
        {
            this.ExecuteNonQuery("DeleteModuleSettings", moduleId);
        }

        public virtual void DeleteTabModule(int tabId, int moduleId, bool softDelete, int lastModifiedByUserId = -1)
        {
            this.ExecuteNonQuery("DeleteTabModule", tabId, moduleId, softDelete, lastModifiedByUserId);
        }

        public virtual void DeleteTabModuleSetting(int tabModuleId, string settingName)
        {
            this.ExecuteNonQuery("DeleteTabModuleSetting", tabModuleId, settingName);
        }

        public virtual void DeleteTabModuleSettings(int tabModuleId)
        {
            this.ExecuteNonQuery("DeleteTabModuleSettings", tabModuleId);
        }

        public virtual IDataReader GetTabModuleSettingsByName(int portalId, string settingName)
        {
            return this.ExecuteReader("GetTabModuleSettingsByName", portalId, settingName);
        }

        public virtual IDataReader GetTabModuleIdsBySettingNameAndValue(int portalId, string settingName, string expectedValue)
        {
            return this.ExecuteReader("GetTabModuleIdsBySettingNameAndValue", portalId, settingName, expectedValue);
        }

        public virtual IDataReader GetAllModules()
        {
            return this.ExecuteReader("GetAllModules");
        }

        public virtual IDataReader GetAllTabsModules(int portalId, bool allTabs)
        {
            return this.ExecuteReader("GetAllTabsModules", portalId, allTabs);
        }

        public virtual IDataReader GetAllTabsModulesByModuleID(int moduleId)
        {
            return this.ExecuteReader("GetAllTabsModulesByModuleID", moduleId);
        }

        public virtual IDataReader GetModule(int moduleId, int tabId)
        {
            return this.ExecuteReader("GetModule", moduleId, this.GetNull(tabId));
        }

        public virtual IDataReader GetModuleByDefinition(int portalId, string definitionName)
        {
            return this.ExecuteReader("GetModuleByDefinition", this.GetNull(portalId), definitionName);
        }

        public virtual IDataReader GetModuleByUniqueID(Guid uniqueId)
        {
            return this.ExecuteReader("GetModuleByUniqueID", uniqueId);
        }

        public virtual IDataReader GetModules(int portalId)
        {
            return this.ExecuteReader("GetModules", portalId);
        }

        public virtual IDataReader GetModuleSetting(int moduleId, string settingName)
        {
            return this.ExecuteReader("GetModuleSetting", moduleId, settingName);
        }

        public virtual IDataReader GetModuleSettings(int moduleId)
        {
            return this.ExecuteReader("GetModuleSettings", moduleId);
        }

        public virtual IDataReader GetModuleSettingsByTab(int tabId)
        {
            return this.ExecuteReader("GetModuleSettingsByTab", tabId);
        }

        public virtual IDataReader GetSearchModules(int portalId)
        {
            return this.ExecuteReader("GetSearchModules", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabModule(int tabModuleId)
        {
            return this.ExecuteReader("GetTabModule", tabModuleId);
        }

        public virtual IDataReader GetTabModuleOrder(int tabId, string paneName)
        {
            return this.ExecuteReader("GetTabModuleOrder", tabId, paneName);
        }

        public virtual IDataReader GetTabModules(int tabId)
        {
            return this.ExecuteReader("GetTabModules", tabId);
        }

        public virtual IDataReader GetTabModuleSetting(int tabModuleId, string settingName)
        {
            return this.ExecuteReader("GetTabModuleSetting", tabModuleId, settingName);
        }

        public virtual IDataReader GetTabModuleSettings(int tabModuleId)
        {
            return this.ExecuteReader("GetTabModuleSettings", tabModuleId);
        }

        public virtual IDataReader GetTabModuleSettingsByTab(int tabId)
        {
            return this.ExecuteReader("GetTabModuleSettingsByTab", tabId);
        }

        public virtual void MoveTabModule(int fromTabId, int moduleId, int toTabId, string toPaneName,
                                          int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("MoveTabModule", fromTabId, moduleId, toTabId, toPaneName, lastModifiedByUserID);
        }

        public virtual void RestoreTabModule(int tabId, int moduleId)
        {
            this.ExecuteNonQuery("RestoreTabModule", tabId, moduleId);
        }

        public virtual void UpdateModule(int moduleId, int moduleDefId, int contentItemId, bool allTabs,
                                         DateTime startDate,
                                         DateTime endDate, bool inheritViewPermissions, bool isShareable,
                                         bool isShareableViewOnly,
                                         bool isDeleted, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateModule",
                moduleId,
                moduleDefId,
                contentItemId,
                allTabs,
                this.GetNull(startDate),
                this.GetNull(endDate),
                inheritViewPermissions,
                isShareable,
                isShareableViewOnly,
                isDeleted,
                lastModifiedByUserID);
        }

        public virtual void UpdateModuleLastContentModifiedOnDate(int moduleId)
        {
            this.ExecuteNonQuery("UpdateModuleLastContentModifiedOnDate", moduleId);
        }

        public virtual void UpdateModuleOrder(int tabId, int moduleId, int moduleOrder, string paneName)
        {
            this.ExecuteNonQuery("UpdateModuleOrder", tabId, moduleId, moduleOrder, paneName);
        }

        public virtual void UpdateModuleSetting(int moduleId, string settingName, string settingValue,
                                                int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateModuleSetting", moduleId, settingName, settingValue, lastModifiedByUserID);
        }

        public virtual void UpdateTabModule(int TabModuleId, int TabId, int ModuleId, string ModuleTitle, string Header,
                                            string Footer, int ModuleOrder, string PaneName, int CacheTime,
                                            string CacheMethod, string Alignment, string Color, string Border,
                                            string IconFile, int Visibility, string ContainerSrc, bool DisplayTitle,
                                            bool DisplayPrint, bool DisplaySyndicate, bool IsWebSlice,
                                            string WebSliceTitle, DateTime WebSliceExpiryDate, int WebSliceTTL,
                                            Guid VersionGuid,
                                            Guid DefaultLanguageGuid, Guid LocalizedVersionGuid, string CultureCode,
                                            int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateTabModule",
                TabModuleId,
                TabId,
                ModuleId,
                ModuleTitle,
                this.GetNull(Header),
                this.GetNull(Footer),
                ModuleOrder,
                PaneName,
                CacheTime,
                this.GetNull(CacheMethod),
                this.GetNull(Alignment),
                this.GetNull(Color),
                this.GetNull(Border),
                this.GetNull(IconFile),
                Visibility,
                this.GetNull(ContainerSrc),
                DisplayTitle,
                DisplayPrint,
                DisplaySyndicate,
                IsWebSlice,
                WebSliceTitle,
                this.GetNull(WebSliceExpiryDate),
                WebSliceTTL,
                VersionGuid,
                this.GetNull(DefaultLanguageGuid),
                LocalizedVersionGuid,
                CultureCode,
                lastModifiedByUserID);
        }

        public virtual void UpdateTabModuleTranslationStatus(int tabModuleId, Guid localizedVersionGuid,
                                                             int lastModifiedByUserId)
        {
            this.ExecuteNonQuery("UpdateTabModuleTranslationStatus", tabModuleId, localizedVersionGuid, lastModifiedByUserId);
        }

        public virtual void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue,
                                                   int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateTabModuleSetting", tabModuleId, settingName, settingValue, lastModifiedByUserID);
        }

        public virtual void UpdateTabModuleVersion(int tabModuleId, Guid versionGuid)
        {
            this.ExecuteNonQuery("UpdateTabModuleVersion", tabModuleId, versionGuid);
        }

        public virtual void UpdateTabModuleVersionByModule(int moduleId)
        {
            this.ExecuteNonQuery("UpdateTabModuleVersionByModule", moduleId);
        }

        public virtual IDataReader GetInstalledModules()
        {
            return this.ExecuteReader("GetInstalledModules");
        }

        public virtual int AddDesktopModule(int packageID, string moduleName, string folderName, string friendlyName,
                                            string description, string version, bool isPremium, bool isAdmin,
                                            string businessControllerClass, int supportedFeatures, int shareable,
                                            string compatibleVersions, string dependencies, string permissions,
                                            int contentItemId, int createdByUserID, string adminPage, string hostPage)
        {
            return this.ExecuteScalar<int>(
                "AddDesktopModule",
                packageID,
                moduleName,
                folderName,
                friendlyName,
                this.GetNull(description),
                this.GetNull(version),
                isPremium,
                isAdmin,
                businessControllerClass,
                supportedFeatures,
                shareable,
                this.GetNull(compatibleVersions),
                this.GetNull(dependencies),
                this.GetNull(permissions),
                contentItemId,
                createdByUserID,
                adminPage,
                hostPage);
        }

        public virtual void DeleteDesktopModule(int desktopModuleId)
        {
            this.ExecuteNonQuery("DeleteDesktopModule", desktopModuleId);
        }

        public virtual IDataReader GetDesktopModules()
        {
            return this.ExecuteReader("GetDesktopModules");
        }

        public virtual IDataReader GetDesktopModulesByPortal(int portalId)
        {
            return this.ExecuteReader("GetDesktopModulesByPortal", portalId);
        }

        public virtual void UpdateDesktopModule(int desktopModuleId, int packageID, string moduleName, string folderName,
                                                string friendlyName, string description, string version, bool isPremium,
                                                bool isAdmin, string businessControllerClass, int supportedFeatures,
                                                int shareable, string compatibleVersions, string dependencies,
                                                string permissions,
                                                int contentItemId, int lastModifiedByUserID, string adminpage, string hostpage)
        {
            this.ExecuteNonQuery(
                "UpdateDesktopModule",
                desktopModuleId,
                packageID,
                moduleName,
                folderName,
                friendlyName,
                this.GetNull(description),
                this.GetNull(version),
                isPremium,
                isAdmin,
                businessControllerClass,
                supportedFeatures,
                shareable,
                this.GetNull(compatibleVersions),
                this.GetNull(dependencies),
                this.GetNull(permissions),
                contentItemId,
                lastModifiedByUserID,
                adminpage,
                hostpage);
        }

        public virtual int AddPortalDesktopModule(int portalId, int desktopModuleId, int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddPortalDesktopModule", portalId, desktopModuleId, createdByUserID);
        }

        public virtual void DeletePortalDesktopModules(int portalId, int desktopModuleId)
        {
            this.ExecuteNonQuery("DeletePortalDesktopModules", this.GetNull(portalId), this.GetNull(desktopModuleId));
        }

        public virtual IDataReader GetPortalDesktopModules(int portalId, int desktopModuleId)
        {
            return this.ExecuteReader("GetPortalDesktopModules", this.GetNull(portalId), this.GetNull(desktopModuleId));
        }

        public virtual int AddModuleDefinition(int desktopModuleId, string friendlyName, string definitionName,
                                               int defaultCacheTime, int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddModuleDefinition", desktopModuleId, friendlyName, definitionName,
                                      defaultCacheTime, createdByUserID);
        }

        public virtual void DeleteModuleDefinition(int moduleDefId)
        {
            this.ExecuteNonQuery("DeleteModuleDefinition", moduleDefId);
        }

        public virtual IDataReader GetModuleDefinitions()
        {
            return this.ExecuteReader("GetModuleDefinitions");
        }

        public virtual void UpdateModuleDefinition(int moduleDefId, string friendlyName, string definitionName,
                                                   int defaultCacheTime, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateModuleDefinition", moduleDefId, friendlyName, definitionName, defaultCacheTime,
                            lastModifiedByUserID);
        }

        public virtual int AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc,
                                            string iconFile, int controlType, int viewOrder, string helpUrl,
                                            bool supportsPartialRendering, bool supportsPopUps, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddModuleControl",
                this.GetNull(moduleDefId),
                this.GetNull(controlKey),
                this.GetNull(controlTitle),
                controlSrc,
                this.GetNull(iconFile),
                controlType,
                this.GetNull(viewOrder),
                this.GetNull(helpUrl),
                supportsPartialRendering,
                supportsPopUps,
                createdByUserID);
        }

        public virtual void DeleteModuleControl(int moduleControlId)
        {
            this.ExecuteNonQuery("DeleteModuleControl", moduleControlId);
        }

        public virtual IDataReader GetModuleControls()
        {
            return this.ExecuteReader("GetModuleControls");
        }

        public virtual void UpdateModuleControl(int moduleControlId, int moduleDefId, string controlKey,
                                                string controlTitle,
                                                string controlSrc, string iconFile, int controlType, int viewOrder,
                                                string helpUrl,
                                                bool supportsPartialRendering, bool supportsPopUps,
                                                int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateModuleControl",
                moduleControlId,
                this.GetNull(moduleDefId),
                this.GetNull(controlKey),
                this.GetNull(controlTitle),
                controlSrc,
                this.GetNull(iconFile),
                controlType,
                this.GetNull(viewOrder),
                this.GetNull(helpUrl),
                supportsPartialRendering,
                supportsPopUps,
                lastModifiedByUserID);
        }

        public virtual int AddFolder(int portalId, Guid uniqueId, Guid versionGuid, string folderPath, string mappedPath,
                                        int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated,
                                        int createdByUserId, int folderMappingId, bool isVersioned, int workflowId,
                                        int parentId)
        {
            return this.ExecuteScalar<int>(
                "AddFolder",
                this.GetNull(portalId),
                uniqueId,
                versionGuid,
                folderPath,
                mappedPath,
                storageLocation,
                isProtected,
                isCached,
                this.GetNull(lastUpdated),
                createdByUserId,
                folderMappingId,
                isVersioned,
                this.GetNull(workflowId),
                this.GetNull(parentId));
        }

        public virtual void DeleteFolder(int portalId, string folderPath)
        {
            this.ExecuteNonQuery("DeleteFolder", this.GetNull(portalId), folderPath);
        }

        public virtual IDataReader GetFolder(int folderId)
        {
            return this.ExecuteReader("GetFolderByFolderID", folderId);
        }

        public virtual IDataReader GetFolder(int portalId, string folderPath)
        {
            return this.ExecuteReader("GetFolderByFolderPath", this.GetNull(portalId), folderPath);
        }

        public virtual IDataReader GetFolderByUniqueID(Guid uniqueId)
        {
            return this.ExecuteReader("GetFolderByUniqueID", uniqueId);
        }

        public virtual IDataReader GetFoldersByPortal(int portalId)
        {
            return this.ExecuteReader("GetFolders", this.GetNull(portalId));
        }

        public virtual IDataReader GetFoldersByPortalAndPermissions(int portalId, string permissions, int userId)
        {
            return this.ExecuteReader("GetFoldersByPermissions", this.GetNull(portalId), this.GetNull(permissions), this.GetNull(userId), -1, string.Empty);
        }

        public virtual int GetLegacyFolderCount()
        {
            return this.ExecuteScalar<int>("GetLegacyFolderCount");
        }

        public virtual void UpdateFolder(int portalId, Guid versionGuid, int folderId, string folderPath,
                                            int storageLocation, string mappedPath,
                                            bool isProtected, bool isCached, DateTime lastUpdated, int lastModifiedByUserID,
                                            int folderMappingID, bool isVersioned, int workflowID, int parentID)
        {
            this.ExecuteNonQuery(
                "UpdateFolder",
                this.GetNull(portalId),
                versionGuid,
                folderId,
                folderPath,
                mappedPath,
                storageLocation,
                isProtected,
                isCached,
                this.GetNull(lastUpdated),
                lastModifiedByUserID,
                folderMappingID,
                isVersioned,
                this.GetNull(workflowID),
                this.GetNull(parentID));
        }

        public virtual void UpdateFolderVersion(int folderId, Guid versionGuid)
        {
            this.ExecuteNonQuery("UpdateFolderVersion", folderId, versionGuid);
        }

        public virtual IDataReader UpdateLegacyFolders()
        {
            return this.ExecuteReader("UpdateLegacyFolders");
        }

        public virtual int AddFile(int portalId, Guid uniqueId, Guid versionGuid, string fileName, string extension,
                                    long size, int width, int height, string contentType, string folder, int folderId,
                                    int createdByUserID, string hash, DateTime lastModificationTime,
                                    string title, string description, DateTime startDate, DateTime endDate, bool enablePublishPeriod, int contentItemId)
        {
            return this.ExecuteScalar<int>(
                "AddFile",
                this.GetNull(portalId),
                uniqueId,
                versionGuid,
                fileName,
                extension,
                size,
                this.GetNull(width),
                this.GetNull(height),
                contentType,
                folder,
                folderId,
                createdByUserID,
                hash,
                lastModificationTime,
                title,
                description,
                enablePublishPeriod,
                startDate,
                this.GetNull(endDate),
                this.GetNull(contentItemId));
        }

        public virtual void SetFileHasBeenPublished(int fileId, bool hasBeenPublished)
        {
            this.ExecuteNonQuery("SetFileHasBeenPublished", fileId, hasBeenPublished);
        }

        public virtual int CountLegacyFiles()
        {
            return this.ExecuteScalar<int>("CountLegacyFiles");
        }

        public virtual void ClearFileContent(int fileId)
        {
            this.ExecuteNonQuery("ClearFileContent", fileId);
        }

        public virtual void DeleteFile(int portalId, string fileName, int folderId)
        {
            this.ExecuteNonQuery("DeleteFile", this.GetNull(portalId), fileName, folderId);
        }

        public virtual void DeleteFiles(int portalId)
        {
            this.ExecuteNonQuery("DeleteFiles", this.GetNull(portalId));
        }

        public virtual DataTable GetAllFiles()
        {
            return Globals.ConvertDataReaderToDataTable(this.ExecuteReader("GetAllFiles"));
        }

        public virtual IDataReader GetFile(string fileName, int folderId, bool retrieveUnpublishedFiles = false)
        {
            return this.ExecuteReader("GetFile", fileName, folderId, retrieveUnpublishedFiles);
        }

        public virtual IDataReader GetFileById(int fileId, bool retrieveUnpublishedFiles = false)
        {
            return this.ExecuteReader("GetFileById", fileId, retrieveUnpublishedFiles);
        }

        public virtual IDataReader GetFileByUniqueID(Guid uniqueId)
        {
            return this.ExecuteReader("GetFileByUniqueID", uniqueId);
        }

        public virtual IDataReader GetFileContent(int fileId)
        {
            return this.ExecuteReader("GetFileContent", fileId);
        }

        public virtual IDataReader GetFileVersionContent(int fileId, int version)
        {
            return this.ExecuteReader("GetFileVersionContent", fileId, version);
        }

        public virtual IDataReader GetFiles(int folderId, bool retrieveUnpublishedFiles, bool recursive)
        {
            return this.ExecuteReader("GetFiles", folderId, retrieveUnpublishedFiles, recursive);
        }

        /// <summary>
        /// This is an internal method for communication between DNN business layer and SQL database.
        /// Do not use in custom modules, please use API (DotNetNuke.Services.FileSystem.FileManager.UpdateFile)
        ///
        /// Stores information about a specific file, stored in DNN filesystem
        /// calling petapoco method to call the underlying stored procedure "UpdateFile".
        /// </summary>
        /// <param name="fileId">ID of the (already existing) file.</param>
        /// <param name="versionGuid">GUID of this file version  (should usually not be modified).</param>
        /// <param name="fileName">Name of the file in the file system (including extension).</param>
        /// <param name="extension">File type - should meet extension in FileName.</param>
        /// <param name="size">Size of file (bytes).</param>
        /// <param name="width">Width of images/video (lazy load: pass Null, might be retrieved by DNN platform on db file sync).</param>
        /// <param name="height">Height of images/video (lazy load: pass Null, might be retrieved by DNN platform on db file snyc).</param>
        /// <param name="contentType">MIME type of the file.</param>
        /// <param name="folderId">ID of the folder, the file resides in.</param>
        /// <param name="lastModifiedByUserID">ID of the user, who performed last update of file or file info.</param>
        /// <param name="hash">SHa1 hash of the file content, used for file versioning (lazy load: pass Null, will be generated by DNN platform on db file sync).</param>
        /// <param name="lastModificationTime">timestamp, when last update of file or file info happened.</param>
        /// <param name="title">Display title of the file - optional (pass Null if not provided).</param>
        /// <param name="description">Description of the file.</param>
        /// <param name="startDate">date and time (server TZ), from which the file should be displayed/accessible (according to folder permission).</param>
        /// <param name="endDate">date and time (server TZ), until which the file should be displayed/accessible (according to folder permission).</param>
        /// <param name="enablePublishPeriod">shall startdate/end date be used?.</param>
        /// <param name="contentItemId">ID of the associated contentitem with description etc. (optional).</param>
        public virtual void UpdateFile(int fileId, Guid versionGuid, string fileName, string extension, long size,
                                        int width, int height, string contentType, int folderId,
                                        int lastModifiedByUserID, string hash, DateTime lastModificationTime,
                                        string title, string description, DateTime startDate, DateTime endDate, bool enablePublishPeriod, int contentItemId)
        {
            this.ExecuteNonQuery(
                "UpdateFile",
                fileId,
                versionGuid,
                fileName,
                extension,
                size,
                this.GetNull(width),
                this.GetNull(height),
                contentType,
                folderId,
                lastModifiedByUserID,
                hash,
                lastModificationTime,
                title,
                description,
                enablePublishPeriod,
                startDate,
                this.GetNull(endDate),
                this.GetNull(contentItemId));
        }

        public virtual void UpdateFileLastModificationTime(int fileId, DateTime lastModificationTime)
        {
            this.ExecuteNonQuery(
                "UpdateFileLastModificationTime",
                fileId,
                lastModificationTime);
        }

        public virtual void UpdateFileHashCode(int fileId, string hashCode)
        {
            this.ExecuteNonQuery(
                "UpdateFileHashCode",
                fileId,
                hashCode);
        }

        public virtual void UpdateFileContent(int fileId, byte[] content)
        {
            this.ExecuteNonQuery("UpdateFileContent", fileId, this.GetNull(content));
        }

        public virtual void UpdateFileVersion(int fileId, Guid versionGuid)
        {
            this.ExecuteNonQuery("UpdateFileVersion", fileId, versionGuid);
        }

        public virtual int AddPermission(string permissionCode, int moduleDefID, string permissionKey, string permissionName, int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddPermission", moduleDefID, permissionCode, permissionKey, permissionName, createdByUserID);
        }

        public virtual void DeletePermission(int permissionID)
        {
            this.ExecuteNonQuery("DeletePermission", permissionID);
        }

        public virtual void UpdatePermission(int permissionID, string permissionCode, int moduleDefID, string permissionKey, string permissionName, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdatePermission", permissionID, permissionCode, moduleDefID, permissionKey, permissionName, lastModifiedByUserID);
        }

        public virtual int AddModulePermission(int moduleId, int portalId, int permissionId, int roleId,
                                               bool allowAccess, int userId,
                                               int createdByUserId)
        {
            return this.ExecuteScalar<int>(
                "AddModulePermission",
                moduleId,
                portalId,
                permissionId,
                this.GetRoleNull(roleId),
                allowAccess,
                this.GetNull(userId),
                createdByUserId);
        }

        public virtual void DeleteModulePermission(int modulePermissionId)
        {
            this.ExecuteNonQuery("DeleteModulePermission", modulePermissionId);
        }

        public virtual void DeleteModulePermissionsByModuleID(int moduleId, int portalId)
        {
            this.ExecuteNonQuery("DeleteModulePermissionsByModuleID", moduleId, portalId);
        }

        public virtual void DeleteModulePermissionsByUserID(int portalId, int userId)
        {
            this.ExecuteNonQuery("DeleteModulePermissionsByUserID", portalId, userId);
        }

        public virtual IDataReader GetModulePermission(int modulePermissionId)
        {
            return this.ExecuteReader("GetModulePermission", modulePermissionId);
        }

        public virtual IDataReader GetModulePermissionsByModuleID(int moduleID, int permissionId)
        {
            return this.ExecuteReader("GetModulePermissionsByModuleID", moduleID, permissionId);
        }

        public virtual IDataReader GetModulePermissionsByPortal(int portalId)
        {
            return this.ExecuteReader("GetModulePermissionsByPortal", portalId);
        }

        public virtual IDataReader GetModulePermissionsByTabID(int tabId)
        {
            return this.ExecuteReader("GetModulePermissionsByTabID", tabId);
        }

        public virtual void UpdateModulePermission(int modulePermissionId, int moduleId, int portalId, int permissionId,
                                                   int roleId,
                                                   bool allowAccess, int userId, int lastModifiedByUserId)
        {
            this.ExecuteNonQuery(
                "UpdateModulePermission",
                modulePermissionId,
                moduleId,
                portalId,
                permissionId,
                this.GetRoleNull(roleId),
                allowAccess,
                this.GetNull(userId),
                lastModifiedByUserId);
        }

        public virtual int AddTabPermission(int tabId, int permissionId, int roleID, bool allowAccess, int userId,
                                            int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddTabPermission",
                tabId,
                permissionId,
                this.GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userId),
                createdByUserID);
        }

        public virtual void DeleteTabPermission(int tabPermissionId)
        {
            this.ExecuteNonQuery("DeleteTabPermission", tabPermissionId);
        }

        public virtual void DeleteTabPermissionsByTabID(int tabId)
        {
            this.ExecuteNonQuery("DeleteTabPermissionsByTabID", tabId);
        }

        public virtual void DeleteTabPermissionsByUserID(int portalId, int userId)
        {
            this.ExecuteNonQuery("DeleteTabPermissionsByUserID", portalId, userId);
        }

        public virtual IDataReader GetTabPermissionsByPortal(int portalId)
        {
            return this.ExecuteReader("GetTabPermissionsByPortal", this.GetNull(portalId));
        }

        public virtual IDataReader GetTabPermissionsByTabID(int tabId, int permissionId)
        {
            return this.ExecuteReader("GetTabPermissionsByTabID", tabId, permissionId);
        }

        public virtual void UpdateTabPermission(int tabPermissionId, int tabId, int permissionId, int roleID,
                                                bool allowAccess,
                                                int userId, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateTabPermission",
                tabPermissionId,
                tabId,
                permissionId,
                this.GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userId),
                lastModifiedByUserID);
        }

        public virtual int AddFolderPermission(int folderId, int permissionId, int roleID, bool allowAccess, int userId,
                                               int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddFolderPermission",
                folderId,
                permissionId,
                this.GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userId),
                createdByUserID);
        }

        public virtual void DeleteFolderPermission(int folderPermissionId)
        {
            this.ExecuteNonQuery("DeleteFolderPermission", folderPermissionId);
        }

        public virtual void DeleteFolderPermissionsByFolderPath(int portalId, string folderPath)
        {
            this.ExecuteNonQuery("DeleteFolderPermissionsByFolderPath", this.GetNull(portalId), folderPath);
        }

        public virtual void DeleteFolderPermissionsByUserID(int portalId, int userId)
        {
            this.ExecuteNonQuery("DeleteFolderPermissionsByUserID", portalId, userId);
        }

        public virtual IDataReader GetFolderPermission(int folderPermissionId)
        {
            return this.ExecuteReader("GetFolderPermission", folderPermissionId);
        }

        public virtual IDataReader GetFolderPermissionsByFolderPath(int portalId, string folderPath, int permissionId)
        {
            return this.ExecuteReader("GetFolderPermissionsByFolderPath", this.GetNull(portalId), folderPath, permissionId);
        }

        public virtual IDataReader GetFolderPermissionsByPortal(int portalId)
        {
            return this.ExecuteReader("GetFolderPermissionsByPortal", this.GetNull(portalId));
        }

        public virtual IDataReader GetFolderPermissionsByPortalAndPath(int portalId, string pathName)
        {
            return this.ExecuteReader("GetFolderPermissionsByPortalAndPath", this.GetNull(portalId), pathName ?? string.Empty);
        }

        public virtual void UpdateFolderPermission(int FolderPermissionID, int FolderID, int PermissionID, int roleID,
                                                   bool AllowAccess, int UserID, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateFolderPermission",
                FolderPermissionID,
                FolderID,
                PermissionID,
                this.GetRoleNull(roleID),
                AllowAccess,
                this.GetNull(UserID),
                lastModifiedByUserID);
        }

        public virtual int AddDesktopModulePermission(int portalDesktopModuleID, int permissionID, int roleID,
                                            bool allowAccess, int userID, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddDesktopModulePermission",
                portalDesktopModuleID,
                permissionID,
                this.GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userID),
                createdByUserID);
        }

        public virtual void DeleteDesktopModulePermission(int desktopModulePermissionID)
        {
            this.ExecuteNonQuery("DeleteDesktopModulePermission", desktopModulePermissionID);
        }

        public virtual void DeleteDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
        {
            this.ExecuteNonQuery("DeleteDesktopModulePermissionsByPortalDesktopModuleID", portalDesktopModuleID);
        }

        public virtual void DeleteDesktopModulePermissionsByUserID(int userID, int portalID)
        {
            this.ExecuteNonQuery("DeleteDesktopModulePermissionsByUserID", userID, portalID);
        }

        public virtual IDataReader GetDesktopModulePermission(int desktopModulePermissionID)
        {
            return this.ExecuteReader("GetDesktopModulePermission", desktopModulePermissionID);
        }

        public virtual IDataReader GetDesktopModulePermissions()
        {
            return this.ExecuteReader("GetDesktopModulePermissions");
        }

        public virtual IDataReader GetDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
        {
            return this.ExecuteReader("GetDesktopModulePermissionsByPortalDesktopModuleID", portalDesktopModuleID);
        }

        public virtual void UpdateDesktopModulePermission(int desktopModulePermissionID, int portalDesktopModuleID,
                                            int permissionID, int roleID, bool allowAccess, int userID,
                                            int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateDesktopModulePermission",
                desktopModulePermissionID,
                portalDesktopModuleID,
                permissionID,
                this.GetRoleNull(roleID),
                allowAccess,
                this.GetNull(userID),
                lastModifiedByUserID);
        }

        public virtual int AddRoleGroup(int portalId, string groupName, string description, int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddRoleGroup", portalId, groupName, description, createdByUserID);
        }

        public virtual void DeleteRoleGroup(int roleGroupId)
        {
            this.ExecuteNonQuery("DeleteRoleGroup", roleGroupId);
        }

        public virtual IDataReader GetRoleGroup(int portalId, int roleGroupId)
        {
            return this.ExecuteReader("GetRoleGroup", portalId, roleGroupId);
        }

        public virtual IDataReader GetRoleGroupByName(int portalID, string roleGroupName)
        {
            return this.ExecuteReader("GetRoleGroupByName", portalID, roleGroupName);
        }

        public virtual IDataReader GetRoleGroups(int portalId)
        {
            return this.ExecuteReader("GetRoleGroups", portalId);
        }

        public virtual void UpdateRoleGroup(int roleGroupId, string groupName, string description,
                                            int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateRoleGroup", roleGroupId, groupName, description, lastModifiedByUserID);
        }

        public virtual int AddRole(int portalId, int roleGroupId, string roleName, string description, float serviceFee,
                                   string billingPeriod, string billingFrequency, float trialFee, int trialPeriod,
                                   string trialFrequency, bool isPublic, bool autoAssignment, string rsvpCode,
                                   string iconFile, int createdByUserID, int status, int securityMode, bool isSystemRole)
        {
            return this.ExecuteScalar<int>(
                "AddRole",
                portalId,
                this.GetNull(roleGroupId),
                roleName,
                description,
                serviceFee,
                billingPeriod,
                this.GetNull(billingFrequency),
                trialFee,
                trialPeriod,
                this.GetNull(trialFrequency),
                isPublic,
                autoAssignment,
                rsvpCode,
                iconFile,
                createdByUserID,
                status,
                securityMode,
                isSystemRole);
        }

        public virtual void DeleteRole(int roleId)
        {
            this.ExecuteNonQuery("DeleteRole", roleId);
        }

        public virtual IDataReader GetPortalRoles(int portalId)
        {
            return this.ExecuteReader("GetPortalRoles", portalId);
        }

        public virtual IDataReader GetRoles()
        {
            return this.ExecuteReader("GetRoles");
        }

        public virtual IDataReader GetRolesBasicSearch(int portalID, int pageIndex, int pageSize, string filterBy)
        {
            return this.ExecuteReader("GetRolesBasicSearch", portalID, pageIndex, pageSize, filterBy);
        }

        public virtual IDataReader GetRoleSettings(int roleId)
        {
            return this.ExecuteReader("GetRoleSettings", roleId);
        }

        public virtual void UpdateRole(int roleId, int roleGroupId, string roleName, string description,
                                       float serviceFee, string billingPeriod, string billingFrequency, float trialFee,
                                       int trialPeriod,
                                       string trialFrequency, bool isPublic, bool autoAssignment, string rsvpCode,
                                       string iconFile, int lastModifiedByUserID, int status, int securityMode,
                                       bool isSystemRole)
        {
            this.ExecuteNonQuery(
                "UpdateRole",
                roleId,
                this.GetNull(roleGroupId),
                roleName,
                description,
                serviceFee,
                billingPeriod,
                this.GetNull(billingFrequency),
                trialFee,
                trialPeriod,
                this.GetNull(trialFrequency),
                isPublic,
                autoAssignment,
                rsvpCode,
                iconFile,
                lastModifiedByUserID,
                status,
                securityMode,
                isSystemRole);
        }

        public virtual void UpdateRoleSetting(int roleId, string settingName, string settingValue,
                                              int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateRoleSetting",
                roleId,
                settingName,
                settingValue,
                lastModifiedByUserID);
        }

        public virtual int AddUser(int portalID, string username, string firstName, string lastName, int affiliateId,
                                    bool isSuperUser, string email, string displayName, bool updatePassword,
                                    bool isApproved, int createdByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddUser",
                portalID,
                username,
                firstName,
                lastName,
                this.GetNull(affiliateId),
                isSuperUser,
                email,
                displayName,
                updatePassword,
                isApproved,
                createdByUserID);
        }

        public virtual void AddUserPortal(int portalId, int userId)
        {
            this.ExecuteNonQuery("AddUserPortal", portalId, userId);
        }

        public virtual void ChangeUsername(int userId, string newUsername)
        {
            this.ExecuteNonQuery("ChangeUsername", userId, newUsername);
        }

        public virtual void DeleteUserFromPortal(int userId, int portalId)
        {
            this.ExecuteNonQuery("DeleteUserPortal", userId, this.GetNull(portalId));
        }

        public virtual IDataReader GetAllUsers(int portalID, int pageIndex, int pageSize, bool includeDeleted,
                                               bool superUsersOnly)
        {
            return this.ExecuteReader("GetAllUsers", this.GetNull(portalID), pageIndex, pageSize, includeDeleted, superUsersOnly);
        }

        public virtual IDataReader GetDeletedUsers(int portalId)
        {
            return this.ExecuteReader("GetDeletedUsers", this.GetNull(portalId));
        }

        public virtual IDataReader GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
        {
            return this.ExecuteReader("GetUnAuthorizedUsers", this.GetNull(portalId), includeDeleted, superUsersOnly);
        }

        public virtual IDataReader GetUser(int portalId, int userId)
        {
            return this.ExecuteReader("GetUser", portalId, userId);
        }

        public virtual IDataReader GetUserByAuthToken(int portalID, string userToken, string authType)
        {
            return this.ExecuteReader("GetUserByAuthToken", portalID, userToken, authType);
        }

        public virtual IDataReader GetUserByDisplayName(int portalId, string displayName)
        {
            return this.ExecuteReader("GetUserByDisplayName", this.GetNull(portalId), displayName);
        }

        public virtual IDataReader GetUserByUsername(int portalId, string username)
        {
            return this.ExecuteReader("GetUserByUsername", this.GetNull(portalId), username);
        }

        public virtual IDataReader GetUserByUsername(string username, string spaceReplacement)
        {
            return this.ExecuteReader("GetUserByUsernameForUrl", username, spaceReplacement);
        }

        public virtual IDataReader GetUserByVanityUrl(int portalId, string vanityUrl)
        {
            return this.ExecuteReader("GetUserByVanityUrl", this.GetNull(portalId), vanityUrl);
        }

        public virtual IDataReader GetUserByPasswordResetToken(int portalId, string resetToken)
        {
            return this.ExecuteReader("GetUserByPasswordResetToken", this.GetNull(portalId), resetToken);
        }

        public virtual IDataReader GetDisplayNameForUser(int userId, string spaceReplacement)
        {
            return this.ExecuteReader("GetDisplayNameForUser", userId, spaceReplacement);
        }

        public virtual int GetUserCountByPortal(int portalId)
        {
            return this.ExecuteScalar<int>("GetUserCountByPortal", portalId);
        }

        public virtual IDataReader GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int fitlerRoleId,
                                                          int relationTypeId,
                                                          bool isAdmin, int pageIndex, int pageSize, string sortColumn,
                                                          bool sortAscending,
                                                          string propertyNames, string propertyValues)
        {
            var ps = PortalSecurity.Instance;
            string filterSort = ps.InputFilter(sortColumn, PortalSecurity.FilterFlag.NoSQL);
            string filterName = ps.InputFilter(propertyNames, PortalSecurity.FilterFlag.NoSQL);
            string filterValue = ps.InputFilter(propertyValues, PortalSecurity.FilterFlag.NoSQL);
            return this.ExecuteReader("GetUsersAdvancedSearch", portalId, userId, filterUserId, fitlerRoleId, relationTypeId,
                                 isAdmin, pageSize, pageIndex, filterSort, sortAscending, filterName, filterValue);
        }

        public virtual IDataReader GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn,
                                    bool sortAscending, string propertyName, string propertyValue)
        {
            var ps = PortalSecurity.Instance;
            string filterSort = ps.InputFilter(sortColumn, PortalSecurity.FilterFlag.NoSQL);
            string filterName = ps.InputFilter(propertyName, PortalSecurity.FilterFlag.NoSQL);
            string filterValue = ps.InputFilter(propertyValue, PortalSecurity.FilterFlag.NoSQL);
            return this.ExecuteReader("GetUsersBasicSearch", portalId, pageSize, pageIndex, filterSort, sortAscending,
                                 filterName, filterValue);
        }

        public virtual IDataReader GetUsersByEmail(int portalID, string email, int pageIndex, int pageSize,
                                                   bool includeDeleted, bool superUsersOnly)
        {
            return this.ExecuteReader("GetUsersByEmail", this.GetNull(portalID), email, pageIndex, pageSize, includeDeleted,
                                 superUsersOnly);
        }

        public virtual IDataReader GetUsersByProfileProperty(int portalID, string propertyName, string propertyValue,
                                                             int pageIndex, int pageSize, bool includeDeleted,
                                                             bool superUsersOnly)
        {
            return this.ExecuteReader("GetUsersByProfileProperty", this.GetNull(portalID), propertyName, propertyValue, pageIndex,
                                 pageSize, includeDeleted, superUsersOnly);
        }

        public virtual IDataReader GetUsersByRolename(int portalID, string rolename)
        {
            return this.ExecuteReader("GetUsersByRolename", this.GetNull(portalID), rolename);
        }

        public virtual IDataReader GetUsersByUsername(int portalID, string username, int pageIndex, int pageSize,
                                                      bool includeDeleted, bool superUsersOnly)
        {
            return this.ExecuteReader("GetUsersByUsername", this.GetNull(portalID), username, pageIndex, pageSize, includeDeleted,
                                 superUsersOnly);
        }

        public virtual IDataReader GetUsersByDisplayname(int portalId, string name, int pageIndex, int pageSize,
                                                      bool includeDeleted, bool superUsersOnly)
        {
            return this.ExecuteReader("GetUsersByDisplayname", this.GetNull(portalId), name, pageIndex, pageSize, includeDeleted,
                                 superUsersOnly);
        }

        public virtual int GetDuplicateEmailCount(int portalId)
        {
            return this.ExecuteScalar<int>("GetDuplicateEmailCount", portalId);
        }

        public virtual int GetSingleUserByEmail(int portalId, string emailToMatch)
        {
            return this.ExecuteScalar<int>("GetSingleUserByEmail", portalId, emailToMatch);
        }

        public virtual void RemoveUser(int userId, int portalId)
        {
            this.ExecuteNonQuery("RemoveUser", userId, this.GetNull(portalId));
        }

        public virtual void ResetTermsAgreement(int portalId)
        {
            this.ExecuteNonQuery("ResetTermsAgreement", portalId);
        }

        public virtual void RestoreUser(int userId, int portalId)
        {
            this.ExecuteNonQuery("RestoreUser", userId, this.GetNull(portalId));
        }

        public virtual void UpdateUser(int userId, int portalID, string firstName, string lastName, bool isSuperUser,
                                        string email, string displayName, string vanityUrl, bool updatePassword,
                                        bool isApproved, bool refreshRoles, string lastIpAddress, Guid passwordResetToken,
                                        DateTime passwordResetExpiration, bool isDeleted, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateUser",
                userId,
                this.GetNull(portalID),
                firstName,
                lastName,
                isSuperUser,
                email,
                displayName,
                vanityUrl,
                updatePassword,
                isApproved,
                refreshRoles,
                lastIpAddress,
                this.GetNull(passwordResetToken),
                this.GetNull(passwordResetExpiration),
                isDeleted,
                lastModifiedByUserID);
        }

        public virtual void UpdateUserLastIpAddress(int userId, string lastIpAddress)
        {
            this.ExecuteNonQuery("UpdateUserLastIpAddress", userId, lastIpAddress);
        }

        public virtual void UserAgreedToTerms(int portalId, int userId)
        {
            this.ExecuteNonQuery("UserAgreedToTerms", portalId, userId);
        }

        public virtual void UserRequestsRemoval(int portalId, int userId, bool remove)
        {
            this.ExecuteNonQuery("UserRequestsRemoval", portalId, userId, remove);
        }

        public virtual int AddUserRole(int portalId, int userId, int roleId, int status, bool isOwner,
                                       DateTime effectiveDate, DateTime expiryDate, int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddUserRole", portalId, userId, roleId, status, isOwner, this.GetNull(effectiveDate),
                                      this.GetNull(expiryDate), createdByUserID);
        }

        public virtual void DeleteUserRole(int userId, int roleId)
        {
            this.ExecuteNonQuery("DeleteUserRole", userId, roleId);
        }

        public virtual IDataReader GetServices(int portalId, int userId)
        {
            return this.ExecuteReader("GetServices", portalId, this.GetNull(userId));
        }

        public virtual IDataReader GetUserRole(int portalID, int userId, int roleId)
        {
            return this.ExecuteReader("GetUserRole", portalID, userId, roleId);
        }

        public virtual IDataReader GetUserRoles(int portalID, int userId)
        {
            return this.ExecuteReader("GetUserRoles", portalID, userId);
        }

        public virtual IDataReader GetUserRolesByUsername(int portalID, string username, string rolename)
        {
            return this.ExecuteReader("GetUserRolesByUsername", portalID, this.GetNull(username), this.GetNull(rolename));
        }

        public virtual void UpdateUserRole(int userRoleId, int status, bool isOwner, DateTime effectiveDate,
                                           DateTime expiryDate, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateUserRole", userRoleId, status, isOwner, this.GetNull(effectiveDate), this.GetNull(expiryDate),
                            lastModifiedByUserID);
        }

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public virtual void DeleteUsersOnline(int timeWindow)
        {
            this.ExecuteNonQuery("DeleteUsersOnline", timeWindow);
        }

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public virtual IDataReader GetOnlineUser(int userId)
        {
            return this.ExecuteReader("GetOnlineUser", userId);
        }

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public virtual IDataReader GetOnlineUsers(int portalId)
        {
            return this.ExecuteReader("GetOnlineUsers", portalId);
        }

        [Obsolete("Support for users online was removed in 8.x, other solutions exist outside of the DNN Platform.  Scheduled removal in v11.0.0.")]
        public virtual void UpdateUsersOnline(Hashtable userList)
        {
            if (userList.Count == 0)
            {
                // No users to process, quit method
                return;
            }

            foreach (string key in userList.Keys)
            {
                var info = userList[key] as AnonymousUserInfo;
                if (info != null)
                {
                    var user = info;
                    this.ExecuteNonQuery("UpdateAnonymousUser", user.UserID, user.PortalID, user.TabID, user.LastActiveDate);
                }
                else if (userList[key] is OnlineUserInfo)
                {
                    var user = (OnlineUserInfo)userList[key];
                    this.ExecuteNonQuery("UpdateOnlineUser", user.UserID, user.PortalID, user.TabID, user.LastActiveDate);
                }
            }
        }

        public virtual int AddPropertyDefinition(int portalId, int moduleDefId, int dataType, string defaultValue,
                                                    string propertyCategory, string propertyName, bool readOnly,
                                                    bool required, string validationExpression, int viewOrder,
                                                    bool visible, int length, int defaultVisibility, int createdByUserId)
        {
            int retValue;
            try
            {
                retValue = this.ExecuteScalar<int>(
                    "AddPropertyDefinition",
                    this.GetNull(portalId),
                    moduleDefId,
                    dataType,
                    defaultValue,
                    propertyCategory,
                    propertyName,
                    readOnly,
                    required,
                    validationExpression,
                    viewOrder,
                    visible,
                    length,
                    defaultVisibility,
                    createdByUserId);
            }
            catch (SqlException ex)
            {
                Logger.Debug(ex);

                // If not a duplicate (throw an Exception)
                retValue = -ex.Number;
                if (ex.Number != DuplicateKey)
                {
                    throw;
                }
            }

            return retValue;
        }

        public virtual void DeletePropertyDefinition(int definitionId)
        {
            this.ExecuteNonQuery("DeletePropertyDefinition", definitionId);
        }

        public virtual IDataReader GetPropertyDefinition(int definitionId)
        {
            return this.ExecuteReader("GetPropertyDefinition", definitionId);
        }

        public virtual IDataReader GetPropertyDefinitionByName(int portalId, string name)
        {
            return this.ExecuteReader("GetPropertyDefinitionByName", this.GetNull(portalId), name);
        }

        public virtual IDataReader GetPropertyDefinitionsByPortal(int portalId)
        {
            return this.ExecuteReader("GetPropertyDefinitionsByPortal", this.GetNull(portalId));
        }

        public virtual IDataReader GetUserProfile(int userId)
        {
            return this.ExecuteReader("GetUserProfile", userId);
        }

        public virtual void UpdateProfileProperty(int profileId, int userId, int propertyDefinitionID,
                                                  string propertyValue,
                                                  int visibility, string extendedVisibility, DateTime lastUpdatedDate)
        {
            this.ExecuteNonQuery("UpdateUserProfileProperty", this.GetNull(profileId), userId, propertyDefinitionID, propertyValue,
                                                    visibility, extendedVisibility, lastUpdatedDate);
        }

        public virtual void UpdatePropertyDefinition(int propertyDefinitionId, int dataType, string defaultValue,
                                                        string propertyCategory, string propertyName, bool readOnly,
                                                        bool required, string validation, int viewOrder, bool visible,
                                                        int length, int defaultVisibility, int lastModifiedByUserId)
        {
            this.ExecuteNonQuery(
                "UpdatePropertyDefinition",
                propertyDefinitionId,
                dataType,
                defaultValue,
                propertyCategory,
                propertyName,
                readOnly,
                required,
                validation,
                viewOrder,
                visible,
                length,
                defaultVisibility,
                lastModifiedByUserId);
        }

        public virtual IDataReader SearchProfilePropertyValues(int portalId, string propertyName, string searchString)
        {
            return this.ExecuteReader("SearchProfilePropertyValues", portalId, propertyName, searchString);
        }

        public virtual int AddSkinControl(int packageID, string ControlKey, string ControlSrc,
                                          bool SupportsPartialRendering,
                                          int CreatedByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddSkinControl",
                this.GetNull(packageID),
                this.GetNull(ControlKey),
                ControlSrc,
                SupportsPartialRendering,
                CreatedByUserID);
        }

        public virtual void DeleteSkinControl(int skinControlID)
        {
            this.ExecuteNonQuery("DeleteSkinControl", skinControlID);
        }

        public virtual IDataReader GetSkinControls()
        {
            return this.ExecuteReader("GetSkinControls");
        }

        public virtual IDataReader GetSkinControl(int skinControlID)
        {
            return this.ExecuteReader("GetSkinControl", skinControlID);
        }

        public virtual IDataReader GetSkinControlByKey(string controlKey)
        {
            return this.ExecuteReader("GetSkinControlByKey", controlKey);
        }

        public virtual IDataReader GetSkinControlByPackageID(int packageID)
        {
            return this.ExecuteReader("GetSkinControlByPackageID", packageID);
        }

        public virtual void UpdateSkinControl(int skinControlID, int packageID, string ControlKey, string ControlSrc,
                                                bool SupportsPartialRendering, int LastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateSkinControl",
                skinControlID,
                this.GetNull(packageID),
                this.GetNull(ControlKey),
                ControlSrc,
                SupportsPartialRendering,
                LastModifiedByUserID);
        }

        public virtual int AddSkin(int skinPackageID, string skinSrc)
        {
            return this.ExecuteScalar<int>("AddSkin", skinPackageID, skinSrc);
        }

        public virtual int AddSkinPackage(int packageID, int portalID, string skinName, string skinType,
                                          int createdByUserId)
        {
            return this.ExecuteScalar<int>("AddSkinPackage", packageID, this.GetNull(portalID), skinName, skinType,
                                      createdByUserId);
        }

        public virtual bool CanDeleteSkin(string skinType, string skinFoldername)
        {
            return this.ExecuteScalar<int>("CanDeleteSkin", skinType, skinFoldername) == 1;
        }

        public virtual void DeleteSkin(int skinID)
        {
            this.ExecuteNonQuery("DeleteSkin", skinID);
        }

        public virtual void DeleteSkinPackage(int skinPackageID)
        {
            this.ExecuteNonQuery("DeleteSkinPackage", skinPackageID);
        }

        public virtual IDataReader GetSkinByPackageID(int packageID)
        {
            return this.ExecuteReader("GetSkinPackageByPackageID", packageID);
        }

        public virtual IDataReader GetSkinPackage(int portalID, string skinName, string skinType)
        {
            return this.ExecuteReader("GetSkinPackage", this.GetNull(portalID), skinName, skinType);
        }

        public virtual void UpdateSkin(int skinID, string skinSrc)
        {
            this.ExecuteNonQuery("UpdateSkin", skinID, skinSrc);
        }

        public virtual void UpdateSkinPackage(int skinPackageID, int packageID, int portalID, string skinName,
                                              string skinType, int LastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateSkinPackage", skinPackageID, packageID, this.GetNull(portalID), skinName, skinType,
                            LastModifiedByUserID);
        }

        public virtual void AddProfile(int userId, int portalId)
        {
            this.ExecuteNonQuery("AddProfile", userId, portalId);
        }

        public virtual IDataReader GetAllProfiles()
        {
            return this.ExecuteReader("GetAllProfiles");
        }

        public virtual IDataReader GetProfile(int userId, int portalId)
        {
            return this.ExecuteReader("GetProfile", userId, portalId);
        }

        public virtual void UpdateProfile(int userId, int portalId, string profileData)
        {
            this.ExecuteNonQuery("UpdateProfile", userId, portalId, profileData);
        }

        public virtual void AddUrl(int PortalID, string Url)
        {
            this.ExecuteNonQuery("AddUrl", PortalID, Url);
        }

        public virtual void AddUrlLog(int UrlTrackingID, int UserID)
        {
            this.ExecuteNonQuery("AddUrlLog", UrlTrackingID, this.GetNull(UserID));
        }

        public virtual void AddUrlTracking(int PortalID, string Url, string UrlType, bool LogActivity, bool TrackClicks,
                                           int ModuleID, bool NewWindow)
        {
            this.ExecuteNonQuery("AddUrlTracking", PortalID, Url, UrlType, LogActivity, TrackClicks, this.GetNull(ModuleID),
                            NewWindow);
        }

        public virtual void DeleteUrl(int PortalID, string Url)
        {
            this.ExecuteNonQuery("DeleteUrl", PortalID, Url);
        }

        public virtual void DeleteUrlTracking(int PortalID, string Url, int ModuleID)
        {
            this.ExecuteNonQuery("DeleteUrlTracking", PortalID, Url, this.GetNull(ModuleID));
        }

        public virtual IDataReader GetUrl(int PortalID, string Url)
        {
            return this.ExecuteReader("GetUrl", PortalID, Url);
        }

        public virtual IDataReader GetUrlLog(int UrlTrackingID, DateTime StartDate, DateTime EndDate)
        {
            return this.ExecuteReader("GetUrlLog", UrlTrackingID, this.GetNull(StartDate), this.GetNull(EndDate));
        }

        public virtual IDataReader GetUrls(int PortalID)
        {
            return this.ExecuteReader("GetUrls", PortalID);
        }

        public virtual IDataReader GetUrlTracking(int PortalID, string Url, int ModuleID)
        {
            return this.ExecuteReader("GetUrlTracking", PortalID, Url, this.GetNull(ModuleID));
        }

        public virtual void UpdateUrlTracking(int PortalID, string Url, bool LogActivity, bool TrackClicks, int ModuleID,
                                              bool NewWindow)
        {
            this.ExecuteNonQuery("UpdateUrlTracking", PortalID, Url, LogActivity, TrackClicks, this.GetNull(ModuleID), NewWindow);
        }

        public virtual void UpdateUrlTrackingStats(int PortalID, string Url, int ModuleID)
        {
            this.ExecuteNonQuery("UpdateUrlTrackingStats", PortalID, Url, this.GetNull(ModuleID));
        }

        public virtual IDataReader GetDefaultLanguageByModule(string ModuleList)
        {
            return this.ExecuteReader("GetDefaultLanguageByModule", ModuleList);
        }

        public virtual IDataReader GetSearchCommonWordsByLocale(string Locale)
        {
            return this.ExecuteReader("GetSearchCommonWordsByLocale", Locale);
        }

        public virtual IDataReader GetSearchIndexers()
        {
            return this.ExecuteReader("GetSearchIndexers");
        }

        public virtual IDataReader GetSearchResultModules(int PortalID)
        {
            return this.ExecuteReader("GetSearchResultModules", PortalID);
        }

        public virtual IDataReader GetSearchSettings(int ModuleId)
        {
            return this.ExecuteReader("GetSearchSettings", ModuleId);
        }

        public virtual int AddListEntry(string ListName, string Value, string Text, int ParentID, int Level,
                                        bool EnableSortOrder, int DefinitionID, string Description, int PortalID,
                                        bool SystemList,
                                        int CreatedByUserID)
        {
            try
            {
                return this.ExecuteScalar<int>(
                    "AddListEntry",
                    ListName,
                    Value,
                    Text,
                    ParentID,
                    Level,
                    EnableSortOrder,
                    DefinitionID,
                    Description,
                    PortalID,
                    SystemList,
                    CreatedByUserID);
            }
            catch (SqlException ex)
            {
                if (ex.Number == DuplicateKey)
                {
                    return Null.NullInteger;
                }

                throw;
            }
        }

        public virtual void DeleteList(string ListName, string ParentKey)
        {
            this.ExecuteNonQuery("DeleteList", ListName, ParentKey);
        }

        public virtual void DeleteListEntryByID(int EntryID, bool DeleteChild)
        {
            this.ExecuteNonQuery("DeleteListEntryByID", EntryID, DeleteChild);
        }

        public virtual void DeleteListEntryByListName(string ListName, string Value, bool DeleteChild)
        {
            this.ExecuteNonQuery("DeleteListEntryByListName", ListName, Value, DeleteChild);
        }

        public virtual IDataReader GetList(string ListName, string ParentKey, int PortalID)
        {
            return this.ExecuteReader("GetList", ListName, ParentKey, PortalID);
        }

        public virtual IDataReader GetListEntriesByListName(string ListName, string ParentKey, int PortalID)
        {
            return this.ExecuteReader("GetListEntries", ListName, ParentKey, this.GetNull(PortalID));
        }

        public virtual IDataReader GetListEntry(string ListName, string Value)
        {
            return this.ExecuteReader("GetListEntry", ListName, Value, -1);
        }

        public virtual IDataReader GetListEntry(int EntryID)
        {
            return this.ExecuteReader("GetListEntry", string.Empty, string.Empty, EntryID);
        }

        public virtual IDataReader GetLists(int PortalID)
        {
            return this.ExecuteReader("GetLists", PortalID);
        }

        public virtual void UpdateListEntry(int EntryID, string Value, string Text, string Description,
                                            int LastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateListEntry", EntryID, Value, Text, Description, LastModifiedByUserID);
        }

        public virtual void UpdateListSortOrder(int EntryID, bool MoveUp)
        {
            this.ExecuteNonQuery("UpdateListSortOrder", EntryID, MoveUp);
        }

        public virtual int AddPortalAlias(int PortalID, string HTTPAlias, string cultureCode, string skin, string browserType, bool isPrimary, int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddPortalAlias", PortalID, HTTPAlias, this.GetNull(cultureCode), this.GetNull(skin), this.GetNull(browserType), isPrimary, createdByUserID);
        }

        public virtual void DeletePortalAlias(int PortalAliasID)
        {
            this.ExecuteNonQuery("DeletePortalAlias", PortalAliasID);
        }

        public virtual IDataReader GetPortalAliases()
        {
            return this.ExecuteReader("GetPortalAliases");
        }

        public virtual IDataReader GetPortalByPortalAliasID(int PortalAliasId)
        {
            return this.ExecuteReader("GetPortalByPortalAliasID", PortalAliasId);
        }

        public virtual void UpdatePortalAlias(string PortalAlias, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdatePortalAliasOnInstall", PortalAlias, lastModifiedByUserID);
        }

        public virtual void UpdatePortalAliasInfo(int PortalAliasID, int PortalID, string HTTPAlias, string cultureCode, string skin, string browserType, bool isPrimary, int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdatePortalAlias", PortalAliasID, PortalID, HTTPAlias, this.GetNull(cultureCode), this.GetNull(skin), this.GetNull(browserType), isPrimary, lastModifiedByUserID);
        }

        public virtual int AddEventMessage(string eventName, int priority, string processorType, string processorCommand,
                                           string body, string sender, string subscriberId, string authorizedRoles,
                                           string exceptionMessage, DateTime sentDate, DateTime expirationDate,
                                           string attributes)
        {
            return this.ExecuteScalar<int>(
                "AddEventMessage",
                eventName,
                priority,
                processorType,
                processorCommand,
                body,
                sender,
                subscriberId,
                authorizedRoles,
                exceptionMessage,
                sentDate,
                expirationDate,
                attributes);
        }

        public virtual IDataReader GetEventMessages(string eventName)
        {
            return this.ExecuteReader("GetEventMessages", eventName);
        }

        public virtual IDataReader GetEventMessagesBySubscriber(string eventName, string subscriberId)
        {
            return this.ExecuteReader("GetEventMessagesBySubscriber", eventName, subscriberId);
        }

        public virtual void SetEventMessageComplete(int eventMessageId)
        {
            this.ExecuteNonQuery("SetEventMessageComplete", eventMessageId);
        }

        public virtual int AddAuthentication(int packageID, string authenticationType, bool isEnabled,
                                             string settingsControlSrc, string loginControlSrc, string logoffControlSrc,
                                             int CreatedByUserID)
        {
            return this.ExecuteScalar<int>(
                "AddAuthentication",
                packageID,
                authenticationType,
                isEnabled,
                settingsControlSrc,
                loginControlSrc,
                logoffControlSrc,
                CreatedByUserID);
        }

        public virtual int AddUserAuthentication(int userID, string authenticationType, string authenticationToken,
                                                 int CreatedByUserID)
        {
            return this.ExecuteScalar<int>("AddUserAuthentication", userID, authenticationType, authenticationToken,
                                      CreatedByUserID);
        }

        /// <summary>
        /// Get a User Authentication record from slq database. DNN-4016.
        /// </summary>
        /// <param name="userID"></param>
        /// <returns>UserAuthentication record.</returns>
        public virtual IDataReader GetUserAuthentication(int userID)
        {
            return this.ExecuteReader("GetUserAuthentication", userID);
        }

        public virtual void DeleteAuthentication(int authenticationID)
        {
            this.ExecuteNonQuery("DeleteAuthentication", authenticationID);
        }

        public virtual IDataReader GetAuthenticationService(int authenticationID)
        {
            return this.ExecuteReader("GetAuthenticationService", authenticationID);
        }

        public virtual IDataReader GetAuthenticationServiceByPackageID(int packageID)
        {
            return this.ExecuteReader("GetAuthenticationServiceByPackageID", packageID);
        }

        public virtual IDataReader GetAuthenticationServiceByType(string authenticationType)
        {
            return this.ExecuteReader("GetAuthenticationServiceByType", authenticationType);
        }

        public virtual IDataReader GetAuthenticationServices()
        {
            return this.ExecuteReader("GetAuthenticationServices");
        }

        public virtual IDataReader GetEnabledAuthenticationServices()
        {
            return this.ExecuteReader("GetEnabledAuthenticationServices");
        }

        public virtual void UpdateAuthentication(int authenticationID, int packageID, string authenticationType,
                                                 bool isEnabled, string settingsControlSrc, string loginControlSrc,
                                                 string logoffControlSrc, int LastModifiedByUserID)
        {
            this.ExecuteNonQuery(
                "UpdateAuthentication",
                authenticationID,
                packageID,
                authenticationType,
                isEnabled,
                settingsControlSrc,
                loginControlSrc,
                logoffControlSrc,
                LastModifiedByUserID);
        }

        public virtual int AddPackage(int portalID, string name, string friendlyName, string description, string type,
                                      string version, string license, string manifest, string owner,
                                      string organization, string url, string email, string releaseNotes,
                                      bool isSystemPackage, int createdByUserID, string folderName, string iconFile)
        {
            return this.ExecuteScalar<int>(
                "AddPackage",
                this.GetNull(portalID),
                name,
                friendlyName,
                description,
                type,
                version,
                license,
                manifest,
                owner,
                organization,
                url,
                email,
                releaseNotes,
                isSystemPackage,
                createdByUserID,
                folderName,
                iconFile);
        }

        public virtual void DeletePackage(int packageID)
        {
            this.ExecuteNonQuery("DeletePackage", packageID);
        }

        public virtual IDataReader GetModulePackagesInUse(int portalID, bool forHost)
        {
            return this.ExecuteReader("GetModulePackagesInUse", portalID, forHost);
        }

        public virtual IDataReader GetPackageDependencies()
        {
            return this.ExecuteReader("GetPackageDependencies");
        }

        public virtual IDataReader GetPackages(int portalID)
        {
            return this.ExecuteReader("GetPackages", this.GetNull(portalID));
        }

        public virtual IDataReader GetPackageTypes()
        {
            return this.ExecuteReader("GetPackageTypes");
        }

        public virtual int RegisterAssembly(int packageID, string assemblyName, string version)
        {
            return this.ExecuteScalar<int>("RegisterAssembly", this.GetNull(packageID), assemblyName, version);
        }

        public virtual int SavePackageDependency(int packageDependencyId, int packageId, string packageName, string version)
        {
            return this.ExecuteScalar<int>("SavePackageDependency", packageDependencyId, packageId, packageName, version);
        }

        public virtual bool UnRegisterAssembly(int packageID, string assemblyName)
        {
            return this.ExecuteScalar<int>("UnRegisterAssembly", packageID, assemblyName) == 1;
        }

        public virtual void UpdatePackage(int packageID, int portalID, string friendlyName, string description,
                                          string type, string version, string license, string manifest, string owner,
                                          string organization, string url, string email, string releaseNotes,
                                          bool isSystemPackage, int lastModifiedByUserID, string folderName,
                                          string iconFile)
        {
            this.ExecuteNonQuery(
                "UpdatePackage",
                packageID,
                this.GetNull(portalID),
                friendlyName,
                description,
                type,
                version,
                license,
                manifest,
                owner,
                organization,
                url,
                email,
                releaseNotes,
                isSystemPackage,
                lastModifiedByUserID,
                folderName,
                iconFile);
        }

        public virtual void SetCorePackageVersions()
        {
            this.ExecuteNonQuery("SetCorePackageVersions");
        }

        public virtual int AddLanguage(string cultureCode, string cultureName, string fallbackCulture,
                                       int CreatedByUserID)
        {
            return this.ExecuteScalar<int>("AddLanguage", cultureCode, cultureName, fallbackCulture, CreatedByUserID);
        }

        public virtual int AddLanguagePack(int packageID, int languageID, int dependentPackageID, int CreatedByUserID)
        {
            return this.ExecuteScalar<int>("AddLanguagePack", packageID, languageID, dependentPackageID, CreatedByUserID);
        }

        public virtual int AddPortalLanguage(int portalID, int languageID, bool IsPublished, int CreatedByUserID)
        {
            return this.ExecuteScalar<int>("AddPortalLanguage", portalID, languageID, IsPublished, CreatedByUserID);
        }

        public virtual void DeleteLanguage(int languageID)
        {
            this.ExecuteNonQuery("DeleteLanguage", languageID);
        }

        public virtual void DeleteLanguagePack(int languagePackID)
        {
            this.ExecuteNonQuery("DeleteLanguagePack", languagePackID);
        }

        public virtual void DeletePortalLanguages(int portalID, int languageID)
        {
            this.ExecuteNonQuery("DeletePortalLanguages", this.GetNull(portalID), this.GetNull(languageID));
        }

        public virtual void EnsureLocalizationExists(int portalID, string CultureCode)
        {
            this.ExecuteNonQuery("EnsureLocalizationExists", portalID, CultureCode);
        }

        public virtual void RemovePortalLocalization(int portalID, string CultureCode)
        {
            this.ExecuteNonQuery("RemovePortalLocalization", portalID, CultureCode);
        }

        public virtual IDataReader GetPortalLocalizations(int portalID)
        {
            return this.ExecuteReader("GetPortalLocalizations", portalID);
        }

        public virtual IDataReader GetLanguages()
        {
            return this.ExecuteReader("GetLanguages");
        }

        public virtual IDataReader GetLanguagePackByPackage(int packageID)
        {
            return this.ExecuteReader("GetLanguagePackByPackage", packageID);
        }

        public virtual IDataReader GetLanguagesByPortal(int portalID)
        {
            return this.ExecuteReader("GetLanguagesByPortal", portalID);
        }

        public virtual string GetPortalDefaultLanguage(int portalID)
        {
            return this.ExecuteScalar<string>("GetPortalDefaultLanguage", portalID);
        }

        public virtual void UpdateLanguage(int languageID, string cultureCode, string cultureName,
                                           string fallbackCulture, int LastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateLanguage", languageID, cultureCode, cultureName, fallbackCulture,
                            LastModifiedByUserID);
        }

        public virtual int UpdateLanguagePack(int languagePackID, int packageID, int languageID, int dependentPackageID,
                                              int LastModifiedByUserID)
        {
            return this.ExecuteScalar<int>("UpdateLanguagePack", languagePackID, packageID, languageID, dependentPackageID,
                                      LastModifiedByUserID);
        }

        public virtual void UpdatePortalDefaultLanguage(int portalID, string CultureCode)
        {
            this.ExecuteNonQuery("UpdatePortalDefaultLanguage", portalID, CultureCode);
        }

        public virtual void UpdatePortalLanguage(int portalID, int languageID, bool IsPublished, int UpdatedByUserID)
        {
            this.ExecuteNonQuery("UpdatePortalLanguage", portalID, languageID, IsPublished, UpdatedByUserID);
        }

        public virtual void AddDefaultFolderTypes(int portalID)
        {
            this.ExecuteNonQuery("AddDefaultFolderTypes", portalID);
        }

        public virtual int AddFolderMapping(int portalID, string mappingName, string folderProviderType,
                                            int createdByUserID)
        {
            return this.ExecuteScalar<int>("AddFolderMapping", this.GetNull(portalID), mappingName, folderProviderType,
                                      createdByUserID);
        }

        public virtual void AddFolderMappingSetting(int folderMappingID, string settingName, string settingValue,
                                                    int createdByUserID)
        {
            this.ExecuteNonQuery("AddFolderMappingsSetting", folderMappingID, settingName, settingValue, createdByUserID);
        }

        public virtual void DeleteFolderMapping(int folderMappingID)
        {
            this.ExecuteNonQuery("DeleteFolderMapping", folderMappingID);
        }

        public virtual IDataReader GetFolderMapping(int folderMappingID)
        {
            return this.ExecuteReader("GetFolderMapping", folderMappingID);
        }

        public virtual IDataReader GetFolderMappingByMappingName(int portalID, string mappingName)
        {
            return this.ExecuteReader("GetFolderMappingByMappingName", portalID, mappingName);
        }

        public virtual IDataReader GetFolderMappings(int portalID)
        {
            return this.ExecuteReader("GetFolderMappings", this.GetNull(portalID));
        }

        public virtual IDataReader GetFolderMappingSetting(int folderMappingID, string settingName)
        {
            return this.ExecuteReader("GetFolderMappingsSetting", folderMappingID, settingName);
        }

        public virtual IDataReader GetFolderMappingSettings(int folderMappingID)
        {
            return this.ExecuteReader("GetFolderMappingsSettings", folderMappingID);
        }

        public virtual void UpdateFolderMapping(int folderMappingID, string mappingName, int priority,
                                                int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateFolderMapping", folderMappingID, mappingName, priority, lastModifiedByUserID);
        }

        public virtual void UpdateFolderMappingSetting(int folderMappingID, string settingName, string settingValue,
                                                       int lastModifiedByUserID)
        {
            this.ExecuteNonQuery("UpdateFolderMappingsSetting", folderMappingID, settingName, settingValue,
                            lastModifiedByUserID);
        }

        [Obsolete("Deprecated in Platform 9.2.0, please use the overload that takes passwordsRetained and daysRetained. Scheduled removal in v11.0.0.")]
        public virtual IDataReader GetPasswordHistory(int userId)
        {
            return this.GetPasswordHistory(userId, int.MaxValue, int.MaxValue);
        }

        public virtual IDataReader GetPasswordHistory(int userId, int passwordsRetained, int daysRetained)
        {
            return this.ExecuteReader("GetPasswordHistory", this.GetNull(userId), passwordsRetained, daysRetained);
        }

        [Obsolete("Deprecated in Platform 9.2.0, please use the overload that takes daysRetained. Scheduled removal in v11.0.0.")]
        public virtual void AddPasswordHistory(int userId, string password, string passwordHistory, int retained)
        {
            this.AddPasswordHistory(userId, password, passwordHistory, retained, int.MaxValue);
        }

        public virtual void AddPasswordHistory(int userId, string password, string passwordHistory, int passwordsRetained, int daysRetained)
        {
            this.ExecuteNonQuery("AddPasswordHistory", this.GetNull(userId), password, passwordHistory, passwordsRetained, daysRetained, this.GetNull(userId));
        }

        public virtual DateTime GetDatabaseTimeUtc()
        {
            return this.ExecuteScalar<DateTime>("GetDatabaseTimeUtc");
        }

        public virtual DateTime GetDatabaseTime()
        {
            return this.ExecuteScalar<DateTime>("GetDatabaseTime");
        }

        public virtual DateTimeOffset GetDatabaseTimeOffset()
        {
            using (var reader = (SqlDataReader)this.ExecuteSQL("SELECT SYSDATETIMEOFFSET()"))
            {
                reader.Read();
                return reader.GetDateTimeOffset(0);
            }
        }

        public virtual void DeletePreviewProfile(int id)
        {
            this.ExecuteNonQuery("Mobile_DeletePreviewProfile", id);
        }

        public virtual void DeleteRedirection(int id)
        {
            this.ExecuteNonQuery("Mobile_DeleteRedirection", id);
        }

        public virtual void DeleteRedirectionRule(int id)
        {
            this.ExecuteNonQuery("Mobile_DeleteRedirectionRule", id);
        }

        public virtual IDataReader GetAllRedirections()
        {
            return this.ExecuteReader("Mobile_GetAllRedirections");
        }

        public virtual IDataReader GetPreviewProfiles(int portalId)
        {
            return this.ExecuteReader("Mobile_GetPreviewProfiles", portalId);
        }

        public virtual IDataReader GetRedirectionRules(int redirectionId)
        {
            return this.ExecuteReader("Mobile_GetRedirectionRules", redirectionId);
        }

        public virtual IDataReader GetRedirections(int portalId)
        {
            return this.ExecuteReader("Mobile_GetRedirections", portalId);
        }

        public virtual int SavePreviewProfile(int id, int portalId, string name, int width, int height, string userAgent,
                                              int sortOrder, int userId)
        {
            return this.ExecuteScalar<int>("Mobile_SavePreviewProfile", id, portalId, name, width, height, userAgent,
                                      sortOrder, userId);
        }

        public virtual int SaveRedirection(int id, int portalId, string name, int type, int sortOrder, int sourceTabId,
                                           bool includeChildTabs, int targetType, object targetValue, bool enabled,
                                           int userId)
        {
            return this.ExecuteScalar<int>("Mobile_SaveRedirection", id, portalId, name, type, sortOrder, sourceTabId,
                                      includeChildTabs, targetType, targetValue, enabled, userId);
        }

        public virtual void SaveRedirectionRule(int id, int redirectionId, string capbility, string expression)
        {
            this.ExecuteNonQuery("Mobile_SaveRedirectionRule", id, redirectionId, capbility, expression);
        }

        public virtual void AddLog(string logGUID, string logTypeKey, int logUserID, string logUserName, int logPortalID,
                                   string logPortalName, DateTime logCreateDate, string logServerName,
                                   string logProperties, int logConfigID, ExceptionInfo exception, bool notificationActive)
        {
            if (exception != null)
            {
                if (!string.IsNullOrEmpty(exception.ExceptionHash))
                {
                    this.ExecuteNonQuery(
                        "AddException",
                        exception.ExceptionHash,
                        exception.Message,
                        exception.StackTrace,
                        exception.InnerMessage,
                        exception.InnerStackTrace,
                        exception.Source);
                }

                // DNN-6218 + DNN-6239 + DNN-6242: Due to change in the AddEventLog stored
                // procedure in 7.4.0, we need to try a fallback especially during upgrading
                int logEventID;
                try
                {
                    logEventID = this.ExecuteScalar<int>(
                        "AddEventLog",
                        logGUID,
                        logTypeKey,
                        this.GetNull(logUserID),
                        this.GetNull(logUserName),
                        this.GetNull(logPortalID),
                        this.GetNull(logPortalName),
                        logCreateDate,
                        logServerName,
                        logProperties,
                        logConfigID,
                        this.GetNull(exception.ExceptionHash),
                        notificationActive);
                }
                catch (SqlException)
                {
                    var s = this.ExecuteScalar<string>(
                        "AddEventLog",
                        logGUID,
                        logTypeKey,
                        this.GetNull(logUserID),
                        this.GetNull(logUserName),
                        this.GetNull(logPortalID),
                        this.GetNull(logPortalName),
                        logCreateDate,
                        logServerName,
                        logProperties,
                        logConfigID);

                    // old SPROC wasn't returning anything; trying a workaround
                    if (!int.TryParse(s ?? "-1", out logEventID))
                    {
                        logEventID = Null.NullInteger;
                    }
                }

                if (!string.IsNullOrEmpty(exception.AssemblyVersion) && exception.AssemblyVersion != "-1")
                {
                    this.ExecuteNonQuery(
                        "AddExceptionEvent",
                        logEventID,
                        exception.AssemblyVersion,
                        exception.PortalId,
                        exception.UserId,
                        exception.TabId,
                        exception.RawUrl,
                        exception.Referrer,
                        exception.UserAgent);
                }
            }
            else
            {
                this.ExecuteScalar<int>(
                    "AddEventLog",
                    logGUID,
                    logTypeKey,
                    this.GetNull(logUserID),
                    this.GetNull(logUserName),
                    this.GetNull(logPortalID),
                    this.GetNull(logPortalName),
                    logCreateDate,
                    logServerName,
                    logProperties,
                    logConfigID,
                    DBNull.Value,
                    notificationActive);
            }
        }

        public virtual void AddLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription,
                                       string logTypeCSSClass, string logTypeOwner)
        {
            this.ExecuteNonQuery("AddEventLogType", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeOwner,
                            logTypeCSSClass);
        }

        public virtual void AddLogTypeConfigInfo(bool loggingIsActive, string logTypeKey, string logTypePortalID,
                                                 int keepMostRecent, bool emailNotificationIsActive, int threshold,
                                                 int notificationThresholdTime, int notificationThresholdTimeType,
                                                 string mailFromAddress, string mailToAddress)
        {
            int portalID;
            if (logTypeKey == "*")
            {
                logTypeKey = string.Empty;
            }

            if (logTypePortalID == "*")
            {
                portalID = -1;
            }
            else
            {
                portalID = Convert.ToInt32(logTypePortalID);
            }

            this.ExecuteNonQuery(
                "AddEventLogConfig",
                this.GetNull(logTypeKey),
                this.GetNull(portalID),
                loggingIsActive,
                keepMostRecent,
                emailNotificationIsActive,
                this.GetNull(threshold),
                this.GetNull(notificationThresholdTime),
                this.GetNull(notificationThresholdTimeType),
                mailFromAddress,
                mailToAddress);
        }

        public virtual void ClearLog()
        {
            this.ExecuteNonQuery("DeleteEventLog", DBNull.Value);
        }

        public virtual void DeleteLog(string logGUID)
        {
            this.ExecuteNonQuery("DeleteEventLog", logGUID);
        }

        public virtual void DeleteLogType(string logTypeKey)
        {
            this.ExecuteNonQuery("DeleteEventLogType", logTypeKey);
        }

        public virtual void DeleteLogTypeConfigInfo(string id)
        {
            this.ExecuteNonQuery("DeleteEventLogConfig", id);
        }

        public virtual IDataReader GetEventLogPendingNotif(int logConfigID)
        {
            return this.ExecuteReader("GetEventLogPendingNotif", logConfigID);
        }

        public virtual IDataReader GetEventLogPendingNotifConfig()
        {
            return this.ExecuteReader("GetEventLogPendingNotifConfig");
        }

        public virtual IDataReader GetLogs(int portalID, string logType, int pageSize, int pageIndex)
        {
            return this.ExecuteReader("GetEventLog", this.GetNull(portalID), this.GetNull(logType), pageSize, pageIndex);
        }

        public virtual IDataReader GetLogTypeConfigInfo()
        {
            return this.ExecuteReader("GetEventLogConfig", DBNull.Value);
        }

        public virtual IDataReader GetLogTypeConfigInfoByID(int id)
        {
            return this.ExecuteReader("GetEventLogConfig", id);
        }

        public virtual IDataReader GetLogTypeInfo()
        {
            return this.ExecuteReader("GetEventLogType");
        }

        public virtual IDataReader GetSingleLog(string logGUID)
        {
            return this.ExecuteReader("GetEventLogByLogGUID", logGUID);
        }

        public virtual void PurgeLog()
        {
            // Because event log is run on application end, app may not be fully installed, so check for the sproc first
            string sql = "IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'" + this.DatabaseOwner + this.ObjectQualifier +
                         "PurgeEventLog') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) " + " BEGIN " +
                         "    EXEC " + this.DatabaseOwner + this.ObjectQualifier + "PurgeEventLog" + " END ";
            PetaPocoHelper.ExecuteNonQuery(this.ConnectionString, CommandType.Text, sql);
        }

        public virtual void UpdateEventLogPendingNotif(int logConfigID)
        {
            this.ExecuteNonQuery("UpdateEventLogPendingNotif", logConfigID);
        }

        public virtual void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription,
                                          string logTypeCSSClass, string logTypeOwner)
        {
            this.ExecuteNonQuery("UpdateEventLogType", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeOwner,
                            logTypeCSSClass);
        }

        public virtual void UpdateLogTypeConfigInfo(string id, bool loggingIsActive, string logTypeKey,
                                                    string logTypePortalID, int keepMostRecent,
                                                    bool emailNotificationIsActive,
                                                    int threshold, int notificationThresholdTime,
                                                    int notificationThresholdTimeType, string mailFromAddress,
                                                    string mailToAddress)
        {
            int portalID;
            if (logTypeKey == "*")
            {
                logTypeKey = string.Empty;
            }

            if (logTypePortalID == "*")
            {
                portalID = -1;
            }
            else
            {
                portalID = Convert.ToInt32(logTypePortalID);
            }

            this.ExecuteNonQuery(
                "UpdateEventLogConfig",
                id,
                this.GetNull(logTypeKey),
                this.GetNull(portalID),
                loggingIsActive,
                keepMostRecent,
                emailNotificationIsActive,
                this.GetNull(threshold),
                this.GetNull(notificationThresholdTime),
                this.GetNull(notificationThresholdTimeType),
                mailFromAddress,
                mailToAddress);
        }

        public virtual int AddSchedule(string TypeFullName, int TimeLapse, string TimeLapseMeasurement,
                                       int RetryTimeLapse, string RetryTimeLapseMeasurement, int RetainHistoryNum,
                                       string AttachToEvent, bool CatchUpEnabled, bool Enabled,
                                       string ObjectDependencies, string Servers, int CreatedByUserID,
                                       string FriendlyName, DateTime ScheduleStartDate)
        {
            return this.ExecuteScalar<int>(
                "AddSchedule",
                TypeFullName,
                TimeLapse,
                TimeLapseMeasurement,
                RetryTimeLapse,
                RetryTimeLapseMeasurement,
                RetainHistoryNum,
                AttachToEvent,
                CatchUpEnabled,
                Enabled,
                ObjectDependencies,
                this.GetNull(Servers),
                CreatedByUserID,
                FriendlyName,
                this.GetNull(ScheduleStartDate));
        }

        public virtual int AddScheduleHistory(int ScheduleID, DateTime StartDate, string Server)
        {
            return this.ExecuteScalar<int>("AddScheduleHistory", ScheduleID, FixDate(StartDate), Server);
        }

        public virtual void AddScheduleItemSetting(int ScheduleID, string Name, string Value)
        {
            this.ExecuteNonQuery("AddScheduleItemSetting", ScheduleID, Name, Value);
        }

        public virtual void DeleteSchedule(int ScheduleID)
        {
            this.ExecuteNonQuery("DeleteSchedule", ScheduleID);
        }

        public virtual IDataReader GetNextScheduledTask(string Server)
        {
            return this.ExecuteReader("GetScheduleNextTask", this.GetNull(Server));
        }

        public virtual IDataReader GetSchedule()
        {
            return this.ExecuteReader("GetSchedule", DBNull.Value);
        }

        public virtual IDataReader GetSchedule(string Server)
        {
            return this.ExecuteReader("GetSchedule", this.GetNull(Server));
        }

        public virtual IDataReader GetSchedule(int ScheduleID)
        {
            return this.ExecuteReader("GetScheduleByScheduleID", ScheduleID);
        }

        public virtual IDataReader GetSchedule(string TypeFullName, string Server)
        {
            return this.ExecuteReader("GetScheduleByTypeFullName", TypeFullName, this.GetNull(Server));
        }

        public virtual IDataReader GetScheduleByEvent(string EventName, string Server)
        {
            return this.ExecuteReader("GetScheduleByEvent", EventName, this.GetNull(Server));
        }

        public virtual IDataReader GetScheduleHistory(int ScheduleID)
        {
            return this.ExecuteReader("GetScheduleHistory", ScheduleID);
        }

        public virtual IDataReader GetScheduleItemSettings(int ScheduleID)
        {
            return this.ExecuteReader("GetScheduleItemSettings", ScheduleID);
        }

        public virtual void PurgeScheduleHistory()
        {
            this.ExecuteNonQuery(90, "PurgeScheduleHistory");
        }

        public virtual void UpdateSchedule(int ScheduleID, string TypeFullName, int TimeLapse,
                                           string TimeLapseMeasurement, int RetryTimeLapse,
                                           string RetryTimeLapseMeasurement, int RetainHistoryNum,
                                           string AttachToEvent, bool CatchUpEnabled, bool Enabled,
                                           string ObjectDependencies, string Servers, int LastModifiedByUserID,
                                           string FriendlyName, DateTime ScheduleStartDate)
        {
            this.ExecuteNonQuery(
                "UpdateSchedule",
                ScheduleID,
                TypeFullName,
                TimeLapse,
                TimeLapseMeasurement,
                RetryTimeLapse,
                RetryTimeLapseMeasurement,
                RetainHistoryNum,
                AttachToEvent,
                CatchUpEnabled,
                Enabled,
                ObjectDependencies,
                this.GetNull(Servers),
                LastModifiedByUserID,
                FriendlyName,
                this.GetNull(ScheduleStartDate));
        }

        public virtual void UpdateScheduleHistory(int ScheduleHistoryID, DateTime EndDate, bool Succeeded, string LogNotes, DateTime NextStart)
        {
            this.ExecuteNonQuery("UpdateScheduleHistory", ScheduleHistoryID, FixDate(EndDate), this.GetNull(Succeeded), LogNotes, FixDate(NextStart));
        }

        public virtual int AddExtensionUrlProvider(
            int providerId,
            int desktopModuleId,
            string providerName,
            string providerType,
            string settingsControlSrc,
            bool isActive,
            bool rewriteAllUrls,
            bool redirectAllUrls,
            bool replaceAllUrls)
        {
            return this.ExecuteScalar<int>(
                "AddExtensionUrlProvider",
                providerId,
                desktopModuleId,
                providerName,
                providerType,
                settingsControlSrc,
                isActive,
                rewriteAllUrls,
                redirectAllUrls,
                replaceAllUrls);
        }

        public virtual void DeleteExtensionUrlProvider(int providerId)
        {
            this.ExecuteNonQuery("DeleteExtensionUrlProvider", providerId);
        }

        public virtual IDataReader GetExtensionUrlProviders(int portalId)
        {
            return this.ExecuteReader("GetExtensionUrlProviders", this.GetNull(portalId));
        }

        public virtual void SaveExtensionUrlProviderSetting(int providerId, int portalId, string settingName, string settingValue)
        {
            this.ExecuteNonQuery("SaveExtensionUrlProviderSetting", providerId, portalId, settingName, settingValue);
        }

        public virtual void UpdateExtensionUrlProvider(int providerId, bool isActive)
        {
            this.ExecuteNonQuery("UpdateExtensionUrlProvider", providerId, isActive);
        }

        public virtual IDataReader GetIPFilters()
        {
            return this.ExecuteReader("GetIPFilters");
        }

        public virtual int AddIPFilter(string ipAddress, string subnetMask, int ruleType, int createdByUserId)
        {
            return this.ExecuteScalar<int>("AddIPFilter", ipAddress, subnetMask, ruleType, createdByUserId);
        }

        public virtual void DeleteIPFilter(int ipFilterid)
        {
            this.ExecuteNonQuery("DeleteIPFilter", ipFilterid);
        }

        public virtual void UpdateIPFilter(int ipFilterid, string ipAddress, string subnetMask, int ruleType, int lastModifiedByUserId)
        {
            this.ExecuteNonQuery("UpdateIPFilter", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId);
        }

        public virtual IDataReader GetIPFilter(int ipf)
        {
            return this.ExecuteReader("GetIPFilter", ipf);
        }

        public virtual IDataReader GetFileVersions(int fileId)
        {
            return this.ExecuteReader("GetFileVersions", fileId);
        }

        public virtual IDataReader GetFileVersionsInFolder(int folderId)
        {
            return this.ExecuteReader("GetFileVersionsInFolder", folderId);
        }

        public virtual int AddFileVersion(int fileId, Guid uniqueId, Guid versionGuid, string fileName, string extension,
                            long size, int width, int height, string contentType, string folder, int folderId,
                            int userId, string hash, DateTime lastModificationTime,
                            string title, bool enablePublishPeriod, DateTime startDate, DateTime endDate, int contentItemID, bool published, byte[] content = null)
        {
            if (content == null)
            {
                return this.ExecuteScalar<int>(
                    "AddFileVersion",
                    fileId,
                    uniqueId,
                    versionGuid,
                    fileName,
                    extension,
                    size,
                    this.GetNull(width),
                    this.GetNull(height),
                    contentType,
                    folder,
                    folderId,
                    userId,
                    hash,
                    lastModificationTime,
                    title,
                    enablePublishPeriod,
                    startDate,
                    this.GetNull(endDate),
                    this.GetNull(contentItemID),
                    published);
            }

            return this.ExecuteScalar<int>(
                "AddFileVersion",
                fileId,
                uniqueId,
                versionGuid,
                fileName,
                extension,
                size,
                this.GetNull(width),
                this.GetNull(height),
                contentType,
                folder,
                folderId,
                userId,
                hash,
                lastModificationTime,
                title,
                enablePublishPeriod,
                startDate,
                this.GetNull(endDate),
                this.GetNull(contentItemID),
                published,
                this.GetNull(content));
        }

        public virtual int DeleteFileVersion(int fileId, int version)
        {
            return this.ExecuteScalar<int>("DeleteFileVersion", fileId, version);
        }

        public virtual void ResetFilePublishedVersion(int fileId)
        {
            this.ExecuteNonQuery("ResetFilePublishedVersion", fileId);
        }

        public virtual IDataReader GetFileVersion(int fileId, int version)
        {
            return this.ExecuteReader("GetFileVersion", fileId, version);
        }

        public void SetPublishedVersion(int fileId, int newPublishedVersion)
        {
            this.ExecuteNonQuery("SetPublishedVersion", fileId, newPublishedVersion);
        }

        public virtual int GetContentWorkflowUsageCount(int workflowId)
        {
            return this.ExecuteScalar<int>("GetContentWorkflowUsageCount", workflowId);
        }

        public virtual IDataReader GetContentWorkflowUsage(int workflowId, int pageIndex, int pageSize)
        {
            return this.ExecuteReader("GetContentWorkflowUsage", workflowId, pageIndex, pageSize);
        }

        public virtual int GetContentWorkflowStateUsageCount(int stateId)
        {
            return this.ExecuteScalar<int>("GetContentWorkflowStateUsageCount", stateId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual int AddContentWorkflow(int portalId, string workflowName, string description, bool isDeleted, bool startAfterCreating, bool startAfterEditing, bool dispositionEnabled)
        {
            return this.ExecuteScalar<int>(
                "AddContentWorkflow",
                this.GetNull(portalId),
                workflowName,
                description,
                isDeleted,
                startAfterCreating,
                startAfterEditing,
                dispositionEnabled);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual IDataReader GetContentWorkflow(int workflowId)
        {
            return this.ExecuteReader("GetContentWorkflow", workflowId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual IDataReader GetContentWorkflows(int portalId)
        {
            return this.ExecuteReader("GetContentWorkflows", portalId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual void UpdateContentWorkflow(int workflowId, string workflowName, string description, bool isDeleted, bool startAfterCreating, bool startAfterEditing, bool dispositionEnabled)
        {
            this.ExecuteNonQuery(
                "UpdateContentWorkflow",
                workflowId,
                workflowName,
                description,
                isDeleted,
                startAfterCreating,
                startAfterEditing,
                dispositionEnabled);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual int AddContentWorkflowState(int workflowId, string stateName, int order,
            bool isActive, bool sendEmail, bool sendMessage, bool isDisposalState,
            string onCompleteMessageSubject, string onCompleteMessageBody,
            string onDiscardMessageSubject, string onDiscardMessageBody)
        {
            return this.ExecuteScalar<int>(
                "AddContentWorkflowState",
                workflowId,
                stateName,
                order,
                isActive,
                sendEmail,
                sendMessage,
                isDisposalState,
                onCompleteMessageSubject,
                onCompleteMessageBody,
                onDiscardMessageSubject,
                onDiscardMessageBody);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual void DeleteContentWorkflowState(int stateId)
        {
            this.ExecuteNonQuery("DeleteContentWorkflowState", stateId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual void UpdateContentWorkflowState(int stateId, string stateName, int order,
            bool isActive, bool sendEmail, bool sendMessage, bool isDisposalState,
            string onCompleteMessageSubject, string onCompleteMessageBody,
            string onDiscardMessageSubject, string onDiscardMessageBody)
        {
            this.ExecuteNonQuery(
                "UpdateContentWorkflowState",
                stateId,
                stateName,
                order,
                isActive,
                sendEmail,
                sendMessage,
                isDisposalState,
                onCompleteMessageSubject,
                onCompleteMessageBody,
                onDiscardMessageSubject,
                onDiscardMessageBody);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual IDataReader GetContentWorkflowState(int stateId)
        {
            return this.ExecuteReader("GetContentWorkflowState", stateId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual IDataReader GetContentWorkflowStates(int workflowId)
        {
            return this.ExecuteReader("GetContentWorkflowStates", workflowId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger.AddWorkflowLog. Scheduled removal in v10.0.0.")]
        public virtual int AddContentWorkflowLog(string action, string comment, int user, int workflowId, int contentItemId)
        {
            return this.ExecuteScalar<int>(
                "AddContentWorkflowLog",
                action,
                comment,
                user,
                workflowId,
                contentItemId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger.GetWorkflowLogs. Scheduled removal in v10.0.0.")]
        public virtual IDataReader GetContentWorkflowLogs(int contentItemId, int workflowId)
        {
            return this.ExecuteReader("GetContentWorkflowLogs", contentItemId, workflowId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual int DeleteContentWorkflowLogs(int contentItemId, int workflowId)
        {
            return this.ExecuteScalar<int>("DeleteContentWorkflowLogs", contentItemId, workflowId);
        }

        public virtual int AddContentWorkflowStatePermission(int stateId, int permissionId, int roleId, bool allowAccess, int userId, int createdByUserId)
        {
            return this.ExecuteScalar<int>(
                "AddContentWorkflowStatePermission",
                stateId,
                permissionId,
                this.GetNull(roleId),
                allowAccess,
                this.GetNull(userId),
                this.GetNull(createdByUserId));
        }

        public virtual void UpdateContentWorkflowStatePermission(int workflowStatePermissionId, int stateId, int permissionId, int roleId, bool allowAccess, int userId, int lastModifiedByUserId)
        {
            this.ExecuteNonQuery(
                "UpdateContentWorkflowStatePermission",
                workflowStatePermissionId,
                stateId,
                permissionId,
                this.GetNull(roleId),
                allowAccess,
                this.GetNull(userId),
                this.GetNull(lastModifiedByUserId));
        }

        public virtual void DeleteContentWorkflowStatePermission(int workflowStatePermissionId)
        {
            this.ExecuteNonQuery("DeleteContentWorkflowStatePermission", workflowStatePermissionId);
        }

        public virtual IDataReader GetContentWorkflowStatePermission(int workflowStatePermissionId)
        {
            return this.ExecuteReader("GetContentWorkflowStatePermission", workflowStatePermissionId);
        }

        public virtual IDataReader GetContentWorkflowStatePermissions()
        {
            return this.ExecuteReader("GetContentWorkflowStatePermissions");
        }

        public virtual IDataReader GetContentWorkflowStatePermissionsByStateID(int stateId)
        {
            return this.ExecuteReader("GetContentWorkflowStatePermissionsByStateID", stateId);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual IDataReader GetContentWorkflowSource(int workflowId, string sourceName)
        {
            return this.ExecuteReader("GetContentWorkflowSource", workflowId, sourceName);
        }

        [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
        public virtual int AddContentWorkflowSource(int workflowId, string sourceName, string sourceType)
        {
            return this.ExecuteScalar<int>("AddContentWorkflowSource", workflowId, sourceName, sourceType);
        }

        public virtual IDataReader GetAllSearchTypes()
        {
            return this.ExecuteReader("SearchTypes_GetAll");
        }

        public virtual IDataReader GetAllSynonymsGroups(int portalId, string cultureCode)
        {
            return this.ExecuteReader("GetAllSynonymsGroups", portalId, cultureCode);
        }

        public virtual int AddSynonymsGroup(string synonymsTags, int createdByUserId, int portalId, string cultureCode)
        {
            return this.ExecuteScalar<int>(
                "AddSynonymsGroup",
                synonymsTags,
                createdByUserId,
                portalId,
                cultureCode);
        }

        public virtual void UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int lastModifiedUserId)
        {
            this.ExecuteNonQuery("UpdateSynonymsGroup", synonymsGroupId, synonymsTags, lastModifiedUserId);
        }

        public virtual void DeleteSynonymsGroup(int synonymsGroupId)
        {
            this.ExecuteNonQuery("DeleteSynonymsGroup", synonymsGroupId);
        }

        public virtual IDataReader GetSearchStopWords(int portalId, string cultureCode)
        {
            return this.ExecuteReader("GetSearchStopWords", portalId, cultureCode);
        }

        public virtual int AddSearchStopWords(string stopWords, int createdByUserId, int portalId, string cultureCode)
        {
            return this.ExecuteScalar<int>("InsertSearchStopWords", stopWords, createdByUserId, portalId, cultureCode);
        }

        public virtual void UpdateSearchStopWords(int stopWordsId, string stopWords, int lastModifiedUserId)
        {
            this.ExecuteNonQuery("UpdateSearchStopWords", stopWordsId, stopWords, lastModifiedUserId);
        }

        public virtual void DeleteSearchStopWords(int stopWordsId)
        {
            this.ExecuteNonQuery("DeleteSearchStopWords", stopWordsId);
        }

        public void AddSearchDeletedItems(SearchDocumentToDelete deletedIDocument)
        {
            try
            {
                this.ExecuteNonQuery("SearchDeletedItems_Add", deletedIDocument.ToJsonString());
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
            }
        }

        public void DeleteProcessedSearchDeletedItems(DateTime cutoffTime)
        {
            try
            {
                this.ExecuteNonQuery("SearchDeletedItems_DeleteProcessed", cutoffTime);
            }
            catch (SqlException ex)
            {
                Logger.Error(ex);
            }
        }

        public IDataReader GetSearchDeletedItems(DateTime cutoffTime)
        {
            return this.ExecuteReader("SearchDeletedItems_Select", cutoffTime);
        }

        public virtual IDataReader GetAvailableUsersForIndex(int portalId, DateTime startDate, int startUserId, int numberOfUsers)
        {
            return this.ExecuteReader(90, "GetAvailableUsersForIndex", portalId, startDate, startUserId, numberOfUsers);
        }

        public virtual void AddOutputCacheItem(int itemId, string cacheKey, string output, DateTime expiration)
        {
            Instance().ExecuteNonQuery("OutputCacheAddItem", itemId, cacheKey, output, expiration);
        }

        public virtual IDataReader GetOutputCacheItem(string cacheKey)
        {
            return Instance().ExecuteReader("OutputCacheGetItem", cacheKey);
        }

        public virtual int GetOutputCacheItemCount(int itemId)
        {
            return Instance().ExecuteScalar<int>("OutputCacheGetItemCount", itemId);
        }

        public virtual IDataReader GetOutputCacheKeys()
        {
            return Instance().ExecuteReader("OutputCacheGetKeys", DBNull.Value);
        }

        public virtual IDataReader GetOutputCacheKeys(int itemId)
        {
            return Instance().ExecuteReader("OutputCacheGetKeys", itemId);
        }

        public virtual void PurgeExpiredOutputCacheItems()
        {
            Instance().ExecuteNonQuery("OutputCachePurgeExpiredItems", DateTime.UtcNow);
        }

        public virtual void PurgeOutputCache()
        {
            Instance().ExecuteNonQuery("OutputCachePurgeCache");
        }

        public virtual void RemoveOutputCacheItem(int itemId)
        {
            Instance().ExecuteNonQuery("OutputCacheRemoveItem", itemId);
        }

        public virtual Guid AddRedirectMessage(int userId, int tabId, string text)
        {
            return this.ExecuteScalar<Guid>("AddRedirectMessage", userId, tabId, text);
        }

        public virtual string GetRedirectMessage(Guid id)
        {
            return this.ExecuteScalar<string>("GetRedirectMessage", id);
        }

        public virtual void DeleteOldRedirectMessage(DateTime cutofDateTime)
        {
            this.ExecuteNonQuery("DeleteOldRedirectMessage", FixDate(cutofDateTime));
        }

        public virtual void UpdateAuthCookie(string cookieValue, DateTime utcExpiry, int userId)
        {
            this.ExecuteNonQuery("AuthCookies_Update", cookieValue, FixDate(utcExpiry), userId);
        }

        public virtual IDataReader FindAuthCookie(string cookieValue)
        {
            return this.ExecuteReader("AuthCookies_Find", cookieValue);
        }

        public virtual void DeleteAuthCookie(string cookieValue)
        {
            this.ExecuteNonQuery("AuthCookies_DeleteByValue", cookieValue);
        }

        public virtual void DeleteExpiredAuthCookies(DateTime utcExpiredBefore)
        {
            this.ExecuteNonQuery("AuthCookies_DeleteOld", FixDate(utcExpiredBefore));
        }

        [Obsolete("Deprecated in 7.0.0.  This method is unneccessary.  You can get a reader and convert it to a DataSet. Scheduled removal in v10.0.0.")]
        public virtual DataSet ExecuteDataSet(string procedureName, params object[] commandParameters)
        {
            return Globals.ConvertDataReaderToDataSet(this.ExecuteReader(procedureName, commandParameters));
        }

        [Obsolete("Deprecated in 7.0.0.  This method is unneccessary.  Use the generic version ExecuteScalar<T>.. Scheduled removal in v10.0.0.")]
        public virtual object ExecuteScalar(string procedureName, params object[] commandParameters)
        {
            return this.ExecuteScalar<object>(procedureName, commandParameters);
        }

        [Obsolete("Temporarily Added in DNN 5.4.2. This will be removed and replaced with named instance support.. Scheduled removal in v10.0.0.")]
        public virtual IDataReader ExecuteSQL(string sql, params IDataParameter[] commandParameters)
        {
            SqlParameter[] sqlCommandParameters = null;
            if (commandParameters != null)
            {
                sqlCommandParameters = new SqlParameter[commandParameters.Length];
                for (int intIndex = 0; intIndex <= commandParameters.Length - 1; intIndex++)
                {
                    sqlCommandParameters[intIndex] = (SqlParameter)commandParameters[intIndex];
                }
            }

            sql = DataUtil.ReplaceTokens(sql);
            try
            {
                return SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql, sqlCommandParameters);
            }
            catch
            {
                // error in SQL query
                return null;
            }
        }

        [Obsolete("Obsoleted in 9.3.0, please use GetFiles(int, bool, boo) instead. schedule to remove in 11.0.0.")]
        public virtual IDataReader GetFiles(int folderId, bool retrieveUnpublishedFiles = false)
        {
            return this.GetFiles(folderId, retrieveUnpublishedFiles, false);
        }

        internal virtual IDictionary<int, string> GetPortalSettingsBySetting(string settingName, string cultureCode)
        {
            var result = new Dictionary<int, string>();
            using (var reader = this.ExecuteReader("GetPortalSettingsBySetting", settingName, cultureCode))
            {
                while (reader.Read())
                {
                    result[reader.GetInt32(0)] = reader.GetString(1);
                }
            }

            return result;
        }

        private static DateTime FixDate(DateTime dateToFix)
        {
            // Fix for Sql Dates having a minimum value of January 1, 1753
            // or maximum value of December 31, 9999
            if (dateToFix <= SqlDateTime.MinValue.Value)
            {
                dateToFix = SqlDateTime.MinValue.Value.AddDays(1);
            }
            else if (dateToFix >= SqlDateTime.MaxValue.Value)
            {
                dateToFix = SqlDateTime.MaxValue.Value.AddDays(-1);
            }

            return dateToFix;
        }

        private object GetRoleNull(int RoleID)
        {
            if (RoleID.ToString(CultureInfo.InvariantCulture) == Globals.glbRoleNothing)
            {
                return DBNull.Value;
            }

            return RoleID;
        }

        private Version GetVersionInternal(bool current)
        {
            Version version = null;
            IDataReader dr = null;
            try
            {
                dr = current ? this.GetDatabaseVersion() : this.GetDatabaseInstallVersion();
                if (dr.Read())
                {
                    version = new Version(Convert.ToInt32(dr["Major"]), Convert.ToInt32(dr["Minor"]),
                                          Convert.ToInt32(dr["Build"]));
                }
            }
            catch (SqlException ex)
            {
                bool noStoredProc = false;
                for (int i = 0; i <= ex.Errors.Count - 1; i++)
                {
                    SqlError sqlError = ex.Errors[i];
                    if (sqlError.Number == 2812 && sqlError.Class == 16) // 2812 - 16 means SP could not be found
                    {
                        noStoredProc = true;
                        break;
                    }
                }

                if (!noStoredProc)
                {
                    throw;
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return version;
        }
    }
}
