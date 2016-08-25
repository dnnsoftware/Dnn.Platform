#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

#region Usings

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

#endregion

// ReSharper disable InconsistentNaming

namespace DotNetNuke.Data
{
	public abstract class DataProvider
	{
		private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DataProvider));

        private const int DuplicateKey = 2601;

        #region Shared/Static Methods

        public static DataProvider Instance()
		{
			return ComponentFactory.GetComponent<DataProvider>();
		}

		#endregion

		#region Public Properties

		public virtual string ConnectionString
		{
			get
			{
				//Get Connection string from web.config
				string connectionString = Config.GetConnectionString();
				if (string.IsNullOrEmpty(connectionString))
				{
					//Use connection string specified in provider
					connectionString = Settings["connectionString"];
				}
				return connectionString;
			}
		}

		public virtual string DatabaseOwner
		{
			get
			{
				string databaseOwner = Settings["databaseOwner"];
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
				string objectQualifier = Settings["objectQualifier"];
				if (!string.IsNullOrEmpty(objectQualifier) && objectQualifier.EndsWith("_") == false)
				{
					objectQualifier += "_";
				}
				return objectQualifier;
			}
		}

		public virtual string ProviderName
		{
			get { return Settings["providerName"]; }
		}

		public virtual string ProviderPath
		{
			get { return Settings["providerPath"]; }
		}

		public abstract Dictionary<string, string> Settings { get; }

		public virtual string UpgradeConnectionString
		{
			get
			{
				return !String.IsNullOrEmpty(Settings["upgradeConnectionString"])
										? Settings["upgradeConnectionString"]
										: ConnectionString;
			}
		}

		#endregion

		#region Private Methods

		private DateTime FixDate(DateTime dateToFix)
		{
			//Fix for Sql Dates having a minimum value of 1/1/1753
			if (dateToFix < SqlDateTime.MinValue.Value)
			{
				dateToFix = SqlDateTime.MinValue.Value;
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

		#endregion

		#region Abstract Methods

		#region DAL + Methods

		public abstract void ExecuteNonQuery(string procedureName, params object[] commandParameters);

        public abstract void BulkInsert(string procedureName, string tableParameterName, DataTable dataTable);

        public abstract IDataReader ExecuteReader(string procedureName, params object[] commandParameters);

		public abstract T ExecuteScalar<T>(string procedureName, params object[] commandParameters);

		public abstract IDataReader ExecuteSQL(string sql);

		#endregion

		#region ExecuteScript Methods

		public abstract string ExecuteScript(string script);

		public abstract string ExecuteScript(string connectionString, string sql);

		public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql);
		public abstract IDataReader ExecuteSQLTemp(string connectionString, string sql, out string errorMessage);

		#endregion

		#region Transaction Methods

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
			var Conn = new SqlConnection(UpgradeConnectionString);
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

		#endregion

		public virtual object GetNull(object Field)
		{
			return Null.GetNull(Field, DBNull.Value);
		}

		# region Upgrade Methods

		public virtual IDataReader FindDatabaseVersion(int Major, int Minor, int Build)
		{
			return ExecuteReader("FindDatabaseVersion", Major, Minor, Build);
		}

		public virtual Version GetDatabaseEngineVersion()
		{
			string version = "0.0";
			IDataReader dr = null;
			try
			{
				dr = ExecuteReader("GetDatabaseServer");
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
			return ExecuteReader("GetDatabaseVersion");
		}

		public virtual IDataReader GetDatabaseInstallVersion()
		{
			return ExecuteReader("GetDatabaseInstallVersion");
		}

		public virtual Version GetVersion()
		{
			return GetVersionInternal(true);
		}

		public virtual Version GetInstallVersion()
		{
			return GetVersionInternal(false);
		}


		private Version GetVersionInternal(bool current)
		{
			Version version = null;
			IDataReader dr = null;
			try
			{
				dr = current ? GetDatabaseVersion() : GetDatabaseInstallVersion();
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
					if (sqlError.Number == 2812 && sqlError.Class == 16) //2812 - 16 means SP could not be found
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

		public virtual DbConnectionStringBuilder GetConnectionStringBuilder()
		{
			return new SqlConnectionStringBuilder();
		}

		public virtual string GetProviderPath()
		{
			string path = ProviderPath;
			if (!String.IsNullOrEmpty(path))
			{
				path = HostingEnvironment.MapPath(path);

				// ReSharper disable AssignNullToNotNullAttribute
				if (Directory.Exists(path))
				// ReSharper restore AssignNullToNotNullAttribute
				{
					if (!IsConnectionValid)
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
						string filteredMessage = String.Empty;
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
			if ((Major >= 5 || (Major == 4 && Minor == 9 && Build > 0)))
			{
				//If the version > 4.9.0 use the new sproc, which is added in 4.9.1 script
				ExecuteNonQuery("UpdateDatabaseVersionAndName", Major, Minor, Build, Name);
			}
			else
			{
				ExecuteNonQuery("UpdateDatabaseVersion", Major, Minor, Build);
			}
		}

        public virtual void UpdateDatabaseVersionIncrement(int Major, int Minor, int Build, int Increment, string AppName)
        {
            ExecuteNonQuery("UpdateDatabaseVersionIncrement", Major, Minor, Build, Increment, AppName);
        }

        public virtual int GetLastAppliedIteration(int Major, int Minor, int Build)
        {
            return ExecuteScalar<int>("GetLastAppliedIteration", Major, Minor, Build);
        }

        public virtual string GetUnappliedIterations(string version)
        {
            return ExecuteScalar<string>("GetUnappliedIterations", version);
        }

		#endregion

		#region Host Settings Methods

		public virtual IDataReader GetHostSetting(string SettingName)
		{
			return ExecuteReader("GetHostSetting", SettingName);
		}

		public virtual IDataReader GetHostSettings()
		{
			return ExecuteReader("GetHostSettings");
		}

		public virtual void UpdateHostSetting(string SettingName, string SettingValue, bool SettingIsSecure,
											  int LastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateHostSetting", SettingName, SettingValue, SettingIsSecure, LastModifiedByUserID);
		}

		#endregion

		#region Server Methods

		public virtual void DeleteServer(int ServerId)
		{
			ExecuteNonQuery("DeleteServer", ServerId);
		}

		public virtual IDataReader GetServers()
		{
			return ExecuteReader("GetServers");
		}

		public virtual void UpdateServer(int serverId, string url, string uniqueId, bool enabled, string group)
		{
			ExecuteNonQuery("UpdateServer", serverId, url, uniqueId, enabled, group);
		}

		public virtual int UpdateServerActivity(string serverName, string iisAppName, DateTime createdDate,
												 DateTime lastActivityDate, int pingFailureCount, bool enabled)
		{
			return ExecuteScalar<int>("UpdateServerActivity", serverName, iisAppName, createdDate, lastActivityDate, pingFailureCount, enabled);
		}

		#endregion

		#region Portal Methods

        [Obsolete("Deprecated in Platform 7.4.0, please use CreatePortal version that contain's culturecode")]
		public virtual int CreatePortal(string portalname, string currency, DateTime ExpiryDate, double HostFee,
										double HostSpace, int PageQuota, int UserQuota, int SiteLogHistory,
										 string HomeDirectory, int CreatedByUserID)
		{
			return
				CreatePortal(
											portalname,
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
                                         string HomeDirectory, string CultureCode,int CreatedByUserID )
        {
            return
                ExecuteScalar<int>("AddPortalInfo",
                                            portalname,
                                            currency,
                                            GetNull(ExpiryDate),
                                            HostFee,
                                            HostSpace,
                                            PageQuota,
                                            UserQuota,
                                            GetNull(SiteLogHistory),
                                            HomeDirectory,
                                            CultureCode,
                                            CreatedByUserID);
        }

		public virtual void DeletePortalInfo(int PortalId)
		{
			ExecuteNonQuery("DeletePortalInfo", PortalId);
		}

		public virtual void DeletePortalSetting(int PortalId, string SettingName, string CultureCode)
		{
			ExecuteNonQuery("DeletePortalSetting", PortalId, SettingName, CultureCode);
		}

        public virtual void DeletePortalSettings(int PortalId, string CultureCode)
		{
            ExecuteNonQuery("DeletePortalSettings", PortalId, CultureCode);
		}

		public virtual IDataReader GetExpiredPortals()
		{
			return ExecuteReader("GetExpiredPortals");
		}

		public virtual IDataReader GetPortals(string CultureCode)
		{
			IDataReader reader;
			if (Globals.Status == Globals.UpgradeStatus.Upgrade && Globals.DataBaseVersion < new Version(6, 1, 0))
			{
				reader = ExecuteReader("GetPortals");
			}
			else
			{
				reader = ExecuteReader("GetPortals", CultureCode);
			}
			return reader;
		}

        public virtual IDataReader GetAllPortals()
        {
            return ExecuteReader("GetAllPortals");
        }

		public virtual IDataReader GetPortalsByName(string nameToMatch, int pageIndex, int pageSize)
		{
			return ExecuteReader("GetPortalsByName", nameToMatch, pageIndex, pageSize);
		}

		public virtual IDataReader GetPortalsByUser(int userId)
		{
			return ExecuteReader("GetPortalsByUser", userId);
		}


		public virtual IDataReader GetPortalSettings(int PortalId, string CultureCode)
		{
			return ExecuteReader("GetPortalSettings", PortalId, CultureCode);
		}

		public virtual IDataReader GetPortalSpaceUsed(int PortalId)
		{
			return ExecuteReader("GetPortalSpaceUsed", GetNull(PortalId));
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
											 int registerTabId, int userTabId, int searchTabId, int custom404TabId, int custom500TabId, string defaultLanguage,
											 string homeDirectory, int lastModifiedByUserID, string cultureCode)
		{
			ExecuteNonQuery("UpdatePortalInfo",
									  portalId,
									  portalGroupId,
									  portalName,
									  GetNull(logoFile),
									  GetNull(footerText),
									  GetNull(expiryDate),
									  userRegistration,
									  bannerAdvertising,
									  currency,
									  GetNull(administratorId),
									  hostFee,
									  hostSpace,
									  pageQuota,
									  userQuota,
									  GetNull(paymentProcessor),
									  GetNull(processorUserId),
									  GetNull(processorPassword),
									  GetNull(description),
									  GetNull(keyWords),
									  GetNull(backgroundFile),
									  GetNull(siteLogHistory),
									  GetNull(splashTabId),
									  GetNull(homeTabId),
									  GetNull(loginTabId),
									  GetNull(registerTabId),
									  GetNull(userTabId),
									  GetNull(searchTabId),
									  GetNull(custom404TabId),
									  GetNull(custom500TabId),
									  GetNull(defaultLanguage),
									  homeDirectory,
									  lastModifiedByUserID,
									  cultureCode);
		}

		public virtual void UpdatePortalSetting(int portalId, string settingName, string settingValue, int userId,
												string cultureCode)
		{
			ExecuteNonQuery("UpdatePortalSetting", portalId, settingName, settingValue, userId, cultureCode);
		}

		public virtual void UpdatePortalSetup(int portalId, int administratorId, int administratorRoleId,
											  int registeredRoleId, int splashTabId, int homeTabId, int loginTabId,
											  int registerTabId,
											   int userTabId, int searchTabId, int custom404TabId, int custom500TabId, int adminTabId, string cultureCode)
		{
			ExecuteNonQuery("UpdatePortalSetup",
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
									  adminTabId,
									  cultureCode);
		}

		#endregion

		#region Tab Methods

		
        public virtual int AddTabAfter(TabInfo tab, int afterTabId, int createdByUserID)
        {
            return ExecuteScalar<int>("AddTabAfter",
                                        afterTabId,
                                        tab.ContentItemId,
                                        GetNull(tab.PortalID),
                                        tab.UniqueId,
                                        tab.VersionGuid,
                                        GetNull(tab.DefaultLanguageGuid),
                                        tab.LocalizedVersionGuid,
                                        tab.TabName,
                                        tab.IsVisible,
                                        tab.DisableLink,
                                        GetNull(tab.ParentId),
                                        tab.IconFile,
                                        tab.IconFileLarge,
                                        tab.Title,
                                        tab.Description,
                                        tab.KeyWords,
                                        tab.Url,
                                        GetNull(tab.SkinSrc),
                                        GetNull(tab.ContainerSrc),
                                        GetNull(tab.StartDate),
                                        GetNull(tab.EndDate),
                                        GetNull(tab.RefreshInterval),
                                        GetNull(tab.PageHeadText),
                                        tab.IsSecure,
                                        tab.PermanentRedirect,
                                        tab.SiteMapPriority,
                                        createdByUserID,
                                        GetNull(tab.CultureCode),
                                        tab.IsSystem);
        }

        public virtual int AddTabBefore(TabInfo tab, int beforeTabId, int createdByUserID)
        {
            return ExecuteScalar<int>("AddTabBefore",
                                        beforeTabId,
                                        tab.ContentItemId,
                                        GetNull(tab.PortalID),
                                        tab.UniqueId,
                                        tab.VersionGuid,
                                        GetNull(tab.DefaultLanguageGuid),
                                        tab.LocalizedVersionGuid,
                                        tab.TabName,
                                        tab.IsVisible,
                                        tab.DisableLink,
                                        GetNull(tab.ParentId),
                                        tab.IconFile,
                                        tab.IconFileLarge,
                                        tab.Title,
                                        tab.Description,
                                        tab.KeyWords,
                                        tab.Url,
                                        GetNull(tab.SkinSrc),
                                        GetNull(tab.ContainerSrc),
                                        GetNull(tab.StartDate),
                                        GetNull(tab.EndDate),
                                        GetNull(tab.RefreshInterval),
                                        GetNull(tab.PageHeadText),
                                        tab.IsSecure,
                                        tab.PermanentRedirect,
                                        tab.SiteMapPriority,
                                        createdByUserID,
                                        GetNull(tab.CultureCode),
                                        tab.IsSystem);
        }


        public virtual int AddTabToEnd(TabInfo tab, int createdByUserID)
        {
            return ExecuteScalar<int>("AddTabToEnd",
                                        tab.ContentItemId,
                                        GetNull(tab.PortalID),
                                        tab.UniqueId,
                                        tab.VersionGuid,
                                        GetNull(tab.DefaultLanguageGuid),
                                        tab.LocalizedVersionGuid,
                                        tab.TabName,
                                        tab.IsVisible,
                                        tab.DisableLink,
                                        GetNull(tab.ParentId),
                                        tab.IconFile,
                                        tab.IconFileLarge,
                                        tab.Title,
                                        tab.Description,
                                        tab.KeyWords,
                                        tab.Url,
                                        GetNull(tab.SkinSrc),
                                        GetNull(tab.ContainerSrc),
                                        GetNull(tab.StartDate),
                                        GetNull(tab.EndDate),
                                        GetNull(tab.RefreshInterval),
                                        GetNull(tab.PageHeadText),
                                        tab.IsSecure,
                                        tab.PermanentRedirect,
                                        tab.SiteMapPriority,
                                        createdByUserID,
                                        GetNull(tab.CultureCode),
                                        tab.IsSystem);
        }

		public virtual void DeleteTab(int tabId)
		{
			ExecuteNonQuery("DeleteTab", tabId);
		}

		public virtual void DeleteTabSetting(int TabId, string SettingName)
		{
			ExecuteNonQuery("DeleteTabSetting", TabId, SettingName);
		}

		public virtual void DeleteTabSettings(int TabId)
		{
			ExecuteNonQuery("DeleteTabSettings", TabId);
		}

		public virtual void DeleteTabUrl(int tabId, int seqNum)
		{
			ExecuteNonQuery("DeleteTabUrl", tabId, seqNum);
		}

		public virtual void DeleteTabVersion(int tabVersionId)
		{
			ExecuteNonQuery("DeleteTabVersion", tabVersionId);
		}

		public virtual void DeleteTabVersionDetail(int tabVersionDetailId)
		{
			ExecuteNonQuery("DeleteTabVersionDetail", tabVersionDetailId);
		}

	    public virtual void DeleteTabVersionDetailByModule(int moduleId)
	    {
            ExecuteNonQuery("DeleteTabVersionDetailByModule", moduleId);
        }

        public virtual void DeleteTranslatedTabs(int tabId, string cultureCode)
		{
			ExecuteNonQuery("DeleteTranslatedTabs", tabId, cultureCode);
		}

		public virtual void EnsureNeutralLanguage(int portalId, string cultureCode)
		{
			ExecuteNonQuery("EnsureNeutralLanguage", portalId, cultureCode);
		}

		public virtual void ConvertTabToNeutralLanguage(int portalId, int tabId, string cultureCode)
		{
			ExecuteNonQuery("ConvertTabToNeutralLanguage", portalId, tabId, cultureCode);
		}

		public virtual IDataReader GetAllTabs()
		{
			return ExecuteReader("GetAllTabs");
		}

		public virtual IDataReader GetTab(int tabId)
		{
			return ExecuteReader("GetTab", tabId);
		}

		public virtual IDataReader GetTabByUniqueID(Guid uniqueId)
		{
			return ExecuteReader("GetTabByUniqueID", uniqueId);
		}

		public virtual IDataReader GetTabPanes(int tabId)
		{
			return ExecuteReader("GetTabPanes", tabId);
		}

		public virtual IDataReader GetTabPaths(int portalId, string cultureCode)
		{
			return ExecuteReader("GetTabPaths", GetNull(portalId), cultureCode);
		}

		public virtual IDataReader GetTabs(int portalId)
		{
			return ExecuteReader("GetTabs", GetNull(portalId));
		}

		public virtual IDataReader GetTabsByModuleID(int moduleID)
		{
			return ExecuteReader("GetTabsByModuleID", moduleID);
		}

		public virtual IDataReader GetTabsByTabModuleID(int tabModuleID)
		{
			return ExecuteReader("GetTabsByTabModuleID", tabModuleID);
		}

		public virtual IDataReader GetTabsByPackageID(int portalID, int packageID, bool forHost)
		{
			return ExecuteReader("GetTabsByPackageID", GetNull(portalID), packageID, forHost);
		}

		public virtual IDataReader GetTabSetting(int TabID, string SettingName)
		{
			return ExecuteReader("GetTabSetting", TabID, SettingName);
		}

		public virtual IDataReader GetTabSettings(int portalId)
		{
            return ExecuteReader("GetTabSettings", GetNull(portalId));
		}

		public virtual IDataReader GetTabAliasSkins(int portalId)
		{
			return ExecuteReader("GetTabAliasSkins", GetNull(portalId));
		}

		public virtual IDataReader GetTabCustomAliases(int portalId)
		{
			return ExecuteReader("GetTabCustomAliases", GetNull(portalId));
		}

		public virtual IDataReader GetTabUrls(int portalId)
		{
			return ExecuteReader("GetTabUrls", GetNull(portalId));
		}

		public virtual IDataReader GetTabVersions(int tabId)
		{
			return ExecuteReader("GetTabVersions", GetNull(tabId));
		}

		public virtual IDataReader GetTabVersionDetails(int tabVersionId)
		{
			return ExecuteReader("GetTabVersionDetails", GetNull(tabVersionId));
		}

		public virtual IDataReader GetTabVersionDetailsHistory(int tabId, int version)
		{
			return ExecuteReader("GetTabVersionDetailsHistory", GetNull(tabId), GetNull(version));
		}

		public virtual IDataReader GetCustomAliasesForTabs()
		{
			return ExecuteReader("GetCustomAliasesForTabs");
		}

		public virtual void LocalizeTab(int tabId, string cultureCode, int lastModifiedByUserID)
		{
			ExecuteNonQuery("LocalizeTab", tabId, cultureCode, lastModifiedByUserID);
		}

		public virtual void MoveTabAfter(int tabId, int afterTabId, int lastModifiedByUserID)
		{
			ExecuteNonQuery("MoveTabAfter", tabId, afterTabId, lastModifiedByUserID);
		}

		public virtual void MoveTabBefore(int tabId, int beforeTabId, int lastModifiedByUserID)
		{
			ExecuteNonQuery("MoveTabBefore", tabId, beforeTabId, lastModifiedByUserID);
		}

		public virtual void MoveTabToParent(int tabId, int parentId, int lastModifiedByUserID)
		{
			ExecuteNonQuery("MoveTabToParent", tabId, GetNull(parentId), lastModifiedByUserID);
		}

		public virtual void SaveTabUrl(int tabId, int seqNum, int portalAliasId, int portalAliasUsage, string url, string queryString, string cultureCode, string httpStatus, bool isSystem, int modifiedByUserID)
		{
			ExecuteNonQuery("SaveTabUrl", tabId, seqNum, GetNull(portalAliasId), portalAliasUsage, url, queryString, cultureCode, httpStatus, isSystem, modifiedByUserID);
		}

		public virtual int SaveTabVersion(int tabVersionId, int tabId, DateTime timeStamp, int version, bool isPublished, int createdByUserID, int modifiedByUserID)
		{
			return ExecuteScalar<int>("SaveTabVersion", tabVersionId, tabId, timeStamp, version, isPublished, createdByUserID, modifiedByUserID);
		}

		public virtual int SaveTabVersionDetail(int tabVersionDetailId, int tabVersionId, int moduleId, int moduleVersion, string paneName, int moduleOrder, int action, int createdByUserID, int modifiedByUserID)
		{
			return ExecuteScalar<int>("SaveTabVersionDetail", tabVersionDetailId, tabVersionId, moduleId, moduleVersion, paneName, moduleOrder, action, createdByUserID, modifiedByUserID);
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
            ExecuteNonQuery("UpdateTab",
                                      tabId,
                                      contentItemId,
                                      GetNull(portalId),
                                      versionGuid,
                                      GetNull(defaultLanguageGuid),
                                      localizedVersionGuid,
                                      tabName,
                                      isVisible,
                                      disableLink,
                                      GetNull(parentId),
                                      iconFile,
                                      iconFileLarge,
                                      title,
                                      description,
                                      keyWords,
                                      isDeleted,
                                      url,
                                      GetNull(skinSrc),
                                      GetNull(containerSrc),
                                      GetNull(startDate),
                                      GetNull(endDate),
                                      GetNull(refreshInterval),
                                      GetNull(pageHeadText),
                                      isSecure,
                                      permanentRedirect,
                                      siteMapPriority,
                                      lastModifiedByuserID,
                                      GetNull(cultureCode),
                                      IsSystem);
        }

		public virtual void UpdateTabOrder(int tabId, int tabOrder, int parentId, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateTabOrder", tabId, tabOrder, GetNull(parentId), lastModifiedByUserID);
		}

		public virtual void UpdateTabSetting(int TabId, string SettingName, string SettingValue,
											 int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateTabSetting", TabId, SettingName, SettingValue, lastModifiedByUserID);
		}

		public virtual void UpdateTabTranslationStatus(int tabId, Guid localizedVersionGuid, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateTabTranslationStatus", tabId, localizedVersionGuid, lastModifiedByUserID);
		}

		public virtual void MarkAsPublished(int tabId)
		{
			ExecuteNonQuery("PublishTab", tabId);
		}

		public virtual void UpdateTabVersion(int tabId, Guid versionGuid)
		{
			ExecuteNonQuery("UpdateTabVersion", tabId, versionGuid);
		}

		#endregion

		#region Module Methods

		public virtual int AddModule(int contentItemId, int portalId, int moduleDefId, bool allTabs, DateTime startDate,
									 DateTime endDate, bool inheritViewPermissions, bool isShareable,
									 bool isShareableViewOnly,
								bool isDeleted, int createdByUserID)
		{
			return ExecuteScalar<int>("AddModule",
											contentItemId,
											GetNull(portalId),
											moduleDefId,
											allTabs,
											GetNull(startDate),
											GetNull(endDate),
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
			ExecuteNonQuery("AddTabModule",
									  TabId,
									  ModuleId,
									  ModuleTitle,
									  GetNull(Header),
									  GetNull(Footer),
									  ModuleOrder,
									  PaneName,
									  CacheTime,
									  GetNull(CacheMethod),
									  GetNull(Alignment),
									  GetNull(Color),
									  GetNull(Border),
									  GetNull(IconFile),
									  Visibility,
									  GetNull(ContainerSrc),
									  DisplayTitle,
									  DisplayPrint,
									  DisplaySyndicate,
									  IsWebSlice,
									  WebSliceTitle,
									  GetNull(WebSliceExpiryDate),
									  WebSliceTTL,
									  UniqueId,
									  VersionGuid,
									  GetNull(DefaultLanguageGuid),
									  LocalizedVersionGuid,
									  CultureCode,
									  createdByUserID);
		}


		public virtual void DeleteModule(int moduleId)
		{
			ExecuteNonQuery("DeleteModule", moduleId);
		}

		public virtual void DeleteModuleSetting(int moduleId, string settingName)
		{
			ExecuteNonQuery("DeleteModuleSetting", moduleId, settingName);
		}

		public virtual void DeleteModuleSettings(int moduleId)
		{
			ExecuteNonQuery("DeleteModuleSettings", moduleId);
		}

		public virtual void DeleteTabModule(int tabId, int moduleId, bool softDelete)
		{
			ExecuteNonQuery("DeleteTabModule", tabId, moduleId, softDelete);
		}

		public virtual void DeleteTabModuleSetting(int tabModuleId, string settingName)
		{
			ExecuteNonQuery("DeleteTabModuleSetting", tabModuleId, settingName);
		}

		public virtual void DeleteTabModuleSettings(int tabModuleId)
		{
			ExecuteNonQuery("DeleteTabModuleSettings", tabModuleId);
		}

	    public virtual IDataReader GetTabModuleSettingsByName(int portalId, string settingName)
	    {
            return ExecuteReader("GetTabModuleSettingsByName", portalId, settingName);
        }

	    public virtual IDataReader GetTabModuleIdsBySettingNameAndValue(int portalId, string settingName, string expectedValue)
	    {
            return ExecuteReader("GetTabModuleIdsBySettingNameAndValue", portalId, settingName, expectedValue);
        }

        public virtual IDataReader GetAllModules()
		{
			return ExecuteReader("GetAllModules");
		}

		public virtual IDataReader GetAllTabsModules(int portalId, bool allTabs)
		{
			return ExecuteReader("GetAllTabsModules", portalId, allTabs);
		}

		public virtual IDataReader GetAllTabsModulesByModuleID(int moduleId)
		{
			return ExecuteReader("GetAllTabsModulesByModuleID", moduleId);
		}

		public virtual IDataReader GetModule(int moduleId, int tabId)
		{
			return ExecuteReader("GetModule", moduleId, GetNull(tabId));
		}

		public virtual IDataReader GetModuleByDefinition(int portalId, string definitionName)
		{
			return ExecuteReader("GetModuleByDefinition", GetNull(portalId), definitionName);
		}

		public virtual IDataReader GetModuleByUniqueID(Guid uniqueId)
		{
			return ExecuteReader("GetModuleByUniqueID", uniqueId);
		}

		public virtual IDataReader GetModules(int portalId)
		{
			return ExecuteReader("GetModules", portalId);
		}

		public virtual IDataReader GetModuleSetting(int moduleId, string settingName)
		{
			return ExecuteReader("GetModuleSetting", moduleId, settingName);
		}

		public virtual IDataReader GetModuleSettings(int moduleId)
		{
			return ExecuteReader("GetModuleSettings", moduleId);
		}

		public virtual IDataReader GetModuleSettingsByTab(int tabId)
		{
			return ExecuteReader("GetModuleSettingsByTab", tabId);
		}

		public virtual IDataReader GetSearchModules(int portalId)
		{
			return ExecuteReader("GetSearchModules", GetNull(portalId));
		}

		public virtual IDataReader GetTabModule(int tabModuleId)
		{
			return ExecuteReader("GetTabModule", tabModuleId);
		}

		public virtual IDataReader GetTabModuleOrder(int tabId, string paneName)
		{
			return ExecuteReader("GetTabModuleOrder", tabId, paneName);
		}

		public virtual IDataReader GetTabModules(int tabId)
		{
			return ExecuteReader("GetTabModules", tabId);
		}

		public virtual IDataReader GetTabModuleSetting(int tabModuleId, string settingName)
		{
			return ExecuteReader("GetTabModuleSetting", tabModuleId, settingName);
		}

		public virtual IDataReader GetTabModuleSettings(int tabModuleId)
		{
			return ExecuteReader("GetTabModuleSettings", tabModuleId);
		}

		public virtual IDataReader GetTabModuleSettingsByTab(int tabId)
		{
			return ExecuteReader("GetTabModuleSettingsByTab", tabId);
		}

		public virtual void MoveTabModule(int fromTabId, int moduleId, int toTabId, string toPaneName,
										  int lastModifiedByUserID)
		{
			ExecuteNonQuery("MoveTabModule", fromTabId, moduleId, toTabId, toPaneName, lastModifiedByUserID);
		}

		public virtual void RestoreTabModule(int tabId, int moduleId)
		{
			ExecuteNonQuery("RestoreTabModule", tabId, moduleId);
		}

		public virtual void UpdateModule(int moduleId, int moduleDefId, int contentItemId, bool allTabs,
										 DateTime startDate,
										 DateTime endDate, bool inheritViewPermissions, bool isShareable,
										 bool isShareableViewOnly,
									bool isDeleted, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateModule",
									  moduleId,
									  moduleDefId,
									  contentItemId,
									  allTabs,
									  GetNull(startDate),
									  GetNull(endDate),
									  inheritViewPermissions,
									  isShareable,
									  isShareableViewOnly,
									  isDeleted,
									  lastModifiedByUserID);
		}

		public virtual void UpdateModuleLastContentModifiedOnDate(int moduleId)
		{
			ExecuteNonQuery("UpdateModuleLastContentModifiedOnDate", moduleId);
		}

		public virtual void UpdateModuleOrder(int tabId, int moduleId, int moduleOrder, string paneName)
		{
			ExecuteNonQuery("UpdateModuleOrder", tabId, moduleId, moduleOrder, paneName);
		}

		public virtual void UpdateModuleSetting(int moduleId, string settingName, string settingValue,
												int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateModuleSetting", moduleId, settingName, settingValue, lastModifiedByUserID);
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
			ExecuteNonQuery("UpdateTabModule",
									  TabModuleId,
									  TabId,
									  ModuleId,
									  ModuleTitle,
									  GetNull(Header),
									  GetNull(Footer),
									  ModuleOrder,
									  PaneName,
									  CacheTime,
									  GetNull(CacheMethod),
									  GetNull(Alignment),
									  GetNull(Color),
									  GetNull(Border),
									  GetNull(IconFile),
									  Visibility,
									  GetNull(ContainerSrc),
									  DisplayTitle,
									  DisplayPrint,
									  DisplaySyndicate,
									  IsWebSlice,
									  WebSliceTitle,
									  GetNull(WebSliceExpiryDate),
									  WebSliceTTL,
									  VersionGuid,
									  GetNull(DefaultLanguageGuid),
									  LocalizedVersionGuid,
									  CultureCode,
									  lastModifiedByUserID);
		}

		public virtual void UpdateTabModuleTranslationStatus(int tabModuleId, Guid localizedVersionGuid,
															 int lastModifiedByUserId)
		{
			ExecuteNonQuery("UpdateTabModuleTranslationStatus", tabModuleId, localizedVersionGuid, lastModifiedByUserId);
		}

		public virtual void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue,
												   int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateTabModuleSetting", tabModuleId, settingName, settingValue, lastModifiedByUserID);
		}

		public virtual void UpdateTabModuleVersion(int tabModuleId, Guid versionGuid)
		{
			ExecuteNonQuery("UpdateTabModuleVersion", tabModuleId, versionGuid);
		}

		public virtual void UpdateTabModuleVersionByModule(int moduleId)
		{
			ExecuteNonQuery("UpdateTabModuleVersionByModule", moduleId);
		}

        public virtual IDataReader GetInstalledModules()
        {
            return ExecuteReader("GetInstalledModules");
        }

		#endregion

		#region DesktopModule Methods

		public virtual int AddDesktopModule(int packageID, string moduleName, string folderName, string friendlyName,
											string description, string version, bool isPremium, bool isAdmin,
											string businessControllerClass, int supportedFeatures, int shareable,
											string compatibleVersions, string dependencies, string permissions,
											 int contentItemId, int createdByUserID, string adminPage, string hostPage)
		{
			return ExecuteScalar<int>("AddDesktopModule",
														packageID,
														moduleName,
														folderName,
														friendlyName,
														GetNull(description),
														GetNull(version),
														isPremium,
														isAdmin,
														businessControllerClass,
														supportedFeatures,
														shareable,
														GetNull(compatibleVersions),
														GetNull(dependencies),
														GetNull(permissions),
														contentItemId,
														createdByUserID,
                                                        adminPage,
                                                        hostPage);
		}

		public virtual void DeleteDesktopModule(int desktopModuleId)
		{
			ExecuteNonQuery("DeleteDesktopModule", desktopModuleId);
		}

		public virtual IDataReader GetDesktopModules()
		{
			return ExecuteReader("GetDesktopModules");
		}

		public virtual IDataReader GetDesktopModulesByPortal(int portalId)
		{
			return ExecuteReader("GetDesktopModulesByPortal", portalId);
		}

		public virtual void UpdateDesktopModule(int desktopModuleId, int packageID, string moduleName, string folderName,
												string friendlyName, string description, string version, bool isPremium,
												bool isAdmin, string businessControllerClass, int supportedFeatures,
												int shareable, string compatibleVersions, string dependencies,
												string permissions,
												 int contentItemId, int lastModifiedByUserID, string adminpage, string hostpage)
		{
			ExecuteNonQuery("UpdateDesktopModule",
									  desktopModuleId,
									  packageID,
									  moduleName,
									  folderName,
									  friendlyName,
									  GetNull(description),
									  GetNull(version),
									  isPremium,
									  isAdmin,
									  businessControllerClass,
									  supportedFeatures,
									  shareable,
									  GetNull(compatibleVersions),
									  GetNull(dependencies),
									  GetNull(permissions),
									  contentItemId,
									  lastModifiedByUserID,
                                      adminpage,
                                      hostpage);
		}

		#endregion

		#region PortalDesktopModule Methods

		public virtual int AddPortalDesktopModule(int portalId, int desktopModuleId, int createdByUserID)
		{
			return ExecuteScalar<int>("AddPortalDesktopModule", portalId, desktopModuleId, createdByUserID);
		}

		public virtual void DeletePortalDesktopModules(int portalId, int desktopModuleId)
		{
			ExecuteNonQuery("DeletePortalDesktopModules", GetNull(portalId), GetNull(desktopModuleId));
		}

		public virtual IDataReader GetPortalDesktopModules(int portalId, int desktopModuleId)
		{
			return ExecuteReader("GetPortalDesktopModules", GetNull(portalId), GetNull(desktopModuleId));
		}

		#endregion

		#region ModuleDefinition Methods

		public virtual int AddModuleDefinition(int desktopModuleId, string friendlyName, string definitionName,
											   int defaultCacheTime, int createdByUserID)
		{
			return ExecuteScalar<int>("AddModuleDefinition", desktopModuleId, friendlyName, definitionName,
									  defaultCacheTime, createdByUserID);
		}

		public virtual void DeleteModuleDefinition(int moduleDefId)
		{
			ExecuteNonQuery("DeleteModuleDefinition", moduleDefId);
		}

		public virtual IDataReader GetModuleDefinitions()
		{
			return ExecuteReader("GetModuleDefinitions");
		}

		public virtual void UpdateModuleDefinition(int moduleDefId, string friendlyName, string definitionName,
												   int defaultCacheTime, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateModuleDefinition", moduleDefId, friendlyName, definitionName, defaultCacheTime,
							lastModifiedByUserID);
		}

		#endregion

		#region ModuleControl Methods

		public virtual int AddModuleControl(int moduleDefId, string controlKey, string controlTitle, string controlSrc,
											string iconFile, int controlType, int viewOrder, string helpUrl,
											bool supportsPartialRendering, bool supportsPopUps, int createdByUserID)
		{
			return ExecuteScalar<int>("AddModuleControl",
														GetNull(moduleDefId),
														GetNull(controlKey),
														GetNull(controlTitle),
														controlSrc,
														GetNull(iconFile),
														controlType,
														GetNull(viewOrder),
														GetNull(helpUrl),
														supportsPartialRendering,
														supportsPopUps,
														createdByUserID);
		}

		public virtual void DeleteModuleControl(int moduleControlId)
		{
			ExecuteNonQuery("DeleteModuleControl", moduleControlId);
		}

		public virtual IDataReader GetModuleControls()
		{
			return ExecuteReader("GetModuleControls");
		}

		public virtual void UpdateModuleControl(int moduleControlId, int moduleDefId, string controlKey,
												string controlTitle,
												string controlSrc, string iconFile, int controlType, int viewOrder,
												string helpUrl,
												bool supportsPartialRendering, bool supportsPopUps,
												int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateModuleControl",
									  moduleControlId,
									  GetNull(moduleDefId),
									  GetNull(controlKey),
									  GetNull(controlTitle),
									  controlSrc,
									  GetNull(iconFile),
									  controlType,
									  GetNull(viewOrder),
									  GetNull(helpUrl),
									  supportsPartialRendering,
									  supportsPopUps,
									  lastModifiedByUserID);
		}

		#endregion

		#region Folder Methods

		public virtual int AddFolder(int portalId, Guid uniqueId, Guid versionGuid, string folderPath, string mappedPath,
										int storageLocation, bool isProtected, bool isCached, DateTime lastUpdated,
										int createdByUserId, int folderMappingId, bool isVersioned, int workflowId,
										int parentId)
		{
			return ExecuteScalar<int>("AddFolder",
											GetNull(portalId),
											uniqueId,
											versionGuid,
											folderPath,
											mappedPath,
											storageLocation,
											isProtected,
											isCached,
											GetNull(lastUpdated),
											createdByUserId,
											folderMappingId,
											isVersioned,
											GetNull(workflowId),
											GetNull(parentId));
		}

		public virtual void DeleteFolder(int portalId, string folderPath)
		{
			ExecuteNonQuery("DeleteFolder", GetNull(portalId), folderPath);
		}

		public virtual IDataReader GetFolder(int folderId)
		{
			return ExecuteReader("GetFolderByFolderID", folderId);
		}

		public virtual IDataReader GetFolder(int portalId, string folderPath)
		{
			return ExecuteReader("GetFolderByFolderPath", GetNull(portalId), folderPath);
		}

		public virtual IDataReader GetFolderByUniqueID(Guid uniqueId)
		{
			return ExecuteReader("GetFolderByUniqueID", uniqueId);
		}

		public virtual IDataReader GetFoldersByPortal(int portalId)
		{
			return ExecuteReader("GetFolders", GetNull(portalId));
		}

		public virtual IDataReader GetFoldersByPortalAndPermissions(int portalId, string permissions, int userId)
		{
			return ExecuteReader("GetFoldersByPermissions", GetNull(portalId), GetNull(permissions), GetNull(userId), -1, "");
		}

		public virtual int GetLegacyFolderCount()
		{
			return ExecuteScalar<int>("GetLegacyFolderCount");
		}

		public virtual void UpdateFolder(int portalId, Guid versionGuid, int folderId, string folderPath,
											int storageLocation, string mappedPath,
											bool isProtected, bool isCached, DateTime lastUpdated, int lastModifiedByUserID,
											int folderMappingID, bool isVersioned, int workflowID, int parentID)
		{
			ExecuteNonQuery("UpdateFolder",
									GetNull(portalId),
									versionGuid,
									folderId,
									folderPath,
									mappedPath,
									storageLocation,
									isProtected,
									isCached,
									GetNull(lastUpdated),
									lastModifiedByUserID,
									folderMappingID,
									isVersioned,
									GetNull(workflowID),
									GetNull(parentID));
		}

		public virtual void UpdateFolderVersion(int folderId, Guid versionGuid)
		{
			ExecuteNonQuery("UpdateFolderVersion", folderId, versionGuid);
		}

		public virtual IDataReader UpdateLegacyFolders()
		{
			return ExecuteReader("UpdateLegacyFolders");
		}

		#endregion

		#region File Methods

		public virtual int AddFile(int portalId, Guid uniqueId, Guid versionGuid, string fileName, string extension,
									long size, int width, int height, string contentType, string folder, int folderId,
									int createdByUserID, string hash, DateTime lastModificationTime,
									string title, string description, DateTime startDate, DateTime endDate, bool enablePublishPeriod, int contentItemId)
		{
			return ExecuteScalar<int>("AddFile",
											GetNull(portalId),
											uniqueId,
											versionGuid,
											fileName,
											extension,
											size,
											GetNull(width),
											GetNull(height),
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
											GetNull(endDate),
											GetNull(contentItemId));
		}

	    public virtual void SetFileHasBeenPublished(int fileId, bool hasBeenPublished)
	    {
            ExecuteNonQuery("SetFileHasBeenPublished", fileId, hasBeenPublished);
        }
        
        public virtual int CountLegacyFiles()
		{
			return ExecuteScalar<int>("CountLegacyFiles");
		}

		public virtual void ClearFileContent(int fileId)
		{
			ExecuteNonQuery("ClearFileContent", fileId);
		}

		public virtual void DeleteFile(int portalId, string fileName, int folderId)
		{
			ExecuteNonQuery("DeleteFile", GetNull(portalId), fileName, folderId);
		}

		public virtual void DeleteFiles(int portalId)
		{
			ExecuteNonQuery("DeleteFiles", GetNull(portalId));
		}

		public virtual DataTable GetAllFiles()
		{
			return Globals.ConvertDataReaderToDataTable(ExecuteReader("GetAllFiles"));
		}

		public virtual IDataReader GetFile(string fileName, int folderId, bool retrieveUnpublishedFiles = false)
		{
			return ExecuteReader("GetFile", fileName, folderId, retrieveUnpublishedFiles);
		}

		public virtual IDataReader GetFileById(int fileId, bool retrieveUnpublishedFiles = false)
		{
			return ExecuteReader("GetFileById", fileId, retrieveUnpublishedFiles);
		}

		public virtual IDataReader GetFileByUniqueID(Guid uniqueId)
		{
			return ExecuteReader("GetFileByUniqueID", uniqueId);
		}

		public virtual IDataReader GetFileContent(int fileId)
		{
			return ExecuteReader("GetFileContent", fileId);
		}

		public virtual IDataReader GetFileVersionContent(int fileId, int version)
		{
			return ExecuteReader("GetFileVersionContent", fileId, version);
		}

		public virtual IDataReader GetFiles(int folderId, bool retrieveUnpublishedFiles = false)
		{
			return ExecuteReader("GetFiles", folderId, retrieveUnpublishedFiles);
		}

        /// <summary>
        /// This is an internal method for communication between DNN business layer and SQL database.
        /// Do not use in custom modules, please use API (DotNetNuke.Services.FileSystem.FileManager.UpdateFile)
        /// 
        /// Stores information about a specific file, stored in DNN filesystem
        /// calling petapoco method to call the underlying stored procedure "UpdateFile"
        /// </summary>
        /// <param name="fileId">ID of the (already existing) file</param>
        /// <param name="versionGuid">GUID of this file version  (should usually not be modified)</param>
        /// <param name="fileName">Name of the file in the file system (including extension)</param>
        /// <param name="extension">File type - should meet extension in FileName</param>
        /// <param name="size">Size of file (bytes)</param>
        /// <param name="width">Width of images/video (lazy load: pass Null, might be retrieved by DNN platform on db file sync)</param>
        /// <param name="height">Height of images/video (lazy load: pass Null, might be retrieved by DNN platform on db file snyc)</param>
        /// <param name="contentType">MIME type of the file</param>
        /// <param name="folderId">ID of the folder, the file resides in</param>
        /// <param name="lastModifiedByUserID">ID of the user, who performed last update of file or file info</param>
        /// <param name="hash">SHa1 hash of the file content, used for file versioning (lazy load: pass Null, will be generated by DNN platform on db file sync)</param>
        /// <param name="lastModificationTime">timestamp, when last update of file or file info happened</param>
        /// <param name="title">Display title of the file - optional (pass Null if not provided)</param>
        /// <param name="description">Description of the file.</param>
        /// <param name="startDate">date and time (server TZ), from which the file should be displayed/accessible (according to folder permission)</param>
        /// <param name="endDate">date and time (server TZ), until which the file should be displayed/accessible (according to folder permission)</param>
        /// <param name="enablePublishPeriod">shall startdate/end date be used?</param>
        /// <param name="contentItemId">ID of the associated contentitem with description etc. (optional)</param>
        public virtual void UpdateFile(int fileId, Guid versionGuid, string fileName, string extension, long size,
										int width, int height, string contentType, int folderId,
										int lastModifiedByUserID, string hash, DateTime lastModificationTime,
										string title, string description, DateTime startDate, DateTime endDate, bool enablePublishPeriod, int contentItemId)
		{
			ExecuteNonQuery("UpdateFile",
									  fileId,
									  versionGuid,
									  fileName,
									  extension,
									  size,
									  GetNull(width),
									  GetNull(height),
									  contentType,
									  folderId,
									  lastModifiedByUserID,
									  hash,
									  lastModificationTime,
									  title,
                                      description,
									  enablePublishPeriod,
									  startDate,
									  GetNull(endDate),
									  GetNull(contentItemId));
		}

		public virtual void UpdateFileLastModificationTime(int fileId, DateTime lastModificationTime)
		{
			ExecuteNonQuery("UpdateFileLastModificationTime",
									  fileId,
									  lastModificationTime);
		}

		public virtual void UpdateFileHashCode(int fileId, string hashCode)
		{
			ExecuteNonQuery("UpdateFileHashCode",
									  fileId,
									  hashCode);
		}

		public virtual void UpdateFileContent(int fileId, byte[] content)
		{
			ExecuteNonQuery("UpdateFileContent", fileId, GetNull(content));
		}


		public virtual void UpdateFileVersion(int fileId, Guid versionGuid)
		{
			ExecuteNonQuery("UpdateFileVersion", fileId, versionGuid);
		}

		#endregion

		#region Permission Methods

		public virtual int AddPermission(string permissionCode, int moduleDefID, string permissionKey, string permissionName, int createdByUserID)
		{
			return ExecuteScalar<int>("AddPermission", moduleDefID, permissionCode, permissionKey, permissionName, createdByUserID);
		}

		public virtual void DeletePermission(int permissionID)
		{
			ExecuteNonQuery("DeletePermission", permissionID);
		}

		public virtual void UpdatePermission(int permissionID, string permissionCode, int moduleDefID, string permissionKey, string permissionName, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdatePermission", permissionID, permissionCode, moduleDefID, permissionKey, permissionName, lastModifiedByUserID);
		}

		#endregion

		#region Module Permission Methods

		public virtual int AddModulePermission(int moduleId, int portalId, int permissionId, int roleId,
											   bool allowAccess, int userId,
												int createdByUserId)
		{
			return ExecuteScalar<int>("AddModulePermission",
											moduleId,
											portalId,
											permissionId,
											GetRoleNull(roleId),
											allowAccess,
											GetNull(userId),
											createdByUserId);
		}

		public virtual void DeleteModulePermission(int modulePermissionId)
		{
			ExecuteNonQuery("DeleteModulePermission", modulePermissionId);
		}

		public virtual void DeleteModulePermissionsByModuleID(int moduleId, int portalId)
		{
			ExecuteNonQuery("DeleteModulePermissionsByModuleID", moduleId, portalId);
		}

		public virtual void DeleteModulePermissionsByUserID(int portalId, int userId)
		{
			ExecuteNonQuery("DeleteModulePermissionsByUserID", portalId, userId);
		}

		public virtual IDataReader GetModulePermission(int modulePermissionId)
		{
			return ExecuteReader("GetModulePermission", modulePermissionId);
		}

		public virtual IDataReader GetModulePermissionsByModuleID(int moduleID, int permissionId)
		{
			return ExecuteReader("GetModulePermissionsByModuleID", moduleID, permissionId);
		}

		public virtual IDataReader GetModulePermissionsByPortal(int portalId)
		{
			return ExecuteReader("GetModulePermissionsByPortal", portalId);
		}

		public virtual IDataReader GetModulePermissionsByTabID(int tabId)
		{
			return ExecuteReader("GetModulePermissionsByTabID", tabId);
		}

		public virtual void UpdateModulePermission(int modulePermissionId, int moduleId, int portalId, int permissionId,
												   int roleId,
													bool allowAccess, int userId, int lastModifiedByUserId)
		{
			ExecuteNonQuery("UpdateModulePermission",
									  modulePermissionId,
									  moduleId,
									  portalId,
									  permissionId,
									  GetRoleNull(roleId),
									  allowAccess,
									  GetNull(userId),
									  lastModifiedByUserId);
		}

		#endregion

		#region TabPermission Methods

		public virtual int AddTabPermission(int tabId, int permissionId, int roleID, bool allowAccess, int userId,
											int createdByUserID)
		{
			return ExecuteScalar<int>("AddTabPermission",
											tabId,
											permissionId,
											GetRoleNull(roleID),
											allowAccess,
											GetNull(userId),
											createdByUserID);
		}

		public virtual void DeleteTabPermission(int tabPermissionId)
		{
			ExecuteNonQuery("DeleteTabPermission", tabPermissionId);
		}

		public virtual void DeleteTabPermissionsByTabID(int tabId)
		{
			ExecuteNonQuery("DeleteTabPermissionsByTabID", tabId);
		}

		public virtual void DeleteTabPermissionsByUserID(int portalId, int userId)
		{
			ExecuteNonQuery("DeleteTabPermissionsByUserID", portalId, userId);
		}

		public virtual IDataReader GetTabPermissionsByPortal(int portalId)
		{
			return ExecuteReader("GetTabPermissionsByPortal", GetNull(portalId));
		}

		public virtual IDataReader GetTabPermissionsByTabID(int tabId, int permissionId)
		{
			return ExecuteReader("GetTabPermissionsByTabID", tabId, permissionId);
		}

		public virtual void UpdateTabPermission(int tabPermissionId, int tabId, int permissionId, int roleID,
												bool allowAccess,
													int userId, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateTabPermission",
									  tabPermissionId,
									  tabId,
									  permissionId,
									  GetRoleNull(roleID),
									  allowAccess,
									  GetNull(userId),
									  lastModifiedByUserID);
		}

		#endregion

		#region FolderPermission Methods

		public virtual int AddFolderPermission(int folderId, int permissionId, int roleID, bool allowAccess, int userId,
											   int createdByUserID)
		{
			return ExecuteScalar<int>("AddFolderPermission",
											folderId,
											permissionId,
											GetRoleNull(roleID),
											allowAccess,
											GetNull(userId),
											createdByUserID);
		}

		public virtual void DeleteFolderPermission(int folderPermissionId)
		{
			ExecuteNonQuery("DeleteFolderPermission", folderPermissionId);
		}

		public virtual void DeleteFolderPermissionsByFolderPath(int portalId, string folderPath)
		{
			ExecuteNonQuery("DeleteFolderPermissionsByFolderPath", GetNull(portalId), folderPath);
		}

		public virtual void DeleteFolderPermissionsByUserID(int portalId, int userId)
		{
			ExecuteNonQuery("DeleteFolderPermissionsByUserID", portalId, userId);
		}

		public virtual IDataReader GetFolderPermission(int folderPermissionId)
		{
			return ExecuteReader("GetFolderPermission", folderPermissionId);
		}

		public virtual IDataReader GetFolderPermissionsByFolderPath(int portalId, string folderPath, int permissionId)
		{
			return ExecuteReader("GetFolderPermissionsByFolderPath", GetNull(portalId), folderPath, permissionId);
		}

		public virtual IDataReader GetFolderPermissionsByPortal(int portalId)
		{
			return ExecuteReader("GetFolderPermissionsByPortal", GetNull(portalId));
		}

        public virtual IDataReader GetFolderPermissionsByPortalAndPath(int portalId, string pathName)
        {
            return ExecuteReader("GetFolderPermissionsByPortalAndPath", GetNull(portalId), GetNull(pathName) ?? "");
        }

        public virtual void UpdateFolderPermission(int FolderPermissionID, int FolderID, int PermissionID, int roleID,
												   bool AllowAccess, int UserID, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateFolderPermission",
									  FolderPermissionID,
									  FolderID,
									  PermissionID,
									  GetRoleNull(roleID),
									  AllowAccess,
									  GetNull(UserID),
									  lastModifiedByUserID);
		}

        #endregion

        #region DesktopModulePermission Methods

        public virtual int AddDesktopModulePermission(int portalDesktopModuleID, int permissionID, int roleID,
											bool allowAccess, int userID, int createdByUserID)
		{
			return ExecuteScalar<int>("AddDesktopModulePermission",
														portalDesktopModuleID,
														permissionID,
														GetRoleNull(roleID),
														allowAccess,
														GetNull(userID),
														createdByUserID);
		}

		public virtual void DeleteDesktopModulePermission(int desktopModulePermissionID)
		{
			ExecuteNonQuery("DeleteDesktopModulePermission", desktopModulePermissionID);
		}

		public virtual void DeleteDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
		{
			ExecuteNonQuery("DeleteDesktopModulePermissionsByPortalDesktopModuleID", portalDesktopModuleID);
		}

		public virtual void DeleteDesktopModulePermissionsByUserID(int userID, int portalID)
		{
			ExecuteNonQuery("DeleteDesktopModulePermissionsByUserID", userID, portalID);
		}

		public virtual IDataReader GetDesktopModulePermission(int desktopModulePermissionID)
		{
			return ExecuteReader("GetDesktopModulePermission", desktopModulePermissionID);
		}

		public virtual IDataReader GetDesktopModulePermissions()
		{
			return ExecuteReader("GetDesktopModulePermissions");
		}

		public virtual IDataReader GetDesktopModulePermissionsByPortalDesktopModuleID(int portalDesktopModuleID)
		{
			return ExecuteReader("GetDesktopModulePermissionsByPortalDesktopModuleID", portalDesktopModuleID);
		}

		public virtual void UpdateDesktopModulePermission(int desktopModulePermissionID, int portalDesktopModuleID,
											int permissionID, int roleID, bool allowAccess, int userID,
											int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateDesktopModulePermission",
									  desktopModulePermissionID,
									  portalDesktopModuleID,
									  permissionID,
									  GetRoleNull(roleID),
									  allowAccess,
									  GetNull(userID),
									  lastModifiedByUserID);
		}

		#endregion

		#region RoleGroup Methods

		public virtual int AddRoleGroup(int portalId, string groupName, string description, int createdByUserID)
		{
			return ExecuteScalar<int>("AddRoleGroup", portalId, groupName, description, createdByUserID);
		}

		public virtual void DeleteRoleGroup(int roleGroupId)
		{
			ExecuteNonQuery("DeleteRoleGroup", roleGroupId);
		}

		public virtual IDataReader GetRoleGroup(int portalId, int roleGroupId)
		{
			return ExecuteReader("GetRoleGroup", portalId, roleGroupId);
		}

		public virtual IDataReader GetRoleGroupByName(int portalID, string roleGroupName)
		{
			return ExecuteReader("GetRoleGroupByName", portalID, roleGroupName);
		}

		public virtual IDataReader GetRoleGroups(int portalId)
		{
			return ExecuteReader("GetRoleGroups", portalId);
		}

		public virtual void UpdateRoleGroup(int roleGroupId, string groupName, string description,
											int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateRoleGroup", roleGroupId, groupName, description, lastModifiedByUserID);
		}

		#endregion

		#region Role Methods

		public virtual int AddRole(int portalId, int roleGroupId, string roleName, string description, float serviceFee,
								   string billingPeriod, string billingFrequency, float trialFee, int trialPeriod,
								   string trialFrequency, bool isPublic, bool autoAssignment, string rsvpCode,
								   string iconFile, int createdByUserID, int status, int securityMode, bool isSystemRole)
		{
			return ExecuteScalar<int>("AddRole",
											portalId,
											GetNull(roleGroupId),
											roleName,
											description,
											serviceFee,
											billingPeriod,
											GetNull(billingFrequency),
											trialFee,
											trialPeriod,
											GetNull(trialFrequency),
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
			ExecuteNonQuery("DeleteRole", roleId);
		}

		public virtual IDataReader GetPortalRoles(int portalId)
		{
			return ExecuteReader("GetPortalRoles", portalId);
		}

		public virtual IDataReader GetRoles()
		{
			return ExecuteReader("GetRoles");
		}

		public virtual IDataReader GetRolesBasicSearch(int portalID, int pageIndex, int pageSize, string filterBy)
		{
			return ExecuteReader("GetRolesBasicSearch", portalID, pageIndex, pageSize, filterBy);
		}

		public virtual IDataReader GetRoleSettings(int roleId)
		{
			return ExecuteReader("GetRoleSettings", roleId);
		}

		public virtual void UpdateRole(int roleId, int roleGroupId, string roleName, string description,
									   float serviceFee, string billingPeriod, string billingFrequency, float trialFee,
									   int trialPeriod,
									   string trialFrequency, bool isPublic, bool autoAssignment, string rsvpCode,
									   string iconFile, int lastModifiedByUserID, int status, int securityMode,
									   bool isSystemRole)
		{
			ExecuteNonQuery("UpdateRole",
									  roleId,
									  GetNull(roleGroupId),
									  roleName,
									  description,
									  serviceFee,
									  billingPeriod,
									  GetNull(billingFrequency),
									  trialFee,
									  trialPeriod,
									  GetNull(trialFrequency),
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
			ExecuteNonQuery("UpdateRoleSetting",
									  roleId,
									  settingName,
									  settingValue,
									  lastModifiedByUserID);
		}

		#endregion

		#region User Methods

		public virtual int AddUser(int portalID, string username, string firstName, string lastName, int affiliateId,
									bool isSuperUser, string email, string displayName, bool updatePassword,
									bool isApproved, int createdByUserID)
		{
			return ExecuteScalar<int>("AddUser",
														portalID,
														username,
														firstName,
														lastName,
														GetNull(affiliateId),
														isSuperUser,
														email,
														displayName,
														updatePassword,
														isApproved,
														createdByUserID);
		}

		public virtual void AddUserPortal(int portalId, int userId)
		{
			ExecuteNonQuery("AddUserPortal", portalId, userId);
		}

		public virtual void ChangeUsername(int userId, string newUsername)
		{
			ExecuteNonQuery("ChangeUsername", userId, newUsername);
		}

		public virtual void DeleteUserFromPortal(int userId, int portalId)
		{
			ExecuteNonQuery("DeleteUserPortal", userId, GetNull(portalId));
		}

		public virtual IDataReader GetAllUsers(int portalID, int pageIndex, int pageSize, bool includeDeleted,
											   bool superUsersOnly)
		{
			return ExecuteReader("GetAllUsers", GetNull(portalID), pageIndex, pageSize, includeDeleted, superUsersOnly);
		}

		public virtual IDataReader GetDeletedUsers(int portalId)
		{
			return ExecuteReader("GetDeletedUsers", GetNull(portalId));
		}

		public virtual IDataReader GetUnAuthorizedUsers(int portalId, bool includeDeleted, bool superUsersOnly)
		{
			return ExecuteReader("GetUnAuthorizedUsers", GetNull(portalId), includeDeleted, superUsersOnly);
		}

		public virtual IDataReader GetUser(int portalId, int userId)
		{
			return ExecuteReader("GetUser", portalId, userId);
		}

		public virtual IDataReader GetUserByAuthToken(int portalID, string userToken, string authType)
		{
			return ExecuteReader("GetUserByAuthToken", portalID, userToken, authType);
		}

		public virtual IDataReader GetUserByDisplayName(int portalId, string displayName)
		{
			return ExecuteReader("GetUserByDisplayName", GetNull(portalId), displayName);
		}

		public virtual IDataReader GetUserByUsername(int portalId, string username)
		{
			return ExecuteReader("GetUserByUsername", GetNull(portalId), username);
		}

		public virtual IDataReader GetUserByUsername(string username, string spaceReplacement)
		{
			return ExecuteReader("GetUserByUsernameForUrl", username, spaceReplacement);
		}

		public virtual IDataReader GetUserByVanityUrl(int portalId, string vanityUrl)
		{
			return ExecuteReader("GetUserByVanityUrl", GetNull(portalId), vanityUrl);
		}

        public virtual IDataReader GetUserByPasswordResetToken(int portalId, string resetToken)
        {
            return ExecuteReader("GetUserByPasswordResetToken", GetNull(portalId), resetToken);
        }

        public virtual IDataReader GetDisplayNameForUser(int userId, string spaceReplacement)
		{
			return ExecuteReader("GetDisplayNameForUser", userId, spaceReplacement);
		}

		public virtual int GetUserCountByPortal(int portalId)
		{
			return ExecuteScalar<int>("GetUserCountByPortal", portalId);
		}

		public virtual IDataReader GetUsersAdvancedSearch(int portalId, int userId, int filterUserId, int fitlerRoleId,
														  int relationTypeId,
														  bool isAdmin, int pageIndex, int pageSize, string sortColumn,
														  bool sortAscending,
															string propertyNames, string propertyValues)
		{
			var ps = new PortalSecurity();
			string filterSort = ps.InputFilter(sortColumn, PortalSecurity.FilterFlag.NoSQL);
			string filterName = ps.InputFilter(propertyNames, PortalSecurity.FilterFlag.NoSQL);
			string filterValue = ps.InputFilter(propertyValues, PortalSecurity.FilterFlag.NoSQL);
			return ExecuteReader("GetUsersAdvancedSearch", portalId, userId, filterUserId, fitlerRoleId, relationTypeId,
								 isAdmin, pageSize, pageIndex, filterSort, sortAscending, filterName, filterValue);
		}

		public virtual IDataReader GetUsersBasicSearch(int portalId, int pageIndex, int pageSize, string sortColumn,
									bool sortAscending, string propertyName, string propertyValue)
		{
			var ps = new PortalSecurity();
			string filterSort = ps.InputFilter(sortColumn, PortalSecurity.FilterFlag.NoSQL);
			string filterName = ps.InputFilter(propertyName, PortalSecurity.FilterFlag.NoSQL);
			string filterValue = ps.InputFilter(propertyValue, PortalSecurity.FilterFlag.NoSQL);
			return ExecuteReader("GetUsersBasicSearch", portalId, pageSize, pageIndex, filterSort, sortAscending,
								 filterName, filterValue);
		}

		public virtual IDataReader GetUsersByEmail(int portalID, string email, int pageIndex, int pageSize,
												   bool includeDeleted, bool superUsersOnly)
		{
			return ExecuteReader("GetUsersByEmail", GetNull(portalID), email, pageIndex, pageSize, includeDeleted,
								 superUsersOnly);
		}

		public virtual IDataReader GetUsersByProfileProperty(int portalID, string propertyName, string propertyValue,
															 int pageIndex, int pageSize, bool includeDeleted,
															 bool superUsersOnly)
		{
			return ExecuteReader("GetUsersByProfileProperty", GetNull(portalID), propertyName, propertyValue, pageIndex,
								 pageSize, includeDeleted, superUsersOnly);
		}

		public virtual IDataReader GetUsersByRolename(int portalID, string rolename)
		{
			return ExecuteReader("GetUsersByRolename", GetNull(portalID), rolename);
		}

		public virtual IDataReader GetUsersByUsername(int portalID, string username, int pageIndex, int pageSize,
													  bool includeDeleted, bool superUsersOnly)
		{
			return ExecuteReader("GetUsersByUsername", GetNull(portalID), username, pageIndex, pageSize, includeDeleted,
								 superUsersOnly);
		}

		public virtual IDataReader GetUsersByDisplayname(int portalId, string name, int pageIndex, int pageSize,
													  bool includeDeleted, bool superUsersOnly)
		{
			return ExecuteReader("GetUsersByDisplayname", GetNull(portalId), name, pageIndex, pageSize, includeDeleted,
								 superUsersOnly);
		}

        public virtual int GetDuplicateEmailCount(int portalId)
        {
            return ExecuteScalar<int>("GetDuplicateEmailCount", portalId);
        }

        public virtual int GetSingleUserByEmail(int portalId, string emailToMatch)
        {
            return ExecuteScalar<int>("GetSingleUserByEmail", portalId, emailToMatch);
        }

		public virtual void RemoveUser(int userId, int portalId)
		{
			ExecuteNonQuery("RemoveUser", userId, GetNull(portalId));
		}

		public virtual void RestoreUser(int userId, int portalId)
		{
			ExecuteNonQuery("RestoreUser", userId, GetNull(portalId));
		}

		public virtual void UpdateUser(int userId, int portalID, string firstName, string lastName, bool isSuperUser,
										string email, string displayName, string vanityUrl, bool updatePassword,
										bool isApproved, bool refreshRoles, string lastIpAddress, Guid passwordResetToken,
                                        DateTime passwordResetExpiration, bool isDeleted, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateUser",
									  userId,
									  GetNull(portalID),
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
									  passwordResetToken,
									  GetNull(passwordResetExpiration),
									  isDeleted,
									  lastModifiedByUserID);
		}

	    public virtual void UpdateUserLastIpAddress(int userId, string lastIpAddress)
	    {
	        ExecuteNonQuery("UpdateUserLastIpAddress", userId, lastIpAddress);
	    }

		#endregion

		#region UserRole Methods

		public virtual int AddUserRole(int portalId, int userId, int roleId, int status, bool isOwner,
									   DateTime effectiveDate, DateTime expiryDate, int createdByUserID)
		{
			return ExecuteScalar<int>("AddUserRole", portalId, userId, roleId, status, isOwner, GetNull(effectiveDate),
									  GetNull(expiryDate), createdByUserID);
		}

		public virtual void DeleteUserRole(int userId, int roleId)
		{
			ExecuteNonQuery("DeleteUserRole", userId, roleId);
		}

		public virtual IDataReader GetServices(int portalId, int userId)
		{
			return ExecuteReader("GetServices", portalId, GetNull(userId));
		}

		public virtual IDataReader GetUserRole(int portalID, int userId, int roleId)
		{
			return ExecuteReader("GetUserRole", portalID, userId, roleId);
		}

		public virtual IDataReader GetUserRoles(int portalID, int userId)
		{
			return ExecuteReader("GetUserRoles", portalID, userId);
		}

		public virtual IDataReader GetUserRolesByUsername(int portalID, string username, string rolename)
		{
			return ExecuteReader("GetUserRolesByUsername", portalID, GetNull(username), GetNull(rolename));
		}

		public virtual void UpdateUserRole(int userRoleId, int status, bool isOwner, DateTime effectiveDate,
										   DateTime expiryDate, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateUserRole", userRoleId, status, isOwner, GetNull(effectiveDate), GetNull(expiryDate),
							lastModifiedByUserID);
		}

		#endregion

		#region Users Online Methods

		public virtual void DeleteUsersOnline(int timeWindow)
		{
			ExecuteNonQuery("DeleteUsersOnline", timeWindow);
		}

		public virtual IDataReader GetOnlineUser(int userId)
		{
			return ExecuteReader("GetOnlineUser", userId);
		}

		public virtual IDataReader GetOnlineUsers(int portalId)
		{
			return ExecuteReader("GetOnlineUsers", portalId);
		}

		public virtual void UpdateUsersOnline(Hashtable userList)
		{
			if ((userList.Count == 0))
			{
				//No users to process, quit method
				return;
			}
			foreach (string key in userList.Keys)
			{
			    var info = userList[key] as AnonymousUserInfo;
			    if (info != null)
				{
					var user = info;
					ExecuteNonQuery("UpdateAnonymousUser", user.UserID, user.PortalID, user.TabID, user.LastActiveDate);
				}
				else if (userList[key] is OnlineUserInfo)
				{
					var user = (OnlineUserInfo)userList[key];
					ExecuteNonQuery("UpdateOnlineUser", user.UserID, user.PortalID, user.TabID, user.LastActiveDate);
				}
			}
		}

		#endregion

		#region UserProfile Methods

		public virtual int AddPropertyDefinition(int portalId, int moduleDefId, int dataType, string defaultValue,
													string propertyCategory, string propertyName, bool readOnly,
													bool required, string validationExpression, int viewOrder,
													bool visible, int length, int defaultVisibility, int createdByUserId)
		{
			int retValue;
			try
			{
				retValue = ExecuteScalar<int>("AddPropertyDefinition",
															GetNull(portalId),
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

				//If not a duplicate (throw an Exception)
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
			ExecuteNonQuery("DeletePropertyDefinition", definitionId);
		}

		public virtual IDataReader GetPropertyDefinition(int definitionId)
		{
			return ExecuteReader("GetPropertyDefinition", definitionId);
		}

		public virtual IDataReader GetPropertyDefinitionByName(int portalId, string name)
		{
			return ExecuteReader("GetPropertyDefinitionByName", GetNull(portalId), name);
		}

		public virtual IDataReader GetPropertyDefinitionsByPortal(int portalId)
		{
			return ExecuteReader("GetPropertyDefinitionsByPortal", GetNull(portalId));
		}

		public virtual IDataReader GetUserProfile(int userId)
		{
			return ExecuteReader("GetUserProfile", userId);
		}

		public virtual void UpdateProfileProperty(int profileId, int userId, int propertyDefinitionID,
												  string propertyValue,
													int visibility, string extendedVisibility, DateTime lastUpdatedDate)
		{
			ExecuteNonQuery("UpdateUserProfileProperty", GetNull(profileId), userId, propertyDefinitionID, propertyValue,
													visibility, extendedVisibility, lastUpdatedDate);
		}

		public virtual void UpdatePropertyDefinition(int propertyDefinitionId, int dataType, string defaultValue,
														string propertyCategory, string propertyName, bool readOnly,
														bool required, string validation, int viewOrder, bool visible,
														int length, int defaultVisibility, int lastModifiedByUserId)
		{
			ExecuteNonQuery("UpdatePropertyDefinition",
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
            return ExecuteReader("SearchProfilePropertyValues", portalId, propertyName, searchString);
	    }

	    #endregion

		#region SkinControls

		public virtual int AddSkinControl(int packageID, string ControlKey, string ControlSrc,
										  bool SupportsPartialRendering,
											int CreatedByUserID)
		{
			return ExecuteScalar<int>("AddSkinControl",
											GetNull(packageID),
											GetNull(ControlKey),
											ControlSrc,
											SupportsPartialRendering,
											CreatedByUserID);
		}

		public virtual void DeleteSkinControl(int skinControlID)
		{
			ExecuteNonQuery("DeleteSkinControl", skinControlID);
		}

		public virtual IDataReader GetSkinControls()
		{
			return ExecuteReader("GetSkinControls");
		}

		public virtual IDataReader GetSkinControl(int skinControlID)
		{
			return ExecuteReader("GetSkinControl", skinControlID);
		}

		public virtual IDataReader GetSkinControlByKey(string controlKey)
		{
			return ExecuteReader("GetSkinControlByKey", controlKey);
		}

		public virtual IDataReader GetSkinControlByPackageID(int packageID)
		{
			return ExecuteReader("GetSkinControlByPackageID", packageID);
		}

		public virtual void UpdateSkinControl(int skinControlID, int packageID, string ControlKey, string ControlSrc,
												bool SupportsPartialRendering, int LastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateSkinControl",
									  skinControlID,
									  GetNull(packageID),
									  GetNull(ControlKey),
									  ControlSrc,
									  SupportsPartialRendering,
									  LastModifiedByUserID);
		}

		#endregion

		#region Skins/Containers

		public virtual int AddSkin(int skinPackageID, string skinSrc)
		{
			return ExecuteScalar<int>("AddSkin", skinPackageID, skinSrc);
		}

		public virtual int AddSkinPackage(int packageID, int portalID, string skinName, string skinType,
										  int createdByUserId)
		{
			return ExecuteScalar<int>("AddSkinPackage", packageID, GetNull(portalID), skinName, skinType,
									  createdByUserId);
		}

		public virtual bool CanDeleteSkin(string skinType, string skinFoldername)
		{
			return ExecuteScalar<int>("CanDeleteSkin", skinType, skinFoldername) == 1;
		}

		public virtual void DeleteSkin(int skinID)
		{
			ExecuteNonQuery("DeleteSkin", skinID);
		}

		public virtual void DeleteSkinPackage(int skinPackageID)
		{
			ExecuteNonQuery("DeleteSkinPackage", skinPackageID);
		}

		public virtual IDataReader GetSkinByPackageID(int packageID)
		{
			return ExecuteReader("GetSkinPackageByPackageID", packageID);
		}

		public virtual IDataReader GetSkinPackage(int portalID, string skinName, string skinType)
		{
			return ExecuteReader("GetSkinPackage", GetNull(portalID), skinName, skinType);
		}

		public virtual void UpdateSkin(int skinID, string skinSrc)
		{
			ExecuteNonQuery("UpdateSkin", skinID, skinSrc);
		}

		public virtual void UpdateSkinPackage(int skinPackageID, int packageID, int portalID, string skinName,
											  string skinType, int LastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateSkinPackage", skinPackageID, packageID, GetNull(portalID), skinName, skinType,
							LastModifiedByUserID);
		}

		#endregion

		#region Personalization

		public virtual void AddProfile(int userId, int portalId)
		{
			ExecuteNonQuery("AddProfile", userId, portalId);
		}

		public virtual IDataReader GetAllProfiles()
		{
			return ExecuteReader("GetAllProfiles");
		}

		public virtual IDataReader GetProfile(int userId, int portalId)
		{
			return ExecuteReader("GetProfile", userId, portalId);
		}

		public virtual void UpdateProfile(int userId, int portalId, string profileData)
		{
			ExecuteNonQuery("UpdateProfile", userId, portalId, profileData);
		}

		#endregion

		#region Urls

		public virtual void AddUrl(int PortalID, string Url)
		{
			ExecuteNonQuery("AddUrl", PortalID, Url);
		}

		public virtual void AddUrlLog(int UrlTrackingID, int UserID)
		{
			ExecuteNonQuery("AddUrlLog", UrlTrackingID, GetNull(UserID));
		}

		public virtual void AddUrlTracking(int PortalID, string Url, string UrlType, bool LogActivity, bool TrackClicks,
										   int ModuleID, bool NewWindow)
		{
			ExecuteNonQuery("AddUrlTracking", PortalID, Url, UrlType, LogActivity, TrackClicks, GetNull(ModuleID),
							NewWindow);
		}

		public virtual void DeleteUrl(int PortalID, string Url)
		{
			ExecuteNonQuery("DeleteUrl", PortalID, Url);
		}

		public virtual void DeleteUrlTracking(int PortalID, string Url, int ModuleID)
		{
			ExecuteNonQuery("DeleteUrlTracking", PortalID, Url, GetNull(ModuleID));
		}

		public virtual IDataReader GetUrl(int PortalID, string Url)
		{
			return ExecuteReader("GetUrl", PortalID, Url);
		}

		public virtual IDataReader GetUrlLog(int UrlTrackingID, DateTime StartDate, DateTime EndDate)
		{
			return ExecuteReader("GetUrlLog", UrlTrackingID, GetNull(StartDate), GetNull(EndDate));
		}

		public virtual IDataReader GetUrls(int PortalID)
		{
			return ExecuteReader("GetUrls", PortalID);
		}

		public virtual IDataReader GetUrlTracking(int PortalID, string Url, int ModuleID)
		{
			return ExecuteReader("GetUrlTracking", PortalID, Url, GetNull(ModuleID));
		}

		public virtual void UpdateUrlTracking(int PortalID, string Url, bool LogActivity, bool TrackClicks, int ModuleID,
											  bool NewWindow)
		{
			ExecuteNonQuery("UpdateUrlTracking", PortalID, Url, LogActivity, TrackClicks, GetNull(ModuleID), NewWindow);
		}

		public virtual void UpdateUrlTrackingStats(int PortalID, string Url, int ModuleID)
		{
			ExecuteNonQuery("UpdateUrlTrackingStats", PortalID, Url, GetNull(ModuleID));
		}

		#endregion

		#region Search

		public virtual IDataReader GetDefaultLanguageByModule(string ModuleList)
		{
			return ExecuteReader("GetDefaultLanguageByModule", ModuleList);
		}

		public virtual IDataReader GetSearchCommonWordsByLocale(string Locale)
		{
			return ExecuteReader("GetSearchCommonWordsByLocale", Locale);
		}

		public virtual IDataReader GetSearchIndexers()
		{
			return ExecuteReader("GetSearchIndexers");
		}

		public virtual IDataReader GetSearchResultModules(int PortalID)
		{
			return ExecuteReader("GetSearchResultModules", PortalID);
		}

		public virtual IDataReader GetSearchSettings(int ModuleId)
		{
			return ExecuteReader("GetSearchSettings", ModuleId);
		}

		#endregion

		#region Lists

        public virtual int AddListEntry(string ListName, string Value, string Text, int ParentID, int Level,
										bool EnableSortOrder, int DefinitionID, string Description, int PortalID,
										bool SystemList,
										 int CreatedByUserID)
		{
		    try
		    {
		        return ExecuteScalar<int>("AddListEntry",
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
			ExecuteNonQuery("DeleteList", ListName, ParentKey);
		}

		public virtual void DeleteListEntryByID(int EntryID, bool DeleteChild)
		{
			ExecuteNonQuery("DeleteListEntryByID", EntryID, DeleteChild);
		}

		public virtual void DeleteListEntryByListName(string ListName, string Value, bool DeleteChild)
		{
			ExecuteNonQuery("DeleteListEntryByListName", ListName, Value, DeleteChild);
		}

		public virtual IDataReader GetList(string ListName, string ParentKey, int PortalID)
		{
			return ExecuteReader("GetList", ListName, ParentKey, PortalID);
		}

		public virtual IDataReader GetListEntriesByListName(string ListName, string ParentKey, int PortalID)
		{
			return ExecuteReader("GetListEntries", ListName, ParentKey, GetNull(PortalID));
		}

		public virtual IDataReader GetListEntry(string ListName, string Value)
		{
			return ExecuteReader("GetListEntry", ListName, Value, -1);
		}

		public virtual IDataReader GetListEntry(int EntryID)
		{
			return ExecuteReader("GetListEntry", "", "", EntryID);
		}

		public virtual IDataReader GetLists(int PortalID)
		{
			return ExecuteReader("GetLists", PortalID);
		}

		public virtual void UpdateListEntry(int EntryID, string Value, string Text, string Description,
											int LastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateListEntry", EntryID, Value, Text, Description, LastModifiedByUserID);
		}

		public virtual void UpdateListSortOrder(int EntryID, bool MoveUp)
		{
			ExecuteNonQuery("UpdateListSortOrder", EntryID, MoveUp);
		}

		#endregion

		#region Portal Aliases

		public virtual int AddPortalAlias(int PortalID, string HTTPAlias, string cultureCode, string skin, string browserType, bool isPrimary, int createdByUserID)
		{
			return ExecuteScalar<int>("AddPortalAlias", PortalID, HTTPAlias, GetNull(cultureCode), GetNull(skin), GetNull(browserType), isPrimary, createdByUserID);
		}

		public virtual void DeletePortalAlias(int PortalAliasID)
		{
			ExecuteNonQuery("DeletePortalAlias", PortalAliasID);
		}

		public virtual IDataReader GetPortalAliases()
		{
			return ExecuteReader("GetPortalAliases");
		}

		public virtual IDataReader GetPortalByPortalAliasID(int PortalAliasId)
		{
			return ExecuteReader("GetPortalByPortalAliasID", PortalAliasId);
		}

		public virtual void UpdatePortalAlias(string PortalAlias, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdatePortalAliasOnInstall", PortalAlias, lastModifiedByUserID);
		}

		public virtual void UpdatePortalAliasInfo(int PortalAliasID, int PortalID, string HTTPAlias, string cultureCode, string skin, string browserType, bool isPrimary, int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdatePortalAlias", PortalAliasID, PortalID, HTTPAlias, GetNull(cultureCode), GetNull(skin), GetNull(browserType), isPrimary, lastModifiedByUserID);
		}

		#endregion

		#region Event Queue

		public virtual int AddEventMessage(string eventName, int priority, string processorType, string processorCommand,
										   string body, string sender, string subscriberId, string authorizedRoles,
										   string exceptionMessage, DateTime sentDate, DateTime expirationDate,
										   string attributes)
		{
			return ExecuteScalar<int>("AddEventMessage",
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
			return ExecuteReader("GetEventMessages", eventName);
		}

		public virtual IDataReader GetEventMessagesBySubscriber(string eventName, string subscriberId)
		{
			return ExecuteReader("GetEventMessagesBySubscriber", eventName, subscriberId);
		}

		public virtual void SetEventMessageComplete(int eventMessageId)
		{
			ExecuteNonQuery("SetEventMessageComplete", eventMessageId);
		}

		#endregion

		#region Authentication

		public virtual int AddAuthentication(int packageID, string authenticationType, bool isEnabled,
											 string settingsControlSrc, string loginControlSrc, string logoffControlSrc,
											 int CreatedByUserID)
		{
			return ExecuteScalar<int>("AddAuthentication",
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
			return ExecuteScalar<int>("AddUserAuthentication", userID, authenticationType, authenticationToken,
									  CreatedByUserID);
		}

		/// <summary>
		/// Get a User Authentication record from slq database. DNN-4016
		/// </summary>
		/// <param name="userID"></param>
		/// <returns>UserAuthentication record</returns>
		public virtual IDataReader GetUserAuthentication(int userID)
		{
			return ExecuteReader("GetUserAuthentication", userID);
		}

		public virtual void DeleteAuthentication(int authenticationID)
		{
			ExecuteNonQuery("DeleteAuthentication", authenticationID);
		}

		public virtual IDataReader GetAuthenticationService(int authenticationID)
		{
			return ExecuteReader("GetAuthenticationService", authenticationID);
		}

		public virtual IDataReader GetAuthenticationServiceByPackageID(int packageID)
		{
			return ExecuteReader("GetAuthenticationServiceByPackageID", packageID);
		}

		public virtual IDataReader GetAuthenticationServiceByType(string authenticationType)
		{
			return ExecuteReader("GetAuthenticationServiceByType", authenticationType);
		}

		public virtual IDataReader GetAuthenticationServices()
		{
			return ExecuteReader("GetAuthenticationServices");
		}

		public virtual IDataReader GetEnabledAuthenticationServices()
		{
			return ExecuteReader("GetEnabledAuthenticationServices");
		}

		public virtual void UpdateAuthentication(int authenticationID, int packageID, string authenticationType,
												 bool isEnabled, string settingsControlSrc, string loginControlSrc,
												  string logoffControlSrc, int LastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateAuthentication",
									  authenticationID,
									  packageID,
									  authenticationType,
									  isEnabled,
									  settingsControlSrc,
									  loginControlSrc,
									  logoffControlSrc,
									  LastModifiedByUserID);
		}

		#endregion

		#region Packages

		public virtual int AddPackage(int portalID, string name, string friendlyName, string description, string type,
									  string version, string license, string manifest, string owner,
									  string organization, string url, string email, string releaseNotes,
									  bool isSystemPackage, int createdByUserID, string folderName, string iconFile)
		{
			return ExecuteScalar<int>("AddPackage",
										GetNull(portalID),
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
			ExecuteNonQuery("DeletePackage", packageID);
		}

		public virtual IDataReader GetModulePackagesInUse(int portalID, bool forHost)
		{
			return ExecuteReader("GetModulePackagesInUse", portalID, forHost);
		}

		public virtual IDataReader GetPackageDependencies()
		{
			return ExecuteReader("GetPackageDependencies");
		}


		public virtual IDataReader GetPackages(int portalID)
		{
			return ExecuteReader("GetPackages", GetNull(portalID));
		}

		public virtual IDataReader GetPackageTypes()
		{
			return ExecuteReader("GetPackageTypes");
		}

		public virtual int RegisterAssembly(int packageID, string assemblyName, string version)
		{
			return ExecuteScalar<int>("RegisterAssembly", GetNull(packageID), assemblyName, version);
		}

		public virtual int SavePackageDependency(int packageDependencyId, int packageId, string packageName, string version)
		{
			return ExecuteScalar<int>("SavePackageDependency", packageDependencyId, packageId, packageName, version);
		}

		public virtual bool UnRegisterAssembly(int packageID, string assemblyName)
		{
			return ExecuteScalar<int>("UnRegisterAssembly", packageID, assemblyName) == 1;
		}

		public virtual void UpdatePackage(int packageID, int portalID, string friendlyName, string description,
										  string type, string version, string license, string manifest, string owner,
										  string organization, string url, string email, string releaseNotes,
										  bool isSystemPackage, int lastModifiedByUserID, string folderName,
										  string iconFile)
		{
			ExecuteNonQuery("UpdatePackage",
									  packageID,
									  GetNull(portalID),
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

		#endregion

		#region Languages/Localization

		public virtual int AddLanguage(string cultureCode, string cultureName, string fallbackCulture,
									   int CreatedByUserID)
		{
			return ExecuteScalar<int>("AddLanguage", cultureCode, cultureName, fallbackCulture, CreatedByUserID);
		}

		public virtual int AddLanguagePack(int packageID, int languageID, int dependentPackageID, int CreatedByUserID)
		{
			return ExecuteScalar<int>("AddLanguagePack", packageID, languageID, dependentPackageID, CreatedByUserID);
		}

		public virtual int AddPortalLanguage(int portalID, int languageID, bool IsPublished, int CreatedByUserID)
		{
			return ExecuteScalar<int>("AddPortalLanguage", portalID, languageID, IsPublished, CreatedByUserID);
		}

		public virtual void DeleteLanguage(int languageID)
		{
			ExecuteNonQuery("DeleteLanguage", languageID);
		}

		public virtual void DeleteLanguagePack(int languagePackID)
		{
			ExecuteNonQuery("DeleteLanguagePack", languagePackID);
		}

		public virtual void DeletePortalLanguages(int portalID, int languageID)
		{
			ExecuteNonQuery("DeletePortalLanguages", GetNull(portalID), GetNull(languageID));
		}

		public virtual void EnsureLocalizationExists(int portalID, string CultureCode)
		{
			ExecuteNonQuery("EnsureLocalizationExists", portalID, CultureCode);
		}

		public virtual void RemovePortalLocalization(int portalID, string CultureCode)
		{
			ExecuteNonQuery("RemovePortalLocalization", portalID, CultureCode);
		}

		public virtual IDataReader GetPortalLocalizations(int portalID)
		{
            return ExecuteReader("GetPortalLocalizations", portalID);
		}

		public virtual IDataReader GetLanguages()
		{
			return ExecuteReader("GetLanguages");
		}

		public virtual IDataReader GetLanguagePackByPackage(int packageID)
		{
			return ExecuteReader("GetLanguagePackByPackage", packageID);
		}

		public virtual IDataReader GetLanguagesByPortal(int portalID)
		{
			return ExecuteReader("GetLanguagesByPortal", portalID);
		}

		public virtual string GetPortalDefaultLanguage(int portalID)
		{
			return ExecuteScalar<string>("GetPortalDefaultLanguage", portalID);
		}

		public virtual void UpdateLanguage(int languageID, string cultureCode, string cultureName,
										   string fallbackCulture, int LastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateLanguage", languageID, cultureCode, cultureName, fallbackCulture,
							LastModifiedByUserID);
		}

		public virtual int UpdateLanguagePack(int languagePackID, int packageID, int languageID, int dependentPackageID,
											  int LastModifiedByUserID)
		{
			return ExecuteScalar<int>("UpdateLanguagePack", languagePackID, packageID, languageID, dependentPackageID,
									  LastModifiedByUserID);
		}

		public virtual void UpdatePortalDefaultLanguage(int portalID, string CultureCode)
		{
			ExecuteNonQuery("UpdatePortalDefaultLanguage", portalID, CultureCode);
		}

		public virtual void UpdatePortalLanguage(int portalID, int languageID, bool IsPublished, int UpdatedByUserID)
		{
			ExecuteNonQuery("UpdatePortalLanguage", portalID, languageID, IsPublished, UpdatedByUserID);
		}

		#endregion

		#region Folder Mappings

		public virtual void AddDefaultFolderTypes(int portalID)
		{
			ExecuteNonQuery("AddDefaultFolderTypes", portalID);
		}

		public virtual int AddFolderMapping(int portalID, string mappingName, string folderProviderType,
											int createdByUserID)
		{
			return ExecuteScalar<int>("AddFolderMapping", GetNull(portalID), mappingName, folderProviderType,
									  createdByUserID);
		}

		public virtual void AddFolderMappingSetting(int folderMappingID, string settingName, string settingValue,
													int createdByUserID)
		{
			ExecuteNonQuery("AddFolderMappingsSetting", folderMappingID, settingName, settingValue, createdByUserID);
		}

		public virtual void DeleteFolderMapping(int folderMappingID)
		{
			ExecuteNonQuery("DeleteFolderMapping", folderMappingID);
		}

		public virtual IDataReader GetFolderMapping(int folderMappingID)
		{
			return ExecuteReader("GetFolderMapping", folderMappingID);
		}

		public virtual IDataReader GetFolderMappingByMappingName(int portalID, string mappingName)
		{
			return ExecuteReader("GetFolderMappingByMappingName", portalID, mappingName);
		}

		public virtual IDataReader GetFolderMappings(int portalID)
		{
			return ExecuteReader("GetFolderMappings", GetNull(portalID));
		}

		public virtual IDataReader GetFolderMappingSetting(int folderMappingID, string settingName)
		{
			return ExecuteReader("GetFolderMappingsSetting", folderMappingID, settingName);
		}

		public virtual IDataReader GetFolderMappingSettings(int folderMappingID)
		{
			return ExecuteReader("GetFolderMappingsSettings", folderMappingID);
		}

		public virtual void UpdateFolderMapping(int folderMappingID, string mappingName, int priority,
												int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateFolderMapping", folderMappingID, mappingName, priority, lastModifiedByUserID);
		}

		public virtual void UpdateFolderMappingSetting(int folderMappingID, string settingName, string settingValue,
													   int lastModifiedByUserID)
		{
			ExecuteNonQuery("UpdateFolderMappingsSetting", folderMappingID, settingName, settingValue,
							lastModifiedByUserID);
		}

		#endregion

		#region Password History

		public virtual IDataReader GetPasswordHistory(int userId)
		{
			return ExecuteReader("GetPasswordHistory", GetNull(userId));
		}

		public virtual void AddPasswordHistory(int userId, string password, string passwordHistory, int retained)
		{
			ExecuteNonQuery("AddPasswordHistory", GetNull(userId), password, passwordHistory, retained, GetNull(userId));
		}

		#endregion

		#region Date/Time Utiliies

		public virtual DateTime GetDatabaseTimeUtc()
		{
			return ExecuteScalar<DateTime>("GetDatabaseTimeUtc");
		}

		public virtual DateTime GetDatabaseTime()
		{
			return ExecuteScalar<DateTime>("GetDatabaseTime");
		}

		#endregion

		#region Mobile Stuff

		public virtual void DeletePreviewProfile(int id)
		{
			ExecuteNonQuery("Mobile_DeletePreviewProfile", id);
		}

		public virtual void DeleteRedirection(int id)
		{
			ExecuteNonQuery("Mobile_DeleteRedirection", id);
		}

		public virtual void DeleteRedirectionRule(int id)
		{
			ExecuteNonQuery("Mobile_DeleteRedirectionRule", id);
		}

		public virtual IDataReader GetAllRedirections()
		{
			return ExecuteReader("Mobile_GetAllRedirections");
		}

		public virtual IDataReader GetPreviewProfiles(int portalId)
		{
			return ExecuteReader("Mobile_GetPreviewProfiles", portalId);
		}

		public virtual IDataReader GetRedirectionRules(int redirectionId)
		{
			return ExecuteReader("Mobile_GetRedirectionRules", redirectionId);
		}

		public virtual IDataReader GetRedirections(int portalId)
		{
			return ExecuteReader("Mobile_GetRedirections", portalId);
		}

		public virtual int SavePreviewProfile(int id, int portalId, string name, int width, int height, string userAgent,
											  int sortOrder, int userId)
		{
			return ExecuteScalar<int>("Mobile_SavePreviewProfile", id, portalId, name, width, height, userAgent,
									  sortOrder, userId);
		}

		public virtual int SaveRedirection(int id, int portalId, string name, int type, int sortOrder, int sourceTabId,
										   bool includeChildTabs, int targetType, object targetValue, bool enabled,
										   int userId)
		{
			return ExecuteScalar<int>("Mobile_SaveRedirection", id, portalId, name, type, sortOrder, sourceTabId,
									  includeChildTabs, targetType, targetValue, enabled, userId);
		}

		public virtual void SaveRedirectionRule(int id, int redirectionId, string capbility, string expression)
		{
			ExecuteNonQuery("Mobile_SaveRedirectionRule", id, redirectionId, capbility, expression);
		}

		#endregion

		#region Logging

		public virtual void AddLog(string logGUID, string logTypeKey, int logUserID, string logUserName, int logPortalID,
								   string logPortalName, DateTime logCreateDate, string logServerName,
							string logProperties, int logConfigID, ExceptionInfo exception, bool notificationActive)
		{
		    if (exception != null)
            {
                if (!string.IsNullOrEmpty(exception.ExceptionHash))
                    ExecuteNonQuery("AddException",
                                    exception.ExceptionHash,
                                    exception.Message,
                                    exception.StackTrace,
                                    exception.InnerMessage,
                                    exception.InnerStackTrace,
                                    exception.Source);


                // DNN-6218 + DNN-6239 + DNN-6242: Due to change in the AddEventLog stored
                // procedure in 7.4.0, we need to try a fallback especially during upgrading
                int logEventID;
                try
                {
                    logEventID = ExecuteScalar<int>("AddEventLog",
                        logGUID,
                        logTypeKey,
                        GetNull(logUserID),
                        GetNull(logUserName),
                        GetNull(logPortalID),
                        GetNull(logPortalName),
                        logCreateDate,
                        logServerName,
                        logProperties,
                        logConfigID,
                        GetNull(exception.ExceptionHash),
                        notificationActive);
                }
                catch (SqlException)
                {
                    var s = ExecuteScalar<string>("AddEventLog",
                        logGUID,
                        logTypeKey,
                        GetNull(logUserID),
                        GetNull(logUserName),
                        GetNull(logPortalID),
                        GetNull(logPortalName),
                        logCreateDate,
                        logServerName,
                        logProperties,
                        logConfigID);

                    // old SPROC wasn't returning anything; trying a workaround
                    if (!int.TryParse(s ?? "-1", out logEventID))
                        logEventID = Null.NullInteger;
                }

                if (!string.IsNullOrEmpty(exception.AssemblyVersion) && exception.AssemblyVersion != "-1")
                {
                    ExecuteNonQuery("AddExceptionEvent",
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
                ExecuteScalar<int>("AddEventLog",
                                                logGUID,
                                                logTypeKey,
                                                GetNull(logUserID),
                                                GetNull(logUserName),
                                                GetNull(logPortalID),
                                                GetNull(logPortalName),
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
			ExecuteNonQuery("AddEventLogType", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeOwner,
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
				logTypeKey = "";
			}
			if (logTypePortalID == "*")
			{
				portalID = -1;
			}
			else
			{
				portalID = Convert.ToInt32(logTypePortalID);
			}
			ExecuteNonQuery("AddEventLogConfig",
									  GetNull(logTypeKey),
									  GetNull(portalID),
									  loggingIsActive,
									  keepMostRecent,
									  emailNotificationIsActive,
									  GetNull(threshold),
									  GetNull(notificationThresholdTime),
									  GetNull(notificationThresholdTimeType),
									  mailFromAddress,
									  mailToAddress);
		}

		public virtual void ClearLog()
		{
			ExecuteNonQuery("DeleteEventLog", DBNull.Value);
		}

		public virtual void DeleteLog(string logGUID)
		{
			ExecuteNonQuery("DeleteEventLog", logGUID);
		}

		public virtual void DeleteLogType(string logTypeKey)
		{
			ExecuteNonQuery("DeleteEventLogType", logTypeKey);
		}

		public virtual void DeleteLogTypeConfigInfo(string id)
		{
			ExecuteNonQuery("DeleteEventLogConfig", id);
		}

		public virtual IDataReader GetEventLogPendingNotif(int logConfigID)
		{
			return ExecuteReader("GetEventLogPendingNotif", logConfigID);
		}

		public virtual IDataReader GetEventLogPendingNotifConfig()
		{
			return ExecuteReader("GetEventLogPendingNotifConfig");
		}

		public virtual IDataReader GetLogs(int portalID, string logType, int pageSize, int pageIndex)
		{
			return ExecuteReader("GetEventLog", GetNull(portalID), GetNull(logType), pageSize, pageIndex);
		}

		public virtual IDataReader GetLogTypeConfigInfo()
		{
			return ExecuteReader("GetEventLogConfig", DBNull.Value);
		}

		public virtual IDataReader GetLogTypeConfigInfoByID(int id)
		{
			return ExecuteReader("GetEventLogConfig", id);
		}

		public virtual IDataReader GetLogTypeInfo()
		{
			return ExecuteReader("GetEventLogType");
		}

		public virtual IDataReader GetSingleLog(string logGUID)
		{
			return ExecuteReader("GetEventLogByLogGUID", logGUID);
		}

		public virtual void PurgeLog()
		{
			//Because event log is run on application end, app may not be fully installed, so check for the sproc first
			string sql = "IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'" + DatabaseOwner + ObjectQualifier +
						 "PurgeEventLog') AND OBJECTPROPERTY(id, N'IsProcedure') = 1) " + " BEGIN " +
						 "    EXEC " + DatabaseOwner + ObjectQualifier + "PurgeEventLog" + " END ";
			PetaPocoHelper.ExecuteNonQuery(ConnectionString, CommandType.Text, sql);
		}

		public virtual void UpdateEventLogPendingNotif(int logConfigID)
		{
			ExecuteNonQuery("UpdateEventLogPendingNotif", logConfigID);
		}

		public virtual void UpdateLogType(string logTypeKey, string logTypeFriendlyName, string logTypeDescription,
										  string logTypeCSSClass, string logTypeOwner)
		{
			ExecuteNonQuery("UpdateEventLogType", logTypeKey, logTypeFriendlyName, logTypeDescription, logTypeOwner,
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
				logTypeKey = "";
			}
			if (logTypePortalID == "*")
			{
				portalID = -1;
			}
			else
			{
				portalID = Convert.ToInt32(logTypePortalID);
			}
			ExecuteNonQuery("UpdateEventLogConfig",
									  id,
									  GetNull(logTypeKey),
									  GetNull(portalID),
									  loggingIsActive,
									  keepMostRecent,
									  emailNotificationIsActive,
									  GetNull(threshold),
									  GetNull(notificationThresholdTime),
									  GetNull(notificationThresholdTimeType),
									  mailFromAddress,
									  mailToAddress);
		}

		#endregion

		#region Scheduling

		public virtual int AddSchedule(string TypeFullName, int TimeLapse, string TimeLapseMeasurement,
									   int RetryTimeLapse, string RetryTimeLapseMeasurement, int RetainHistoryNum,
									   string AttachToEvent, bool CatchUpEnabled, bool Enabled,
									   string ObjectDependencies, string Servers, int CreatedByUserID,
									   string FriendlyName, DateTime ScheduleStartDate)
		{
			return ExecuteScalar<int>("AddSchedule",
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
											GetNull(Servers),
											CreatedByUserID,
											FriendlyName,
											GetNull(ScheduleStartDate));
		}

		public virtual int AddScheduleHistory(int ScheduleID, DateTime StartDate, string Server)
		{
			return ExecuteScalar<int>("AddScheduleHistory", ScheduleID, FixDate(StartDate), Server);
		}

		public virtual void AddScheduleItemSetting(int ScheduleID, string Name, string Value)
		{
			ExecuteNonQuery("AddScheduleItemSetting", ScheduleID, Name, Value);
		}

		public virtual void DeleteSchedule(int ScheduleID)
		{
			ExecuteNonQuery("DeleteSchedule", ScheduleID);
		}

		public virtual IDataReader GetNextScheduledTask(string Server)
		{
			return ExecuteReader("GetScheduleNextTask", GetNull(Server));
		}

		public virtual IDataReader GetSchedule()
		{
			return ExecuteReader("GetSchedule", DBNull.Value);
		}

		public virtual IDataReader GetSchedule(string Server)
		{
			return ExecuteReader("GetSchedule", GetNull(Server));
		}

		public virtual IDataReader GetSchedule(int ScheduleID)
		{
			return ExecuteReader("GetScheduleByScheduleID", ScheduleID);
		}

		public virtual IDataReader GetSchedule(string TypeFullName, string Server)
		{
			return ExecuteReader("GetScheduleByTypeFullName", TypeFullName, GetNull(Server));
		}

		public virtual IDataReader GetScheduleByEvent(string EventName, string Server)
		{
			return ExecuteReader("GetScheduleByEvent", EventName, GetNull(Server));
		}

		public virtual IDataReader GetScheduleHistory(int ScheduleID)
		{
			return ExecuteReader("GetScheduleHistory", ScheduleID);
		}

		public virtual IDataReader GetScheduleItemSettings(int ScheduleID)
		{
			return ExecuteReader("GetScheduleItemSettings", ScheduleID);
		}

		public virtual void PurgeScheduleHistory()
		{
			ExecuteNonQuery("PurgeScheduleHistory");
		}

		public virtual void UpdateSchedule(int ScheduleID, string TypeFullName, int TimeLapse,
										   string TimeLapseMeasurement, int RetryTimeLapse,
										   string RetryTimeLapseMeasurement, int RetainHistoryNum,
										   string AttachToEvent, bool CatchUpEnabled, bool Enabled,
										   string ObjectDependencies, string Servers, int LastModifiedByUserID,
										   string FriendlyName, DateTime ScheduleStartDate)
		{
			ExecuteNonQuery("UpdateSchedule",
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
									  GetNull(Servers),
									  LastModifiedByUserID,
									  FriendlyName,
									  GetNull(ScheduleStartDate));
		}

		public virtual void UpdateScheduleHistory(int ScheduleHistoryID, DateTime EndDate, bool Succeeded, string LogNotes, DateTime NextStart)
		{
			ExecuteNonQuery("UpdateScheduleHistory", ScheduleHistoryID, FixDate(EndDate), GetNull(Succeeded), LogNotes, FixDate(NextStart));
		}

		#endregion

		#region Extension Url Providers

		public virtual int AddExtensionUrlProvider(int providerId,
													int desktopModuleId,
													string providerName,
													string providerType,
													string settingsControlSrc,
													bool isActive,
													bool rewriteAllUrls,
													bool redirectAllUrls,
													bool replaceAllUrls)
		{
			return ExecuteScalar<int>("AddExtensionUrlProvider",
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
			ExecuteNonQuery("DeleteExtensionUrlProvider", providerId);
		}

		public virtual IDataReader GetExtensionUrlProviders(int portalId)
		{
			return ExecuteReader("GetExtensionUrlProviders", GetNull(portalId));
		}

		public virtual void SaveExtensionUrlProviderSetting(int providerId, int portalId, string settingName, string settingValue)
		{
			ExecuteNonQuery("SaveExtensionUrlProviderSetting", providerId, portalId, settingName, settingValue);
		}

		public virtual void UpdateExtensionUrlProvider(int providerId, bool isActive)
		{
			ExecuteNonQuery("UpdateExtensionUrlProvider", providerId, isActive);
		}

		#endregion

		#region IP Filter


		public virtual IDataReader GetIPFilters()
		{
			return ExecuteReader("GetIPFilters");
		}

		public virtual int AddIPFilter(string ipAddress, string subnetMask, int ruleType, int createdByUserId)
		{
			return ExecuteScalar<int>("AddIPFilter", ipAddress, subnetMask, ruleType, createdByUserId);
		}

		public virtual void DeleteIPFilter(int ipFilterid)
		{
			ExecuteNonQuery("DeleteIPFilter", ipFilterid);
		}

		public virtual void UpdateIPFilter(int ipFilterid, string ipAddress, string subnetMask, int ruleType, int lastModifiedByUserId)
		{
			ExecuteNonQuery("UpdateIPFilter", ipFilterid, ipAddress, subnetMask, ruleType, lastModifiedByUserId);
		}

		public virtual IDataReader GetIPFilter(int ipf)
		{
			return ExecuteReader("GetIPFilter", ipf);
		}

		#endregion

		#region File Versions

		public virtual IDataReader GetFileVersions(int fileId)
		{
			return ExecuteReader("GetFileVersions", fileId);
		}

		public virtual IDataReader GetFileVersionsInFolder(int folderId)
		{
			return ExecuteReader("GetFileVersionsInFolder", folderId);
		}

		public virtual int AddFileVersion(int fileId, Guid uniqueId, Guid versionGuid, string fileName, string extension,
							long size, int width, int height, string contentType, string folder, int folderId,
							int userId, string hash, DateTime lastModificationTime,
							string title, bool enablePublishPeriod, DateTime startDate, DateTime endDate, int contentItemID, bool published, byte[] content = null)
		{
			if (content == null)
			{
				return ExecuteScalar<int>("AddFileVersion",
									fileId,
									uniqueId,
									versionGuid,
									fileName,
									extension,
									size,
									GetNull(width),
									GetNull(height),
									contentType,
									folder,
									folderId,
									userId,
									hash,
									lastModificationTime,
									title,
									enablePublishPeriod,
									startDate,
									GetNull(endDate),
									GetNull(contentItemID),
									published);
			}
			return ExecuteScalar<int>("AddFileVersion",
								fileId,
								uniqueId,
								versionGuid,
								fileName,
								extension,
								size,
								GetNull(width),
								GetNull(height),
								contentType,
								folder,
								folderId,
								userId,
								hash,
								lastModificationTime,
								title,
								enablePublishPeriod,
								startDate,
								GetNull(endDate),
								GetNull(contentItemID),
								published,
								GetNull(content));
		}

		public virtual int DeleteFileVersion(int fileId, int version)
		{
			return ExecuteScalar<int>("DeleteFileVersion", fileId, version);
		}

		public virtual void ResetFilePublishedVersion(int fileId)
		{
			ExecuteNonQuery("ResetFilePublishedVersion", fileId);
		}

		public virtual IDataReader GetFileVersion(int fileId, int version)
		{
			return ExecuteReader("GetFileVersion", fileId, version);
		}

		public void SetPublishedVersion(int fileId, int newPublishedVersion)
		{
			ExecuteNonQuery("SetPublishedVersion", fileId, newPublishedVersion);
		}

		#endregion

		#region Content Workflow

		public virtual int GetContentWorkflowUsageCount(int workflowId)
		{
			return ExecuteScalar<int>("GetContentWorkflowUsageCount", workflowId);
		}

		public virtual IDataReader GetContentWorkflowUsage(int workflowId, int pageIndex, int pageSize)
		{
			return ExecuteReader("GetContentWorkflowUsage", workflowId, pageIndex, pageSize);
		}

		public virtual int GetContentWorkflowStateUsageCount(int stateId)
		{
			return ExecuteScalar<int>("GetContentWorkflowStateUsageCount", stateId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual int AddContentWorkflow(int portalId, string workflowName, string description, bool isDeleted, bool startAfterCreating, bool startAfterEditing, bool dispositionEnabled)
		{
			return ExecuteScalar<int>("AddContentWorkflow",
				GetNull(portalId),
				workflowName,
				description,
				isDeleted,
				startAfterCreating,
				startAfterEditing,
				dispositionEnabled);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual IDataReader GetContentWorkflow(int workflowId)
		{
			return ExecuteReader("GetContentWorkflow", workflowId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual IDataReader GetContentWorkflows(int portalId)
		{
			return ExecuteReader("GetContentWorkflows", portalId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual void UpdateContentWorkflow(int workflowId, string workflowName, string description, bool isDeleted, bool startAfterCreating, bool startAfterEditing, bool dispositionEnabled)
		{
			ExecuteNonQuery("UpdateContentWorkflow",
				workflowId,
				workflowName,
				description,
				isDeleted,
				startAfterCreating,
				startAfterEditing,
				dispositionEnabled);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual int AddContentWorkflowState(int workflowId, string stateName, int order,
			bool isActive, bool sendEmail, bool sendMessage, bool isDisposalState,
			string onCompleteMessageSubject, string onCompleteMessageBody,
			string onDiscardMessageSubject, string onDiscardMessageBody)
		{
			return ExecuteScalar<int>("AddContentWorkflowState",
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

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual void DeleteContentWorkflowState(int stateId)
		{
			ExecuteNonQuery("DeleteContentWorkflowState", stateId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual void UpdateContentWorkflowState(int stateId, string stateName, int order,
			bool isActive, bool sendEmail, bool sendMessage, bool isDisposalState,
			string onCompleteMessageSubject, string onCompleteMessageBody,
			string onDiscardMessageSubject, string onDiscardMessageBody)
		{
			ExecuteNonQuery("UpdateContentWorkflowState",
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

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual IDataReader GetContentWorkflowState(int stateId)
		{
			return ExecuteReader("GetContentWorkflowState", stateId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual IDataReader GetContentWorkflowStates(int workflowId)
		{
			return ExecuteReader("GetContentWorkflowStates", workflowId);
		}

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger.AddWorkflowLog")]
		public virtual int AddContentWorkflowLog(string action, string comment, int user, int workflowId, int contentItemId)
		{
			return ExecuteScalar<int>("AddContentWorkflowLog",
				action,
				comment,
				user,
				workflowId,
				contentItemId);
		}

        [Obsolete("Deprecated in Platform 7.4.0. Use instead IWorkflowLogger.GetWorkflowLogs")]
		public virtual IDataReader GetContentWorkflowLogs(int contentItemId, int workflowId)
		{
			return ExecuteReader("GetContentWorkflowLogs", contentItemId, workflowId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual int DeleteContentWorkflowLogs(int contentItemId, int workflowId)
		{
			return ExecuteScalar<int>("DeleteContentWorkflowLogs", contentItemId, workflowId);
		}

		public virtual int AddContentWorkflowStatePermission(int stateId, int permissionId, int roleId, bool allowAccess, int userId, int createdByUserId)
		{
			return ExecuteScalar<int>("AddContentWorkflowStatePermission",
				stateId,
				permissionId,
				GetNull(roleId),
				allowAccess,
				GetNull(userId),
				GetNull(createdByUserId));
		}

		public virtual void UpdateContentWorkflowStatePermission(int workflowStatePermissionId, int stateId, int permissionId, int roleId, bool allowAccess, int userId, int lastModifiedByUserId)
		{
			ExecuteNonQuery("UpdateContentWorkflowStatePermission",
				workflowStatePermissionId,
				stateId,
				permissionId,
				GetNull(roleId),
				allowAccess,
				GetNull(userId),
				GetNull(lastModifiedByUserId));
		}

		public virtual void DeleteContentWorkflowStatePermission(int workflowStatePermissionId)
		{
			ExecuteNonQuery("DeleteContentWorkflowStatePermission", workflowStatePermissionId);
		}

		public virtual IDataReader GetContentWorkflowStatePermission(int workflowStatePermissionId)
		{
			return ExecuteReader("GetContentWorkflowStatePermission", workflowStatePermissionId);
		}

		public virtual IDataReader GetContentWorkflowStatePermissions()
		{
			return ExecuteReader("GetContentWorkflowStatePermissions");
		}

		public virtual IDataReader GetContentWorkflowStatePermissionsByStateID(int stateId)
		{
			return ExecuteReader("GetContentWorkflowStatePermissionsByStateID", stateId);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual IDataReader GetContentWorkflowSource(int workflowId, string sourceName)
		{
			return ExecuteReader("GetContentWorkflowSource", workflowId, sourceName);
		}

        [Obsolete("Deprecated in Platform 7.4.0")]
		public virtual int AddContentWorkflowSource(int workflowId, string sourceName, string sourceType)
		{
			return ExecuteScalar<int>("AddContentWorkflowSource", workflowId, sourceName, sourceType);
		}

		#endregion

		#region Search Crawler

		public virtual IDataReader GetAllSearchTypes()
		{
			return ExecuteReader("SearchTypes_GetAll");
		}

		#endregion

		#region Search Synonyms Groups

		public virtual IDataReader GetAllSynonymsGroups(int portalId, string cultureCode)
		{
			return ExecuteReader("GetAllSynonymsGroups", portalId, cultureCode);
		}

		public virtual int AddSynonymsGroup(string synonymsTags, int createdByUserId, int portalId, string cultureCode)
		{
			return ExecuteScalar<int>("AddSynonymsGroup",
				synonymsTags,
				createdByUserId,
				portalId,
				cultureCode);
		}

		public virtual void UpdateSynonymsGroup(int synonymsGroupId, string synonymsTags, int lastModifiedUserId)
		{
			ExecuteNonQuery("UpdateSynonymsGroup", synonymsGroupId, synonymsTags, lastModifiedUserId);
		}

		public virtual void DeleteSynonymsGroup(int synonymsGroupId)
		{
			ExecuteNonQuery("DeleteSynonymsGroup", synonymsGroupId);
		}

		#endregion

		#region  Search Stop Words

		public virtual IDataReader GetSearchStopWords(int portalId, string cultureCode)
		{
			return ExecuteReader("GetSearchStopWords", portalId, cultureCode);
		}

		public virtual int AddSearchStopWords(string stopWords, int createdByUserId, int portalId, string cultureCode)
		{
			return ExecuteScalar<int>("InsertSearchStopWords", stopWords, createdByUserId, portalId, cultureCode);
		}

		public virtual void UpdateSearchStopWords(int stopWordsId, string stopWords, int lastModifiedUserId)
		{
			ExecuteNonQuery("UpdateSearchStopWords", stopWordsId, stopWords, lastModifiedUserId);
		}

		public virtual void DeleteSearchStopWords(int stopWordsId)
		{
			ExecuteNonQuery("DeleteSearchStopWords", stopWordsId);
		}

		#endregion

		#region Search deleted items related methods

		public void AddSearchDeletedItems(SearchDocumentToDelete deletedIDocument)
		{
			try
			{
				ExecuteNonQuery("SearchDeletedItems_Add", deletedIDocument.ToString());
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
				ExecuteNonQuery("SearchDeletedItems_DeleteProcessed", cutoffTime);
			}
			catch (SqlException ex)
			{
				Logger.Error(ex);
			}
		}

		public IDataReader GetSearchDeletedItems(DateTime cutoffTime)
		{
			return ExecuteReader("SearchDeletedItems_Select", cutoffTime);
		}

		#endregion

		#region User Index Methods

		public virtual IDataReader GetAvailableUsersForIndex(int portalId, DateTime startDate, int startUserId, int numberOfUsers)
		{
			return ExecuteReader("GetAvailableUsersForIndex", portalId, startDate, startUserId, numberOfUsers);
		}

		#endregion

		#region OutputCache Methods

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

		#endregion

		#endregion

		#region Obsolete Methods

		[Obsolete(
			"Deprecated in 7.0.0.  This method is unneccessary.  You can get a reader and convert it to a DataSet.")]
		public virtual DataSet ExecuteDataSet(string procedureName, params object[] commandParameters)
		{
			return Globals.ConvertDataReaderToDataSet(ExecuteReader(procedureName, commandParameters));
		}

		[Obsolete("Deprecated in 7.0.0.  This method is unneccessary.  Use the generic version ExecuteScalar<T>.")]
		public virtual object ExecuteScalar(string procedureName, params object[] commandParameters)
		{
			return ExecuteScalar<object>(procedureName, commandParameters);
		}

		[Obsolete("Temporarily Added in DNN 5.4.2. This will be removed and replaced with named instance support.")]
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
				return SqlHelper.ExecuteReader(ConnectionString, CommandType.Text, sql, sqlCommandParameters);
			}
			catch
			{
				//error in SQL query
				return null;
			}
		}

		#endregion

	}
}