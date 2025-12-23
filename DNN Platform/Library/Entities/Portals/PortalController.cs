// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.XPath;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Portals.Templates;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Portals.Templates;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Security.Membership;
    using DotNetNuke.Services.Cryptography;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Web.Client;

    using Microsoft.Extensions.DependencyInjection;

    using IAbPortalSettings = DotNetNuke.Abstractions.Portals.IPortalSettings;

    /// <summary>PortalController provides business layer of portal.</summary>
    /// <remarks>
    /// DotNetNuke supports the concept of virtualized sites in a single install. This means that multiple sites,
    /// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
    /// </remarks>
    public partial class PortalController : ServiceLocator<IPortalController, PortalController>, IPortalController
    {
        protected const string HttpContextKeyPortalSettingsDictionary = "PortalSettingsDictionary{0}{1}";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
        private readonly IBusinessControllerProvider businessControllerProvider;
        private readonly IHostSettings hostSettings;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with IBusinessControllerProvider. Scheduled removal in v12.0.0.")]
        public PortalController()
            : this(null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public PortalController(IBusinessControllerProvider businessControllerProvider)
            : this(businessControllerProvider, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="PortalController"/> class.</summary>
        /// <param name="businessControllerProvider">The business controller provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public PortalController(IBusinessControllerProvider businessControllerProvider, IHostSettings hostSettings, IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.businessControllerProvider = businessControllerProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<IBusinessControllerProvider>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <summary>Adds the portal dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial void AddPortalDictionary(int portalId, int tabId)
            => AddPortalDictionary(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                portalId,
                tabId);

        /// <summary>Adds the portal dictionary.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="tabId">The tab ID.</param>
        public static void AddPortalDictionary(IHostSettings hostSettings, DataProvider dataProvider, int portalId, int tabId)
        {
            var portalDic = GetPortalDictionary(hostSettings, dataProvider);
            portalDic[tabId] = portalId;
            DataCache.SetCache(DataCache.PortalDictionaryCacheKey, portalDic);
        }

        /// <summary>Creates the root folder for a child portal.</summary>
        /// <remarks>
        /// If call this method, it will create the specific folder if the folder doesn't exist;
        /// and will copy subhost.aspx to the folder if there is no 'Default.aspx'.
        /// </remarks>
        /// <param name="childPath">The child path.</param>
        /// <returns>
        /// If the method executed successful, it will return NullString, otherwise return error message.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// string childPhysicalPath = Server.MapPath(childPath);
        /// message = PortalController.CreateChildPortalFolder(childPhysicalPath);
        /// </code>
        /// </example>
        public static string CreateChildPortalFolder(string childPath)
        {
            string message = Null.NullString;

            // Set up Child Portal
            try
            {
                // create the subdirectory for the new portal
                if (!Directory.Exists(childPath))
                {
                    Directory.CreateDirectory(childPath);
                }

                // create the subhost default.aspx file
                if (!File.Exists(childPath + "\\" + Globals.glbDefaultPage))
                {
                    File.Copy(Globals.HostMapPath + "subhost.aspx", childPath + "\\" + Globals.glbDefaultPage);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                message += Localization.GetString("ChildPortal.Error") + exc.Message + exc.StackTrace;
            }

            return message;
        }

        /// <summary>Deletes all expired portals.</summary>
        /// <param name="serverPath">The server path.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void DeleteExpiredPortals(string serverPath)
            => DeleteExpiredPortals(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(),
                serverPath);

        /// <summary>Deletes all expired portals.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="serverPath">The server path.</param>
        public static void DeleteExpiredPortals(IPortalController portalController, DataProvider dataProvider, IPortalAliasService portalAliasService, string serverPath)
        {
            foreach (IPortalInfo portal in GetExpiredPortals(dataProvider))
            {
                DeletePortal(portalController, portalAliasService, portal, serverPath);
            }
        }

        /// <summary>Deletes the portal.</summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return <see cref="Null.NullString"/>, otherwise return error message.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial string DeletePortal(PortalInfo portal, string serverPath)
            => DeletePortal(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), portal, serverPath);

        /// <summary>Deletes the portal.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return <see cref="Null.NullString"/>, otherwise return error message.</returns>
        public static string DeletePortal(IPortalController portalController, IPortalAliasService portalAliasService, IPortalInfo portal, string serverPath)
        {
            string message = string.Empty;

            // check if this is the last portal
            var portals = portalController.GetPortals();
            if (portals.Count > 1)
            {
                if (portal == null)
                {
                    return message;
                }

                // delete custom resource files
                Globals.DeleteFilesRecursive(serverPath, $".Portal-{portal.PortalId}.resx");

                // If child portal delete child folder
                var arr = portalAliasService.GetPortalAliasesByPortalId(portal.PortalId).ToList();
                if (arr.Count > 0)
                {
                    var portalAliasInfo = arr[0];
                    var portalName = Globals.GetPortalDomainName(portalAliasInfo.HttpAlias, null, true);
                    if (portalAliasInfo.HttpAlias.IndexOf("/", StringComparison.Ordinal) > -1)
                    {
                        portalName = GetPortalFolder(portalAliasInfo.HttpAlias);
                    }

                    if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                    {
                        DeletePortalFolder(serverPath, portalName);
                    }
                }

                // delete upload directory
                if (!string.IsNullOrEmpty(portal.HomeDirectory))
                {
                    var homeDirectory = portal.HomeDirectoryMapPath;

                    // check whether home directory is not used by other portal
                    // it happens when new portal creation failed, but home directory defined by user is already in use with other portal
                    var homeDirectoryInUse = portals.OfType<IPortalInfo>().Any(x =>
                        x.PortalId != portal.PortalId &&
                        x.HomeDirectoryMapPath.Equals(homeDirectory, StringComparison.OrdinalIgnoreCase));

                    if (!homeDirectoryInUse)
                    {
                        if (Directory.Exists(homeDirectory))
                        {
                            Globals.DeleteFolderRecursive(homeDirectory);
                        }
                    }
                }

                // remove database references
                DeletePortalInternal(portal.PortalId);
            }
            else
            {
                message = Localization.GetString("LastPortal");
            }

            return message;
        }

        /// <summary>Get the portal folder from a child portal alias.</summary>
        /// <param name="alias">portal alias.</param>
        /// <returns>folder path of the child portal.</returns>
        public static string GetPortalFolder(string alias)
        {
            alias = alias.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);
            var appPath = Globals.ApplicationPath + "/";
            if (string.IsNullOrEmpty(Globals.ApplicationPath) || alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) == Null.NullInteger)
            {
                return alias.Contains('/') ? alias.Substring(alias.IndexOf('/') + 1) : string.Empty;
            }

            return alias.Substring(alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) + appPath.Length);
        }

        /// <summary>Delete the child portal folder and try to remove its parent when parent folder is empty.</summary>
        /// <param name="serverPath">the server path.</param>
        /// <param name="portalFolder">the child folder path.</param>
        public static void DeletePortalFolder(string serverPath, string portalFolder)
        {
            var physicalPath = serverPath + portalFolder;
            Globals.DeleteFolderRecursive(physicalPath);

            // remove parent folder if its empty.
            var parentFolder = Directory.GetParent(physicalPath);
            while (parentFolder != null && !parentFolder.FullName.Equals(serverPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
            {
                if (parentFolder.GetDirectories().Length + parentFolder.GetFiles().Length == 0)
                {
                    parentFolder.Delete();
                    parentFolder = parentFolder.Parent;
                }
                else
                {
                    break;
                }
            }
        }

        /// <summary>Gets the portal dictionary.</summary>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial Dictionary<int, int> GetPortalDictionary()
            => GetPortalDictionary(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>());

        /// <summary>Gets the portal dictionary.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        public static Dictionary<int, int> GetPortalDictionary(IHostSettings hostSettings, DataProvider dataProvider)
        {
            string cacheKey = string.Format(DataCache.PortalDictionaryCacheKey);
            return CBO.GetCachedObject<Dictionary<int, int>>(
                hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalDictionaryTimeOut, DataCache.PortalDictionaryCachePriority, hostSettings, dataProvider),
                GetPortalDictionaryCallback);
        }

        /// <summary>GetPortalsByName gets all the portals whose name matches a provided filter expression.</summary>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> objects.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial ArrayList GetPortalsByName(string nameToMatch, int pageIndex, int pageSize, ref int totalRecords)
            => GetPortalsByName(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), nameToMatch, pageIndex, pageSize, ref totalRecords);

        /// <summary>GetPortalsByName gets all the portals whose name matches a provided filter expression.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page.</param>
        /// <param name="totalRecords">The total number of records that satisfy the criteria.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> objects.</returns>
        public static ArrayList GetPortalsByName(DataProvider dataProvider, string nameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }

            var type = typeof(PortalInfo);
            return CBO.FillCollection(dataProvider.GetPortalsByName(nameToMatch, pageIndex, pageSize), ref type, ref totalRecords);
        }

        /// <summary>Gets a list of all the portals of which the given user is a member.</summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> instances.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial ArrayList GetPortalsByUser(int userId)
            => GetPortalsByUser(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), userId);

        /// <summary>Gets a list of all the portals of which the given user is a member.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="userId">The user ID.</param>
        /// <returns>An <see cref="ArrayList"/> of <see cref="PortalInfo"/> instances.</returns>
        public static ArrayList GetPortalsByUser(DataProvider dataProvider, int userId)
        {
            return CBO.FillCollection(dataProvider.GetPortalsByUser(userId), typeof(PortalInfo));
        }

        /// <summary>Gets the master portal ID for the given portal, if the portal is part of a portal group.</summary>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The effective portal ID.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial int GetEffectivePortalId(int portalId)
            => GetEffectivePortalId(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalGroupController>(),
                portalId);

        /// <summary>Gets the master portal ID for the given portal, if the portal is part of a portal group.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        /// <param name="portalId">The ID of the portal.</param>
        /// <returns>The effective portal ID.</returns>
        public static int GetEffectivePortalId(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController, int portalId)
        {
            if (portalId <= Null.NullInteger || appStatus.Status == UpgradeStatus.Upgrade)
            {
                return portalId;
            }

            var portal = portalController.GetPortal(portalId);
            var portalGroup = (
                    from p in portalGroupController.GetPortalGroups()
                    where p.PortalGroupId == ((IPortalInfo)portal).PortalGroupId
                    select p)
                .SingleOrDefault();

            if (portalGroup != null)
            {
                portalId = portalGroup.MasterPortalId;
            }

            return portalId;
        }

        /// <summary>Gets all expired portals.</summary>
        /// <returns>all expired portals as array list.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial ArrayList GetExpiredPortals()
            => GetExpiredPortals(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>());

        /// <summary>Gets all expired portals.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <returns>all expired portals as array list.</returns>
        public static ArrayList GetExpiredPortals(DataProvider dataProvider)
        {
            return CBO.FillCollection(dataProvider.GetExpiredPortals(), typeof(PortalInfo));
        }

        /// <summary>Determines whether the portal is child portal.</summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <see langword="true"/> if the portal is child portal; otherwise, <see langword="false"/>.
        /// </returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalAliasService")]
        public static partial bool IsChildPortal(PortalInfo portal, string serverPath)
            => IsChildPortal(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalAliasService>(), portal, serverPath);

        /// <summary>Determines whether the portal is child portal.</summary>
        /// <param name="portalAliasService">The portal alias service.</param>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <see langword="true"/> if the portal is child portal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsChildPortal(IPortalAliasService portalAliasService, IPortalInfo portal, string serverPath)
        {
            var arr = portalAliasService.GetPortalAliasesByPortalId(portal.PortalId).ToList();
            if (arr.Count <= 0)
            {
                return false;
            }

            var portalAlias = arr[0];
            var portalName = Globals.GetPortalDomainName(portalAlias.HttpAlias, null, true);
            if (portalAlias.HttpAlias.IndexOf('/') > -1)
            {
                portalName = GetPortalFolder(portalAlias.HttpAlias);
            }

            return !string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName);
        }

        /// <summary>Gets a value indicating whether the portal with the given <paramref name="portalId"/> is in a portal group.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if the portal is a member of a portal group, otherwise <see langword="false"/>.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial bool IsMemberOfPortalGroup(int portalId)
            => IsMemberOfPortalGroup(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalId);

        /// <summary>Gets a value indicating whether the portal with the given <paramref name="portalId"/> is in a portal group.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns><see langword="true"/> if the portal is a member of a portal group, otherwise <see langword="false"/>.</returns>
        public static bool IsMemberOfPortalGroup(IPortalController portalController, int portalId)
        {
            IPortalInfo portal = portalController.GetPortal(portalId);
            return portal != null && portal.PortalGroupId > Null.NullInteger;
        }

        /// <summary>Deletes the portal setting (neutral and for all languages).</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSetting(int portalID, string settingName)
            => DeletePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID,
                settingName);

        /// <summary>Deletes the portal setting (neutral and for all languages).</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        public static void DeletePortalSetting(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId, string settingName)
        {
            DeletePortalSetting(dataProvider, eventLogger, userController, portalId, settingName, Null.NullString);
        }

        /// <summary>Deletes the portal setting in this language.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="cultureCode">The culture code.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSetting(int portalID, string settingName, string cultureCode)
            => DeletePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID,
                settingName,
                cultureCode);

        /// <summary>Deletes the portal setting in this language.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="cultureCode">The culture code.</param>
        public static void DeletePortalSetting(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId, string settingName, string cultureCode)
        {
            dataProvider.DeletePortalSetting(portalId, settingName, cultureCode.ToLowerInvariant());
            eventLogger.AddLog(
                "SettingName",
                settingName + (cultureCode == Null.NullString ? string.Empty : $" ({cultureCode})"),
                GetCurrentPortalSettingsInternal(),
                userController.GetCurrentUserInfo().UserID,
                EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>Deletes all portal settings by portal ID.</summary>
        /// <param name="portalID">The portal ID.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSettings(int portalID)
            => DeletePortalSettings(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID);

        /// <summary>Deletes all portal settings by portal ID.</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void DeletePortalSettings(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId)
            => DeletePortalSettings(dataProvider, eventLogger, userController, portalId, Null.NullString);

        /// <summary>Deletes all portal settings by portal ID and for a given language (Null: all languages and neutral settings).</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="cultureCode">The culture code or <see cref="Null.NullString"/>.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void DeletePortalSettings(int portalID, string cultureCode)
            => DeletePortalSettings(
                Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                portalID,
                cultureCode);

        /// <summary>Deletes all portal settings by portal id and for a given language (Null: all languages and neutral settings).</summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code or <see cref="Null.NullString"/>.</param>
        public static void DeletePortalSettings(DataProvider dataProvider, IEventLogger eventLogger, IUserController userController, int portalId, string cultureCode)
        {
            dataProvider.DeletePortalSettings(portalId, cultureCode);
            eventLogger.AddLog(
                "PortalID",
                portalId + (cultureCode == Null.NullString ? string.Empty : $" ({cultureCode})"),
                GetCurrentPortalSettingsInternal(),
                userController.GetCurrentUserInfo().UserID,
                EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value.</summary>
        /// <param name="settingName">the setting to read.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption.</param>
        /// <returns>The decrypted setting value.</returns>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial string GetEncryptedString(string settingName, int portalID, string passPhrase)
            => GetEncryptedString(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                settingName,
                portalID,
                passPhrase);

        /// <summary>takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="settingName">the setting to read.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption.</param>
        /// <returns>The decrypted setting value.</returns>
        public static string GetEncryptedString(IHostSettings hostSettings, IPortalController portalController, string settingName, int portalId, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", settingName);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = GetPortalSetting(portalController, settingName, portalId, string.Empty);

            return Security.FIPSCompliant.DecryptAES(cipherText, passPhrase, hostSettings.Guid);
        }

        /// <summary>Gets the portal setting.</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial string GetPortalSetting(string settingName, int portalID, string defaultValue)
            => GetPortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                settingName,
                portalID,
                defaultValue);

        /// <summary>Gets the portal setting.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(IPortalController portalController, string settingName, int portalId, string defaultValue)
        {
            var retValue = Null.NullString;
            try
            {
                portalController.GetPortalSettings(portalId).TryGetValue(settingName, out var setting);
                retValue = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting for a specific language (or neutral).</summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial string GetPortalSetting(string settingName, int portalID, string defaultValue, string cultureCode)
            => GetPortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                settingName,
                portalID,
                defaultValue,
                cultureCode);

        /// <summary>Gets the portal setting for a specific language (or neutral).</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(IPortalController portalController, string settingName, int portalId, string defaultValue, string cultureCode)
        {
            try
            {
                portalController.GetPortalSettings(portalId, cultureCode).TryGetValue(settingName, out var setting);
                return string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return Null.NullString;
        }

        /// <summary>Gets the portal setting as boolean.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue)
            => GetPortalSettingAsBoolean(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                key,
                portalID,
                defaultValue);

        /// <summary>Gets the portal setting as boolean.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(IPortalController portalController, string key, int portalId, bool defaultValue)
        {
            var retValue = Null.NullBoolean;
            try
            {
                Instance.GetPortalSettings(portalId).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as boolean for a specific language (or neutral).</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial bool GetPortalSettingAsBoolean(
            string key,
            int portalID,
            bool defaultValue,
            string cultureCode)
            => GetPortalSettingAsBoolean(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                key,
                portalID,
                defaultValue,
                cultureCode);

        /// <summary>Gets the portal setting as boolean for a specific language (or neutral).</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(IHostSettings hostSettings, IApplicationStatusInfo appStatus, string key, int portalId, bool defaultValue, string cultureCode)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                GetPortalSettingsDictionary(hostSettings, appStatus, portalId, cultureCode).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as integer.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial int GetPortalSettingAsInteger(string key, int portalID, int defaultValue)
            => GetPortalSettingAsInteger(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                key,
                portalID,
                defaultValue);

        /// <summary>Gets the portal setting as integer.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(IPortalController portalController, string key, int portalId, int defaultValue)
        {
            int retValue = Null.NullInteger;
            try
            {
                portalController.GetPortalSettings(portalId).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as double.</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
            => GetPortalSettingAsDouble(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                key,
                portalId,
                defaultValue);

        /// <summary>Gets the portal setting as <see cref="double"/>.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static double GetPortalSettingAsDouble(IPortalController portalController, string key, int portalId, double defaultValue)
        {
            double retValue = Null.NullDouble;
            try
            {
                portalController.GetPortalSettings(portalId).TryGetValue(key, out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToDouble(setting);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>Gets the portal setting as integer for a specific language (or neutral).</summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial int GetPortalSettingAsInteger(string key, int portalID, int defaultValue, string cultureCode)
            => GetPortalSettingAsInteger(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                key,
                portalID,
                defaultValue,
                cultureCode);

        /// <summary>Gets the portal setting as integer for a specific language (or neutral).</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="key">The key.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="cultureCode">culture code of the language to retrieve (not empty).</param>
        /// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(IHostSettings hostSettings, IApplicationStatusInfo appStatus, string key, int portalId, int defaultValue, string cultureCode)
        {
            int retValue = Null.NullInteger;
            try
            {
                string setting;
                GetPortalSettingsDictionary(hostSettings, appStatus, portalId, cultureCode).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = Convert.ToInt32(setting);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        [DnnDeprecated(10, 0, 2, "Use overload taking IHostSettings")]
        public static partial void UpdateEncryptedString(int portalID, string settingName, string settingValue, string passPhrase)
            => UpdateEncryptedString(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalID, settingName, settingValue, passPhrase);

        /// <summary>takes in a text value, encrypts it with a FIPS compliant algorithm and stores.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">host settings key.</param>
        /// <param name="settingValue">host settings value.</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption.</param>
        public static void UpdateEncryptedString(IHostSettings hostSettings, int portalId, string settingName, string settingValue, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", settingName);
            Requires.PropertyNotNull("value", settingValue);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = Security.FIPSCompliant.EncryptAES(settingValue, passPhrase, hostSettings.Guid);

            UpdatePortalSetting(portalId, settingName, cipherText);
        }

        /// <summary>Updates a single neutral (not language specific) portal setting and clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue)
            => UpdatePortalSetting(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalID, settingName, settingValue);

        /// <summary>Updates a single neutral (not language specific) portal setting and clears it from the cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue)
        {
            UpdatePortalSetting(portalController, portalId, settingName, settingValue, true);
        }

        /// <summary>Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache)
            => UpdatePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                portalID,
                settingName,
                settingValue,
                clearCache);

        /// <summary>Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue, bool clearCache)
        {
            UpdatePortalSetting(portalController, portalId, settingName, settingValue, clearCache, Null.NullString, false);
        }

        /// <summary>Updates a language specific or neutral portal setting and clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
        [DnnDeprecated(9, 2, 0, "Use the overload with the 'isSecure' parameter instead")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue, string cultureCode)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, true, cultureCode, false);
        }

        /// <summary>Updates a language specific or neutral portal setting and optionally clears it from the cache.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
            => UpdatePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode);

        /// <summary>Updates a language specific or neutral portal setting and optionally clears it from the cache.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSetting(portalController, portalId, settingName, settingValue, clearCache, cultureCode, false);
        }

        /// <summary>
        /// Updates a language specific or neutral portal setting and optionally clears it from the cache.
        /// All overloaded methods will not encrypt the setting value. Therefore, call this method whenever
        /// there is a need to encrypt the setting value before storing it in the database.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        /// <param name="isSecure">When true it encrypt the value before storing it in the database.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void UpdatePortalSetting(
            int portalID,
            string settingName,
            string settingValue,
            bool clearCache,
            string cultureCode,
            bool isSecure)
            => UpdatePortalSetting(
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode,
                isSecure);

        /// <summary>
        /// Updates a language specific or neutral portal setting and optionally clears it from the cache.
        /// All overloaded methods will not encrypt the setting value. Therefore, call this method whenever
        /// there is a need to encrypt the setting value before storing it in the database.
        /// </summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <see langword="true"/> [clear cache].</param>
        /// <param name="cultureCode">culture code for language specific settings, null string otherwise.</param>
        /// <param name="isSecure">When true it encrypt the value before storing it in the database.</param>
        public static void UpdatePortalSetting(IPortalController portalController, int portalId, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            portalController.UpdatePortalSetting(portalId, settingName, settingValue, clearCache, cultureCode, isSecure);
        }

        /// <summary>Checks the desktop modules whether is installed.</summary>
        /// <param name="nav">The nav.</param>
        /// <returns>Empty string if the module hasn't been installed, otherwise return the friendly name.</returns>
        public static string CheckDesktopModulesInstalled(XPathNavigator nav)
        {
            string friendlyName;
            DesktopModuleInfo desktopModule;
            StringBuilder modulesNotInstalled = new StringBuilder();

            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
                friendlyName = XmlUtils.GetNodeValue(desktopModuleNav, "friendlyname");

                if (!string.IsNullOrEmpty(friendlyName))
                {
                    desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(friendlyName);
                    if (desktopModule == null)
                    {
                        // PE and EE templates have HTML as friendly name so check to make sure
                        // there is really no HTML module installed
                        if (friendlyName == "HTML")
                        {
                            desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName("HTML Pro");
                            if (desktopModule == null)
                            {
                                modulesNotInstalled.Append(friendlyName);
                                modulesNotInstalled.Append("<br/>");
                            }
                        }
                        else
                        {
                            modulesNotInstalled.Append(friendlyName);
                            modulesNotInstalled.Append("<br/>");
                        }
                    }
                }
            }

            return modulesNotInstalled.ToString();
        }

        /// <summary>
        ///   function provides the language for <see cref="PortalInfo"/> requests
        ///   in case where language has not been installed yet, will return the core install default of en-us.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial string GetActivePortalLanguage(int portalID)
            => GetActivePortalLanguage(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>(),
                portalID);

        /// <summary>
        ///   function provides the language for <see cref="PortalInfo"/> requests
        ///   in case where language has not been installed yet, will return the core install default of en-us.
        /// </summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        public static string GetActivePortalLanguage(IHostSettings hostSettings, IApplicationStatusInfo appStatus, int portalId)
        {
            // get Language
            var language = Localization.SystemLocale;
            var tmpLanguage = GetPortalDefaultLanguage(hostSettings, portalId);
            var isDefaultLanguage = false;
            if (!string.IsNullOrEmpty(tmpLanguage))
            {
                language = tmpLanguage;
                isDefaultLanguage = true;
            }

            // handles case where PortalController methods invoked before a language is installed
            if (portalId > Null.NullInteger && appStatus.Status == UpgradeStatus.None && Localization.ActiveLanguagesByPortalID(portalId) == 1)
            {
                return language;
            }

            var context = HttpContextSource.Current;
            if (context == null || appStatus.Status != UpgradeStatus.None)
            {
                return language;
            }

            if (context.Request.QueryString["language"] != null)
            {
                return context.Request.QueryString["language"];
            }

            var portalSettings = GetCurrentPortalSettingsInternal();
            if (portalSettings is { ActiveTab: not null } && !string.IsNullOrEmpty(portalSettings.ActiveTab.CultureCode))
            {
                return portalSettings.ActiveTab.CultureCode;
            }

            // PortalSettings IS Nothing - probably means we haven't set it yet (in Begin Request)
            // so try detecting the user's cookie
            if (context.Request["language"] != null)
            {
                language = context.Request["language"];
                isDefaultLanguage = false;
            }

            if ((!string.IsNullOrEmpty(language) && !isDefaultLanguage) || !EnableBrowserLanguageInDefault(hostSettings, appStatus, portalId))
            {
                return language;
            }

            var culture = Localization.GetBrowserCulture(portalId);
            if (culture != null)
            {
                language = culture.Name;
            }

            return language;
        }

        /// <summary>return the current DefaultLanguage value from the Portals table for the requested <paramref name="portalID"/>.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        [DnnDeprecated(10, 0, 2, "Please use overload with IHostSettings")]
        public static partial string GetPortalDefaultLanguage(int portalID)
            => GetPortalDefaultLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), portalID);

        /// <summary>return the current DefaultLanguage value from the Portals table for the requested <paramref name="portalId"/>.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The language identifier.</returns>
        public static string GetPortalDefaultLanguage(IHostSettings hostSettings, int portalId)
        {
            string cacheKey = $"PortalDefaultLanguage_{portalId}";
            return CBO.GetCachedObject<string>(
                hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, portalId),
                GetPortalDefaultLanguageCallBack);
        }

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with DataProvider")]
        public static partial void UpdatePortalDefaultLanguage(int portalID, string cultureCode)
            => UpdatePortalDefaultLanguage(Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>(), portalID, cultureCode);

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        public static void UpdatePortalDefaultLanguage(DataProvider dataProvider, int portalId, string cultureCode)
        {
            dataProvider.UpdatePortalDefaultLanguage(portalId, cultureCode);

            // ensure localization record exists as new portal default language may be relying on fallback chain
            // of which it is now the final part
            dataProvider.EnsureLocalizationExists(portalId, cultureCode);
        }

        /// <summary>Increments the Client Resource Manager version.</summary>
        /// <param name="portalID">The portal ID.</param>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void IncrementCrmVersion(int portalID)
            => IncrementCrmVersion(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(), portalID);

        /// <summary>Increments the Client Resource Manager version.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void IncrementCrmVersion(IPortalController portalController, int portalId)
        {
            var versionSetting = GetPortalSetting(portalController, ClientResourceSettings.VersionKey, portalId, "1");
            if (int.TryParse(versionSetting, out var currentVersion))
            {
                var newVersion = currentVersion + 1;
                UpdatePortalSetting(portalController, portalId, ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);
            }
        }

        /// <summary>Increment the CRM version for all portals that are overriding the host CRM version.</summary>
        [DnnDeprecated(10, 0, 2, "Please use overload with IPortalController")]
        public static partial void IncrementOverridingPortalsCrmVersion()
            => IncrementOverridingPortalsCrmVersion(Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>());

        /// <summary>Increment the CRM version for all portals that are overriding the host CRM version.</summary>
        /// <param name="portalController">The portal controller.</param>
        public static void IncrementOverridingPortalsCrmVersion(IPortalController portalController)
        {
            foreach (IPortalInfo portal in portalController.GetPortals())
            {
                var setting = GetPortalSetting(portalController, ClientResourceSettings.OverrideDefaultSettingsKey, portal.PortalId, "False");

                // if this portal is overriding the host level...
                if (bool.TryParse(setting, out var overriden) && overriden)
                {
                    // increment its version
                    IncrementCrmVersion(portalController, portal.PortalId);
                }
            }
        }

        /// <summary>Creates a new portal alias.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <param name="portalAlias">Portal Alias to be created.</param>
        public void AddPortalAlias(int portalId, string portalAlias)
        {
            // Check if the Alias exists
            PortalAliasInfo portalAliasInfo = PortalAliasController.Instance.GetPortalAlias(portalAlias, portalId);

            // If alias does not exist add new
            if (portalAliasInfo == null)
            {
                portalAliasInfo = new PortalAliasInfo { PortalID = portalId, HTTPAlias = portalAlias, IsPrimary = true };
                PortalAliasController.Instance.AddPortalAlias(portalAliasInfo);
            }
        }

        /// <summary>Copies the page template.</summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="mappedHomeDirectory">The mapped home directory.</param>
        public void CopyPageTemplate(string templateFile, string mappedHomeDirectory)
        {
            string hostTemplateFile = $"{Globals.HostMapPath}Templates\\{templateFile}";
            if (!File.Exists(hostTemplateFile))
            {
                return;
            }

            string portalTemplateFolder = $"{mappedHomeDirectory}Templates\\";
            if (!Directory.Exists(portalTemplateFolder))
            {
                // Create Portal Templates folder
                Directory.CreateDirectory(portalTemplateFolder);
            }

            string portalTemplateFile = portalTemplateFolder + templateFile;
            if (!File.Exists(portalTemplateFile))
            {
                File.Copy(hostTemplateFile, portalTemplateFile);
            }
        }

        /// <summary>Creates the portal.</summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUserId">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template">The portal template.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <see langword="true"/> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, int adminUserId, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            // Attempt to create a new portal
            int portalId = CreatePortal(this.hostSettings, portalName, homeDirectory, template.CultureCode);

            // Log the portal if into http context, if exception occurred in next step, we can remove the portal which is not really created.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("CreatingPortalId", portalId);
            }

            string message = Null.NullString;

            if (portalId != -1)
            {
                // add administrator
                int administratorId = adminUserId;
                var adminUser = new UserInfo();

                // add userportal record
                UserController.AddUserPortal(portalId, administratorId);

                // retrieve existing administrator
                try
                {
                    adminUser = UserController.GetUserById(portalId, administratorId);
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                }

                if (administratorId > 0)
                {
                    this.CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new CreatePortalException(message);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            // remove the portal id from http context as there is no exception.
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
            {
                HttpContext.Current.Items.Remove("CreatingPortalId");
            }

            return portalId;
        }

        /// <summary>Creates the portal.</summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUser">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template">The portal template.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <see langword="true"/> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            // Attempt to create a new portal
            int portalId = CreatePortal(this.hostSettings, portalName, homeDirectory, template.CultureCode);

            // Log the portal if into http context, if exception occurred in next step, we can remove the portal which is not really created.
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Items.Add("CreatingPortalId", portalId);
            }

            string message = Null.NullString;

            if (portalId != -1)
            {
                // add administrator
                int administratorId = Null.NullInteger;
                adminUser.PortalID = portalId;
                try
                {
                    UserCreateStatus createStatus = UserController.CreateUser(ref adminUser);
                    if (createStatus == UserCreateStatus.Success)
                    {
                        administratorId = adminUser.UserID;

                        // reload the UserInfo as when it was first created, it had no portal id and therefore
                        // used host profile definitions
                        adminUser = UserController.GetUserById(adminUser.PortalID, adminUser.UserID);
                    }
                    else
                    {
                        message += UserController.GetUserCreateStatus(createStatus);
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc);
                    message += Localization.GetString("CreateAdminUser.Error") + exc.Message + exc.StackTrace;
                }

                if (administratorId > 0)
                {
                    this.CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new CreatePortalException(message);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            // remove the portal id from http context as there is no exception.
            if (HttpContext.Current != null && HttpContext.Current.Items.Contains("CreatingPortalId"))
            {
                HttpContext.Current.Items.Remove("CreatingPortalId");
            }

            return portalId;
        }

        /// <summary>Get all the available portal templates grouped by culture.</summary>
        /// <returns>List of PortalTemplateInfo objects.</returns>
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.GetPortalTemplates instead")]
        public partial IList<PortalTemplateInfo> GetAvailablePortalTemplates()
        {
            var list = new List<PortalTemplateInfo>();

            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(Path.GetFileName).ToList();

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
                var langs = languageFileNames
                                .Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.OrdinalIgnoreCase))
                                .Select(x => GetCultureCode(x))
                                .Distinct()
                                .ToList();

                if (langs.Count != 0)
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(this.hostSettings, templateFilePath, x)));
                }
                else
                {
                    list.Add(new PortalTemplateInfo(this.hostSettings, templateFilePath, string.Empty));
                }
            }

            return list;
        }

        /// <summary>Gets information of a portal.</summary>
        /// <param name="portalId">ID of the portal.</param>
        /// <returns>PortalInfo object with portal definition.</returns>
        public PortalInfo GetPortal(int portalId)
        {
            if (portalId == -1)
            {
                return null;
            }

            string defaultLanguage = GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId);
            PortalInfo portal = this.GetPortal(portalId, defaultLanguage);
            if (portal == null)
            {
                // Active language may not be valid, so fallback to default language
                defaultLanguage = GetPortalDefaultLanguage(this.hostSettings, portalId);
                portal = this.GetPortal(portalId, defaultLanguage);
            }

            return portal;
        }

        /// <summary>Gets information of a portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>The <see cref="PortalInfo"/> instance or <see langword="null"/>.</returns>
        public PortalInfo GetPortal(int portalId, string cultureCode)
        {
            if (portalId == -1)
            {
                return null;
            }

            var portal = GetPortalInternal(this, portalId, cultureCode);
            if (Localization.ActiveLanguagesByPortalID(portalId) <= 1 || portal != null)
            {
                return portal;
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetPortalDefaultLanguage(this.hostSettings, portalId);
            }

            var fallbackLanguage = string.Empty;
            var userLocale = LocaleController.Instance.GetLocale(cultureCode);
            if (userLocale != null && !string.IsNullOrEmpty(userLocale.Fallback))
            {
                fallbackLanguage = userLocale.Fallback;
            }

            if (string.IsNullOrEmpty(fallbackLanguage))
            {
                fallbackLanguage = Localization.SystemLocale;
            }

            portal = GetPortalInternal(this, portalId, fallbackLanguage);
            if (portal != null)
            {
                return portal;
            }

            // if we cannot find any fallback, it means it's a non-portal default language
            DataProvider.Instance().EnsureLocalizationExists(portalId, GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId));
            DataCache.ClearHostCache(true);
            return GetPortalInternal(this, portalId, GetActivePortalLanguage(this.hostSettings, this.appStatus, portalId));
        }

        /// <summary>Gets the portal.</summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <returns>Portal info.</returns>
        public PortalInfo GetPortal(Guid uniqueId)
        {
            return this.GetPortalList(Null.NullString).SingleOrDefault(p => p.GUID == uniqueId);
        }

        /// <summary>Gets information from all portals.</summary>
        /// <returns>ArrayList of PortalInfo objects.</returns>
        public ArrayList GetPortals()
        {
            return new ArrayList(this.GetPortalList(Null.NullString));
        }

        /// <summary>Get portals in specific culture.</summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>A <see cref="List{T}"/> of <see cref="PortalInfo"/> instances.</returns>
        public List<PortalInfo> GetPortalList(string cultureCode)
        {
            string cacheKey = string.Format(DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
            return CBO.GetCachedObject<List<PortalInfo>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, cultureCode),
                c => CBO.FillCollection<PortalInfo>(DataProvider.Instance().GetPortals(cultureCode)));
        }

        /// <summary>Gets the portal settings dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>portal settings.</returns>
        public Dictionary<string, string> GetPortalSettings(int portalId)
        {
            return GetPortalSettingsDictionary(this.hostSettings, this.appStatus, portalId, string.Empty);
        }

        /// <summary>Gets the portal settings dictionary.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns>portal settings.</returns>
        public Dictionary<string, string> GetPortalSettings(int portalId, string cultureCode)
        {
            return GetPortalSettingsDictionary(this.hostSettings, this.appStatus, portalId, cultureCode);
        }

        /// <summary>Load info for a portal template.</summary>
        /// <param name="templatePath">Full path to the portal template.</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template.</param>
        /// <returns>A portal template.</returns>
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.GetPortalTemplate instead")]
        public partial PortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
            var template = new PortalTemplateInfo(this.hostSettings, templatePath, cultureCode);

            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

            return template;
        }

        /// <summary>Gets the portal space used bytes.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>Space used in bytes.</returns>
        public long GetPortalSpaceUsedBytes(int portalId)
        {
            long size = 0;
            IDataReader dr = DataProvider.Instance().GetPortalSpaceUsed(portalId);
            try
            {
                if (dr.Read())
                {
                    if (dr["SpaceUsed"] != DBNull.Value)
                    {
                        size = Convert.ToInt64(dr["SpaceUsed"]);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return size;
        }

        /// <summary>Verifies if there's enough space to upload a new file on the given portal.</summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded.</param>
        /// <returns>True if there's enough space available to upload the file.</returns>
        public bool HasSpaceAvailable(int portalId, long fileSizeBytes)
        {
            int hostSpace;
            if (portalId == -1)
            {
                hostSpace = 0;
            }
            else
            {
                PortalSettings ps = GetCurrentPortalSettingsInternal();
                if (ps != null && ps.PortalId == portalId)
                {
                    hostSpace = ps.HostSpace;
                }
                else
                {
                    PortalInfo portal = this.GetPortal(portalId);
                    hostSpace = portal.HostSpace;
                }
            }

            return (((this.GetPortalSpaceUsedBytes(portalId) + fileSizeBytes) / Math.Pow(1024, 2)) <= hostSpace) || hostSpace == 0;
        }

        /// <inheritdoc />
        public void MapLocalizedSpecialPages(int portalId, string cultureCode)
        {
            DataCache.ClearHostCache(true);
            DataProvider.Instance().EnsureLocalizationExists(portalId, cultureCode);

            PortalInfo defaultPortal = this.GetPortal(portalId, GetPortalDefaultLanguage(this.hostSettings, portalId));
            PortalInfo targetPortal = this.GetPortal(portalId, cultureCode);

            Locale targetLocale = LocaleController.Instance.GetLocale(cultureCode);
            TabInfo tempTab;
            if (defaultPortal.HomeTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.HomeTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.HomeTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.LoginTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.LoginTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.LoginTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.RegisterTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.RegisterTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.RegisterTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.SplashTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.SplashTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SplashTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.UserTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.UserTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.UserTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.SearchTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.SearchTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SearchTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.Custom404TabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.Custom404TabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.Custom404TabId = tempTab.TabID;
                }
            }

            if (defaultPortal.Custom500TabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.Custom500TabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.Custom500TabId = tempTab.TabID;
                }
            }

            if (defaultPortal.TermsTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.TermsTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.TermsTabId = tempTab.TabID;
                }
            }

            if (defaultPortal.PrivacyTabId != Null.NullInteger)
            {
                tempTab = TabController.Instance.GetTabByCulture(defaultPortal.PrivacyTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.PrivacyTabId = tempTab.TabID;
                }
            }

            this.UpdatePortalInternal(targetPortal, false);
        }

        /// <inheritdoc />
        public void RemovePortalLocalization(int portalId, string cultureCode, bool clearCache = true)
        {
            DataProvider.Instance().RemovePortalLocalization(portalId, cultureCode);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, false);
            }
        }

        /// <summary>Processes a template file for the new portal.</summary>
        /// <param name="portalId">PortalId of the new portal.</param>
        /// <param name="template">The template.</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user.</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateController.Instance.ApplyPortalTemplate instead")]
        public partial void ParseTemplate(int portalId, PortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            var t = new Templates.PortalTemplateInfo(template.TemplateFilePath, template.CultureCode);
            var portalTemplateImporter = new PortalTemplateImporter(t);
            portalTemplateImporter.ParseTemplate(this.businessControllerProvider, portalId, administratorId, mergeTabs.ToNewEnum(), isNewPortal);
        }

        /// <summary>Processes the resource file for the template file selected.</summary>
        /// <param name="portalPath">New portal's folder.</param>
        /// <param name="resoureceFile">full path to the resource file.</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to disable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        public void ProcessResourceFileExplicit(string portalPath, string resoureceFile)
        {
            try
            {
                FileSystemUtils.UnzipResources(new ZipArchive(new FileStream(resoureceFile, FileMode.Open, FileAccess.Read)), portalPath);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        /// <summary>Updates the portal expiry.</summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        public void UpdatePortalExpiry(int portalId, string cultureCode)
        {
            var portal = this.GetPortal(portalId, cultureCode);

            if (portal.ExpiryDate == Null.NullDate)
            {
                portal.ExpiryDate = DateTime.Now;
            }

            portal.ExpiryDate = portal.ExpiryDate.AddMonths(1);

            this.UpdatePortalInfo(portal);
        }

        /// <inheritdoc />
        public void UpdatePortalInfo(PortalInfo portal)
        {
            this.UpdatePortalInternal(portal, true);
        }

        /// <summary>Gets the current portal settings.</summary>
        /// <returns>portal settings.</returns>
        PortalSettings IPortalController.GetCurrentPortalSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        /// <inheritdoc/>
        IAbPortalSettings IPortalController.GetCurrentSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        /// <inheritdoc/>
        [Obsolete("Deprecated in DotNetNuke 9.2.0. Use the overloaded one with the 'isSecure' parameter instead. Scheduled removal in v11.0.0.")]
        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSettingInternal(
                this.hostSettings,
                this.appStatus,
                this.eventLogger,
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode,
                false);
        }

        /// <inheritdoc />
        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            UpdatePortalSettingInternal(
                this.hostSettings,
                this.appStatus,
                this.eventLogger,
                portalID,
                settingName,
                settingValue,
                clearCache,
                cultureCode,
                isSecure);
        }

        /// <inheritdoc cref="DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo" />
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead")]
        public partial int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, PortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            return this.CreatePortal(portalName, adminUser, description, keyWords, template.ToNewPortalTemplateInfo(), homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        /// <inheritdoc cref="DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo" />
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead")]
        public partial int CreatePortal(string portalName, int adminUserId, string description, string keyWords, PortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            return this.CreatePortal(portalName, adminUserId, description, keyWords, template.ToNewPortalTemplateInfo(), homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        internal static void EnsureRequiredEventLogTypesExist()
        {
            if (!DoesLogTypeExists(EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString()))
            {
                // Add 404 Log
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                    LogTypeFriendlyName = "HTTP Error Code 404 Page Not Found",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);

                // Add LogType
                var logTypeConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.IP_LOGIN_BANNED.ToString()))
            {
                // Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                    LogTypeFriendlyName = "HTTP Error Code Forbidden IP address rejected",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                // Add LogType
                var logTypeFilterConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.TABURL_CREATED.ToString()))
            {
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                    LogTypeFriendlyName = "TabURL created",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationSuccess",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL updated";
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
                LogController.Instance.AddLogType(logTypeInfo);

                // Add LogType
                var logTypeUrlConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = false,
                    LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.SCRIPT_COLLISION.ToString()))
            {
                // Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.SCRIPT_COLLISION.ToString(),
                    LogTypeFriendlyName = "Javscript library registration resolved script collision",
                    LogTypeDescription = string.Empty,
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType",
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                // Add LogType
                var logTypeFilterConf = new LogTypeConfigInfo
                {
                    LoggingIsActive = true,
                    LogTypeKey = EventLogController.EventLogType.SCRIPT_COLLISION.ToString(),
                    KeepMostRecent = "100",
                    NotificationThreshold = 1,
                    NotificationThresholdTime = 1,
                    NotificationThresholdTimeType = LogTypeConfigInfo.NotificationThresholdTimeTypes.Seconds,
                    MailFromAddress = Null.NullString,
                    MailToAddress = Null.NullString,
                    LogTypePortalID = "*",
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }
        }

        /// <inheritdoc/>
        protected override Func<IPortalController> GetFactory()
        {
            return Globals.DependencyProvider.GetRequiredService<IPortalController>;
        }

        private static int CreatePortal(IHostSettings hostSettings, string portalName, string homeDirectory, string cultureCode)
        {
            // add portal
            int portalId = -1;
            try
            {
                // Use host settings as default values for these parameters
                // This can be overwritten on the portal template
                var hostController = HostController.Instance;
                var demoPeriod = TimeSpan.FromDays(hostController.GetInteger("DemoPeriod"));
                var datExpiryDate = demoPeriod > TimeSpan.Zero
                    ? Convert.ToDateTime(Globals.GetMediumDate(DateTime.Now.Add(demoPeriod).ToString(CultureInfo.InvariantCulture)))
                    : Null.NullDate;

                var hostCurrency = hostController.GetString("HostCurrency");
                if (string.IsNullOrEmpty(hostCurrency))
                {
                    hostCurrency = "USD";
                }

                portalId = DataProvider.Instance().CreatePortal(
                    portalName,
                    hostCurrency,
                    datExpiryDate,
                    hostController.GetDouble("HostFee", 0),
                    hostSettings.HostSpace,
                    hostSettings.PageQuota,
                    hostSettings.UserQuota,
                    0, // site log history function has been removed.
                    homeDirectory,
                    cultureCode,
                    UserController.Instance.GetCurrentUserInfo().UserID);

                // clear portal cache
                DataCache.ClearHostCache(true);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                // should be no exception, but suppress just in case
            }

            return portalId;
        }

        private static void DeletePortalInternal(int portalId)
        {
            UserController.DeleteUsers(portalId, false, true);

            var portal = Instance.GetPortal(portalId);

            DataProvider.Instance().DeletePortalInfo(portalId);

            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = EventLogController.EventLogType.PORTAL_DELETED.ToString(),
                };
                log.LogProperties.Add(new LogDetailInfo("Delete Portal:", portal.PortalName));
                log.LogProperties.Add(new LogDetailInfo("PortalID:", portal.PortalID.ToString()));
                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            DataCache.ClearHostCache(true);

            // queue remove portal from search index
            var document = new SearchDocumentToDelete
            {
                PortalId = portalId,
            };

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        private static bool DoesLogTypeExists(string logTypeKey)
        {
            LogTypeInfo logType;
            Dictionary<string, LogTypeInfo> logTypeDictionary = LogController.Instance.GetLogTypeInfoDictionary();
            logTypeDictionary.TryGetValue(logTypeKey, out logType);
            if (logType == null)
            {
                return false;
            }

            return true;
        }

        private static PortalSettings GetCurrentPortalSettingsInternal()
        {
            PortalSettings objPortalSettings = null;
            if (HttpContext.Current != null)
            {
                objPortalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }

            return objPortalSettings;
        }

        private static PortalInfo GetPortalInternal(PortalController portalController, int portalId, string cultureCode)
        {
            return portalController.GetPortalList(cultureCode).SingleOrDefault(p => ((IPortalInfo)p).PortalId == portalId);
        }

        private static object GetPortalDefaultLanguageCallBack(CacheItemArgs cacheItemArgs)
        {
            int portalId = (int)cacheItemArgs.ParamList[0];
            return DataProvider.Instance().GetPortalDefaultLanguage(portalId);
        }

        private static object GetPortalDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            var hostSettings = (IHostSettings)cacheItemArgs.Params[0];
            var dataProvider = (DataProvider)cacheItemArgs.Params[1];
            var portalDic = new Dictionary<int, int>();
            if (hostSettings.PerformanceSetting == PerformanceSettings.NoCaching)
            {
                return portalDic;
            }

            // get all tabs
            const int intField = 0;
            using var dr = dataProvider.GetTabPaths(Null.NullInteger, Null.NullString);
            try
            {
                while (dr.Read())
                {
                    // add to dictionary
                    portalDic[Convert.ToInt32(Null.SetNull(dr["TabID"], intField))] = Convert.ToInt32(Null.SetNull(dr["PortalID"], intField));
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }

            return portalDic;
        }

        private static object GetPortalSettingsDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dicSettings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (portalId <= -1)
            {
                return dicSettings;
            }

            var cultureCode = Convert.ToString(cacheItemArgs.ParamList[1]);
            if (string.IsNullOrEmpty(cultureCode))
            {
                var hostSettings = (IHostSettings)cacheItemArgs.ParamList[2];
                var appStatus = (IApplicationStatusInfo)cacheItemArgs.ParamList[3];
                cultureCode = GetActivePortalLanguage(hostSettings, appStatus, portalId);
            }

            var dr = DataProvider.Instance().GetPortalSettings(portalId, cultureCode);
            try
            {
                while (dr.Read())
                {
                    if (dr.IsDBNull(1))
                    {
                        continue;
                    }

                    var key = dr.GetString(0);
                    if (dicSettings.ContainsKey(key))
                    {
                        dicSettings[key] = dr.GetString(1);
                        var log = new LogInfo
                        {
                            LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString(),
                        };
                        log.AddProperty("Duplicate PortalSettings Key", key);
                        LogController.Instance.AddLog(log);
                    }
                    else
                    {
                        dicSettings.Add(key, dr.GetString(1));
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.LogException(exc);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return dicSettings;
        }

        private static void EnsureRequiredProvidersForFolderTypes()
        {
            if (ComponentFactory.GetComponent<CryptographyProvider>() == null)
            {
                ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(FipsCompilanceCryptographyProvider)));
                ComponentFactory.RegisterComponentInstance<CryptographyProvider>(new FipsCompilanceCryptographyProvider());
            }
        }

        private static void EnsureFolderProviderRegistration<TAbstract>(FolderTypeConfig folderTypeConfig, XmlDocument webConfig)
            where TAbstract : class
        {
            var providerBusinessClassNode = webConfig.SelectSingleNode("configuration/dotnetnuke/folder/providers/add[@name='" + folderTypeConfig.Provider + "']");

            var typeClass = Type.GetType(providerBusinessClassNode.Attributes["type"].Value);
            if (typeClass != null)
            {
                ComponentFactory.RegisterComponentInstance<TAbstract>(folderTypeConfig.Provider, Activator.CreateInstance(typeClass));
            }
        }

        private static bool EnableBrowserLanguageInDefault(IHostSettings hostSettings, IApplicationStatusInfo appStatus, int portalId)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                GetPortalSettingsDictionary(hostSettings, appStatus, portalId, Localization.SystemLocale).TryGetValue("EnableBrowserLanguage", out var setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = hostSettings.EnableBrowserLanguage;
                }
                else
                {
                    retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return retValue;
        }

        private static Dictionary<string, string> GetPortalSettingsDictionary(IHostSettings hostSettings, IApplicationStatusInfo appStatus, int portalId, string cultureCode)
        {
            var httpContext = HttpContext.Current;

            if (string.IsNullOrEmpty(cultureCode) && portalId > -1)
            {
                cultureCode = GetActivePortalLanguageFromHttpContext(hostSettings, appStatus, httpContext, portalId);
            }

            // Get PortalSettings from Context or from cache
            var dictionaryKey = string.Format(HttpContextKeyPortalSettingsDictionary, portalId, cultureCode);
            Dictionary<string, string> dictionary = null;
            if (httpContext != null)
            {
                dictionary = httpContext.Items[dictionaryKey] as Dictionary<string, string>;
            }

            if (dictionary == null)
            {
                var cacheKey = string.Format(DataCache.PortalSettingsCacheKey, portalId, cultureCode);
                dictionary = CBO.GetCachedObject<Dictionary<string, string>>(
                    hostSettings,
                    new CacheItemArgs(
                        cacheKey,
                        DataCache.PortalSettingsCacheTimeOut,
                        DataCache.PortalSettingsCachePriority,
                        portalId,
                        cultureCode,
                        hostSettings,
                        appStatus),
                    GetPortalSettingsDictionaryCallback,
                    true);
                if (httpContext != null)
                {
                    httpContext.Items[dictionaryKey] = dictionary;
                }
            }

            return dictionary;
        }

        private static string GetActivePortalLanguageFromHttpContext(IHostSettings hostSettings, IApplicationStatusInfo appStatus, HttpContext httpContext, int portalId)
        {
            var cultureCode = string.Empty;

            // Lookup culturecode but cache it in the HttpContext for performance
            var activeLanguageKey = $"ActivePortalLanguage{portalId}";
            if (httpContext != null)
            {
                cultureCode = (string)httpContext.Items[activeLanguageKey];
            }

            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetActivePortalLanguage(hostSettings, appStatus, portalId);
                if (httpContext != null)
                {
                    httpContext.Items[activeLanguageKey] = cultureCode;
                }
            }

            return cultureCode;
        }

        private static void UpdatePortalSettingInternal(IHostSettings hostSettings, IApplicationStatusInfo appStatus, IEventLogger eventLogger, int portalId, string settingName, string settingValue, bool clearCache, string cultureCode, bool isSecure)
        {
            string currentSetting = GetPortalSetting(settingName, portalId, string.Empty, cultureCode);

            if (isSecure && !string.IsNullOrEmpty(settingName) && !string.IsNullOrEmpty(settingValue))
            {
                settingValue = Security.FIPSCompliant.EncryptAES(settingValue, Config.GetDecryptionkey(), hostSettings.Guid);
            }

            if (currentSetting != settingValue)
            {
                DataProvider.Instance().UpdatePortalSetting(portalId, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID, cultureCode, isSecure);
                eventLogger.AddLog(settingName + ((cultureCode == Null.NullString) ? string.Empty : " (" + cultureCode + ")"), settingValue, GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogType.PORTAL_SETTING_UPDATED);
                if (clearCache)
                {
                    DataCache.ClearPortalCache(portalId, false);
                    DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);

                    var httpContext = HttpContext.Current;
                    if (httpContext != null)
                    {
                        var cultureCodeForKey = GetActivePortalLanguageFromHttpContext(hostSettings, appStatus, httpContext, portalId);
                        var dictionaryKey = string.Format(HttpContextKeyPortalSettingsDictionary, portalId, cultureCodeForKey);
                        httpContext.Items[dictionaryKey] = null;
                    }
                }

                EventManager.Instance.OnPortalSettingUpdated(new PortalSettingUpdatedEventArgs
                {
                    PortalId = portalId,
                    SettingName = settingName,
                    SettingValue = settingValue,
                });
            }
        }

        private static string GetCultureCode(string languageFileName)
        {
            // e.g. "default template.template.en-US.resx"
            return languageFileName.GetLocaleCodeFromFileName();
        }

        private static string GetTemplateName(string languageFileName)
        {
            // e.g. "default template.template.en-US.resx"
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private void CreatePortalInternal(int portalId, string portalName, UserInfo adminUser, string description, string keyWords, IPortalTemplateInfo template, string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal, ref string message)
        {
            // Add default workflows
            try
            {
                SystemWorkflowManager.Instance.CreateSystemWorkflows(portalId);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            var portalTemplateImporter = new PortalTemplateImporter(template);

            if (string.IsNullOrEmpty(homeDirectory))
            {
                homeDirectory = "Portals/" + portalId;
            }

            string mappedHomeDirectory = string.Format($@"{Globals.ApplicationMapPath}\{homeDirectory}\").Replace("/", @"\");

            if (Directory.Exists(mappedHomeDirectory))
            {
                message += string.Format(Localization.GetString("CreatePortalHomeFolderExists.Error"), homeDirectory);
                throw new CreatePortalException(message);
            }

            message += portalTemplateImporter.CreateProfileDefinitions(portalId);

            if (string.IsNullOrEmpty(message))
            {
                try
                {
                    // the upload directory may already exist if this is a new DB working with a previously installed application
                    if (Directory.Exists(mappedHomeDirectory))
                    {
                        Globals.DeleteFolderRecursive(mappedHomeDirectory);
                    }
                }
                catch (Exception exc1)
                {
                    Logger.Error(exc1);
                    message += Localization.GetString("DeleteUploadFolder.Error") + exc1.Message + exc1.StackTrace;
                }

                // Set up Child Portal
                if (message == Null.NullString)
                {
                    if (isChildPortal)
                    {
                        message = CreateChildPortalFolder(childPath);
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        // create the upload directory for the new portal
                        Directory.CreateDirectory(mappedHomeDirectory);

                        // ensure that the Templates folder exists
                        string templateFolder = $"{mappedHomeDirectory}Templates";
                        if (!Directory.Exists(templateFolder))
                        {
                            Directory.CreateDirectory(templateFolder);
                        }

                        // ensure that the Users folder exists
                        string usersFolder = string.Format("{0}Users", mappedHomeDirectory);
                        if (!Directory.Exists(usersFolder))
                        {
                            Directory.CreateDirectory(usersFolder);
                        }

                        // copy the default page template
                        this.CopyPageTemplate("Default.page.template", mappedHomeDirectory);

                        // process zip resource file if present
                        if (File.Exists(template.ResourceFilePath))
                        {
                            this.ProcessResourceFileExplicit(mappedHomeDirectory, template.ResourceFilePath);
                        }
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("ChildPortal.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        FolderMappingController.Instance.AddDefaultFolderTypes(portalId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("DefaultFolderMappings.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                LocaleCollection newPortalLocales = null;
                if (message == Null.NullString)
                {
                    try
                    {
                        this.CreatePredefinedFolderTypes(portalId);
                        portalTemplateImporter.ParseTemplateInternal(this.businessControllerProvider, portalId, adminUser.UserID, PortalTemplateModuleAction.Replace.ToNewEnum(), true, out newPortalLocales);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                        message += Localization.GetString("PortalTemplate.Error") + exc1.Message + exc1.StackTrace;
                    }
                }
                else
                {
                    throw new CreatePortalException(message);
                }

                if (message == Null.NullString)
                {
                    var portal = this.GetPortal(portalId);
                    portal.Description = description;
                    portal.KeyWords = keyWords;
                    portal.UserTabId = TabController.GetTabByTabPath(portal.PortalID, "//UserProfile", portal.CultureCode);
                    if (portal.UserTabId == -1)
                    {
                        portal.UserTabId = TabController.GetTabByTabPath(portal.PortalID, "//ActivityFeed", portal.CultureCode);
                    }

                    portal.SearchTabId = TabController.GetTabByTabPath(portal.PortalID, "//SearchResults", portal.CultureCode);
                    this.UpdatePortalInfo(portal);

                    adminUser.Profile.PreferredLocale = portal.DefaultLanguage;
                    var portalSettings = new PortalSettings(portal);
                    adminUser.Profile.PreferredTimeZone = portalSettings.TimeZone;
                    UserController.UpdateUser(portal.PortalID, adminUser);

                    DesktopModuleController.AddDesktopModulesToPortal(portalId);

                    this.AddPortalAlias(portalId, portalAlias);

                    UpdatePortalSetting(portalId, "DefaultPortalAlias", portalAlias, false);

                    DataCache.ClearPortalCache(portalId, true);

                    if (newPortalLocales != null)
                    {
                        foreach (Locale newPortalLocale in newPortalLocales.AllValues)
                        {
                            Localization.AddLanguageToPortal(portalId, newPortalLocale.LanguageId, false);
                            if (portalSettings.ContentLocalizationEnabled)
                            {
                                this.MapLocalizedSpecialPages(portalId, newPortalLocale.Code);
                            }
                        }
                    }

                    try
                    {
                        RelationshipController.Instance.CreateDefaultRelationshipsForPortal(portalId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    // add profanity list to new portal
                    try
                    {
                        const string listName = "ProfanityFilter";
                        var listController = new ListController();
                        var entry = new ListEntryInfo
                        {
                            PortalID = portalId,
                            SystemList = false,
                            ListName = listName + "-" + portalId,
                        };
                        listController.AddListEntry(entry);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    // add banned password list to new portal
                    try
                    {
                        const string listName = "BannedPasswords";
                        var listController = new ListController();
                        var entry = new ListEntryInfo
                        {
                            PortalID = portalId,
                            SystemList = false,
                            ListName = listName + "-" + portalId,
                        };
                        listController.AddListEntry(entry);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Error(exc1);
                    }

                    ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();

                    try
                    {
                        var log = new LogInfo
                        {
                            BypassBuffering = true,
                            LogTypeKey = EventLogController.EventLogType.PORTAL_CREATED.ToString(),
                        };
                        log.LogProperties.Add(new LogDetailInfo("Install Portal:", portalName));
                        log.LogProperties.Add(new LogDetailInfo("FirstName:", adminUser.FirstName));
                        log.LogProperties.Add(new LogDetailInfo("LastName:", adminUser.LastName));
                        log.LogProperties.Add(new LogDetailInfo("Username:", adminUser.Username));
                        log.LogProperties.Add(new LogDetailInfo("Email:", adminUser.Email));
                        log.LogProperties.Add(new LogDetailInfo("Description:", description));
                        log.LogProperties.Add(new LogDetailInfo("Keywords:", keyWords));
                        log.LogProperties.Add(new LogDetailInfo("Template:", template.TemplateFilePath));
                        log.LogProperties.Add(new LogDetailInfo("TemplateCulture:", template.CultureCode));
                        log.LogProperties.Add(new LogDetailInfo("HomeDirectory:", homeDirectory));
                        log.LogProperties.Add(new LogDetailInfo("PortalAlias:", portalAlias));
                        log.LogProperties.Add(new LogDetailInfo("ServerPath:", serverPath));
                        log.LogProperties.Add(new LogDetailInfo("ChildPath:", childPath));
                        log.LogProperties.Add(new LogDetailInfo("IsChildPortal:", isChildPortal.ToString()));
                        LogController.Instance.AddLog(log);
                    }
                    catch (Exception exc)
                    {
                        Logger.Error(exc);
                    }

                    EventManager.Instance.OnPortalCreated(new PortalCreatedEventArgs { PortalId = portalId });
                }
                else
                {
                    throw new CreatePortalException(message);
                }
            }
            else
            {
                DeletePortalInternal(portalId);
                throw new CreatePortalException(message);
            }
        }

        private void CreatePredefinedFolderTypes(int portalId)
        {
            try
            {
                EnsureRequiredProvidersForFolderTypes();
            }
            catch (Exception ex)
            {
                Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error"), ex);
            }

            var webConfig = Config.Load();
            foreach (FolderTypeConfig folderTypeConfig in FolderMappingsConfigController.Instance.FolderTypes)
            {
                try
                {
                    EnsureFolderProviderRegistration<FolderProvider>(folderTypeConfig, webConfig);
                    FolderMappingController.Instance.AddFolderMapping(this.GetFolderMappingFromConfig(
                        folderTypeConfig,
                        portalId));
                }
                catch (Exception ex)
                {
                    Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error") + ": " + folderTypeConfig.Name, ex);
                }
            }
        }

        private string EnsureSettingValue(string folderProviderType, FolderTypeSettingConfig settingNode, int portalId)
        {
            var ensuredSettingValue =
                settingNode.Value.Replace("{PortalId}", (portalId != -1) ? portalId.ToString(CultureInfo.InvariantCulture) : "_default").Replace("{HostId}", this.hostSettings.Guid);
            if (settingNode.Encrypt)
            {
                return FolderProvider.Instance(folderProviderType).EncryptValue(ensuredSettingValue);
            }

            return ensuredSettingValue;
        }

        private FolderMappingInfo GetFolderMappingFromConfig(FolderTypeConfig node, int portalId)
        {
            var folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName = node.Name,
                FolderProviderType = node.Provider,
            };

            foreach (FolderTypeSettingConfig settingNode in node.Settings)
            {
                var settingValue = this.EnsureSettingValue(folderMapping.FolderProviderType, settingNode, portalId);
                folderMapping.FolderMappingSettings.Add(settingNode.Name, settingValue);
            }

            return folderMapping;
        }

        private void UpdatePortalInternal(PortalInfo portal, bool clearCache)
        {
            var processorPassword = portal.ProcessorPassword;
            if (!string.IsNullOrEmpty(processorPassword))
            {
                processorPassword = Security.FIPSCompliant.EncryptAES(processorPassword, Config.GetDecryptionkey(), this.hostSettings.Guid);
            }

            DataProvider.Instance().UpdatePortalInfo(
                portal.PortalID,
                portal.PortalGroupID,
                portal.PortalName,
                portal.LogoFile,
                portal.FooterText,
                portal.ExpiryDate,
                portal.UserRegistration,
                portal.BannerAdvertising,
                portal.Currency,
                portal.AdministratorId,
                portal.HostFee,
                portal.HostSpace,
                portal.PageQuota,
                portal.UserQuota,
                portal.PaymentProcessor,
                portal.ProcessorUserId,
                processorPassword,
                portal.Description,
                portal.KeyWords,
                portal.BackgroundFile,
                0, // site log history function has been removed.
                portal.SplashTabId,
                portal.HomeTabId,
                portal.LoginTabId,
                portal.RegisterTabId,
                portal.UserTabId,
                portal.SearchTabId,
                portal.Custom404TabId,
                portal.Custom500TabId,
                portal.TermsTabId,
                portal.PrivacyTabId,
                portal.DefaultLanguage,
                portal.HomeDirectory,
                UserController.Instance.GetCurrentUserInfo().UserID,
                portal.CultureCode);

            EventLogController.Instance.AddLog("PortalId", portal.PortalID.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_UPDATED);

            // ensure a localization item exists (in case a new default language has been set)
            DataProvider.Instance().EnsureLocalizationExists(portal.PortalID, portal.DefaultLanguage);

            // clear portal cache
            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }
        }

        /// <inheritdoc cref="DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo" />
        [DnnDeprecated(9, 11, 1, "Use DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo instead")]
        public partial class PortalTemplateInfo
        {
            private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));
            private readonly IHostSettings hostSettings;
            private readonly string originalCultureCode;
            private string resourceFilePath;

            /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
            /// <param name="templateFilePath">The path to the template file.</param>
            /// <param name="cultureCode">The culture code.</param>
            public PortalTemplateInfo(string templateFilePath, string cultureCode)
                : this(Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(), templateFilePath, cultureCode)
            {
            }

            /// <summary>Initializes a new instance of the <see cref="PortalTemplateInfo"/> class.</summary>
            /// <param name="hostSettings">The host settings.</param>
            /// <param name="templateFilePath">The path to the template file.</param>
            /// <param name="cultureCode">The culture code.</param>
            public PortalTemplateInfo(IHostSettings hostSettings, string templateFilePath, string cultureCode)
            {
                this.TemplateFilePath = templateFilePath;
                this.hostSettings = hostSettings;
                this.originalCultureCode = cultureCode;

                this.InitLocalizationFields(cultureCode);
                this.InitNameAndDescription();
            }

            public string ResourceFilePath
            {
                get
                {
                    if (this.resourceFilePath == null)
                    {
                        this.resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(this.TemplateFilePath);
                    }

                    return this.resourceFilePath;
                }
            }

            public string Name { get; private set; }

            public string CultureCode { get; private set; }

            public string TemplateFilePath { get; private set; }

            public string LanguageFilePath { get; private set; }

            public string Description { get; private set; }

            internal DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo ToNewPortalTemplateInfo()
            {
                return new DotNetNuke.Entities.Portals.Templates.PortalTemplateInfo(this.TemplateFilePath, this.originalCultureCode);
            }

            private static string ReadLanguageFileValue(XDocument xmlDoc, string name)
            {
                return (from f in xmlDoc.Descendants("data")
                        where (string)f.Attribute("name") == name
                        select (string)f.Element("value")).SingleOrDefault();
            }

            private void InitNameAndDescription()
            {
                if (!string.IsNullOrEmpty(this.LanguageFilePath))
                {
                    this.LoadNameAndDescriptionFromLanguageFile();
                }

                if (string.IsNullOrEmpty(this.Name))
                {
                    this.Name = Path.GetFileNameWithoutExtension(this.TemplateFilePath);
                }

                if (string.IsNullOrEmpty(this.Description))
                {
                    this.LoadDescriptionFromTemplateFile();
                }
            }

            private void LoadDescriptionFromTemplateFile()
            {
                try
                {
                    XDocument xmlDoc;
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.TemplateFilePath))
                    {
                        xmlDoc = XDocument.Load(reader);
                    }

                    this.Description = xmlDoc.Elements("portal").Elements("description").SingleOrDefault().Value;
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
                }
            }

            private void LoadNameAndDescriptionFromLanguageFile()
            {
                try
                {
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(this.LanguageFilePath))
                    {
                        var xmlDoc = XDocument.Load(reader);

                        this.Name = ReadLanguageFileValue(xmlDoc, "LocalizedTemplateName.Text");
                        this.Description = ReadLanguageFileValue(xmlDoc, "PortalDescription.Text");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + this.TemplateFilePath, e);
                }
            }

            private void InitLocalizationFields(string cultureCode)
            {
                this.LanguageFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(this.TemplateFilePath, cultureCode);
                if (string.IsNullOrEmpty(this.LanguageFilePath))
                {
                    (cultureCode, _) = PortalTemplateIO.Instance.GetTemplateLanguages(this.TemplateFilePath);
                    if (string.IsNullOrEmpty(cultureCode))
                    {
                        var portalSettings = PortalSettings.Current;
                        cultureCode = portalSettings != null ? PortalController.GetPortalDefaultLanguage(this.hostSettings, portalSettings.PortalId) : Localization.SystemLocale;
                    }
                }

                this.CultureCode = cultureCode;
            }
        }
    }
}
