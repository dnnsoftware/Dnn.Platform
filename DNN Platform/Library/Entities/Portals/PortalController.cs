#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cryptography;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
//using DotNetNuke.Services.Upgrade.Internals.InstallConfiguration;
using DotNetNuke.Services.Search.Entities;
using DotNetNuke.Web.Client;
using ICSharpCode.SharpZipLib.Zip;
using Assembly = System.Reflection.Assembly;
using FileInfo = DotNetNuke.Services.FileSystem.FileInfo;

#endregion

namespace DotNetNuke.Entities.Portals
{
    /// <summary>
	/// PoralController provides business layer of poatal.
	/// </summary>
	/// <remarks>
	/// DotNetNuke supports the concept of virtualised sites in a single install. This means that multiple sites, 
	/// each potentially with multiple unique URL's, can exist in one instance of DotNetNuke i.e. one set of files and one database.
	/// </remarks>
    public partial class PortalController : ServiceLocator<IPortalController, PortalController>, IPortalController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (PortalController));

        public const string HtmlText_TimeToAutoSave = "HtmlText_TimeToAutoSave";
        public const string HtmlText_AutoSaveEnabled = "HtmlText_AutoSaveEnabled";

        private event EventHandler<PortalCreatedEventArgs> PortalCreated;

        protected override Func<IPortalController> GetFactory()
        {
            return () => new PortalController();
        }

        public PortalController()
        {
            foreach (var handlers in EventHandlersContainer<IPortalEventHandlers>.Instance.EventHandlers)
            {
                PortalCreated += handlers.Value.PortalCreated;
            }
        }

        #region Private Methods

        private void AddFolderPermissions(int portalId, int folderId)
        {
            var portal = GetPortal(portalId);
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);
            var permissionController = new PermissionController();
            foreach (PermissionInfo permission in permissionController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", ""))
            {
                var folderPermission = new FolderPermissionInfo(permission)
                                                    {
                                                        FolderID = folder.FolderID,
                                                        RoleID = portal.AdministratorRoleId,
                                                        AllowAccess = true
                                                    };

                folder.FolderPermissions.Add(folderPermission);
                if (permission.PermissionKey == "READ")
                {
                    //add READ permissions to the All Users Role
                    folderManager.AddAllUserReadPermission(folder, permission);
                }
            }
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        private static void CreateDefaultPortalRoles(int portalId, int administratorId, ref int administratorRoleId, ref int registeredRoleId, ref int subscriberRoleId, int unverifiedRoleId)
        {
            //create required roles if not already created
            if (administratorRoleId == -1)
            {
                administratorRoleId = CreateRole(portalId, "Administrators", "Administrators of this Website", 0, 0, "M", 0, 0, "N", false, false);
            }
            if (registeredRoleId == -1)
            {
                registeredRoleId = CreateRole(portalId, "Registered Users", "Registered Users", 0, 0, "M", 0, 0, "N", false, true);
            }
            if (subscriberRoleId == -1)
            {
                subscriberRoleId = CreateRole(portalId, "Subscribers", "A public role for site subscriptions", 0, 0, "M", 0, 0, "N", true, true);
            }
            if (unverifiedRoleId == -1)
            {
                CreateRole(portalId, "Unverified Users", "Unverified Users", 0, 0, "M", 0, 0, "N", false, false);
            }
            RoleController.Instance.AddUserRole(portalId, administratorId, administratorRoleId, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
            RoleController.Instance.AddUserRole(portalId, administratorId, registeredRoleId, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
            RoleController.Instance.AddUserRole(portalId, administratorId, subscriberRoleId, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
        }

        private void CreatePortalInternal(int portalId, string portalName, UserInfo adminUser, string description, string keyWords, PortalTemplateInfo template,
                                 string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal, ref string message)
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

            string templatePath, templateFile;
            PrepareLocalizedPortalTemplate(template, out templatePath, out templateFile);
            string mergedTemplatePath = Path.Combine(templatePath, templateFile);

            if (String.IsNullOrEmpty(homeDirectory))
            {
                homeDirectory = "Portals/" + portalId;
            }
            string mappedHomeDirectory = String.Format(Globals.ApplicationMapPath + "\\" + homeDirectory + "\\").Replace("/", "\\");

            message += CreateProfileDefinitions(portalId, mergedTemplatePath);

            if (String.IsNullOrEmpty(message))
            {
                try
                {
                    //the upload directory may already exist if this is a new DB working with a previously installed application
                    if (Directory.Exists(mappedHomeDirectory))
                    {
                        Globals.DeleteFolderRecursive(mappedHomeDirectory);
                    }
                }
                catch (Exception Exc)
                {
                    Logger.Error(Exc);
                    message += Localization.GetString("DeleteUploadFolder.Error") + Exc.Message + Exc.StackTrace;
                }

                //Set up Child Portal
                if (message == Null.NullString)
                {
                    if (isChildPortal)
                    {
                        message = CreateChildPortalFolder(childPath);
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        //create the upload directory for the new portal
                        Directory.CreateDirectory(mappedHomeDirectory);
                        //ensure that the Templates folder exists
                        string templateFolder = String.Format("{0}Templates", mappedHomeDirectory);
                        if (!Directory.Exists(templateFolder))
                        {
                            Directory.CreateDirectory(templateFolder);
                        }

                        //ensure that the Users folder exists
                        string usersFolder = String.Format("{0}Users", mappedHomeDirectory);
                        if (!Directory.Exists(usersFolder))
                        {
                            Directory.CreateDirectory(usersFolder);
                        }

                        //copy the default page template
                        CopyPageTemplate("Default.page.template", mappedHomeDirectory);

                        // process zip resource file if present
                        if (File.Exists(template.ResourceFilePath))
                        {
                            ProcessResourceFileExplicit(mappedHomeDirectory, template.ResourceFilePath);
                        }

                        //copy getting started css into portal's folder.
                        var hostGettingStartedFile = string.Format("{0}GettingStarted.css", Globals.HostMapPath);
                        if (File.Exists(hostGettingStartedFile))
                        {
                            var portalFile = mappedHomeDirectory + "GettingStarted.css";
                            if (!File.Exists(portalFile))
                            {
                                File.Copy(hostGettingStartedFile, portalFile);
                            }
                        }
                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                        message += Localization.GetString("ChildPortal.Error") + Exc.Message + Exc.StackTrace;
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                if (message == Null.NullString)
                {
                    try
                    {
                        FolderMappingController.Instance.AddDefaultFolderTypes(portalId);
                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                        message += Localization.GetString("DefaultFolderMappings.Error") + Exc.Message + Exc.StackTrace;
                    }
                }
                else
                {
                    throw new Exception(message);
                }

                LocaleCollection newPortalLocales = null;
                if (message == Null.NullString)
                {
                    try
                    {
                        CreatePredefinedFolderTypes(portalId);
                        ParseTemplateInternal(portalId, templatePath, templateFile, adminUser.UserID, PortalTemplateModuleAction.Replace, true, out newPortalLocales);
                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                        message += Localization.GetString("PortalTemplate.Error") + Exc.Message + Exc.StackTrace;
                    }
                }
                else
                {
                    throw new Exception(message);
                }
                if (message == Null.NullString)
                {

                    var portal = GetPortal(portalId);
                    portal.Description = description;
                    portal.KeyWords = keyWords;
                    portal.UserTabId = TabController.GetTabByTabPath(portal.PortalID, "//UserProfile", portal.CultureCode);
                    if (portal.UserTabId == -1)
                    {
                        portal.UserTabId = TabController.GetTabByTabPath(portal.PortalID, "//ActivityFeed", portal.CultureCode);
                    }
                    portal.SearchTabId = TabController.GetTabByTabPath(portal.PortalID, "//SearchResults", portal.CultureCode);
                    UpdatePortalInfo(portal);

                    adminUser.Profile.PreferredLocale = portal.DefaultLanguage;
                    var portalSettings = new PortalSettings(portal);
                    adminUser.Profile.PreferredTimeZone = portalSettings.TimeZone;
                    UserController.UpdateUser(portal.PortalID, adminUser);
                    
                    DesktopModuleController.AddDesktopModulesToPortal(portalId);
                    
                    AddPortalAlias(portalId, portalAlias);
                    
                    UpdatePortalSetting(portalId, "DefaultPortalAlias", portalAlias, false);
                    
                    DataCache.ClearPortalCache(portalId, true);

                    if (newPortalLocales != null)
                    {
                        foreach (Locale newPortalLocale in newPortalLocales.AllValues)
                        {
                            Localization.AddLanguageToPortal(portalId, newPortalLocale.LanguageId, false);
                        }
                    }

                    try
                    {
                        RelationshipController.Instance.CreateDefaultRelationshipsForPortal(portalId);
                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                    }

                    //add profanity list to new portal
                    try
                    {
                        const string listName = "ProfanityFilter";
                        var listController = new ListController();
                        var entry = new ListEntryInfo
                                            {
                                                PortalID = portalId,
                                                SystemList = false,
                                                ListName = listName + "-" + portalId
                                            };
                        listController.AddListEntry(entry);

                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                    }

                    //add banned password list to new portal
                    try
                    {
                        const string listName = "BannedPasswords";
                        var listController = new ListController();
                        var entry = new ListEntryInfo
                                            {
                                                PortalID = portalId,
                                                SystemList = false,
                                                ListName = listName + "-" + portalId
                                            };
                        listController.AddListEntry(entry);

                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                    }                    

                    ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();

                    try
                    {
                        var log = new LogInfo
                                                    {
                                                        BypassBuffering = true,
                                                        LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString()
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

                    if (PortalCreated != null)
                    {
                        PortalCreated(this, new PortalCreatedEventArgs { PortalId = portalId});
                    }
                }
                else
                {
                    throw new Exception(message);
                }
            }
            else
            {
                DeletePortalInternal(portalId);
                throw new Exception(message);
            }
        }

        private static string CreateProfileDefinitions(int portalId, string templateFilePath)
        {
            string strMessage = Null.NullString;
            try
            {
                //add profile definitions
                XmlDocument xmlDoc = new XmlDocument();
                XmlNode node;
                //open the XML template file
                try
                {
                    xmlDoc.Load(templateFilePath);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                //parse profile definitions if available
                node = xmlDoc.SelectSingleNode("//portal/profiledefinitions");
                if (node != null)
                {
                    ParseProfileDefinitions(node, portalId);
                }
                else //template does not contain profile definitions ( ie. was created prior to DNN 3.3.0 )
                {
                    ProfileController.AddDefaultDefinitions(portalId);
                }
            }
            catch (Exception ex)
            {
                strMessage = Localization.GetString("CreateProfileDefinitions.Error");
                Exceptions.LogException(ex);
            }
            return strMessage;
        }

        private static int CreatePortal(string portalName, string homeDirectory, string cultureCode)
        {
            //add portal
            int PortalId = -1;
            try
            {
                //Use host settings as default values for these parameters
                //This can be overwritten on the portal template
                DateTime datExpiryDate;
                datExpiryDate = Host.Host.DemoPeriod > Null.NullInteger 
                                    ? Convert.ToDateTime(Globals.GetMediumDate(DateTime.Now.AddDays(Host.Host.DemoPeriod).ToString())) 
                                    : Null.NullDate;

                PortalId = DataProvider.Instance().CreatePortal(portalName,
                                                                Host.Host.HostCurrency,
                                                                datExpiryDate,
                                                                Host.Host.HostFee,
                                                                Host.Host.HostSpace,
                                                                Host.Host.PageQuota,
                                                                Host.Host.UserQuota,
                                                                Host.Host.SiteLogHistory,
                                                                homeDirectory,
                                                                cultureCode,
                                                                UserController.Instance.GetCurrentUserInfo().UserID);

                //clear portal cache
                DataCache.ClearHostCache(true);

                EventLogController.Instance.AddLog("PortalName", portalName, GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_CREATED);
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
                //should be no exception, but suppress just in case
            }

            return PortalId;
        }

        private static int CreateRole(RoleInfo role)
        {
            RoleInfo objRoleInfo;
            int roleId = Null.NullInteger;

            //First check if the role exists
            objRoleInfo = RoleController.Instance.GetRole(role.PortalID, r => r.RoleName == role.RoleName);
            if (objRoleInfo == null)
            {
                roleId = RoleController.Instance.AddRole(role);
            }
            else
            {
                roleId = objRoleInfo.RoleID;
            }
            return roleId;
        }

        private static int CreateRole(int portalId, string roleName, string description, float serviceFee, int billingPeriod, string billingFrequency, float trialFee, int trialPeriod, string trialFrequency,
                               bool isPublic, bool isAuto)
        {
            RoleInfo objRoleInfo = new RoleInfo();
            objRoleInfo.PortalID = portalId;
            objRoleInfo.RoleName = roleName;
            objRoleInfo.RoleGroupID = Null.NullInteger;
            objRoleInfo.Description = description;
            objRoleInfo.ServiceFee = Convert.ToSingle(serviceFee < 0 ? 0 : serviceFee);
            objRoleInfo.BillingPeriod = billingPeriod;
            objRoleInfo.BillingFrequency = billingFrequency;
            objRoleInfo.TrialFee = Convert.ToSingle(trialFee < 0 ? 0 : trialFee);
            objRoleInfo.TrialPeriod = trialPeriod;
            objRoleInfo.TrialFrequency = trialFrequency;
            objRoleInfo.IsPublic = isPublic;
            objRoleInfo.AutoAssignment = isAuto;
            return CreateRole(objRoleInfo);
        }

        private static void CreateRoleGroup(RoleGroupInfo roleGroup)
        {
            RoleGroupInfo objRoleGroupInfo;
            //First check if the role exists
            objRoleGroupInfo = RoleController.GetRoleGroupByName(roleGroup.PortalID, roleGroup.RoleGroupName);

            if (objRoleGroupInfo == null)
            {
                roleGroup.RoleGroupID = RoleController.AddRoleGroup(roleGroup);
            }
            else
            {
                roleGroup.RoleGroupID = objRoleGroupInfo.RoleGroupID;
            }
        }

        private static void DeletePortalInternal(int portalId)
        {
            UserController.DeleteUsers(portalId, false, true);

            DataProvider.Instance().DeletePortalInfo(portalId);

            EventLogController.Instance.AddLog("PortalId", portalId.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_DELETED);

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

        internal static void EnsureRequiredEventLogTypesExist()
        {
            if (!DoesLogTypeExists(EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString()))
            {
                //Add 404 Log
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.PAGE_NOT_FOUND_404.ToString(),
                    LogTypeFriendlyName = "HTTP Error Code 404 Page Not Found",
                    LogTypeDescription = "",
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType"
                };
                LogController.Instance.AddLogType(logTypeInfo);

                //Add LogType
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
                    LogTypePortalID = "*"
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeConf);
            }


            if (!DoesLogTypeExists(EventLogController.EventLogType.IP_LOGIN_BANNED.ToString()))
            {
                //Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.IP_LOGIN_BANNED.ToString(),
                    LogTypeFriendlyName = "HTTP Error Code Forbidden IP address rejected",
                    LogTypeDescription = "",
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType"
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                //Add LogType
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
                    LogTypePortalID = "*"
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.TABURL_CREATED.ToString()))
            {
                var logTypeInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.TABURL_CREATED.ToString(),
                    LogTypeFriendlyName = "TabURL created",
                    LogTypeDescription = "",
                    LogTypeCSSClass = "OperationSuccess",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType"
                };
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL updated";
                LogController.Instance.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
                LogController.Instance.AddLogType(logTypeInfo);

                //Add LogType
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
                    LogTypePortalID = "*"
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                LogController.Instance.AddLogTypeConfigInfo(logTypeUrlConf);
            }

            if (!DoesLogTypeExists(EventLogController.EventLogType.SCRIPT_COLLISION.ToString()))
            {
                //Add IP filter log type
                var logTypeFilterInfo = new LogTypeInfo
                {
                    LogTypeKey = EventLogController.EventLogType.SCRIPT_COLLISION.ToString(),
                    LogTypeFriendlyName = "Javscript library registration resolved script collision",
                    LogTypeDescription = "",
                    LogTypeCSSClass = "OperationFailure",
                    LogTypeOwner = "DotNetNuke.Logging.EventLogType"
                };
                LogController.Instance.AddLogType(logTypeFilterInfo);

                //Add LogType
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
                    LogTypePortalID = "*"
                };
                LogController.Instance.AddLogTypeConfigInfo(logTypeFilterConf);
            }

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

        private static PortalInfo GetPortalInternal(int portalId, string cultureCode)
        {
            return PortalController.Instance.GetPortalList(cultureCode).SingleOrDefault(p => p.PortalID == portalId);
        }

        private static object GetPortalDefaultLanguageCallBack(CacheItemArgs cacheItemArgs)
        {
            int portalID = (int)cacheItemArgs.ParamList[0];
            return DataProvider.Instance().GetPortalDefaultLanguage(portalID);
        }

        private static object GetPortalDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            var portalDic = new Dictionary<int, int>();
            if (Host.Host.PerformanceSetting != Globals.PerformanceSettings.NoCaching)
            {
				//get all tabs
                int intField = 0;
                IDataReader dr = DataProvider.Instance().GetTabPaths(Null.NullInteger, Null.NullString);
                try
                {
                    while (dr.Read())
                    {
						//add to dictionary
                        portalDic[Convert.ToInt32(Null.SetNull(dr["TabID"], intField))] = Convert.ToInt32(Null.SetNull(dr["PortalID"], intField));
                    }
                }
                catch (Exception exc)
                {
                    Exceptions.LogException(exc);
                }
                finally
                {
					//close datareader
                    CBO.CloseDataReader(dr, true);
                }
            }
            return portalDic;
        }

        private object GetPortalsCallBack(CacheItemArgs cacheItemArgs)
        {
            var cultureCode = (string)cacheItemArgs.ParamList[0];
            string cacheKey = String.Format(DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
            List<PortalInfo> portals = CBO.FillCollection<PortalInfo>(DataProvider.Instance().GetPortals(cultureCode));
            DataCache.SetCache(cacheKey, portals);
            return portals;
        }

        private static object GetPortalSettingsDictionaryCallback(CacheItemArgs cacheItemArgs)
        {
            var portalId = (int)cacheItemArgs.ParamList[0];
            var dicSettings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (portalId <= -1) return dicSettings;
            
            var cultureCode = Convert.ToString(cacheItemArgs.ParamList[1]);
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetActivePortalLanguage(portalId);
            }
            var dr = DataProvider.Instance().GetPortalSettings(portalId, cultureCode);
            try
            {
                while (dr.Read())
                {
                    if (dr.IsDBNull(1)) continue;
                    
                    var key = dr.GetString(0);
                    if (dicSettings.ContainsKey(key))
                    {
                        dicSettings[key] = dr.GetString(1);
                        var log = new LogInfo { 
                            LogTypeKey = EventLogController.EventLogType.ADMIN_ALERT.ToString() 
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

        private static void ParseFiles(XmlNodeList nodeFiles, int portalId, FolderInfo objFolder)
        {
            var fileManager = FileManager.Instance;

            foreach (XmlNode node in nodeFiles)
            {
                var fileName = XmlUtils.GetNodeValue(node.CreateNavigator(), "filename");

                //First check if the file exists
                var objInfo = fileManager.GetFile(objFolder, fileName);

                if (objInfo != null) continue;

                objInfo = new FileInfo
                {
                    PortalId = portalId,
                    FileName = fileName,
                    Extension = XmlUtils.GetNodeValue(node.CreateNavigator(), "extension"),
                    Size = XmlUtils.GetNodeValueInt(node, "size"),
                    Width = XmlUtils.GetNodeValueInt(node, "width"),
                    Height = XmlUtils.GetNodeValueInt(node, "height"),
                    ContentType = XmlUtils.GetNodeValue(node.CreateNavigator(), "contenttype"),
                    SHA1Hash = XmlUtils.GetNodeValue(node.CreateNavigator(), "sha1hash"),
                    FolderId = objFolder.FolderID,
                    Folder = objFolder.FolderPath,
                    Title = "",
                    StartDate = DateTime.Now,
                    EndDate = Null.NullDate,
                    EnablePublishPeriod = false,
                    ContentItemID = Null.NullInteger
                };

                //Save new File
	            try
	            {
                    //Initially, install files are on local system, then we need the Standard folder provider to read the content regardless the target folderprovider					
                    using (var fileContent = FolderProvider.Instance("StandardFolderProvider").GetFileStream(objInfo))
					{
						objInfo.FileId = fileManager.AddFile(objFolder, fileName, fileContent, false).FileId;
					}

					fileManager.UpdateFile(objInfo);
	            }
				catch (InvalidFileExtensionException ex) //when the file is not allowed, we should not break parse process, but just log the error.
	            {
                    Logger.Error(ex.Message);
	            }
            }
        }

        private static void ParseFolderPermissions(XmlNodeList nodeFolderPermissions, int portalId, FolderInfo folder)
        {
            PermissionController permissionController = new PermissionController();
            int permissionId = 0;

            //Clear the current folder permissions
            folder.FolderPermissions.Clear();
            foreach (XmlNode xmlFolderPermission in nodeFolderPermissions)
            {
                string permissionKey = XmlUtils.GetNodeValue(xmlFolderPermission.CreateNavigator(), "permissionkey");
                string permissionCode = XmlUtils.GetNodeValue(xmlFolderPermission.CreateNavigator(), "permissioncode");
                string roleName = XmlUtils.GetNodeValue(xmlFolderPermission.CreateNavigator(), "rolename");
                bool allowAccess = XmlUtils.GetNodeValueBoolean(xmlFolderPermission, "allowaccess");
                foreach (PermissionInfo permission in permissionController.GetPermissionByCodeAndKey(permissionCode, permissionKey))
                {
                    permissionId = permission.PermissionID;
                }
                int roleId = int.MinValue;
                switch (roleName)
                {
                    case Globals.glbRoleAllUsersName:
                        roleId = Convert.ToInt32(Globals.glbRoleAllUsers);
                        break;
                    case Globals.glbRoleUnauthUserName:
                        roleId = Convert.ToInt32(Globals.glbRoleUnauthUser);
                        break;
                    default:
                        RoleInfo objRole = RoleController.Instance.GetRole(portalId, r => r.RoleName == roleName);
                        if (objRole != null)
                        {
                            roleId = objRole.RoleID;
                        }
                        break;
                }

                //if role was found add, otherwise ignore
                if (roleId != int.MinValue)
                {
                    var folderPermission = new FolderPermissionInfo
                                                  {
                                                      FolderID = folder.FolderID,
                                                      PermissionID = permissionId,
                                                      RoleID = roleId,
                                                      UserID = Null.NullInteger,
                                                      AllowAccess = allowAccess
                                                  };

                    bool canAdd = !folder.FolderPermissions.Cast<FolderPermissionInfo>()
                                                .Any(fp => fp.FolderID == folderPermission.FolderID 
                                                        && fp.PermissionID == folderPermission.PermissionID 
                                                        && fp.RoleID == folderPermission.RoleID 
                                                        && fp.UserID == folderPermission.UserID);
                    if (canAdd)
                    {
                        folder.FolderPermissions.Add(folderPermission);
                    }
                }
            }
            FolderPermissionController.SaveFolderPermissions(folder);
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
                    FolderMappingController.Instance.AddFolderMapping(GetFolderMappingFromConfig(folderTypeConfig,
                        portalId));
                }
                catch (Exception ex)
                {
                    Logger.Error(Localization.GetString("CreatingConfiguredFolderMapping.Error") + ": " + folderTypeConfig.Name, ex);
                }                
            }
        }

        private static void EnsureRequiredProvidersForFolderTypes()
        {
            if (ComponentFactory.GetComponent<CryptographyProvider>() == null)
            {
                ComponentFactory.InstallComponents(new ProviderInstaller("cryptography", typeof(CryptographyProvider), typeof(CoreCryptographyProvider)));
                ComponentFactory.RegisterComponentInstance<CryptographyProvider>(new CoreCryptographyProvider());
            }
        }
        private static void EnsureFolderProviderRegistration<TAbstract>(FolderTypeConfig folderTypeConfig, XmlDocument webConfig)
            where TAbstract : class            
        {
            var providerBusinessClassNode = webConfig.SelectSingleNode("configuration/dotnetnuke/folder/providers/add[@name='"+folderTypeConfig.Provider+"']");
            
            var typeClass = Type.GetType(providerBusinessClassNode.Attributes["type"].Value);
            if (typeClass != null)
            {
                ComponentFactory.RegisterComponentInstance<TAbstract>(folderTypeConfig.Provider, Activator.CreateInstance(typeClass));
            }
        }

        private void ParseExtensionUrlProviders(XPathNavigator providersNavigator, int portalId)
        {
            var providers = ExtensionUrlProviderController.GetProviders(portalId);
            foreach (XPathNavigator providerNavigator in providersNavigator.Select("extensionUrlProvider"))
            {
                HtmlUtils.WriteKeepAlive();
                var providerName = XmlUtils.GetNodeValue(providerNavigator, "name");
                var provider = providers.SingleOrDefault(p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
                if (provider == null)
                {
                    continue;
                }

                var active = XmlUtils.GetNodeValueBoolean(providerNavigator, "active");
                if (active)
                {
                    ExtensionUrlProviderController.EnableProvider(provider.ExtensionUrlProviderId, portalId);
                }
                else
                {
                    ExtensionUrlProviderController.DisableProvider(provider.ExtensionUrlProviderId, portalId);
                }

                var settingsNavigator = providerNavigator.SelectSingleNode("settings");
                if (settingsNavigator != null)
                {
                    foreach (XPathNavigator settingNavigator in settingsNavigator.Select("setting"))
                    {
                        var name = XmlUtils.GetAttributeValue(settingNavigator, "name");
                        var value = XmlUtils.GetAttributeValue(settingNavigator, "value");
                        ExtensionUrlProviderController.SaveSetting(provider.ExtensionUrlProviderId, portalId, name, value);
                    }
                }

                ////var tabIdsNavigator = providerNavigator.SelectSingleNode("tabIds");
                ////if (tabIdsNavigator != null)
                ////{
                ////    foreach (XPathNavigator tabIdNavigator in tabIdsNavigator.Select("tabId"))
                ////    {
                ////        // TODO: figure out how to save a tab ID for a provider
                ////        var tabId = XmlUtils.GetNodeValueInt(tabIdNavigator, ".");
                ////        provider.TabIds.Add(tabId);
                ////    }
                ////}
            }
        }

        private static bool EnableBrowserLanguageInDefault(int portalId)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                var setting = Null.NullString;
                GetPortalSettingsDictionary(portalId, Localization.SystemLocale).TryGetValue("EnableBrowserLanguage", out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = Host.Host.EnableBrowserLanguage;
                }
                else
                {
                    retValue = (setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.ToUpperInvariant() == "TRUE");
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            return retValue;
        }

        private string EnsureSettingValue(string folderProviderType, FolderTypeSettingConfig settingNode, int portalId)
        {
            var ensuredSettingValue =
                settingNode.Value.Replace("{PortalId}", (portalId != -1) ? portalId.ToString(CultureInfo.InvariantCulture) : "_default").Replace("{HostId}", Host.Host.GUID);
            if (settingNode.Encrypt)
            {
                return FolderProvider.Instance(folderProviderType).EncryptValue(ensuredSettingValue);
                //return new PortalSecurity().Encrypt(Host.Host.GUID, ensuredSettingValue.Trim());
            }
            return ensuredSettingValue;
        }

        private string GetCultureCode(string languageFileName)
        {
            //e.g. "default template.template.en-US.resx"
            return languageFileName.GetLocaleCodeFromFileName();
        }

        private FolderMappingInfo GetFolderMappingFromConfig(FolderTypeConfig node, int portalId)
        {
            var folderMapping = new FolderMappingInfo
            {
                PortalID = portalId,
                MappingName = node.Name,
                FolderProviderType = node.Provider
            };

            foreach (FolderTypeSettingConfig settingNode in node.Settings)
            {
                var settingValue = EnsureSettingValue(folderMapping.FolderProviderType, settingNode, portalId);
                folderMapping.FolderMappingSettings.Add(settingNode.Name, settingValue);
            }

            return folderMapping;
        }
        
        private FolderMappingInfo GetFolderMappingFromStorageLocation(int portalId, XmlNode folderNode)
        {
            var storageLocation = Convert.ToInt32(XmlUtils.GetNodeValue(folderNode, "storagelocation", "0"));

            switch (storageLocation)
            {
                case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                    return FolderMappingController.Instance.GetFolderMapping(portalId, "Secure");                    
                case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                    return FolderMappingController.Instance.GetFolderMapping(portalId, "Database");   
                default:
                    return FolderMappingController.Instance.GetDefaultFolderMapping(portalId);             
            }
        }

        private static Dictionary<string, string> GetPortalSettingsDictionary(int portalId, string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                cultureCode = GetActivePortalLanguage(portalId);
            }
            var cacheKey = string.Format(DataCache.PortalSettingsCacheKey, portalId, cultureCode);
            return CBO.GetCachedObject<Dictionary<string, string>>(new CacheItemArgs(cacheKey, DataCache.PortalSettingsCacheTimeOut, DataCache.PortalSettingsCachePriority, portalId, cultureCode),
                                                                   GetPortalSettingsDictionaryCallback,
                                                                   true);
        }

        private string GetTemplateName(string languageFileName)
        {
            //e.g. "default template.template.en-US.resx"
            return languageFileName.GetFileNameFromLocalizedResxFile();
        }

        private static LocaleCollection ParseEnabledLocales(XmlNode nodeEnabledLocales, int PortalId)
        {
            var defaultLocale = LocaleController.Instance.GetDefaultLocale(PortalId);
            var returnCollection = new LocaleCollection { { defaultLocale.Code, defaultLocale } };
            var clearCache = false;
            foreach (XmlNode node in nodeEnabledLocales.SelectNodes("//locale"))
            {
                var cultureCode = node.InnerText;
                var locale = LocaleController.Instance.GetLocale(cultureCode);
                if (locale == null)
                {
                    // if language does not exist in the installation, create it
                    locale = new Locale { Code = cultureCode, Fallback = Localization.SystemLocale, Text = CultureInfo.GetCultureInfo(cultureCode).NativeName };
                    Localization.SaveLanguage(locale,false);
                    clearCache = true;
                }

                if (locale.Code != defaultLocale.Code)
                {
                    returnCollection.Add(locale.Code, locale);
                }
            }
            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }
            return returnCollection;
        }

        private void ParseFolders(XmlNode nodeFolders, int PortalId)
        {
            IFolderInfo objInfo;
            string folderPath;
            bool isProtected = false;
            var folderManager = FolderManager.Instance;
            var folderMappingController = FolderMappingController.Instance;
            FolderMappingInfo folderMapping = null;
            var folderMappingsConfigController = new FolderMappingsConfigController();
            //DNN-2949 set ignorewhitelist to true to allow files with not allowed extensions to be added during portal creation
            HostController.Instance.Update("IgnoreWhiteList", "Y", true);
            try
            {
                foreach (XmlNode node in nodeFolders.SelectNodes("//folder"))
                {
                    HtmlUtils.WriteKeepAlive();
                    folderPath = XmlUtils.GetNodeValue(node.CreateNavigator(), "folderpath");

                    //First check if the folder exists
                    objInfo = folderManager.GetFolder(PortalId, folderPath);

                    if (objInfo == null)
                    {
                        try
                        {
                            folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(PortalId, folderPath) ??
                                            GetFolderMappingFromStorageLocation(PortalId, node);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            folderMapping = folderMappingController.GetDefaultFolderMapping(PortalId);
                        }
                        isProtected = XmlUtils.GetNodeValueBoolean(node, "isprotected");

                        try
                        {
                            //Save new folder
                            objInfo = folderManager.AddFolder(folderMapping, folderPath);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            //Retry with default folderMapping
                            var defaultFolderMapping = folderMappingController.GetDefaultFolderMapping(PortalId);
                            if (folderMapping.FolderMappingID != defaultFolderMapping.FolderMappingID)
                            {
                                objInfo = folderManager.AddFolder(defaultFolderMapping, folderPath);
                            }
                            else
                            {
                                throw ex;
                            }
                        }
                        objInfo.IsProtected = isProtected;

                        folderManager.UpdateFolder(objInfo);
                    }

                    var nodeFolderPermissions = node.SelectNodes("folderpermissions/permission");
                    ParseFolderPermissions(nodeFolderPermissions, PortalId, (FolderInfo)objInfo);

                    var nodeFiles = node.SelectNodes("files/file");

                    if (!String.IsNullOrEmpty(folderPath))
                    {
                        folderPath += "/";
                    }

                    ParseFiles(nodeFiles, PortalId, (FolderInfo)objInfo);
                }
                //DNN-2949 set ignorewhitelist to false right away after ParseFiles was executed on all folders in template nodes
                HostController.Instance.Update("IgnoreWhiteList", "N", true);
            }
            catch (Exception)
            {
                //DNN-2949 ensure ignorewhitelist is set back to false in case of any exceptions during files parsing/adding
                HostController.Instance.Update("IgnoreWhiteList", "N", true);
            }
        }

        private static void ParseProfileDefinitions(XmlNode nodeProfileDefinitions, int PortalId)
        {
            var listController = new ListController();
            Dictionary<string, ListEntryInfo> colDataTypes = listController.GetListEntryInfoDictionary("DataType");

            int orderCounter = -1;
            ProfilePropertyDefinition objProfileDefinition;
            bool preferredTimeZoneFound = false;
            foreach (XmlNode node in nodeProfileDefinitions.SelectNodes("//profiledefinition"))
            {
                orderCounter += 2;
                ListEntryInfo typeInfo;
                if (!colDataTypes.TryGetValue("DataType:" + XmlUtils.GetNodeValue(node.CreateNavigator(), "datatype"), out typeInfo))
                {
                    typeInfo = colDataTypes["DataType:Unknown"];
                }
                objProfileDefinition = new ProfilePropertyDefinition(PortalId);
                objProfileDefinition.DataType = typeInfo.EntryID;
                objProfileDefinition.DefaultValue = "";
                objProfileDefinition.ModuleDefId = Null.NullInteger;
                objProfileDefinition.PropertyCategory = XmlUtils.GetNodeValue(node.CreateNavigator(), "propertycategory");
                objProfileDefinition.PropertyName = XmlUtils.GetNodeValue(node.CreateNavigator(), "propertyname");
                objProfileDefinition.Required = false;
                objProfileDefinition.Visible = true;
                objProfileDefinition.ViewOrder = orderCounter;
                objProfileDefinition.Length = XmlUtils.GetNodeValueInt(node, "length");

                switch (XmlUtils.GetNodeValueInt(node, "defaultvisibility", 2))
                {
                    case 0:
                        objProfileDefinition.DefaultVisibility = UserVisibilityMode.AllUsers;
                        break;
                    case 1:
                        objProfileDefinition.DefaultVisibility = UserVisibilityMode.MembersOnly;
                        break;
                    case 2:
                        objProfileDefinition.DefaultVisibility = UserVisibilityMode.AdminOnly;
                        break;
                }

                if (objProfileDefinition.PropertyName == "PreferredTimeZone")
                {
                    preferredTimeZoneFound = true;
                }

                ProfileController.AddPropertyDefinition(objProfileDefinition);
            }

            //6.0 requires the old TimeZone property to be marked as Deleted
            ProfilePropertyDefinition pdf = ProfileController.GetPropertyDefinitionByName(PortalId, "TimeZone");
            if (pdf != null)
            {
                ProfileController.DeletePropertyDefinition(pdf);
            }

            // 6.0 introduced a new property called as PreferredTimeZone. If this property is not present in template
            // it should be added. Situation will mostly happen while using an older template file.
            if (!preferredTimeZoneFound)
            {
                orderCounter += 2;

                ListEntryInfo typeInfo = colDataTypes["DataType:TimeZoneInfo"];
                if (typeInfo == null)
                {
                    typeInfo = colDataTypes["DataType:Unknown"];
                }

                objProfileDefinition = new ProfilePropertyDefinition(PortalId);
                objProfileDefinition.DataType = typeInfo.EntryID;
                objProfileDefinition.DefaultValue = "";
                objProfileDefinition.ModuleDefId = Null.NullInteger;
                objProfileDefinition.PropertyCategory = "Preferences";
                objProfileDefinition.PropertyName = "PreferredTimeZone";
                objProfileDefinition.Required = false;
                objProfileDefinition.Visible = true;
                objProfileDefinition.ViewOrder = orderCounter;
                objProfileDefinition.Length = 0;
                objProfileDefinition.DefaultVisibility = UserVisibilityMode.AdminOnly;
                ProfileController.AddPropertyDefinition(objProfileDefinition);
            }
        }

        private static void ParsePortalDesktopModules(XPathNavigator nav, int portalID)
        {
            string friendlyName = Null.NullString;
            DesktopModuleInfo desktopModule = null;
            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
                HtmlUtils.WriteKeepAlive();
                friendlyName = XmlUtils.GetNodeValue(desktopModuleNav, "friendlyname");
                if (!string.IsNullOrEmpty(friendlyName))
                {
                    desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(friendlyName);
                    if (desktopModule != null)
                    {
                        //Parse the permissions
                        DesktopModulePermissionCollection permissions = new DesktopModulePermissionCollection();
                        foreach (XPathNavigator permissionNav in
                            desktopModuleNav.Select("portalDesktopModulePermissions/portalDesktopModulePermission"))
                        {
                            string code = XmlUtils.GetNodeValue(permissionNav, "permissioncode");
                            string key = XmlUtils.GetNodeValue(permissionNav, "permissionkey");
                            DesktopModulePermissionInfo desktopModulePermission = null;
                            ArrayList arrPermissions = new PermissionController().GetPermissionByCodeAndKey(code, key);
                            if (arrPermissions.Count > 0)
                            {
                                PermissionInfo permission = arrPermissions[0] as PermissionInfo;
                                if (permission != null)
                                {
                                    desktopModulePermission = new DesktopModulePermissionInfo(permission);
                                }
                            }
                            desktopModulePermission.AllowAccess = bool.Parse(XmlUtils.GetNodeValue(permissionNav, "allowaccess"));
                            string rolename = XmlUtils.GetNodeValue(permissionNav, "rolename");
                            if (!string.IsNullOrEmpty(rolename))
                            {
                                RoleInfo role = RoleController.Instance.GetRole(portalID, r => r.RoleName == rolename);
                                if (role != null)
                                {
                                    desktopModulePermission.RoleID = role.RoleID;
                                }
                            }
                            permissions.Add(desktopModulePermission);
                        }
                        DesktopModuleController.AddDesktopModuleToPortal(portalID, desktopModule, permissions, false);
                    }
                }
            }
        }

        private void ParsePortalSettings(XmlNode nodeSettings, int PortalId)
        {
            PortalInfo objPortal;
			String currentCulture = GetActivePortalLanguage(PortalId);
			objPortal = GetPortal(PortalId);
            objPortal.LogoFile = Globals.ImportFile(PortalId, XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "logofile"));
            objPortal.FooterText = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "footertext");
            if (nodeSettings.SelectSingleNode("expirydate") != null)
            {
                objPortal.ExpiryDate = XmlUtils.GetNodeValueDate(nodeSettings, "expirydate", Null.NullDate);
            }
            objPortal.UserRegistration = XmlUtils.GetNodeValueInt(nodeSettings, "userregistration");
            objPortal.BannerAdvertising = XmlUtils.GetNodeValueInt(nodeSettings, "banneradvertising");
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "currency")))
            {
                objPortal.Currency = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "currency");
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "hostfee")))
            {
                objPortal.HostFee = XmlUtils.GetNodeValueSingle(nodeSettings, "hostfee");
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "hostspace")))
            {
                objPortal.HostSpace = XmlUtils.GetNodeValueInt(nodeSettings, "hostspace");
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "pagequota")))
            {
                objPortal.PageQuota = XmlUtils.GetNodeValueInt(nodeSettings, "pagequota");
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "userquota")))
            {
                objPortal.UserQuota = XmlUtils.GetNodeValueInt(nodeSettings, "userquota");
            }
            objPortal.BackgroundFile = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "backgroundfile");
            objPortal.PaymentProcessor = XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "paymentprocessor");
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings.CreateNavigator(), "siteloghistory")))
            {
                objPortal.SiteLogHistory = XmlUtils.GetNodeValueInt(nodeSettings, "siteloghistory");
            }
            objPortal.DefaultLanguage = XmlUtils.GetNodeValue(nodeSettings, "defaultlanguage", "en-US");
            UpdatePortalInfo(objPortal);

            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "skinsrc", "")))
            {
				UpdatePortalSetting(PortalId, "DefaultPortalSkin", XmlUtils.GetNodeValue(nodeSettings, "skinsrc", ""), currentCulture);
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "skinsrcadmin", "")))
            {
				UpdatePortalSetting(PortalId, "DefaultAdminSkin", XmlUtils.GetNodeValue(nodeSettings, "skinsrcadmin", ""), currentCulture);
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "containersrc", "")))
            {
				UpdatePortalSetting(PortalId, "DefaultPortalContainer", XmlUtils.GetNodeValue(nodeSettings, "containersrc", ""), currentCulture);
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "containersrcadmin", "")))
            {
				UpdatePortalSetting(PortalId, "DefaultAdminContainer", XmlUtils.GetNodeValue(nodeSettings, "containersrcadmin", ""), currentCulture);
            }

            //Enable Skin Widgets Setting
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enableskinwidgets", "")))
            {
                UpdatePortalSetting(PortalId, "EnableSkinWidgets", XmlUtils.GetNodeValue(nodeSettings, "enableskinwidgets", ""));
            }

            //Enable AutoSAve feature
            //Enable Skin Widgets Setting
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enableautosave", "")))
            {
                UpdatePortalSetting(PortalId, HtmlText_AutoSaveEnabled, XmlUtils.GetNodeValue(nodeSettings, "enableautosave", ""));
                //Time to autosave, only if enableautosave exists
                if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "timetoautosave", "")))
                {
                    UpdatePortalSetting(PortalId, HtmlText_TimeToAutoSave, XmlUtils.GetNodeValue(nodeSettings, "timetoautosave", ""));
                }
            }

            //Set Auto alias mapping

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "portalaliasmapping", "CANONICALURL")))
            {
                UpdatePortalSetting(PortalId, "PortalAliasMapping", XmlUtils.GetNodeValue(nodeSettings, "portalaliasmapping", "CANONICALURL").ToUpperInvariant());
            }
            //Set Time Zone maping
            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "timezone", Localization.SystemTimeZone)))
            {
                UpdatePortalSetting(PortalId, "TimeZone", XmlUtils.GetNodeValue(nodeSettings, "timezone", Localization.SystemTimeZone));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "contentlocalizationenabled")))
            {
                UpdatePortalSetting(PortalId, "ContentLocalizationEnabled", XmlUtils.GetNodeValue(nodeSettings, "contentlocalizationenabled"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "inlineeditorenabled")))
            {
                UpdatePortalSetting(PortalId, "InlineEditorEnabled", XmlUtils.GetNodeValue(nodeSettings, "inlineeditorenabled"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "enablepopups")))
            {
                UpdatePortalSetting(PortalId, "EnablePopUps", XmlUtils.GetNodeValue(nodeSettings, "enablepopups"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "hidefoldersenabled")))
            {
                UpdatePortalSetting(PortalId, "HideFoldersEnabled", XmlUtils.GetNodeValue(nodeSettings, "hidefoldersenabled"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "controlpanelmode")))
            {
                UpdatePortalSetting(PortalId, "ControlPanelMode", XmlUtils.GetNodeValue(nodeSettings, "controlpanelmode"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "controlpanelsecurity")))
            {
                UpdatePortalSetting(PortalId, "ControlPanelSecurity", XmlUtils.GetNodeValue(nodeSettings, "controlpanelsecurity"));
            }

            if (!string.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "controlpanelvisibility")))
            {
                UpdatePortalSetting(PortalId, "ControlPanelVisibility", XmlUtils.GetNodeValue(nodeSettings, "controlpanelvisibility"));
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "pageheadtext", "")))
            {
                UpdatePortalSetting(PortalId, "PageHeadText", XmlUtils.GetNodeValue(nodeSettings, "pageheadtext", ""));
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "injectmodulehyperlink", "")))
            {
                UpdatePortalSetting(PortalId, "InjectModuleHyperLink", XmlUtils.GetNodeValue(nodeSettings, "injectmodulehyperlink", ""));
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "addcompatiblehttpheader", "")))
            {
                UpdatePortalSetting(PortalId, "AddCompatibleHttpHeader", XmlUtils.GetNodeValue(nodeSettings, "addcompatiblehttpheader", ""));
            }
        }

        private void ParseRoleGroups(XPathNavigator nav, int portalID, int administratorId)
        {
            var administratorRoleId = -1;
            var registeredRoleId = -1;
            var subscriberRoleId = -1;
            var unverifiedRoleId = -1;

            foreach (XPathNavigator roleGroupNav in nav.Select("rolegroup"))
            {
                HtmlUtils.WriteKeepAlive();
                var roleGroup = CBO.DeserializeObject<RoleGroupInfo>(new StringReader(roleGroupNav.OuterXml));
                if (roleGroup.RoleGroupName != "GlobalRoles")
                {
                    roleGroup.PortalID = portalID;
                    CreateRoleGroup(roleGroup);
                }
                foreach (var role in roleGroup.Roles.Values)
                {
                    role.PortalID = portalID;
                    role.RoleGroupID = roleGroup.RoleGroupID;
                    role.Status = RoleStatus.Approved;
                    switch (role.RoleType)
                    {
                        case RoleType.Administrator:
                            administratorRoleId = CreateRole(role);
                            break;
                        case RoleType.RegisteredUser:
                            registeredRoleId = CreateRole(role);
                            break;
                        case RoleType.Subscriber:
                            subscriberRoleId = CreateRole(role);
                            break;
                        case RoleType.None:
                            CreateRole(role);
                            break;
                        case RoleType.UnverifiedUser:
                            unverifiedRoleId = CreateRole(role);
                            break;
                    }
                }
            }
            CreateDefaultPortalRoles(portalID, administratorId, ref administratorRoleId, ref registeredRoleId, ref subscriberRoleId, unverifiedRoleId);

            //update portal setup
            var portal = GetPortal(portalID);
            UpdatePortalSetup(portalID,
                              administratorId,
                              administratorRoleId,
                              registeredRoleId,
                              portal.SplashTabId,
                              portal.HomeTabId,
                              portal.LoginTabId,
                              portal.RegisterTabId,
                              portal.UserTabId,
                              portal.SearchTabId,
                              portal.Custom404TabId,
                              portal.Custom500TabId,
                              portal.AdminTabId,
                              GetActivePortalLanguage(portalID));
        }

        private void ParseRoles(XPathNavigator nav, int portalID, int administratorId)
        {
            var administratorRoleId = -1;
            var registeredRoleId = -1;
            var subscriberRoleId = -1;
            var unverifiedRoleId = -1;

            foreach (XPathNavigator roleNav in nav.Select("role"))
            {
                HtmlUtils.WriteKeepAlive();
                var role = CBO.DeserializeObject<RoleInfo>(new StringReader(roleNav.OuterXml));
                role.PortalID = portalID;
                role.RoleGroupID = Null.NullInteger;
                switch (role.RoleType)
                {
                    case RoleType.Administrator:
                        administratorRoleId = CreateRole(role);
                        break;
                    case RoleType.RegisteredUser:
                        registeredRoleId = CreateRole(role);
                        break;
                    case RoleType.Subscriber:
                        subscriberRoleId = CreateRole(role);
                        break;
                    case RoleType.None:
                        CreateRole(role);
                        break;
                    case RoleType.UnverifiedUser:
                        unverifiedRoleId = CreateRole(role);
                        break;
                }
            }

            //create required roles if not already created
            CreateDefaultPortalRoles(portalID, administratorId, ref administratorRoleId, ref registeredRoleId, ref subscriberRoleId, unverifiedRoleId);

            //update portal setup
            var portal = GetPortal(portalID);
            UpdatePortalSetup(portalID,
                              administratorId,
                              administratorRoleId,
                              registeredRoleId,
                              portal.SplashTabId,
                              portal.HomeTabId,
                              portal.LoginTabId,
                              portal.RegisterTabId,
                              portal.UserTabId,
                              portal.SearchTabId,
                              portal.Custom404TabId,
                              portal.Custom500TabId,
                              portal.AdminTabId,
                              GetActivePortalLanguage(portalID));
        }

        private void ParseTab(XmlNode nodeTab, int PortalId, bool IsAdminTemplate, PortalTemplateModuleAction mergeTabs, ref Hashtable hModules, ref Hashtable hTabs, bool isNewPortal)
        {
            TabInfo tab = null;
            string strName = XmlUtils.GetNodeValue(nodeTab.CreateNavigator(), "name");
            var portal = GetPortal(PortalId);
            if (!String.IsNullOrEmpty(strName))
            {
                if (!isNewPortal)  //running from wizard: try to find the tab by path
                {
                    string parenttabname = "";
                    if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeTab.CreateNavigator(), "parent")))
                    {
                        parenttabname = XmlUtils.GetNodeValue(nodeTab.CreateNavigator(), "parent") + "/";
                    }
                    if (hTabs[parenttabname + strName] != null)
                    {
                        tab = TabController.Instance.GetTab(Convert.ToInt32(hTabs[parenttabname + strName]), PortalId, false);
                    }
                }
                if (tab == null || isNewPortal)
                {
                    tab = TabController.DeserializeTab(nodeTab, null, hTabs, PortalId, IsAdminTemplate, mergeTabs, hModules);
                }

                //when processing the template we should try and identify the Admin tab
                var logType = "AdminTab";
                if (tab.TabName == "Admin")
                {
                    portal.AdminTabId = tab.TabID;
                }
                //when processing the template we can find: hometab, usertab, logintab
                switch (XmlUtils.GetNodeValue(nodeTab, "tabtype", ""))
                {
                    case "splashtab":
                        portal.SplashTabId = tab.TabID;
                        logType = "SplashTab";
                        break;
                    case "hometab":
                        portal.HomeTabId = tab.TabID;
                        logType = "HomeTab";
                        break;
                    case "logintab":
                        portal.LoginTabId = tab.TabID;
                        logType = "LoginTab";
                        break;
                    case "usertab":
                        portal.UserTabId = tab.TabID;
                        logType = "UserTab";
                        break;
                    case "searchtab":
                        portal.SearchTabId = tab.TabID;
                        logType = "SearchTab";
                        break;
                    case "gettingStartedTab":                        
                        UpdatePortalSetting(PortalId, "GettingStartedTabId", tab.TabID.ToString());
                        logType = "GettingStartedTabId";
                        break;
                    case "404Tab":
                        portal.Custom404TabId = tab.TabID;
                        logType = "Custom404Tab";
                        break;
                    case "500Tab":
                        portal.Custom500TabId = tab.TabID;
                        logType = "Custom500Tab";
                        break;
                }
                UpdatePortalSetup(PortalId,
                                  portal.AdministratorId,
                                  portal.AdministratorRoleId,
                                  portal.RegisteredRoleId,
                                  portal.SplashTabId,
                                  portal.HomeTabId,
                                  portal.LoginTabId,
                                  portal.RegisterTabId,
                                  portal.UserTabId,
                                  portal.SearchTabId,
                                  portal.Custom404TabId,
                                  portal.Custom500TabId,
                                  portal.AdminTabId,
                                  GetActivePortalLanguage(PortalId));
                EventLogController.Instance.AddLog(logType,
                                   tab.TabID.ToString(),
                                   GetCurrentPortalSettingsInternal(),
                                   UserController.Instance.GetCurrentUserInfo().UserID,
                                   EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
            }
        }

        private void ParseTabs(XmlNode nodeTabs, int PortalId, bool IsAdminTemplate, PortalTemplateModuleAction mergeTabs, bool IsNewPortal)
        {
            //used to control if modules are true modules or instances
            //will hold module ID from template / new module ID so new instances can reference right moduleid
            //only first one from the template will be create as a true module, 
            //others will be moduleinstances (tabmodules)
            Hashtable hModules = new Hashtable();
            Hashtable hTabs = new Hashtable();

            //if running from wizard we need to pre populate htabs with existing tabs so ParseTab 
            //can find all existing ones
            string tabname;
            if (!IsNewPortal)
            {
                Hashtable hTabNames = new Hashtable();
                foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(PortalId))
                {
                    TabInfo objTab = tabPair.Value;
                    if (!objTab.IsDeleted)
                    {
                        tabname = objTab.TabName;
                        if (!Null.IsNull(objTab.ParentId))
                        {
                            tabname = Convert.ToString(hTabNames[objTab.ParentId]) + "/" + objTab.TabName;
                        }
                        hTabNames.Add(objTab.TabID, tabname);
                    }
                }

                //when parsing tabs we will need tabid given tabname
                foreach (int i in hTabNames.Keys)
                {
                    if (hTabs[hTabNames[i]] == null)
                    {
                        hTabs.Add(hTabNames[i], i);
                    }
                }
                hTabNames = null;
            }
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab"))
            {
                HtmlUtils.WriteKeepAlive();
                ParseTab(nodeTab, PortalId, IsAdminTemplate, mergeTabs, ref hModules, ref hTabs, IsNewPortal);
            }

            //Process tabs that are linked to tabs
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab[url/@type = 'Tab']"))
            {
                HtmlUtils.WriteKeepAlive();
                int tabId = XmlUtils.GetNodeValueInt(nodeTab, "tabid", Null.NullInteger);
                string tabPath = XmlUtils.GetNodeValue(nodeTab, "url", Null.NullString);
                if (tabId > Null.NullInteger)
                {
                    TabInfo objTab = TabController.Instance.GetTab(tabId, PortalId, false);
                    objTab.Url = TabController.GetTabByTabPath(PortalId, tabPath, Null.NullString).ToString();
                    TabController.Instance.UpdateTab(objTab);
                }
            }
            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;
            //Process tabs that are linked to files
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab[url/@type = 'File']"))
            {
                HtmlUtils.WriteKeepAlive();
                var tabId = XmlUtils.GetNodeValueInt(nodeTab, "tabid", Null.NullInteger);
                var filePath = XmlUtils.GetNodeValue(nodeTab, "url", Null.NullString);
                if (tabId > Null.NullInteger)
                {
                    var objTab = TabController.Instance.GetTab(tabId, PortalId, false);

                    var fileName = Path.GetFileName(filePath);

                    var folderPath = filePath.Substring(0, filePath.LastIndexOf(fileName));
                    var folder = folderManager.GetFolder(PortalId, folderPath);

                    var file = fileManager.GetFile(folder, fileName);

                    objTab.Url = "FileID=" + file.FileId;
                    TabController.Instance.UpdateTab(objTab);
                }
            }
        }

        private void ParseTemplateInternal(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            LocaleCollection localeCollection;
            ParseTemplateInternal(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal, out localeCollection);
        }

        private void ParseTemplateInternal(int portalId, string templatePath, string templateFile, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal, out LocaleCollection localeCollection)
        {
            var xmlPortal = new XmlDocument();
            IFolderInfo objFolder;
            XmlNode node;
            try
            {
                xmlPortal.Load(Path.Combine(templatePath, templateFile));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            node = xmlPortal.SelectSingleNode("//portal/settings");
            if (node != null && isNewPortal)
            {
                HtmlUtils.WriteKeepAlive();
                ParsePortalSettings(node, portalId);
            }
            node = xmlPortal.SelectSingleNode("//locales");
            if (node != null && isNewPortal)
            {
                HtmlUtils.WriteKeepAlive();
                localeCollection = ParseEnabledLocales(node, portalId);
            }
            else
            {
                var portalInfo = PortalController.Instance.GetPortal(portalId);
                var defaultLocale = LocaleController.Instance.GetLocale(portalInfo.DefaultLanguage);
                if (defaultLocale == null)
                {
                    defaultLocale = new Locale { Code = portalInfo.DefaultLanguage, Fallback = Localization.SystemLocale, Text = CultureInfo.GetCultureInfo(portalInfo.DefaultLanguage).NativeName };
                    Localization.SaveLanguage(defaultLocale, false);
                }
                localeCollection = new LocaleCollection { { defaultLocale.Code, defaultLocale } };
            }
            node = xmlPortal.SelectSingleNode("//portal/rolegroups");
            if (node != null)
            {
                ParseRoleGroups(node.CreateNavigator(), portalId, administratorId);
            }
            node = xmlPortal.SelectSingleNode("//portal/roles");
            if (node != null)
            {
                ParseRoles(node.CreateNavigator(), portalId, administratorId);
            }
            node = xmlPortal.SelectSingleNode("//portal/portalDesktopModules");
            if (node != null)
            {
                ParsePortalDesktopModules(node.CreateNavigator(), portalId);
            }

            node = xmlPortal.SelectSingleNode("//portal/folders");
            if (node != null)
            {
                ParseFolders(node, portalId);
            }
            node = xmlPortal.SelectSingleNode("//portal/extensionUrlProviders");
            if (node != null)
            {
                ParseExtensionUrlProviders(node.CreateNavigator(), portalId);
            }

            var defaultFolderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(portalId);

            if (FolderManager.Instance.GetFolder(portalId, "") == null)
            {
                objFolder = FolderManager.Instance.AddFolder(defaultFolderMapping, "");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                AddFolderPermissions(portalId, objFolder.FolderID);
            }

            if (FolderManager.Instance.GetFolder(portalId, "Templates/") == null)
            {
                var folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, "Templates/") ?? defaultFolderMapping;
                objFolder = FolderManager.Instance.AddFolder(folderMapping, "Templates/");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                //AddFolderPermissions(PortalId, objFolder.FolderID);
            }

            // force creation of users folder if not present on template
            if (FolderManager.Instance.GetFolder(portalId, "Users/") == null)
            {
                var folderMapping = FolderMappingsConfigController.Instance.GetFolderMapping(portalId, "Users/") ?? defaultFolderMapping;
                objFolder = FolderManager.Instance.AddFolder(folderMapping, "Users/");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                //AddFolderPermissions(PortalId, objFolder.FolderID);
            }

            if (mergeTabs == PortalTemplateModuleAction.Replace)
            {
                TabInfo objTab;
                foreach (KeyValuePair<int, TabInfo> tabPair in TabController.Instance.GetTabsByPortal(portalId))
                {
                    objTab = tabPair.Value;
                    objTab.TabName = objTab.TabName + "_old";
                    objTab.TabPath = Globals.GenerateTabPath(objTab.ParentId, objTab.TabName);
                    objTab.IsDeleted = true;
                    TabController.Instance.UpdateTab(objTab);
                    ModuleInfo objModule;
                    foreach (KeyValuePair<int, ModuleInfo> modulePair in ModuleController.Instance.GetTabModules(objTab.TabID))
                    {
                        objModule = modulePair.Value;
                        ModuleController.Instance.DeleteTabModule(objModule.TabID, objModule.ModuleID, false);
                    }
                }
            }
            node = xmlPortal.SelectSingleNode("//portal/tabs");
            if (node != null)
            {
                string version = xmlPortal.DocumentElement.GetAttribute("version");
                if (version != "5.0")
                {
                    XmlDocument xmlAdmin = new XmlDocument();
                    try
                    {
                        string path = Path.Combine(templatePath, "admin.template");
                        if (!File.Exists(path))
                        {
                            //if the template is a merged copy of a localized templte the
                            //admin.template may be one director up
                            path = Path.Combine(templatePath, "..\admin.template");
                        }

                        xmlAdmin.Load(path);

                        XmlNode adminNode = xmlAdmin.SelectSingleNode("//portal/tabs");
                        foreach (XmlNode adminTabNode in adminNode.ChildNodes)
                        {
                            node.AppendChild(xmlPortal.ImportNode(adminTabNode, true));
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                ParseTabs(node, portalId, false, mergeTabs, isNewPortal);
            }
        }

        private void PrepareLocalizedPortalTemplate(PortalTemplateInfo template, out string templatePath, out string templateFile)
        {
            if (string.IsNullOrEmpty(template.LanguageFilePath))
            {
                //no language to merge
                templatePath = Path.GetDirectoryName(template.TemplateFilePath) + @"\";
                templateFile = Path.GetFileName(template.TemplateFilePath);
                return;
            }

            templatePath = Path.Combine(TestableGlobals.Instance.HostMapPath, "MergedTemplate");
            Directory.CreateDirectory(templatePath);

            var buffer = new StringBuilder(File.ReadAllText(template.TemplateFilePath));

            XDocument languageDoc;
            using (var reader = PortalTemplateIO.Instance.OpenTextReader(template.LanguageFilePath))
            {
                languageDoc = XDocument.Load(reader);
            }

            var localizedData = languageDoc.Descendants("data");

            foreach (var item in localizedData)
            {
                var nameAttribute = item.Attribute("name");
                if (nameAttribute != null)
                {
                    string name = nameAttribute.Value;
                    var valueElement = item.Descendants("value").FirstOrDefault();
                    if (valueElement != null)
                    {
                        string value = valueElement.Value;

                        buffer = buffer.Replace(string.Format("[{0}]", name), value);
                    }
                }
            }

            templateFile = string.Format("Merged-{0}-{1}", template.CultureCode, Path.GetFileName(template.TemplateFilePath));

            File.WriteAllText(Path.Combine(templatePath, templateFile), buffer.ToString());
        }

        private void UpdatePortalInternal(PortalInfo portal, bool clearCache)
        {
            DataProvider.Instance().UpdatePortalInfo(portal.PortalID,
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
                                            portal.ProcessorPassword,
                                            portal.Description,
                                            portal.KeyWords,
                                            portal.BackgroundFile,
                                            portal.SiteLogHistory,
                                            portal.SplashTabId,
                                            portal.HomeTabId,
                                            portal.LoginTabId,
                                            portal.RegisterTabId,
                                            portal.UserTabId,
                                            portal.SearchTabId,
                                            portal.Custom404TabId,
                                            portal.Custom500TabId,
                                            portal.DefaultLanguage,
                                            portal.HomeDirectory,
                                            UserController.Instance.GetCurrentUserInfo().UserID,
                                            portal.CultureCode);

            EventLogController.Instance.AddLog("PortalId", portal.PortalID.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_UPDATED);

            //ensure a localization item exists (in case a new default language has been set)
            DataProvider.Instance().EnsureLocalizationExists(portal.PortalID, portal.DefaultLanguage);

            //clear portal cache
            if (clearCache)
            {
                DataCache.ClearHostCache(true);
            }
        }

        private static void UpdatePortalSettingInternal(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            string currentSetting = GetPortalSetting(settingName, portalID, string.Empty, cultureCode);

            if (currentSetting != settingValue)
            {
                DataProvider.Instance().UpdatePortalSetting(portalID, settingName, settingValue, UserController.Instance.GetCurrentUserInfo().UserID, cultureCode);
                EventLogController.Instance.AddLog(settingName + ((cultureCode == Null.NullString) ? String.Empty : " (" + cultureCode + ")"), settingValue, GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                if (clearCache)
                {
                    DataCache.ClearPortalCache(portalID, false);
                    DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);
                }
            }
        }

        private void UpdatePortalSetup(int portalId, int administratorId, int administratorRoleId, int registeredRoleId, int splashTabId, int homeTabId, int loginTabId, int registerTabId,
                                       int userTabId, int searchTabId, int custom404TabId, int custom500TabId, int adminTabId, string cultureCode)
        {
            DataProvider.Instance().UpdatePortalSetup(portalId,
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
            EventLogController.Instance.AddLog("PortalId", portalId.ToString(), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_UPDATED);
            DataCache.ClearHostCache(true);
        }
		
        #endregion

		#region Public Methods

        /// <summary>
        /// Creates a new portal alias
        /// </summary>
        /// <param name="portalId">Id of the portal</param>
        /// <param name="portalAlias">Portal Alias to be created</param>
        public void AddPortalAlias(int portalId, string portalAlias)
        {

            //Check if the Alias exists
            PortalAliasInfo portalAliasInfo = PortalAliasController.Instance.GetPortalAlias(portalAlias, portalId);

            //If alias does not exist add new
            if (portalAliasInfo == null)
            {
                portalAliasInfo = new PortalAliasInfo {PortalID = portalId, HTTPAlias = portalAlias, IsPrimary = true};
                PortalAliasController.Instance.AddPortalAlias(portalAliasInfo);
            }
        }

        /// <summary>
        /// Copies the page template.
        /// </summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="mappedHomeDirectory">The mapped home directory.</param>
        public void CopyPageTemplate(string templateFile, string mappedHomeDirectory)
        {
            string hostTemplateFile = string.Format("{0}Templates\\{1}", Globals.HostMapPath, templateFile);
            if (File.Exists(hostTemplateFile))
            {
                string portalTemplateFolder = string.Format("{0}Templates\\", mappedHomeDirectory);
                if (!Directory.Exists(portalTemplateFolder))
                {
                    //Create Portal Templates folder
                    Directory.CreateDirectory(portalTemplateFolder);
                }
                string portalTemplateFile = portalTemplateFolder + templateFile;
                if (!File.Exists(portalTemplateFile))
                {
                    File.Copy(hostTemplateFile, portalTemplateFile);
                }
            }
        }

        /// <summary>
        /// Creates the portal.
        /// </summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUserId">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template"> </param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <c>true</c> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, int adminUserId, string description, string keyWords, PortalTemplateInfo template,
                                string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            //Attempt to create a new portal
            int portalId = CreatePortal(portalName, homeDirectory, template.CultureCode);

            string message = Null.NullString;

            if (portalId != -1)
            {
                //add administrator
                int administratorId = adminUserId;
                var adminUser = new UserInfo();

                //add userportal record
                UserController.AddUserPortal(portalId, administratorId);

                //retrieve existing administrator
                try
                {
                    adminUser = UserController.GetUserById(portalId, administratorId);
                }
                catch (Exception Exc)
                {
                    Logger.Error(Exc);
                }

                if (administratorId > 0)
                {
                    CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new Exception(message);
            }
            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                //should be no exception, but suppress just in case
            }

            return portalId;
        }

        /// <summary>
        /// Creates the portal.
        /// </summary>
        /// <param name="portalName">Name of the portal.</param>
        /// <param name="adminUser">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="template"> </param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <c>true</c> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, PortalTemplateInfo template, 
                                string homeDirectory, string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            //Attempt to create a new portal
            int portalId = CreatePortal(portalName, homeDirectory, template.CultureCode);

            string message = Null.NullString;

            if (portalId != -1)
            {
                //add administrator
                int administratorId = Null.NullInteger;
                adminUser.PortalID = portalId;
                try
                {
                    UserCreateStatus createStatus = UserController.CreateUser(ref adminUser);
                    if (createStatus == UserCreateStatus.Success)
                    {
                        administratorId = adminUser.UserID;
                        //reload the UserInfo as when it was first created, it had no portal id and therefore
                        //used host profile definitions
                        adminUser = UserController.GetUserById(adminUser.PortalID, adminUser.UserID);
                    }
                    else
                    {
                        message += UserController.GetUserCreateStatus(createStatus);
                    }
                }
                catch (Exception Exc)
                {
                    Logger.Error(Exc);
                    message += Localization.GetString("CreateAdminUser.Error") + Exc.Message + Exc.StackTrace;
                }

                if (administratorId > 0)
                {
                    CreatePortalInternal(portalId, portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias, serverPath, childPath, isChildPortal, ref message);
                }
            }
            else
            {
                message += Localization.GetString("CreatePortal.Error");
                throw new Exception(message);
            }
            try
            {
                EnsureRequiredEventLogTypesExist();
            }
            catch (Exception)
            {
                //should be no exception, but suppress just in case
            }

            return portalId;
        }

        /// <summary>
        /// Get all the available portal templates grouped by culture
        /// </summary>
        /// <returns>List of PortalTemplateInfo objects</returns>
        public IList<PortalTemplateInfo> GetAvailablePortalTemplates()
        {
            var list = new List<PortalTemplateInfo>();

            var templateFilePaths = PortalTemplateIO.Instance.EnumerateTemplates();
            var languageFileNames = PortalTemplateIO.Instance.EnumerateLanguageFiles().Select(x => Path.GetFileName(x)).ToList();

            foreach (string templateFilePath in templateFilePaths)
            {
                var currentFileName = Path.GetFileName(templateFilePath);
                var langs = languageFileNames.Where(x => GetTemplateName(x).Equals(currentFileName, StringComparison.InvariantCultureIgnoreCase)).Select(x => GetCultureCode(x)).Distinct().ToList();

                if (langs.Any())
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(templateFilePath, x)));
                }
                else
                {
                    //DNN-6544 portal creation requires valid culture, if template has no culture defined, then use current 
                    list.Add(new PortalTemplateInfo(templateFilePath, (GetCurrentPortalSettingsInternal() != null) ? GetCurrentPortalSettingsInternal().CultureCode : Thread.CurrentThread.CurrentCulture.Name));
                }
            }

            return list;
        }

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        /// <returns>portal settings.</returns>
        PortalSettings IPortalController.GetCurrentPortalSettings()
        {
            return GetCurrentPortalSettingsInternal();
        }

        /// <summary>
        ///   Gets information of a portal
        /// </summary>
        /// <param name = "portalId">Id of the portal</param>
        /// <returns>PortalInfo object with portal definition</returns>
        public PortalInfo GetPortal(int portalId)
        {
            if (portalId == -1)
            {
                return null;
            }

            string defaultLanguage = GetActivePortalLanguage(portalId);
            PortalInfo portal = GetPortal(portalId, defaultLanguage);
            if (portal == null)
            {
                //Active language may not be valid, so fallback to default language
                defaultLanguage = GetPortalDefaultLanguage(portalId);
                portal = GetPortal(portalId, defaultLanguage);
            }
            return portal;
        }

        /// <summary>
        ///   Gets information of a portal
        /// </summary>
        /// <param name = "portalId">Id of the portal</param>
        /// <param name="cultureCode">The culture code.</param>
        public PortalInfo GetPortal(int portalId, string cultureCode)
        {
            if (portalId == -1)
            {
                return null;
            }
            
            PortalInfo portal = GetPortalInternal(portalId, cultureCode);

            if (Localization.ActiveLanguagesByPortalID(portalId) > 1)
            {
                if (portal == null)
                {
                    //Get Fallback language
                    string fallbackLanguage = string.Empty;

                    if (string.IsNullOrEmpty(cultureCode)) cultureCode = GetPortalDefaultLanguage(portalId);

                    Locale userLocale = LocaleController.Instance.GetLocale(cultureCode);
                    if (userLocale != null && !string.IsNullOrEmpty(userLocale.Fallback))
                    {
                        fallbackLanguage = userLocale.Fallback;
                    }
                    if (String.IsNullOrEmpty(fallbackLanguage))
                    {
                        fallbackLanguage = Localization.SystemLocale; 
                    }
                    portal = GetPortalInternal(portalId, fallbackLanguage);
                    //if we cannot find any fallback, it mean's it's a non portal default langauge
                    if (portal == null)
                    {
                        DataProvider.Instance().EnsureLocalizationExists(portalId, GetActivePortalLanguage(portalId));
						DataCache.ClearHostCache(true);
                        portal = GetPortalInternal(portalId, GetActivePortalLanguage(portalId));
                    }
                }
            }
            return portal;
        }

        /// <summary>
        /// Gets the portal.
        /// </summary>
        /// <param name="uniqueId">The unique id.</param>
        /// <returns>Portal info.</returns>
        public PortalInfo GetPortal(Guid uniqueId)
        {
			return GetPortalList(Null.NullString).SingleOrDefault(p => p.GUID == uniqueId);
		}

		/// <summary>
		/// Gets information from all portals
		/// </summary>
		/// <returns>ArrayList of PortalInfo objects</returns>
		public ArrayList GetPortals()
        {
			return new ArrayList(GetPortalList(Null.NullString));
		}

        //public ArrayList GetPortals()
        //{
        //    return new ArrayList(GetPortalList(Localization.SystemLocale));
        //}

        /// <summary>
        /// Get portals in specific culture.
        /// </summary>
        /// <param name="cultureCode">The culture code.</param>
        /// <returns></returns>
        public List<PortalInfo> GetPortalList(string cultureCode)
        {
            string cacheKey = String.Format(DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
            return CBO.GetCachedObject<List<PortalInfo>>(new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, cultureCode),
                                                    GetPortalsCallBack);
        }

        public Dictionary<string, string> GetPortalSettings(int portalId)
        {
            return GetPortalSettingsDictionary(portalId, string.Empty);
        }

        /// <summary>
        /// Load info for a portal template
        /// </summary>
        /// <param name="templatePath">Full path to the portal template</param>
        /// <param name="cultureCode">the culture code if any for the localization of the portal template</param>
        /// <returns>A portal template</returns>
        public PortalTemplateInfo GetPortalTemplate(string templatePath, string cultureCode)
        {
            var template = new PortalTemplateInfo(templatePath, cultureCode);

            if (!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

            return template;
        }

        /// <summary>
		/// Gets the portal space used bytes.
		/// </summary>
		/// <param name="portalId">The portal id.</param>
		/// <returns>Space used in bytes</returns>
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

        /// <summary>
        /// Verifies if there's enough space to upload a new file on the given portal
        /// </summary>
		/// <param name="portalId">Id of the portal</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded</param>
        /// <returns>True if there's enough space available to upload the file</returns>
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
                    PortalInfo portal = GetPortal(portalId);
                    hostSpace = portal.HostSpace;
                }
            }
            return (((GetPortalSpaceUsedBytes(portalId) + fileSizeBytes) / Math.Pow(1024, 2)) <= hostSpace) || hostSpace == 0;
        }

        /// <summary>
        ///   Remaps the Special Pages such as Home, Profile, Search
        ///   to their localized versions
        /// </summary>
        /// <remarks>
        /// </remarks>
        public void MapLocalizedSpecialPages(int portalId, string cultureCode)
        {
            DataCache.ClearHostCache(true);
            DataProvider.Instance().EnsureLocalizationExists(portalId, cultureCode);

            PortalInfo defaultPortal = GetPortal(portalId, GetPortalDefaultLanguage(portalId));
            PortalInfo targetPortal = GetPortal(portalId, cultureCode);

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

            UpdatePortalInternal(targetPortal, false);
        }

        /// <summary>
        /// Removes the related PortalLocalization record from the database, adds optional clear cache
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void RemovePortalLocalization(int portalId, string cultureCode, bool clearCache = true)
        {
            DataProvider.Instance().RemovePortalLocalization(portalId, cultureCode);
            if (clearCache)
            {
                DataCache.ClearPortalCache(portalId, false);
            }
        }

        /// <summary>
        /// Processess a template file for the new portal.
        /// </summary>
        /// <param name="portalId">PortalId of the new portal</param>
        /// <param name="template">The template</param>
        /// <param name="administratorId">UserId for the portal administrator. This is used to assign roles to this user</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="isNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        public void ParseTemplate(int portalId, PortalTemplateInfo template, int administratorId, PortalTemplateModuleAction mergeTabs, bool isNewPortal)
        {
            string templatePath, templateFile;
            PrepareLocalizedPortalTemplate(template, out templatePath, out templateFile);

            ParseTemplateInternal(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal);
        }

        /// <summary>
        /// Processes the resource file for the template file selected
        /// </summary>
        /// <param name="portalPath">New portal's folder</param>
        /// <param name="resoureceFile">full path to the resource file</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to disable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        public void ProcessResourceFileExplicit(string portalPath, string resoureceFile)
        {
            try
            {
                FileSystemUtils.UnzipResources(new ZipInputStream(new FileStream(resoureceFile, FileMode.Open, FileAccess.Read)), portalPath);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

		/// <summary>
		/// Updates the portal expiry.
		/// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="cultureCode">The culture code.</param>
        public void UpdatePortalExpiry(int portalId, string cultureCode)
        {
            var portal = GetPortal(portalId, cultureCode);

            if (portal.ExpiryDate == Null.NullDate)
            {
                portal.ExpiryDate = DateTime.Now;
            }
		    portal.ExpiryDate = portal.ExpiryDate.AddMonths(1);

            UpdatePortalInfo(portal);
        }

        /// <summary>
        /// Updates basic portal information
        /// </summary>
        /// <param name="portal"></param>
        public void UpdatePortalInfo(PortalInfo portal)
        {
            UpdatePortalInternal(portal, true);
        }

        void IPortalController.UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
        {
            UpdatePortalSettingInternal(portalID, settingName, settingValue, clearCache, cultureCode);
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Adds the portal dictionary.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <param name="tabId">The tab id.</param>
        public static void AddPortalDictionary(int portalId, int tabId)
        {
            var portalDic = GetPortalDictionary();
            portalDic[tabId] = portalId;
            DataCache.SetCache(DataCache.PortalDictionaryCacheKey, portalDic);
        }

        /// <summary>
        /// Creates the root folder for a child portal.
        /// </summary>
        /// <remarks>
        /// If call this method, it will create the specific folder if the folder doesn't exist;
        /// and will copy subhost.aspx to the folder if there is no 'Default.aspx';
        /// </remarks>
        /// <param name="ChildPath">The child path.</param>
        /// <returns>
        /// If the method executed successful, it will return NullString, otherwise return error message.
        /// </returns>
        /// <example>
        /// <code lang="C#">
        /// string childPhysicalPath = Server.MapPath(childPath);
        /// message = PortalController.CreateChildPortalFolder(childPhysicalPath);
        /// </code>
        /// </example>
        public static string CreateChildPortalFolder(string ChildPath)
        {
            string message = Null.NullString;

            //Set up Child Portal
            try
            {
                // create the subdirectory for the new portal
                if (!Directory.Exists(ChildPath))
                {
                    Directory.CreateDirectory(ChildPath);
                }

                // create the subhost default.aspx file
                if (!File.Exists(ChildPath + "\\" + Globals.glbDefaultPage))
                {
                    File.Copy(Globals.HostMapPath + "subhost.aspx", ChildPath + "\\" + Globals.glbDefaultPage);
                }
            }
            catch (Exception Exc)
            {
                Logger.Error(Exc);
                message += Localization.GetString("ChildPortal.Error") + Exc.Message + Exc.StackTrace;
            }

            return message;
        }

        /// <summary>
        /// Deletes all expired portals.
        /// </summary>
        /// <param name="serverPath">The server path.</param>
        public static void DeleteExpiredPortals(string serverPath)
        {
            foreach (PortalInfo portal in GetExpiredPortals())
            {
                DeletePortal(portal, serverPath);
            }
        }

        /// <summary>
        /// Deletes the portal.
        /// </summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>If the method executed successful, it will return NullString, otherwise return error message.</returns>
        public static string DeletePortal(PortalInfo portal, string serverPath)
        {
            string message = string.Empty;

            //check if this is the last portal
            int portalCount = Instance.GetPortals().Count;
            if (portalCount > 1)
            {
                if (portal != null)
                {
                    //delete custom resource files
                    Globals.DeleteFilesRecursive(serverPath, ".Portal-" + portal.PortalID + ".resx");

                    //If child portal delete child folder
                    var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
                    if (arr.Count > 0)
                    {
                        var portalAliasInfo = (PortalAliasInfo)arr[0];
                        string portalName = Globals.GetPortalDomainName(portalAliasInfo.HTTPAlias, null, true);
                        if (portalAliasInfo.HTTPAlias.IndexOf("/", StringComparison.Ordinal) > -1)
                        {
                            portalName = GetPortalFolder(portalAliasInfo.HTTPAlias);
                        }
                        if (!String.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                        {
                            DeletePortalFolder(serverPath, portalName);
                        }
                    }
                    //delete upload directory
                    Globals.DeleteFolderRecursive(serverPath + "Portals\\" + portal.PortalID);
                    if (!string.IsNullOrEmpty(portal.HomeDirectory))
                    {
                        string HomeDirectory = portal.HomeDirectoryMapPath;
                        if (Directory.Exists(HomeDirectory))
                        {
                            Globals.DeleteFolderRecursive(HomeDirectory);
                        }
                    }
                    //remove database references
                    DeletePortalInternal(portal.PortalID);
                }
            }
            else
            {
                message = Localization.GetString("LastPortal");
            }
            return message;
        }

        /// <summary>
        /// Get the portal folder froma child portal alias.
        /// </summary>
        /// <param name="alias">portal alias.</param>
        /// <returns>folder path of the child portal.</returns>
        public static string GetPortalFolder(string alias)
        {
            alias = alias.ToLowerInvariant().Replace("http://", string.Empty).Replace("https://", string.Empty);
            var appPath = Globals.ApplicationPath + "/";
            if (string.IsNullOrEmpty(Globals.ApplicationPath) || alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) == Null.NullInteger)
            {
                return alias.Contains("/") ? alias.Substring(alias.IndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1) : string.Empty;
            }

            return alias.Substring(alias.IndexOf(appPath, StringComparison.InvariantCultureIgnoreCase) + appPath.Length);
        }

        /// <summary>
        /// Delete the child portal folder and try to remove its parent when parent folder is empty.
        /// </summary>
        /// <param name="serverPath">the server path.</param>
        /// <param name="portalFolder">the child folder path.</param>
        /// <returns></returns>
        public static void DeletePortalFolder(string serverPath, string portalFolder)
        {
            var physicalPath = serverPath + portalFolder;
            Globals.DeleteFolderRecursive(physicalPath);
            //remove parent folder if its empty.
            var parentFolder = Directory.GetParent(physicalPath);
            while (parentFolder != null && !parentFolder.FullName.Equals(serverPath.TrimEnd('\\'), StringComparison.InvariantCultureIgnoreCase))
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

        /// <summary>
        /// Gets the portal dictionary.
        /// </summary>
        /// <returns>portal dictionary. the dictionary's Key -> Value is: TabId -> PortalId.</returns>
        public static Dictionary<int, int> GetPortalDictionary()
        {
            string cacheKey = string.Format(DataCache.PortalDictionaryCacheKey);
            return CBO.GetCachedObject<Dictionary<int, int>>(new CacheItemArgs(cacheKey, DataCache.PortalDictionaryTimeOut, DataCache.PortalDictionaryCachePriority), GetPortalDictionaryCallback);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetPortalsByName gets all the portals whose name matches a provided filter expression
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="nameToMatch">The email address to use to find a match.</param>
        /// <param name="pageIndex">The page of records to return.</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="totalRecords">The total no of records that satisfy the criteria.</param>
        /// <returns>An ArrayList of PortalInfo objects.</returns>
        /// <history>
        ///     [cnurse]	11/17/2006	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetPortalsByName(string nameToMatch, int pageIndex, int pageSize, ref int totalRecords)
        {
            if (pageIndex == -1)
            {
                pageIndex = 0;
                pageSize = int.MaxValue;
            }
            Type type = typeof(PortalInfo);
            return CBO.FillCollection(DataProvider.Instance().GetPortalsByName(nameToMatch, pageIndex, pageSize), ref type, ref totalRecords);
        }

        public static ArrayList GetPortalsByUser(int userId)
        {
            Type type = typeof(PortalInfo);
            return CBO.FillCollection(DataProvider.Instance().GetPortalsByUser(userId), type);
        }

        public static int GetEffectivePortalId(int portalId)
        {
            if (portalId > Null.NullInteger && Globals.Status != Globals.UpgradeStatus.Upgrade)
            {
                var portal = PortalController.Instance.GetPortal(portalId);
                var portalGroup = (from p in PortalGroupController.Instance.GetPortalGroups()
                                   where p.PortalGroupId == portal.PortalGroupID
                                   select p)
                                .SingleOrDefault();

                if (portalGroup != null)
                {
                    portalId = portalGroup.MasterPortalId;
                }
            }

            return portalId;
        }

        /// <summary>
        /// Gets all expired portals.
        /// </summary>
        /// <returns>all expired portals as array list.</returns>
        public static ArrayList GetExpiredPortals()
        {
            return CBO.FillCollection(DataProvider.Instance().GetExpiredPortals(), typeof(PortalInfo));
        }

        /// <summary>
        /// Determines whether the portal is child portal.
        /// </summary>
        /// <param name="portal">The portal.</param>
        /// <param name="serverPath">The server path.</param>
        /// <returns>
        ///   <c>true</c> if the portal is child portal; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsChildPortal(PortalInfo portal, string serverPath)
        {
            bool isChild = Null.NullBoolean;
            string portalName;
            var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
            if (arr.Count > 0)
            {
                PortalAliasInfo portalAlias = (PortalAliasInfo)arr[0];
                portalName = Globals.GetPortalDomainName(portalAlias.HTTPAlias, null, true);
                if (portalAlias.HTTPAlias.IndexOf("/") > -1)
                {
                    portalName = GetPortalFolder(portalAlias.HTTPAlias);
                }
                if (!String.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                {
                    isChild = true;
                }
            }
            return isChild;
        }

        public static bool IsMemberOfPortalGroup(int portalId)
        {
            var portal = PortalController.Instance.GetPortal(portalId);

			return portal != null && portal.PortalGroupID > Null.NullInteger;
        }

		/// <summary>
		/// Deletes the portal setting (neutral and for all languages).
		/// </summary>
		/// <param name="portalID">The portal ID.</param>
		/// <param name="settingName">Name of the setting.</param>
		public static void DeletePortalSetting(int portalID, string settingName)
		{
			DeletePortalSetting(portalID, settingName, Null.NullString);
		}

        /// <summary>
		/// Deletes the portal setting in this language.
		/// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
		/// <param name="cultureCode">The culture code.</param>
		public static void DeletePortalSetting(int portalID, string settingName, string cultureCode)
		{
			DataProvider.Instance().DeletePortalSetting(portalID, settingName, cultureCode.ToLower());
			EventLogController.Instance.AddLog("SettingName", settingName + ((cultureCode == Null.NullString) ? String.Empty : " (" + cultureCode + ")"), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_DELETED);
			DataCache.ClearHostCache(true);
		}

        /// <summary>
        /// Deletes all portal settings by portal id.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        public static void DeletePortalSettings(int portalID)
        {
			DeletePortalSettings(portalID, Null.NullString);
		}

		/// <summary>
		/// Deletes all portal settings by portal id and for a given language (Null: all languages and neutral settings).
		/// </summary>
		/// <param name="portalID">The portal ID.</param>
		public static void DeletePortalSettings(int portalID, string cultureCode)
		{
			DataProvider.Instance().DeletePortalSettings(portalID, cultureCode);
			EventLogController.Instance.AddLog("PortalID", portalID.ToString() + ((cultureCode == Null.NullString) ? String.Empty : " (" + cultureCode + ")"), GetCurrentPortalSettingsInternal(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_DELETED);
			DataCache.ClearHostCache(true);
		}

        /// <summary>
        /// takes in a text value, decrypts it with a FIPS compliant algorithm and returns the value
        /// </summary>
        /// <param name="settingName">the setting to read</param>
        /// <param name="passPhrase">the pass phrase used for encryption/decryption</param>
        /// <returns></returns>
        public static string GetEncryptedString(string settingName, int portalID, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", settingName);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = GetPortalSetting(settingName, portalID, string.Empty);

            return Security.FIPSCompliant.DecryptAES(cipherText, passPhrase, Host.Host.GUID);
        }

        /// <summary>
        /// Gets the portal setting.
        /// </summary>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static string GetPortalSetting(string settingName, int portalID, string defaultValue)
        {
            var retValue = Null.NullString;
            try
            {
                string setting;
                PortalController.Instance.GetPortalSettings(portalID).TryGetValue(settingName, out setting);
                retValue = string.IsNullOrEmpty(setting) ? defaultValue : setting;
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            return retValue;
        }

        /// <summary>
		/// Gets the portal setting for a specific language (or neutral).
		/// </summary>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="portalID">The portal ID.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="cultureCode">culture code of the language to retrieve (not empty)</param>
		/// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
		public static string GetPortalSetting(string settingName, int portalID, string defaultValue, string cultureCode)
		{
			var retValue = Null.NullString;
			try
			{
				string setting;
				GetPortalSettingsDictionary(portalID, cultureCode).TryGetValue(settingName, out setting);
				retValue = string.IsNullOrEmpty(setting) ? defaultValue : setting;
			}
			catch (Exception exc)
			{
				Logger.Error(exc);
			}
			return retValue;
		}

		/// <summary>
		/// Gets the portal setting as boolean.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue)
        {
            bool retValue = Null.NullBoolean;
            try
            {
                string setting = Null.NullString;
                PortalController.Instance.GetPortalSettings(portalID).TryGetValue(key, out setting);
                if (string.IsNullOrEmpty(setting))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = (setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.ToUpperInvariant() == "TRUE");
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
            return retValue;
        }

        /// <summary>
		/// Gets the portal setting as boolean for a specific language (or neutral).
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="portalID">The portal ID.</param>
		/// <param name="defaultValue">default value.</param>
		/// <param name="cultureCode">culture code of the language to retrieve (not empty)</param>
		/// <returns>Returns setting's value if portal contains the specific setting in specified language or neutral, otherwise return defaultValue.</returns>
		public static bool GetPortalSettingAsBoolean(string key, int portalID, bool defaultValue, string cultureCode)
		{
			bool retValue = Null.NullBoolean;
			try
			{
				string setting = Null.NullString;
				GetPortalSettingsDictionary(portalID, cultureCode).TryGetValue(key, out setting);
				if (string.IsNullOrEmpty(setting))
				{
					retValue = defaultValue;
				}
				else
				{
					retValue = (setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.ToUpperInvariant() == "TRUE");
				}
			}
			catch (Exception exc)
			{
				Logger.Error(exc);
			}
			return retValue;
		}

		/// <summary>
		/// Gets the portal setting as integer.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>Returns setting's value if portal contains the specific setting, otherwise return defaultValue.</returns>
        public static int GetPortalSettingAsInteger(string key, int portalID, int defaultValue)
        {
            int retValue = Null.NullInteger;
            try
            {
                string setting = Null.NullString;
                PortalController.Instance.GetPortalSettings(portalID).TryGetValue(key, out setting);
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

        /// <summary>
		/// Gets the portal setting as integer for a specific language (or neutral).
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="portalID">The portal ID.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <param name="cultureCode">culture code of the language to retrieve (not empty)</param>
		/// <returns>Returns setting's value if portal contains the specific setting (for specified lang, otherwise return defaultValue.</returns>
		public static int GetPortalSettingAsInteger(string key, int portalID, int defaultValue, string cultureCode)
		{
			int retValue = Null.NullInteger;
			try
			{
				string setting = Null.NullString;
				GetPortalSettingsDictionary(portalID, cultureCode).TryGetValue(key, out setting);
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
		/// <summary>
		/// takes in a text value, encrypts it with a FIPS compliant algorithm and stores
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">host settings key</param>
        /// <param name="settingValue">host settings value</param>
        /// <param name="passPhrase">pass phrase to allow encryption/decryption</param>
        public static void UpdateEncryptedString(int portalID, string settingName, string settingValue, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", settingName);
            Requires.NotNull("value", settingValue);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var cipherText = Security.FIPSCompliant.EncryptAES(settingValue, passPhrase, Entities.Host.Host.GUID);

            UpdatePortalSetting(portalID, settingName, cipherText);
        }

        /// <summary>
		/// Updates a single neutral (not language specific) portal setting and clears it from the cache.
		/// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, true);
        }

        /// <summary>
		/// Updates a single neutral (not language specific) portal setting, optionally without clearing the cache.
		/// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache)
        {
			UpdatePortalSetting(portalID, settingName, settingValue, clearCache, Null.NullString);
		}

		/// <summary>
		/// Updates a language specific or neutral portal setting and clears it from the cache.
		/// </summary>
		/// <param name="portalID">The portal ID.</param>
		/// <param name="settingName">Name of the setting.</param>
		/// <param name="settingValue">The setting value.</param>
		/// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
		public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, string cultureCode)
		{
			UpdatePortalSetting(portalID, settingName, settingValue, true, cultureCode);
		}

        /// <summary>
		/// Updates a language specific or neutral portal setting and optionally clears it from the cache.
		/// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
		/// <param name="cultureCode">culture code for language specific settings, null string ontherwise.</param>
		public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string cultureCode)
		{
            Instance.UpdatePortalSetting(portalID, settingName, settingValue, clearCache, cultureCode);

		}

        /// <summary>
        /// Checks the desktop modules whether is installed.
        /// </summary>
        /// <param name="nav">The nav.</param>
        /// <returns>Empty string if the module hasn't been installed, otherwise return the frind name.</returns>
        public static string CheckDesktopModulesInstalled(XPathNavigator nav)
        {
            string friendlyName = Null.NullString;
            DesktopModuleInfo desktopModule = null;
            StringBuilder modulesNotInstalled = new StringBuilder();

            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
                friendlyName = XmlUtils.GetNodeValue(desktopModuleNav, "friendlyname");

                if (!string.IsNullOrEmpty(friendlyName))
                {
                    desktopModule = DesktopModuleController.GetDesktopModuleByFriendlyName(friendlyName);
                    if (desktopModule == null)
                    {
                        //PE and EE templates have HTML as friendly name so check to make sure
                        //there is really no HTML module installed
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
        ///   function provides the language for portalinfo requests
        ///   in case where language has not been installed yet, will return the core install default of en-us
        /// </summary>
        /// <param name = "portalID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static string GetActivePortalLanguage(int portalID)
        {
            // get Language
            string Language = Localization.SystemLocale;
            string tmpLanguage = GetPortalDefaultLanguage(portalID);
        	var isDefaultLanguage = false;
            if (!String.IsNullOrEmpty(tmpLanguage))
            {
                Language = tmpLanguage;
            	isDefaultLanguage = true;
            }
            //handles case where portalcontroller methods invoked before a language is installed
            if (portalID > Null.NullInteger && Globals.Status == Globals.UpgradeStatus.None && Localization.ActiveLanguagesByPortalID(portalID) == 1)
            {
                return Language;
            }
            if (HttpContext.Current != null && Globals.Status == Globals.UpgradeStatus.None)
            {
                if ((HttpContext.Current.Request.QueryString["language"] != null))
                {
                    Language = HttpContext.Current.Request.QueryString["language"];
					isDefaultLanguage = false;
                }
                else
                {
                    PortalSettings _PortalSettings = GetCurrentPortalSettingsInternal();
                    if (_PortalSettings != null && _PortalSettings.ActiveTab != null && !String.IsNullOrEmpty(_PortalSettings.ActiveTab.CultureCode))
                    {
                        Language = _PortalSettings.ActiveTab.CultureCode;
						isDefaultLanguage = false;
                    }
                    else
                    {
                        //PortalSettings IS Nothing - probably means we haven't set it yet (in Begin Request)
                        //so try detecting the user's cookie
                        if (HttpContext.Current.Request["language"] != null)
                        {
                            Language = HttpContext.Current.Request["language"];
							isDefaultLanguage = false;
                        }

                        //if no cookie - try detecting browser
						if ((String.IsNullOrEmpty(Language) || isDefaultLanguage) && EnableBrowserLanguageInDefault(portalID))
                        {
                            CultureInfo Culture = Localization.GetBrowserCulture(portalID);

                            if (Culture != null)
                            {
                                Language = Culture.Name;
                            }
                        }
                    }
                }
            }

            return Language;
        }

        /// <summary>
        ///   return the current DefaultLanguage value from the Portals table for the requested Portalid
        /// </summary>
        /// <param name = "portalID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        public static string GetPortalDefaultLanguage(int portalID)
        {
            string cacheKey = String.Format("PortalDefaultLanguage_{0}", portalID);
            return CBO.GetCachedObject<string>(new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, portalID), GetPortalDefaultLanguageCallBack);
        }

        /// <summary>
        ///   set the required DefaultLanguage in the Portals table for a particular portal
        ///   saves having to update an entire PortalInfo object
        /// </summary>
        /// <param name = "portalID"></param>
        /// <param name = "CultureCode"></param>
        /// <remarks>
        /// </remarks>
        public static void UpdatePortalDefaultLanguage(int portalID, string CultureCode)
        {
            DataProvider.Instance().UpdatePortalDefaultLanguage(portalID, CultureCode);
            //ensure localization record exists as new portal default language may be relying on fallback chain
            //of which it is now the final part
            DataProvider.Instance().EnsureLocalizationExists(portalID, CultureCode);
        }

        public static void IncrementCrmVersion(int portalID)
        {
            int currentVersion;
            var versionSetting = GetPortalSetting(ClientResourceSettings.VersionKey, portalID, "1");
            if (int.TryParse(versionSetting, out currentVersion))
            {
                var newVersion = currentVersion + 1;
                UpdatePortalSetting(portalID, ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);
            }
        }

        public static void IncrementOverridingPortalsCrmVersion()
        {
            foreach (PortalInfo portal in PortalController.Instance.GetPortals())
            {
                string setting = GetPortalSetting(ClientResourceSettings.OverrideDefaultSettingsKey, portal.PortalID, "False");
                bool overriden;

                // if this portal is overriding the host level...
                if (bool.TryParse(setting, out overriden) && overriden)
                {
                    // increment its version
                    IncrementCrmVersion(portal.PortalID);
                }
            }
        }

        #endregion

        public class PortalTemplateInfo
        {
            private string _resourceFilePath;
            private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalController));

            public PortalTemplateInfo(string templateFilePath, string cultureCode)
            {
                TemplateFilePath = templateFilePath;

                InitLocalizationFields(cultureCode);
                InitNameAndDescription();
            }

            private void InitNameAndDescription()
            {
                if (!String.IsNullOrEmpty(LanguageFilePath))
                {
                    LoadNameAndDescriptionFromLanguageFile();
                }

                if (String.IsNullOrEmpty(Name))
                {
                    Name = Path.GetFileNameWithoutExtension(TemplateFilePath);
                }

                if (String.IsNullOrEmpty(Description))
                {
                    LoadDescriptionFromTemplateFile();
                }
            }

            private void LoadDescriptionFromTemplateFile()
            {
                try
                {
                    XDocument xmlDoc;
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(TemplateFilePath))
                    {
                        xmlDoc = XDocument.Load(reader);
                    }

                    Description = xmlDoc.Elements("portal").Elements("description").SingleOrDefault().Value;
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + TemplateFilePath, e);
                }
            }

            private void LoadNameAndDescriptionFromLanguageFile()
            {
                try
                {
                    using (var reader = PortalTemplateIO.Instance.OpenTextReader(LanguageFilePath))
                    {
                        var xmlDoc = XDocument.Load(reader);

                        Name = ReadLanguageFileValue(xmlDoc, "LocalizedTemplateName.Text");
                        Description = ReadLanguageFileValue(xmlDoc, "PortalDescription.Text");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error while parsing: " + TemplateFilePath, e);
                }
            }

            static string ReadLanguageFileValue(XDocument xmlDoc, string name)
            {
                return (from f in xmlDoc.Descendants("data")
                        where (string)f.Attribute("name") == name
                        select (string)f.Element("value")).SingleOrDefault();
            }

            private void InitLocalizationFields(string cultureCode)
            {
                LanguageFilePath = PortalTemplateIO.Instance.GetLanguageFilePath(TemplateFilePath, cultureCode);
                if (!String.IsNullOrEmpty(LanguageFilePath))
                {
                    CultureCode = cultureCode;
                }
                else
                {
                    //DNN-6544 portal creation requires valid culture, if template has no culture defined, then use current
                    CultureCode = (GetCurrentPortalSettingsInternal() != null) ? GetCurrentPortalSettingsInternal().CultureCode : Thread.CurrentThread.CurrentCulture.Name;
                }
            }

            public string Name { get; private set; }
            public string CultureCode { get; private set; }
            public string TemplateFilePath { get; private set; }
            public string LanguageFilePath { get; private set; }
            public string Description { get; private set; }

            public string ResourceFilePath
            {
                get
                {
                    if (_resourceFilePath == null)
                    {
                        _resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(TemplateFilePath);
                    }

                    return _resourceFilePath;
                }
            }
        }
    }
}
