#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.FileSystem;
using DotNetNuke.Services.Installer;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Client;

using ICSharpCode.SharpZipLib.Zip;

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
    public class PortalController : IPortalController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (PortalController));

        #region const values

        public const string HtmlText_TimeToAutoSave = "HtmlText_TimeToAutoSave";
        public const string HtmlText_AutoSaveEnabled = "HtmlText_AutoSaveEnabled";
        
        #endregion

        #region Private Methods

        private void AddFolderPermissions(int portalId, int folderId)
        {
            var objPortal = GetPortal(portalId);
            FolderPermissionInfo objFolderPermission;
            var folderManager = FolderManager.Instance;
            var folder = folderManager.GetFolder(folderId);
            var objPermissionController = new PermissionController();
            foreach (PermissionInfo objpermission in objPermissionController.GetPermissionByCodeAndKey("SYSTEM_FOLDER", ""))
            {
                objFolderPermission = new FolderPermissionInfo(objpermission)
                {
                    FolderID = folder.FolderID,
                    RoleID = objPortal.AdministratorRoleId,
                    AllowAccess = true
                };

                folder.FolderPermissions.Add(objFolderPermission);
                if (objpermission.PermissionKey == "READ")
                {
                    //add READ permissions to the All Users Role
                    folderManager.AddAllUserReadPermission(folder, objpermission);
                }
            }
            FolderPermissionController.SaveFolderPermissions((FolderInfo)folder);
        }

        private static void CreateDefaultPortalRoles(int portalId, int administratorId, ref int administratorRoleId, ref int registeredRoleId, ref int subscriberRoleId, int unverifiedRoleId)
        {
            var controller = new RoleController();

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
            controller.AddUserRole(portalId, administratorId, administratorRoleId, Null.NullDate, Null.NullDate);
            controller.AddUserRole(portalId, administratorId, registeredRoleId, Null.NullDate, Null.NullDate);
            controller.AddUserRole(portalId, administratorId, subscriberRoleId, Null.NullDate, Null.NullDate);
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

        private static int CreatePortal(string portalName, string homeDirectory)
        {
            //add portal
            int PortalId = -1;
            try
            {
                //Use host settings as default values for these parameters
                //This can be overwritten on the portal template
                DateTime datExpiryDate;
                if (Host.Host.DemoPeriod > Null.NullInteger)
                {
                    datExpiryDate = Convert.ToDateTime(Globals.GetMediumDate(DateTime.Now.AddDays(Host.Host.DemoPeriod).ToString()));
                }
                else
                {
                    datExpiryDate = Null.NullDate;
                }
                PortalId = DataProvider.Instance().CreatePortal(portalName,
                                                                Host.Host.HostCurrency,
                                                                datExpiryDate,
                                                                Host.Host.HostFee,
                                                                Host.Host.HostSpace,
                                                                Host.Host.PageQuota,
                                                                Host.Host.UserQuota,
                                                                Host.Host.SiteLogHistory,
                                                                homeDirectory,
                                                                UserController.GetCurrentUserInfo().UserID);

                //clear portal cache
                DataCache.ClearHostCache(true);

                EventLogController objEventLog = new EventLogController();
                objEventLog.AddLog("PortalName", portalName, GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_CREATED);
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
            objRoleInfo = TestableRoleController.Instance.GetRole(role.PortalID, r => r.RoleName == role.RoleName);
            if (objRoleInfo == null)
            {
                roleId = TestableRoleController.Instance.AddRole(role);
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
            RoleController objRoleController = new RoleController();
            int roleGroupId = Null.NullInteger;

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

        private static bool DoesLogTypeExists(string logTypeKey)
        {
            var logController = new LogController();
            LogTypeInfo logType;
            Dictionary<string, LogTypeInfo> logTypeDictionary = logController.GetLogTypeInfoDictionary();
            logTypeDictionary.TryGetValue(logTypeKey, out logType);
            if (logType == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// this ensures that all portals have any logtypes (and sometimes configuration) required
        /// </summary>
        public static void EnsureRequiredEventLogTypesExist()
        {
            var logController = new LogController();
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
                logController.AddLogType(logTypeInfo);

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
                logController.AddLogTypeConfigInfo(logTypeConf);
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
                logController.AddLogType(logTypeFilterInfo);

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
                logController.AddLogTypeConfigInfo(logTypeFilterConf);
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
                logController.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL updated";
                logController.AddLogType(logTypeInfo);

                logTypeInfo.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                logTypeInfo.LogTypeFriendlyName = "TabURL deleted";
                logController.AddLogType(logTypeInfo);

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
                logController.AddLogTypeConfigInfo(logTypeUrlConf);
                
                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_UPDATED.ToString();
                logController.AddLogTypeConfigInfo(logTypeUrlConf);

                logTypeUrlConf.LogTypeKey = EventLogController.EventLogType.TABURL_DELETED.ToString();
                logController.AddLogTypeConfigInfo(logTypeUrlConf);
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
                logController.AddLogType(logTypeFilterInfo);

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
                logController.AddLogTypeConfigInfo(logTypeFilterConf);
            }

        }

        private static PortalInfo GetPortalInternal(int portalID, string cultureCode)
        {
            var controller = new PortalController();
            return controller.GetPortalList(cultureCode).SingleOrDefault(p => p.PortalID == portalID);
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
	        var cultureCode = Convert.ToString(cacheItemArgs.ParamList[1]);
			if (string.IsNullOrEmpty(cultureCode))
			{
				cultureCode = GetActivePortalLanguage(portalId);
			}

            var dicSettings = new Dictionary<string, string>();
			IDataReader dr = DataProvider.Instance().GetPortalSettings(portalId, cultureCode);
            try
            {
                while (dr.Read())
                {
                    if (!dr.IsDBNull(1))
                    {
                        dicSettings.Add(dr.GetString(0), dr.GetString(1));
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
					using (var fileContent = fileManager.GetFileContent(objInfo))
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
                        RoleInfo objRole = TestableRoleController.Instance.GetRole(portalId, r => r.RoleName == roleName);
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

        private void ParseFolders(XmlNode nodeFolders, int PortalId)
        {
            IFolderInfo objInfo;
            string folderPath;
            int storageLocation;
            bool isProtected = false;
            var folderManager = FolderManager.Instance;
            var folderMappingController = FolderMappingController.Instance;
            FolderMappingInfo folderMapping = null;

            foreach (XmlNode node in nodeFolders.SelectNodes("//folder"))
            {
                folderPath = XmlUtils.GetNodeValue(node.CreateNavigator(), "folderpath");

                //First check if the folder exists
                objInfo = folderManager.GetFolder(PortalId, folderPath);

                if (objInfo == null)
                {
                    isProtected = PathUtils.Instance.IsDefaultProtectedPath(folderPath);

                    if (isProtected)
                    {
                        //protected folders must use insecure storage
                        folderMapping = folderMappingController.GetDefaultFolderMapping(PortalId);
                    }
                    else
                    {
                        storageLocation = Convert.ToInt32(XmlUtils.GetNodeValue(node, "storagelocation", "0"));

                        switch (storageLocation)
                        {
                            case (int)FolderController.StorageLocationTypes.InsecureFileSystem:
                                folderMapping = folderMappingController.GetDefaultFolderMapping(PortalId);
                                break;
                            case (int)FolderController.StorageLocationTypes.SecureFileSystem:
                                folderMapping = folderMappingController.GetFolderMapping(PortalId, "Secure");
                                break;
                            case (int)FolderController.StorageLocationTypes.DatabaseSecure:
                                folderMapping = folderMappingController.GetFolderMapping(PortalId, "Database");
                                break;
                            default:
                                break;
                        }

                        isProtected = XmlUtils.GetNodeValueBoolean(node, "isprotected");
                    }
                    //Save new folder
                    objInfo = folderManager.AddFolder(folderMapping, folderPath);
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
        }

        private static void ParsePortalDesktopModules(XPathNavigator nav, int portalID)
        {
            string friendlyName = Null.NullString;
            DesktopModuleInfo desktopModule = null;
            foreach (XPathNavigator desktopModuleNav in nav.Select("portalDesktopModule"))
            {
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
                                RoleInfo role = TestableRoleController.Instance.GetRole(portalID, r => r.RoleName == rolename);
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
                UpdatePortalSetting(PortalId, "DefaultPortalSkin", XmlUtils.GetNodeValue(nodeSettings, "skinsrc", ""));
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "skinsrcadmin", "")))
            {
                UpdatePortalSetting(PortalId, "DefaultAdminSkin", XmlUtils.GetNodeValue(nodeSettings, "skinsrcadmin", ""));
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "containersrc", "")))
            {
                UpdatePortalSetting(PortalId, "DefaultPortalContainer", XmlUtils.GetNodeValue(nodeSettings, "containersrc", ""));
            }
            if (!String.IsNullOrEmpty(XmlUtils.GetNodeValue(nodeSettings, "containersrcadmin", "")))
            {
                UpdatePortalSetting(PortalId, "DefaultAdminContainer", XmlUtils.GetNodeValue(nodeSettings, "containersrcadmin", ""));
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
                    locale = new Locale {Code = cultureCode, Fallback = Localization.SystemLocale, Text = CultureInfo.CreateSpecificCulture(cultureCode).NativeName};
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

        private void ParseRoleGroups(XPathNavigator nav, int portalID, int administratorId)
        {
            var administratorRoleId = -1;
            var registeredRoleId = -1;
            var subscriberRoleId = -1;
            var unverifiedRoleId = -1;

            foreach (XPathNavigator roleGroupNav in nav.Select("rolegroup"))
            {
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
            var objportal = GetPortal(portalID);
            UpdatePortalSetup(portalID,
                              administratorId,
                              administratorRoleId,
                              registeredRoleId,
                              objportal.SplashTabId,
                              objportal.HomeTabId,
                              objportal.LoginTabId,
                              objportal.RegisterTabId,
                              objportal.UserTabId,
                              objportal.SearchTabId,
                              objportal.AdminTabId,
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
            var objportal = GetPortal(portalID);
            UpdatePortalSetup(portalID,
                              administratorId,
                              administratorRoleId,
                              registeredRoleId,
                              objportal.SplashTabId,
                              objportal.HomeTabId,
                              objportal.LoginTabId,
                              objportal.RegisterTabId,
                              objportal.UserTabId,
                              objportal.SearchTabId,
                              objportal.AdminTabId,
                              GetActivePortalLanguage(portalID));
        }

        private void ParseTab(XmlNode nodeTab, int PortalId, bool IsAdminTemplate, PortalTemplateModuleAction mergeTabs, ref Hashtable hModules, ref Hashtable hTabs, bool isNewPortal)
        {
            TabInfo tab = null;
            var tabController = new TabController();
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
                        tab = tabController.GetTab(Convert.ToInt32(hTabs[parenttabname + strName]), PortalId, false);
                    }
                }
                if (tab == null || isNewPortal)
                {
                    tab = TabController.DeserializeTab(nodeTab, null, hTabs, PortalId, IsAdminTemplate, mergeTabs, hModules);
                }
                var eventLogController = new EventLogController();

                //when processing the template we should try and identify the Admin tab
                if (tab.TabName == "Admin")
                {
                    portal.AdminTabId = tab.TabID;
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
                                      portal.AdminTabId,
                                      GetActivePortalLanguage(PortalId));
                    eventLogController.AddLog("AdminTab",
                                       tab.TabID.ToString(),
                                       GetCurrentPortalSettings(),
                                       UserController.GetCurrentUserInfo().UserID,
                                       EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                }
                //when processing the template we can find: hometab, usertab, logintab
                switch (XmlUtils.GetNodeValue(nodeTab, "tabtype", ""))
                {
                    case "splashtab":
                        portal.SplashTabId = tab.TabID;
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
                                          portal.AdminTabId,
                                          GetActivePortalLanguage(PortalId));
                        eventLogController.AddLog("SplashTab",
                                           tab.TabID.ToString(),
                                           GetCurrentPortalSettings(),
                                           UserController.GetCurrentUserInfo().UserID,
                                           EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                        break;
                    case "hometab":
                        portal.HomeTabId = tab.TabID;
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
                                          portal.AdminTabId,
                                          GetActivePortalLanguage(PortalId));
                        eventLogController.AddLog("HomeTab",
                                           tab.TabID.ToString(),
                                           GetCurrentPortalSettings(),
                                           UserController.GetCurrentUserInfo().UserID,
                                           EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                        break;
                    case "logintab":
                        portal.LoginTabId = tab.TabID;
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
                                          portal.AdminTabId,
                                          GetActivePortalLanguage(PortalId));
                        eventLogController.AddLog("LoginTab",
                                           tab.TabID.ToString(),
                                           GetCurrentPortalSettings(),
                                           UserController.GetCurrentUserInfo().UserID,
                                           EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                        break;
                    case "usertab":
                        portal.UserTabId = tab.TabID;
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
                                          portal.AdminTabId,
                                          GetActivePortalLanguage(PortalId));
                        eventLogController.AddLog("UserTab",
                                           tab.TabID.ToString(),
                                           GetCurrentPortalSettings(),
                                           UserController.GetCurrentUserInfo().UserID,
                                           EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                        break;
                    case "searchtab":
                        portal.SearchTabId = tab.TabID;
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
                                          portal.AdminTabId,
                                          GetActivePortalLanguage(PortalId));
                        eventLogController.AddLog("SearchTab",
                                           tab.TabID.ToString(),
                                           GetCurrentPortalSettings(),
                                           UserController.GetCurrentUserInfo().UserID,
                                           EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                        break;
                    case "gettingStartedTab":                        
                        UpdatePortalSetting(PortalId, "GettingStartedTabId", tab.TabID.ToString());
                        break;
                    case "404Tab":
                        UpdatePortalSetting(PortalId, "AUM_ErrorPage404", tab.TabID.ToString());
                        break;
                    case "500Tab":
                        UpdatePortalSetting(PortalId, "AUM_ErrorPage500", tab.TabID.ToString());
                        break;
                }
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
                TabController objTabs = new TabController();
                foreach (KeyValuePair<int, TabInfo> tabPair in objTabs.GetTabsByPortal(PortalId))
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
                ParseTab(nodeTab, PortalId, IsAdminTemplate, mergeTabs, ref hModules, ref hTabs, IsNewPortal);
            }

            //Process tabs that are linked to tabs
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab[url/@type = 'Tab']"))
            {
                int tabId = XmlUtils.GetNodeValueInt(nodeTab, "tabid", Null.NullInteger);
                string tabPath = XmlUtils.GetNodeValue(nodeTab, "url", Null.NullString);
                if (tabId > Null.NullInteger)
                {
                    TabController controller = new TabController();
                    TabInfo objTab = controller.GetTab(tabId, PortalId, false);
                    objTab.Url = TabController.GetTabByTabPath(PortalId, tabPath, Null.NullString).ToString();
                    controller.UpdateTab(objTab);
                }
            }
            var folderManager = FolderManager.Instance;
            var fileManager = FileManager.Instance;
            //Process tabs that are linked to files
            foreach (XmlNode nodeTab in nodeTabs.SelectNodes("//tab[url/@type = 'File']"))
            {
                var tabId = XmlUtils.GetNodeValueInt(nodeTab, "tabid", Null.NullInteger);
                var filePath = XmlUtils.GetNodeValue(nodeTab, "url", Null.NullString);
                if (tabId > Null.NullInteger)
                {
                    var controller = new TabController();
                    var objTab = controller.GetTab(tabId, PortalId, false);

                    var fileName = Path.GetFileName(filePath);

                    var folderPath = filePath.Substring(0, filePath.LastIndexOf(fileName));
                    var folder = folderManager.GetFolder(PortalId, folderPath);

                    var file = fileManager.GetFile(folder, fileName);

                    objTab.Url = "FileID=" + file.FileId;
                    controller.UpdateTab(objTab);
                }
            }
        }

        private void UpdatePortalSetup(int PortalId, int AdministratorId, int AdministratorRoleId, int RegisteredRoleId, int SplashTabId, int HomeTabId, int LoginTabId, int RegisterTabId,
                                       int UserTabId, int SearchTabId, int AdminTabId, string CultureCode)
        {
            DataProvider.Instance().UpdatePortalSetup(PortalId,
                                                      AdministratorId,
                                                      AdministratorRoleId,
                                                      RegisteredRoleId,
                                                      SplashTabId,
                                                      HomeTabId,
                                                      LoginTabId,
                                                      RegisterTabId,
                                                      UserTabId,
                                                      SearchTabId,
                                                      AdminTabId,
                                                      CultureCode);
            EventLogController objEventLog = new EventLogController();
            objEventLog.AddLog("PortalId", PortalId.ToString(), GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_UPDATED);
            DataCache.ClearHostCache(true);
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

		private string GetTemplateName(string languageFileName)
		{
			//e.g. "default template.template.en-US.resx"
			return languageFileName.Substring(0, languageFileName.Length - ".xx-XX.resx".Length);
		}

		private string GetCultureCode(string languageFileName)
		{
			//e.g. "default template.template.en-US.resx"
			return languageFileName.Substring(1 + languageFileName.Length - ".xx-XX.resx".Length, "xx-XX".Length);
		}

		private static Dictionary<string, string> GetPortalSettingsDictionary(int portalId, string cultureCode)
		{
			string cacheKey = string.Format(DataCache.PortalSettingsCacheKey, portalId);
			return CBO.GetCachedObject<Dictionary<string, string>>(new CacheItemArgs(cacheKey, DataCache.PortalSettingsCacheTimeOut, DataCache.PortalSettingsCachePriority, portalId, cultureCode),
																   GetPortalSettingsDictionaryCallback,
																   true);
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

        #endregion

		#region Public Methods

		/// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new portal alias
        /// </summary>
        /// <param name="portalId">Id of the portal</param>
        /// <param name="portalAlias">Portal Alias to be created</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        ///     [cnurse]    01/11/2005  created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void AddPortalAlias(int portalId, string portalAlias)
        {
            var portalAliasController = new PortalAliasController();

            //Check if the Alias exists
            PortalAliasInfo portalAliasInfo = portalAliasController.GetPortalAlias(portalAlias, portalId);

            //If alias does not exist add new
            if (portalAliasInfo == null)
            {
                portalAliasInfo = new PortalAliasInfo {PortalID = portalId, HTTPAlias = portalAlias, IsPrimary = true};
                TestablePortalAliasController.Instance.AddPortalAlias(portalAliasInfo);
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
        /// <param name="adminUser">The obj admin user.</param>
        /// <param name="description">The description.</param>
        /// <param name="keyWords">The key words.</param>
        /// <param name="templatePath">The template path.</param>
        /// <param name="templateFile">The template file.</param>
        /// <param name="homeDirectory">The home directory.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <param name="serverPath">The server path.</param>
        /// <param name="childPath">The child path.</param>
        /// <param name="isChildPortal">if set to <c>true</c> means the portal is child portal.</param>
        /// <returns>Portal id.</returns>
        public int CreatePortal(string portalName, UserInfo adminUser, string description, string keyWords, string templatePath, 
                                string templateFile, string homeDirectory, string portalAlias,
                                string serverPath, string childPath, bool isChildPortal)
        {
            var template = TestablePortalController.Instance.GetPortalTemplate(Path.Combine(templatePath, templateFile), null);

            return CreatePortal(portalName, adminUser, description, keyWords, template, homeDirectory, portalAlias,
                                serverPath, childPath, isChildPortal);
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
            string message = Null.NullString;
            int administratorId = adminUserId;
            UserInfo adminUser=new UserInfo();

            //Attempt to create a new portal
            int portalId = CreatePortal(portalName, homeDirectory);

            string templatePath, templateFile;
            PrepareLocalizedPortalTemplate(template, out templatePath, out templateFile);
            string mergedTemplatePath = Path.Combine(templatePath, templateFile);

            if (portalId != -1)
            {
                //add userportal record
                UserController.AddUserPortal(portalId,administratorId);

                if (String.IsNullOrEmpty(homeDirectory))
                {
                    homeDirectory = "Portals/" + portalId;
                }
                string mappedHomeDirectory = String.Format(Globals.ApplicationMapPath + "\\" + homeDirectory + "\\").Replace("/", "\\");
                message += CreateProfileDefinitions(portalId, mergedTemplatePath);
                if (message == Null.NullString)
                {
                    //retrieve existing administrator
                    try
                    {
                            adminUser = UserController.GetUserById(portalId, administratorId);
                    }
                    catch (Exception Exc)
                    {
                        Logger.Error(Exc);
                    }
                }
                else
                {
                    throw new Exception(message);
                }
                if (String.IsNullOrEmpty(message) && administratorId > 0)
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
                            ParseTemplate(portalId, templatePath, templateFile, administratorId, PortalTemplateModuleAction.Replace, true, out newPortalLocales);
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
                        DataCache.ClearPortalCache(portalId,true);

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
                            var entry = new ListEntryInfo();
                            entry.PortalID = portalId;
                            entry.SystemList = false;
                            entry.ListName = listName + "-" + portalId;
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
                            var entry = new ListEntryInfo();
                            entry.PortalID = portalId;
                            entry.SystemList = false;
                            entry.ListName = listName + "-" + portalId;
                            listController.AddListEntry(entry);

                        }
                        catch (Exception Exc)
                        {
                            Logger.Error(Exc);
                        }

                        // Add default workflows
                        try
                        {
                            ContentWorkflowController.Instance.CreateDefaultWorkflows(portalId);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }

                        ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();

                        try
                        {
                            var objEventLogInfo = new LogInfo();
                            objEventLogInfo.BypassBuffering = true;
                            objEventLogInfo.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString();
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Install Portal:", portalName));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("FirstName:", adminUser.FirstName));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("LastName:", adminUser.LastName));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Username:", adminUser.Username));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Email:", adminUser.Email));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Description:", description));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Keywords:", keyWords));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Template:", template.TemplateFilePath));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("TemplateCulture:", template.CultureCode));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("HomeDirectory:", homeDirectory));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("PortalAlias:", portalAlias));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("ServerPath:", serverPath));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("ChildPath:", childPath));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("IsChildPortal:", isChildPortal.ToString()));
                            var eventLog = new EventLogController();
                            eventLog.AddLog(objEventLogInfo);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error(exc);
                        }
                    }
                    else
                    {
                        throw new Exception(message);
                    }
                }
                else
                {
                    DeletePortalInfo(portalId);
                    portalId = -1;
                    throw new Exception(message);
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
            string message = Null.NullString;
            int administratorId = Null.NullInteger;

            //Attempt to create a new portal
            int portalId = CreatePortal(portalName, homeDirectory);

            string templatePath, templateFile;
            PrepareLocalizedPortalTemplate(template, out templatePath, out templateFile);
            string mergedTemplatePath = Path.Combine(templatePath, templateFile);

            if (portalId != -1)
            {
                if (String.IsNullOrEmpty(homeDirectory))
                {
                    homeDirectory = "Portals/" + portalId;
                }
                string mappedHomeDirectory = String.Format(Globals.ApplicationMapPath + "\\" + homeDirectory + "\\").Replace("/", "\\");
                message += CreateProfileDefinitions(portalId, mergedTemplatePath);
                if (message == Null.NullString)
                {
                    //add administrator
                    try
                    {
                        adminUser.PortalID = portalId;
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
                }
                else
                {
                    throw new Exception(message);
                }
                if (String.IsNullOrEmpty(message) && administratorId > 0)
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
                            ParseTemplate(portalId, templatePath, templateFile, administratorId, PortalTemplateModuleAction.Replace, true, out newPortalLocales);
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
                            var entry = new ListEntryInfo();
                            entry.PortalID = portalId;
                            entry.SystemList = false;
                            entry.ListName = listName + "-" + portalId;
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
                            var entry = new ListEntryInfo();
                            entry.PortalID = portalId;
                            entry.SystemList = false;
                            entry.ListName = listName + "-" + portalId;
                            listController.AddListEntry(entry);

                        }
                        catch (Exception Exc)
                        {
                            Logger.Error(Exc);
                        }

                        // Add default workflows
                        try
                        {
                            ContentWorkflowController.Instance.CreateDefaultWorkflows(portalId);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }

                        ServicesRoutingManager.ReRegisterServiceRoutesWhileSiteIsRunning();

                        try
                        {
                            var objEventLogInfo = new LogInfo();
                            objEventLogInfo.BypassBuffering = true;
                            objEventLogInfo.LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString();
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Install Portal:", portalName));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("FirstName:", adminUser.FirstName));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("LastName:", adminUser.LastName));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Username:", adminUser.Username));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Email:", adminUser.Email));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Description:", description));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Keywords:", keyWords));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("Template:", template.TemplateFilePath));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("TemplateCulture:", template.CultureCode));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("HomeDirectory:", homeDirectory));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("PortalAlias:", portalAlias));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("ServerPath:", serverPath));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("ChildPath:", childPath));
                            objEventLogInfo.LogProperties.Add(new LogDetailInfo("IsChildPortal:", isChildPortal.ToString()));
                            var eventLog = new EventLogController();
                            eventLog.AddLog(objEventLogInfo);
                        }
                        catch (Exception exc)
                        {
                            Logger.Error(exc);
                        }
                    }
                    else
                    {
                        throw new Exception(message);
                    }
                }
                else
                {
                    DeletePortalInfo(portalId);
                    portalId = -1;
                    throw new Exception(message);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Creates a new portal.
        /// </summary>
        /// <param name="portalName">Name of the portal to be created</param>
        /// <param name="firstName">Portal Administrator's first name</param>
        /// <param name="lastName">Portal Administrator's last name</param>
        /// <param name="username">Portal Administrator's username</param>
        /// <param name="password">Portal Administrator's password</param>
        /// <param name="email">Portal Administrator's email</param>
        /// <param name="description">Description for the new portal</param>
        /// <param name="keyWords">KeyWords for the new portal</param>
        /// <param name="templatePath">Path where the templates are stored</param>
        /// <param name="templateFile">Template file</param>
        /// <param name="homeDirectory">Home Directory</param>
        /// <param name="portalAlias">Portal Alias String</param>
        /// <param name="serverPath">The Path to the root of the Application</param>
        /// <param name="childPath">The Path to the Child Portal Folder</param>
        /// <param name="isChildPortal">True if this is a child portal</param>
        /// <returns>PortalId of the new portal if there are no errors, -1 otherwise.</returns>
        /// <remarks>
        /// After the selected portal template is parsed the admin template ("admin.template") will be
        /// also processed. The admin template should only contain the "Admin" menu since it's the same
        /// on all portals. The selected portal template can contain a <settings/> node to specify portal
        /// properties and a <roles/> node to define the roles that will be created on the portal by default.
        /// </remarks>
        /// <history>
        /// 	[cnurse]	11/08/2004	created (most of this code was moved from SignUp.ascx.vb)
        /// </history>
        /// -----------------------------------------------------------------------------
        public int CreatePortal(string portalName, string firstName, string lastName, string username, string password, string email, 
                                string description, string keyWords, string templatePath, string templateFile, string homeDirectory, 
                                string portalAlias, string serverPath, string childPath, bool isChildPortal)
        {
            UserInfo adminUser = new UserInfo();
            adminUser.FirstName = firstName;
            adminUser.LastName = lastName;
            adminUser.Username = username;
            adminUser.DisplayName = firstName + " " + lastName;
            adminUser.Membership.Password = password;
            adminUser.Email = email;
            adminUser.IsSuperUser = false;
            adminUser.Membership.Approved = true;
            adminUser.Profile.FirstName = firstName;
            adminUser.Profile.LastName = lastName;
            return CreatePortal(portalName, adminUser, description, keyWords, templatePath, templateFile, homeDirectory, portalAlias, serverPath, childPath, isChildPortal);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a portal permanently
        /// </summary>
        /// <param name="portalId">PortalId of the portal to be deleted</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	03/09/2004	Created
        /// 	[VMasanas]	26/10/2004	Remove dependent data (skins, modules)
        ///     [cnurse]    24/11/2006  Removal of Modules moved to sproc
        /// </history>
        /// -----------------------------------------------------------------------------
        public void DeletePortalInfo(int portalId)
        {
            UserController.DeleteUsers(portalId, false, true);
            DataProvider.Instance().DeletePortalInfo(portalId);
            EventLogController eventLogController = new EventLogController();
            eventLogController.AddLog("PortalId", portalId.ToString(), GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   Gets information of a portal
        /// </summary>
        /// <param name = "portalId">Id of the portal</param>
        /// <returns>PortalInfo object with portal definition</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public PortalInfo GetPortal(int portalId)
        {
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

        public PortalInfo GetPortal(int portalId, string cultureCode)
        {
            PortalInfo portal = GetPortalInternal(portalId, cultureCode);

            if (Localization.ActiveLanguagesByPortalID(portalId) > 1)
            {
                if (portal == null)
                {
                    //Get Fallback language
                    string fallbackLanguage = string.Empty;

                    if (string.IsNullOrEmpty(cultureCode)) cultureCode = Localization.SystemLocale;

                    Locale userLocale = LocaleController.Instance.GetLocale(cultureCode);
                    if (userLocale != null && !string.IsNullOrEmpty(userLocale.Fallback))
                    {
                        fallbackLanguage = userLocale.Fallback;
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets information from all portals
        /// </summary>
        /// <returns>ArrayList of PortalInfo objects</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// </history>
        /// -----------------------------------------------------------------------------
        public ArrayList GetPortals()
        {
            string cultureCode = Localization.SystemLocale;
            string cacheKey = String.Format(DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
            var portals = CBO.GetCachedObject<List<PortalInfo>>(new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, cultureCode), 
                                                    GetPortalsCallBack);
            return new ArrayList(portals);
        }

        public List<PortalInfo> GetPortalList(string cultureCode)
        {
            string cacheKey = String.Format(DataCache.PortalCacheKey, Null.NullInteger, cultureCode);
            return CBO.GetCachedObject<List<PortalInfo>>(new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, cultureCode),
                                                    GetPortalsCallBack);
        }

	    /// <summary>
		/// Gets the portal.
		/// </summary>
		/// <param name="uniqueId">The unique id.</param>
		/// <returns>Portal info.</returns>
        public PortalInfo GetPortal(Guid uniqueId)
        {
            ArrayList portals = GetPortals();
            PortalInfo targetPortal = null;

            foreach (PortalInfo currentPortal in portals)
            {
                if (currentPortal.GUID == uniqueId)
                {
                    targetPortal = currentPortal;
                    break;
                }
            }
            return targetPortal;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the space used at the host level
        /// </summary>
        /// <returns>Space used in bytes</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	19/04/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public long GetPortalSpaceUsedBytes()
        {
            return GetPortalSpaceUsedBytes(-1);
        }

		/// <summary>
		/// Gets the portal space used bytes.
		/// </summary>
		/// <param name="portalId">The portal id.</param>
		/// <returns>Space used in bytes</returns>
        public long GetPortalSpaceUsedBytes(int portalId)
        {
            long size = 0;
            IDataReader dr = null;
            dr = DataProvider.Instance().GetPortalSpaceUsed(portalId);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Verifies if there's enough space to upload a new file on the given portal
        /// </summary>
		/// <param name="portalId">Id of the portal</param>
        /// <param name="fileSizeBytes">Size of the file being uploaded</param>
        /// <returns>True if there's enough space available to upload the file</returns>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	19/04/2006	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public bool HasSpaceAvailable(int portalId, long fileSizeBytes)
        {
            int hostSpace;
            if (portalId == -1)
            {
                hostSpace = 0;
            }
            else
            {
                PortalSettings ps = GetCurrentPortalSettings();
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
            TabController tabCont = new TabController();

            DataCache.ClearHostCache(true);
            DataProvider.Instance().EnsureLocalizationExists(portalId, cultureCode);

            PortalInfo defaultPortal = GetPortal(portalId, GetPortalDefaultLanguage(portalId));
            PortalInfo targetPortal = GetPortal(portalId, cultureCode);

            Locale targetLocale = LocaleController.Instance.GetLocale(cultureCode);
            TabInfo tempTab;
            if ((defaultPortal.HomeTabId != Null.NullInteger))
            {
                tempTab = tabCont.GetTabByCulture(defaultPortal.HomeTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.HomeTabId = tempTab.TabID;
                }
            }
            if ((defaultPortal.LoginTabId != Null.NullInteger))
            {
                tempTab = tabCont.GetTabByCulture(defaultPortal.LoginTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.LoginTabId = tempTab.TabID;
                }
            }
            if ((defaultPortal.RegisterTabId != Null.NullInteger))
            {
                tempTab = tabCont.GetTabByCulture(defaultPortal.RegisterTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.RegisterTabId = tempTab.TabID;
                }
            }
            if ((defaultPortal.SplashTabId != Null.NullInteger))
            {
                tempTab = tabCont.GetTabByCulture(defaultPortal.SplashTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SplashTabId = tempTab.TabID;
                }
            }
            if ((defaultPortal.UserTabId != Null.NullInteger))
            {
                tempTab = tabCont.GetTabByCulture(defaultPortal.UserTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.UserTabId = tempTab.TabID;
                }
            }
            if ((defaultPortal.SearchTabId != Null.NullInteger))
            {
                tempTab = tabCont.GetTabByCulture(defaultPortal.SearchTabId, portalId, targetLocale);
                if (tempTab != null)
                {
                    targetPortal.SearchTabId = tempTab.TabID;
                }
            }

            UpdatePortalInfo(targetPortal, false);
        }
        /// <summary>
        /// Removes the related PortalLocalization record from the database
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        public void RemovePortalLocalization(int portalId, string cultureCode)
        {
            RemovePortalLocalization(portalId, cultureCode, true);
        }
        /// <summary>
        /// Removes the related PortalLocalization record from the database, adds optional clear cache
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        public void RemovePortalLocalization(int portalId, string cultureCode, bool clearCache)
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
            
            ParseTemplate(portalId, templatePath, templateFile, administratorId, mergeTabs, isNewPortal);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Processess a template file for the new portal. This method will be called twice: for the portal template and for the admin template
        /// </summary>
        /// <param name="PortalId">PortalId of the new portal</param>
        /// <param name="TemplatePath">Path for the folder where templates are stored</param>
        /// <param name="TemplateFile">Template file to process</param>
        /// <param name="AdministratorId">UserId for the portal administrator. This is used to assign roles to this user</param>
        /// <param name="mergeTabs">Flag to determine whether Module content is merged.</param>
        /// <param name="IsNewPortal">Flag to determine is the template is applied to an existing portal or a new one.</param>
        /// <remarks>
        /// The roles and settings nodes will only be processed on the portal template file.
        /// </remarks>
        /// <history>
        /// 	[VMasanas]	27/08/2004	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void ParseTemplate(int PortalId, string TemplatePath, string TemplateFile, int AdministratorId, PortalTemplateModuleAction mergeTabs, bool IsNewPortal)
        {
            LocaleCollection localeCollection;
            ParseTemplate(PortalId, TemplatePath, TemplateFile, AdministratorId, mergeTabs, IsNewPortal, out localeCollection);
        }

        public void ParseTemplate(int PortalId, string TemplatePath, string TemplateFile, int AdministratorId, PortalTemplateModuleAction mergeTabs, bool IsNewPortal, out LocaleCollection localeCollection)
        {
            XmlDocument xmlPortal = new XmlDocument();
            IFolderInfo objFolder;
            XmlNode node;
            try
            {
                xmlPortal.Load(Path.Combine(TemplatePath, TemplateFile));
            }
            catch(Exception ex)
            {
				Logger.Error(ex);
            }
            node = xmlPortal.SelectSingleNode("//portal/settings");
            if (node != null && IsNewPortal)
            {
                ParsePortalSettings(node, PortalId);
            }
            node = xmlPortal.SelectSingleNode("//locales");
            if (node != null && IsNewPortal)
            {
               localeCollection = ParseEnabledLocales(node, PortalId);
            }
            else
            {
                var portalInfo = new PortalController().GetPortal(PortalId);
                var defaultLocale = LocaleController.Instance.GetLocale(portalInfo.DefaultLanguage);
                if (defaultLocale == null)
                {
                    defaultLocale = new Locale { Code = portalInfo.DefaultLanguage, Fallback = Localization.SystemLocale, Text = CultureInfo.CreateSpecificCulture(portalInfo.DefaultLanguage).NativeName };
                    Localization.SaveLanguage(defaultLocale, false);
                }
                localeCollection = new LocaleCollection { { defaultLocale.Code, defaultLocale } };
            }
            node = xmlPortal.SelectSingleNode("//portal/rolegroups");
            if (node != null)
            {
                ParseRoleGroups(node.CreateNavigator(), PortalId, AdministratorId);
            }
            node = xmlPortal.SelectSingleNode("//portal/roles");
            if (node != null)
            {
                ParseRoles(node.CreateNavigator(), PortalId, AdministratorId);
            }
            node = xmlPortal.SelectSingleNode("//portal/portalDesktopModules");
            if (node != null)
            {
                ParsePortalDesktopModules(node.CreateNavigator(), PortalId);
            }
            node = xmlPortal.SelectSingleNode("//portal/folders");
            if (node != null)
            {
                ParseFolders(node, PortalId);
            }

            var defaultFolderMapping = FolderMappingController.Instance.GetDefaultFolderMapping(PortalId);

            if (FolderManager.Instance.GetFolder(PortalId, "") == null)
            {
                objFolder = FolderManager.Instance.AddFolder(defaultFolderMapping, "");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                AddFolderPermissions(PortalId, objFolder.FolderID);
            }

            if (FolderManager.Instance.GetFolder(PortalId, "Templates/") == null)
            {
                objFolder = FolderManager.Instance.AddFolder(defaultFolderMapping, "Templates/");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                //AddFolderPermissions(PortalId, objFolder.FolderID);
            }

            // force creation of users folder if not present on template
            if (FolderManager.Instance.GetFolder(PortalId, "Users/") == null)
            {
                objFolder = FolderManager.Instance.AddFolder(defaultFolderMapping, "Users/");
                objFolder.IsProtected = true;
                FolderManager.Instance.UpdateFolder(objFolder);

                //AddFolderPermissions(PortalId, objFolder.FolderID);
            }
            
            if (mergeTabs == PortalTemplateModuleAction.Replace)
            {
                TabController objTabs = new TabController();
                TabInfo objTab;
                foreach (KeyValuePair<int, TabInfo> tabPair in objTabs.GetTabsByPortal(PortalId))
                {
                    objTab = tabPair.Value;
                    objTab.TabName = objTab.TabName + "_old";
                    objTab.TabPath = Globals.GenerateTabPath(objTab.ParentId, objTab.TabName);
                    objTab.IsDeleted = true;
                    objTabs.UpdateTab(objTab);
                    ModuleController objModules = new ModuleController();
                    ModuleInfo objModule;
                    foreach (KeyValuePair<int, ModuleInfo> modulePair in objModules.GetTabModules(objTab.TabID))
                    {
                        objModule = modulePair.Value;
                        objModules.DeleteTabModule(objModule.TabID, objModule.ModuleID, false);
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
                        string path = Path.Combine(TemplatePath, "admin.template");
                        if(!File.Exists(path))
                        {
                            //if the template is a merged copy of a localized templte the
                            //admin.template may be one director up
                            path = Path.Combine(TemplatePath, "..\admin.template");
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
                ParseTabs(node, PortalId, false, mergeTabs, IsNewPortal);
            }
        }

        /// <summary>
        /// Processes the resource file for the template file selected
        /// </summary>
        /// <param name="portalPath">New portal's folder</param>
        /// <param name="TemplateFile">Selected template file</param>
        /// <remarks>
        /// The resource file is a zip file with the same name as the selected template file and with
        /// an extension of .resources (to disable this file being downloaded).
        /// For example: for template file "portal.template" a resource file "portal.template.resources" can be defined.
        /// </remarks>
        [Obsolete("Deprecated in DNN 6.2.0 use ProcessResourceFileExplicit instead")]
        public void ProcessResourceFile(string portalPath, string TemplateFile)
        {
            ProcessResourceFileExplicit(portalPath, TemplateFile + ".resources");
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
                var zipStream = new ZipInputStream(new FileStream(resoureceFile, FileMode.Open, FileAccess.Read));
                FileSystemUtils.UnzipResources(zipStream, portalPath);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

		/// <summary>
		/// Updates the portal expiry.
		/// </summary>
		/// <param name="PortalId">The portal id.</param>
        public void UpdatePortalExpiry(int PortalId)
        {
            UpdatePortalExpiry(PortalId, GetActivePortalLanguage(PortalId));
        }

		/// <summary>
		/// Updates the portal expiry.
		/// </summary>
		/// <param name="PortalId">The portal id.</param>
		/// <param name="CultureCode">The culture code.</param>
        public void UpdatePortalExpiry(int PortalId, string CultureCode)
        {
		    var portal = GetPortal(PortalId, CultureCode);

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
            UpdatePortalInfo(portal, true);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates basic portal information
        /// </summary>
        /// <param name="portal"></param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[cnurse]	10/13/2004	created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void UpdatePortalInfo(PortalInfo portal, bool clearCache)
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
                                            portal.DefaultLanguage,
                                            portal.HomeDirectory,
                                            UserController.GetCurrentUserInfo().UserID,
                                            portal.CultureCode);
            
            var eventLogController = new EventLogController();
            eventLogController.AddLog("PortalId", portal.PortalID.ToString(), GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTALINFO_UPDATED);
            
            //ensure a localization item exists (in case a new default language has been set)
            DataProvider.Instance().EnsureLocalizationExists(portal.PortalID, portal.DefaultLanguage);
            
            //clear portal cache
            if (clearCache)
                DataCache.ClearHostCache(true);
        }

        public class PortalTemplateInfo
        {
            private string _resourceFilePath;

            public PortalTemplateInfo(string templateFilePath, string cultureCode)
            {
                TemplateFilePath = templateFilePath;

                InitLocalizationFields(cultureCode);
                InitNameAndDescription();
            }

            private void InitNameAndDescription()
            {
                if(!String.IsNullOrEmpty(LanguageFilePath))
                {
                    LoadNameAndDescriptionFromLanguageFile();
                }

                if(String.IsNullOrEmpty(Name))
                {
                    Name = Path.GetFileNameWithoutExtension(TemplateFilePath);
                }

                if(String.IsNullOrEmpty(Description))
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
                if(!String.IsNullOrEmpty(LanguageFilePath))
                {
                    CultureCode = cultureCode;
                }
                else
                {
                    CultureCode = "";
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
                    if(_resourceFilePath == null)
                    {
                        _resourceFilePath = PortalTemplateIO.Instance.GetResourceFilePath(TemplateFilePath);
                    }

                    return _resourceFilePath;
                }
            }
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

                if(langs.Any())
                {
                    langs.ForEach(x => list.Add(new PortalTemplateInfo(templateFilePath, x)));
                }
                else
                {
                    list.Add(new PortalTemplateInfo(templateFilePath, ""));
                }
            }

            return list;
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

            if(!string.IsNullOrEmpty(cultureCode) && template.CultureCode != cultureCode)
            {
                return null;
            }

            return template;
        }

		#endregion

		#region Interface Implementation

		/// <summary>
		/// Gets the current portal settings.
		/// </summary>
		/// <returns>portal settings.</returns>
		PortalSettings IPortalController.GetCurrentPortalSettings()
		{
			return GetCurrentPortalSettings();
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
            var portalController = new PortalController();
            string message = string.Empty;

            //check if this is the last portal
            int portalCount = portalController.GetPortals().Count;
            if (portalCount > 1)
            {
                if (portal != null)
                {
                    //delete custom resource files
                    Globals.DeleteFilesRecursive(serverPath, ".Portal-" + portal.PortalID + ".resx");

                    //If child portal delete child folder
                    var arr = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
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
                    portalController.DeletePortalInfo(portal.PortalID);
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

        /// <summary>
        /// Gets the current portal settings.
        /// </summary>
        /// <returns>portal settings.</returns>
        public static PortalSettings GetCurrentPortalSettings()
        {
            PortalSettings objPortalSettings = null;
            if (HttpContext.Current != null)
            {
                objPortalSettings = (PortalSettings)HttpContext.Current.Items["PortalSettings"];
            }
            return objPortalSettings;
        }

        public static int GetEffectivePortalId(int portalId)
        {
            if (portalId > Null.NullInteger && Globals.Status != Globals.UpgradeStatus.Upgrade)
            {
                //var portalController = new PortalController();
                var portalController = TestablePortalController.Instance;
                var portal = portalController.GetPortal(portalId);
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
            var arr = TestablePortalAliasController.Instance.GetPortalAliasesByPortalId(portal.PortalID).ToList();
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
            //var portalController = new PortalController();
            var portalController = TestablePortalController.Instance;
            var portal = portalController.GetPortal(portalId);

			return portal != null && portal.PortalGroupID > Null.NullInteger;
        }

        /// <summary>
        /// Deletes the portal setting.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        public static void DeletePortalSetting(int portalID, string settingName)
        {
            DeletePortalSetting(portalID, settingName, GetActivePortalLanguage(portalID));
        }

        /// <summary>
        /// Deletes the portal setting.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="CultureCode">The culture code.</param>
        public static void DeletePortalSetting(int portalID, string settingName, string CultureCode)
        {
            DataProvider.Instance().DeletePortalSetting(portalID, settingName, CultureCode.ToLower());
            EventLogController objEventLog = new EventLogController();
            objEventLog.AddLog("SettingName", settingName, GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>
        /// Deletes all portal settings by portal id.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        public static void DeletePortalSettings(int portalID)
        {
            DataProvider.Instance().DeletePortalSettings(portalID);
            EventLogController objEventLog = new EventLogController();
            objEventLog.AddLog("PortalID", portalID.ToString(), GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        /// <summary>
        /// Gets the portal settings dictionary.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <returns>portal settings.</returns>
        public static Dictionary<string, string> GetPortalSettingsDictionary(int portalID)
        {
	        return GetPortalSettingsDictionary(portalID, string.Empty);
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
                GetPortalSettingsDictionary(portalID).TryGetValue(settingName, out setting);
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
                GetPortalSettingsDictionary(portalID).TryGetValue(key, out setting);
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
                GetPortalSettingsDictionary(portalID).TryGetValue(key, out setting);
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
        /// Updates the portal setting.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue)
        {
            UpdatePortalSetting(portalID, settingName, settingValue, true);
        }

        /// <summary>
        /// Updates the portal setting.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache)
        {
            string culture = Thread.CurrentThread.CurrentCulture.ToString().ToLower();
            if ((string.IsNullOrEmpty(culture)))
            {
                culture = GetPortalSetting("DefaultLanguage", portalID, "".ToLower());
            }
            if ((string.IsNullOrEmpty(culture)))
            {
                culture = Localization.SystemLocale.ToLower();
            }
            UpdatePortalSetting(portalID, settingName, settingValue, clearCache, culture);
        }

        /// <summary>
        /// Updates the portal setting.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="settingName">Name of the setting.</param>
        /// <param name="settingValue">The setting value.</param>
        /// <param name="clearCache">if set to <c>true</c> [clear cache].</param>
        /// <param name="culturecode">The culturecode.</param>
        public static void UpdatePortalSetting(int portalID, string settingName, string settingValue, bool clearCache, string culturecode)
        {
            string currentSetting = GetPortalSetting(settingName, portalID, String.Empty);

            if (currentSetting != settingValue)
            {
                DataProvider.Instance().UpdatePortalSetting(portalID, settingName, settingValue, UserController.GetCurrentUserInfo().UserID, culturecode);
                var objEventLog = new EventLogController();
                objEventLog.AddLog(settingName, settingValue, GetCurrentPortalSettings(), UserController.GetCurrentUserInfo().UserID, EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
                if (clearCache)
                {
                    DataCache.ClearPortalCache(portalID, false);
                    DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);
                }
            }
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
                    PortalSettings _PortalSettings = GetCurrentPortalSettings();
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
            //Return DataProvider.Instance().GetPortalDefaultLanguage(portalID)
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
            foreach (PortalInfo portal in new PortalController().GetPortals())
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

        #region Obsolete Methods

        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by GetPortalSpaceUsedBytes")]
        public int GetPortalSpaceUsed(int portalId)
        {
            int size = 0;
            try
            {
                size = Convert.ToInt32(GetPortalSpaceUsedBytes(portalId));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                size = int.MaxValue;
            }

            return size;
        }

        [Obsolete("Deprecated in DotNetNuke 5.0. This function has been replaced by TabController.DeserializePanes")]
        public void ParsePanes(XmlNode nodePanes, int portalId, int TabId, PortalTemplateModuleAction mergeTabs, Hashtable hModules)
        {
            TabController.DeserializePanes(nodePanes, portalId, TabId, mergeTabs, hModules);
        }

        [Obsolete("Deprecated in DotNetNuke 6.1. Replaced by UpdatePortalInfo(PortalInfo)")]
        public void UpdatePortalInfo(int portalId, string portalName, string logoFile, string footerText, DateTime expiryDate, int userRegistration, int bannerAdvertising, string currency,
                            int administratorId, double hostFee, double hostSpace, int pageQuota, int userQuota, string paymentProcessor, string processorUserId, string processorPassword,
                            string description, string KeyWords, string backgroundFile, int siteLogHistory, int splashTabId, int homeTabId, int loginTabId, int registerTabId, int userTabId,
                            int searchTabId, string defaultLanguage, string homeDirectory)
        {
            var portal = new PortalInfo()
            {
                PortalID = portalId,
                PortalName = portalName,
                LogoFile = logoFile,
                FooterText = footerText,
                ExpiryDate = expiryDate,
                UserRegistration = userRegistration,
                BannerAdvertising = bannerAdvertising,
                Currency = currency,
                AdministratorId = administratorId,
                HostFee = (float)hostFee,
                HostSpace = (int)hostSpace,
                PageQuota = pageQuota,
                UserQuota = userQuota,
                PaymentProcessor = paymentProcessor,
                ProcessorUserId = processorUserId,
                ProcessorPassword = processorPassword,
                Description = description,
                KeyWords = KeyWords,
                BackgroundFile = backgroundFile,
                SiteLogHistory = siteLogHistory,
                SplashTabId = splashTabId,
                HomeTabId = homeTabId,
                LoginTabId = loginTabId,
                RegisterTabId = registerTabId,
                UserTabId = userTabId,
                SearchTabId = searchTabId,
                DefaultLanguage = defaultLanguage,
                HomeDirectory = homeDirectory,
                CultureCode = PortalController.GetActivePortalLanguage(portalId)
            };

            UpdatePortalInfo(portal);
        }


        [Obsolete("Deprecated in DotNetNuke 6.1. Replaced by UpdatePortalInfo(PortalInfo)")]
        public void UpdatePortalInfo(int portalId, string portalName, string logoFile, string footerText, DateTime expiryDate, int userRegistration, int bannerAdvertising, string currency,
                            int administratorId, double hostFee, double hostSpace, int pageQuota, int userQuota, string paymentProcessor, string processorUserId, string processorPassword,
                            string description, string keyWords, string backgroundFile, int siteLogHistory, int splashTabId, int homeTabId, int loginTabId, int registerTabId, int userTabId,
                            int searchTabId, string defaultLanguage, string homeDirectory, string cultureCode)
{
            var portal = new PortalInfo()
            {
                PortalID = portalId,
                PortalName = portalName,
                LogoFile = logoFile,
                FooterText = footerText,
                ExpiryDate = expiryDate,
                UserRegistration = userRegistration,
                BannerAdvertising = bannerAdvertising,
                Currency = currency,
                AdministratorId = administratorId,
                HostFee = (float)hostFee,
                HostSpace = (int)hostSpace,
                PageQuota = pageQuota,
                UserQuota = userQuota,
                PaymentProcessor = paymentProcessor,
                ProcessorUserId = processorUserId,
                ProcessorPassword = processorPassword,
                Description = description,
                KeyWords = keyWords,
                BackgroundFile = backgroundFile,
                SiteLogHistory = siteLogHistory,
                SplashTabId = splashTabId,
                HomeTabId = homeTabId,
                LoginTabId = loginTabId,
                RegisterTabId = registerTabId,
                UserTabId = userTabId,
                SearchTabId = searchTabId,
                DefaultLanguage = defaultLanguage,
                HomeDirectory = homeDirectory,
                CultureCode = cultureCode
            };
            UpdatePortalInfo(portal);
        }


        #endregion
	}
}