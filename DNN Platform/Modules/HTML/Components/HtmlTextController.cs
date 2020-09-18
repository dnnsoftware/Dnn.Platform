// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Html
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;
    using System.Xml;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Taxonomy;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.Html.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Services.Tokens;
    using Microsoft.Extensions.DependencyInjection;

    /// -----------------------------------------------------------------------------
    /// Namespace:  DotNetNuke.Modules.Html
    /// Project:    DotNetNuke
    /// Class:      HtmlTextController
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The HtmlTextController is the Controller class for managing HtmlText information the HtmlText module.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class HtmlTextController : ModuleSearchBase, IPortable, IUpgradeable
    {
        public const int MAX_DESCRIPTION_LENGTH = 100;
        private const string PortalRootToken = "{{PortalRoot}}";

        public HtmlTextController()
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected INavigationManager NavigationManager { get; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   FormatHtmlText formats HtmlText content for display in the browser.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="moduleId">The ModuleID.</param>
        /// <param name = "content">The HtmlText Content.</param>
        /// <param name = "settings">Module Settings.</param>
        /// <param name="portalSettings">The Portal Settings.</param>
        /// <param name="page">The Page Instance.</param>
        /// <returns></returns>
        public static string FormatHtmlText(int moduleId, string content, HtmlModuleSettings settings, PortalSettings portalSettings, Page page)
        {
            // token replace
            if (settings.ReplaceTokens)
            {
                var tr = new HtmlTokenReplace(page)
                {
                    AccessingUser = UserController.Instance.GetCurrentUserInfo(),
                    DebugMessages = portalSettings.UserMode != PortalSettings.Mode.View,
                    ModuleId = moduleId,
                    PortalSettings = portalSettings,
                };
                content = tr.ReplaceEnvironmentTokens(content);
            }

            // Html decode content
            content = HttpUtility.HtmlDecode(content);

            // manage relative paths
            content = ManageRelativePaths(content, portalSettings.HomeDirectory, "src", portalSettings.PortalId);
            content = ManageRelativePaths(content, portalSettings.HomeDirectory, "background", portalSettings.PortalId);

            return content;
        }

        public static string ManageRelativePaths(string strHTML, string strUploadDirectory, string strToken, int intPortalID)
        {
            int P = 0;
            int R = 0;
            int S = 0;
            int tLen = 0;
            string strURL = null;
            var sbBuff = new StringBuilder(string.Empty);

            if (!string.IsNullOrEmpty(strHTML))
            {
                tLen = strToken.Length + 2;
                string uploadDirectory = strUploadDirectory.ToLowerInvariant();

                // find position of first occurrance:
                P = strHTML.IndexOf(strToken + "=\"", StringComparison.InvariantCultureIgnoreCase);
                while (P != -1)
                {
                    sbBuff.Append(strHTML.Substring(S, P - S + tLen));

                    // keep charactes left of URL
                    S = P + tLen;

                    // save startpos of URL
                    R = strHTML.IndexOf("\"", S);

                    // end of URL
                    if (R >= 0)
                    {
                        strURL = strHTML.Substring(S, R - S).ToLowerInvariant();
                    }
                    else
                    {
                        strURL = strHTML.Substring(S).ToLowerInvariant();
                    }

                    if (strHTML.Substring(P + tLen, 10).Equals("data:image", StringComparison.InvariantCultureIgnoreCase))
                    {
                        P = strHTML.IndexOf(strToken + "=\"", S + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
                        continue;
                    }

                    // if we are linking internally
                    if (!strURL.Contains("://"))
                    {
                        // remove the leading portion of the path if the URL contains the upload directory structure
                        string strDirectory = uploadDirectory;
                        if (!strDirectory.EndsWith("/"))
                        {
                            strDirectory += "/";
                        }

                        if (strURL.IndexOf(strDirectory) != -1)
                        {
                            S = S + strURL.IndexOf(strDirectory) + strDirectory.Length;
                            strURL = strURL.Substring(strURL.IndexOf(strDirectory) + strDirectory.Length);
                        }

                        // add upload directory
                        if (!strURL.StartsWith("/")
                            && !string.IsNullOrEmpty(strURL.Trim())) // We don't write the UploadDirectory if the token/attribute has not value. Therefore we will avoid an unnecessary request
                        {
                            sbBuff.Append(uploadDirectory);
                        }
                    }

                    // find position of next occurrance
                    P = strHTML.IndexOf(strToken + "=\"", S + strURL.Length + 2, StringComparison.InvariantCultureIgnoreCase);
                }

                if (S > -1)
                {
                    sbBuff.Append(strHTML.Substring(S));
                }

                // append characters of last URL and behind
            }

            return sbBuff.ToString();
        }

        public string ReplaceWithRootToken(Match m)
        {
            var domain = m.Groups["domain"].Value;

            // Relative url
            if (string.IsNullOrEmpty(domain))
            {
                return PortalRootToken;
            }

            var aliases = PortalAliasController.Instance.GetPortalAliases();
            if (!aliases.Contains(domain))
            {
                // this is no not a portal url so even if it contains /portals/..
                // we do not need to replace it with a token
                return m.ToString();
            }

            // full qualified portal url that needs to be tokenized
            var result = domain + PortalRootToken;
            var protocol = m.Groups["protocol"].Value;
            return string.IsNullOrEmpty(protocol) ? result : protocol + result;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   DeleteHtmlText deletes an HtmlTextInfo object for the Module and Item.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module.</param>
        /// <param name = "ItemID">The ID of the Item.</param>
        public void DeleteHtmlText(int ModuleID, int ItemID)
        {
            DataProvider.Instance().DeleteHtmlText(ModuleID, ItemID);

            // refresh output cache
            ModuleController.SynchronizeModule(ModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetAllHtmlText gets a collection of HtmlTextInfo objects for the Module and Workflow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module.</param>
        /// <returns></returns>
        public List<HtmlTextInfo> GetAllHtmlText(int ModuleID)
        {
            return CBO.FillCollection<HtmlTextInfo>(DataProvider.Instance().GetAllHtmlText(ModuleID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetHtmlText gets the HtmlTextInfo object for the Module, Item, and Workflow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module.</param>
        /// <param name = "ItemID">The ID of the Item.</param>
        /// <returns></returns>
        public HtmlTextInfo GetHtmlText(int ModuleID, int ItemID)
        {
            return CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetHtmlText(ModuleID, ItemID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetTopHtmlText gets the most recent HtmlTextInfo object for the Module, Workflow, and State.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "moduleId">The ID of the Module.</param>
        /// <param name = "isPublished">Whether the content has been published or not.</param>
        /// <param name="workflowId">The Workflow ID.</param>
        /// <returns></returns>
        public HtmlTextInfo GetTopHtmlText(int moduleId, bool isPublished, int workflowId)
        {
            var htmlText = CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetTopHtmlText(moduleId, isPublished));
            if (htmlText != null)
            {
                // check if workflow has changed
                if (isPublished == false && htmlText.WorkflowID != workflowId)
                {
                    // get proper state for workflow
                    htmlText.WorkflowID = workflowId;
                    htmlText.WorkflowName = "[REPAIR_WORKFLOW]";

                    var workflowStateController = new WorkflowStateController();
                    htmlText.StateID = htmlText.IsPublished
                                        ? workflowStateController.GetLastWorkflowStateID(workflowId)
                                        : workflowStateController.GetFirstWorkflowStateID(workflowId);

                    // update object
                    this.UpdateHtmlText(htmlText, this.GetMaximumVersionHistory(htmlText.PortalID));

                    // get object again
                    htmlText = CBO.FillObject<HtmlTextInfo>(DataProvider.Instance().GetTopHtmlText(moduleId, false));
                }
            }

            return htmlText;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetWorkFlow retrieves the currently active Workflow for the Portal.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleId">The ID of the Module.</param>
        /// <param name="TabId">The Tab ID.</param>
        /// <param name = "PortalId">The ID of the Portal.</param>
        /// <returns></returns>
        public KeyValuePair<string, int> GetWorkflow(int ModuleId, int TabId, int PortalId)
        {
            int workFlowId = Null.NullInteger;
            string workFlowType = Null.NullString;

            // get module settings
            HtmlModuleSettings settings;
            if (ModuleId > -1)
            {
                var module = ModuleController.Instance.GetModule(ModuleId, TabId, false);
                var repo = new HtmlModuleSettingsRepository();
                settings = repo.GetSettings(module);
            }
            else
            {
                settings = new HtmlModuleSettings();
            }

            if (settings.WorkFlowID != Null.NullInteger)
            {
                workFlowId = settings.WorkFlowID;
                workFlowType = "Module";
            }

            if (workFlowId == Null.NullInteger)
            {
                // if undefined at module level, get from tab settings
                var tabSettings = TabController.Instance.GetTabSettings(TabId);
                if (tabSettings["WorkflowID"] != null)
                {
                    workFlowId = Convert.ToInt32(tabSettings["WorkflowID"]);
                    workFlowType = "Page";
                }
            }

            if (workFlowId == Null.NullInteger)
            {
                // if undefined at tab level, get from portal settings
                workFlowId = int.Parse(PortalController.GetPortalSetting("WorkflowID", PortalId, "-1"));
                workFlowType = "Site";
            }

            // if undefined at portal level, set portal default
            if (workFlowId == Null.NullInteger)
            {
                var objWorkflow = new WorkflowStateController();
                ArrayList arrWorkflows = objWorkflow.GetWorkflows(PortalId);
                foreach (WorkflowStateInfo objState in arrWorkflows)
                {
                    // use direct publish as default
                    if (Null.IsNull(objState.PortalID) && objState.WorkflowName == "Direct Publish")
                    {
                        workFlowId = objState.WorkflowID;
                        workFlowType = "Module";
                    }
                }
            }

            return new KeyValuePair<string, int>(workFlowType, workFlowId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   UpdateHtmlText creates a new HtmlTextInfo object or updates an existing HtmlTextInfo object.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "htmlContent">An HtmlTextInfo object.</param>
        /// <param name = "MaximumVersionHistory">The maximum number of versions to retain.</param>
        public void UpdateHtmlText(HtmlTextInfo htmlContent, int MaximumVersionHistory)
        {
            var _workflowStateController = new WorkflowStateController();
            bool blnCreateNewVersion = false;

            // determine if we are creating a new version of content or updating an existing version
            if (htmlContent.ItemID != -1)
            {
                if (htmlContent.WorkflowName != "[REPAIR_WORKFLOW]")
                {
                    HtmlTextInfo objContent = this.GetTopHtmlText(htmlContent.ModuleID, false, htmlContent.WorkflowID);
                    if (objContent != null)
                    {
                        if (objContent.StateID == _workflowStateController.GetLastWorkflowStateID(htmlContent.WorkflowID))
                        {
                            blnCreateNewVersion = true;
                        }
                    }
                }
            }
            else
            {
                blnCreateNewVersion = true;
            }

            // determine if content is published
            if (htmlContent.StateID == _workflowStateController.GetLastWorkflowStateID(htmlContent.WorkflowID))
            {
                htmlContent.IsPublished = true;
            }
            else
            {
                htmlContent.IsPublished = false;
            }

            if (blnCreateNewVersion)
            {
                // add content
                htmlContent.ItemID = DataProvider.Instance().AddHtmlText(
                    htmlContent.ModuleID,
                    htmlContent.Content,
                    htmlContent.Summary,
                    htmlContent.StateID,
                    htmlContent.IsPublished,
                    UserController.Instance.GetCurrentUserInfo().UserID,
                    MaximumVersionHistory);
            }
            else
            {
                // update content
                DataProvider.Instance().UpdateHtmlText(htmlContent.ItemID, htmlContent.Content, htmlContent.Summary, htmlContent.StateID, htmlContent.IsPublished, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // add log history
            var logInfo = new HtmlTextLogInfo();
            logInfo.ItemID = htmlContent.ItemID;
            logInfo.StateID = htmlContent.StateID;
            logInfo.Approved = htmlContent.Approved;
            logInfo.Comment = htmlContent.Comment;
            var objLogs = new HtmlTextLogController();
            objLogs.AddHtmlTextLog(logInfo);

            // create user notifications
            this.CreateUserNotifications(htmlContent);

            // refresh output cache
            ModuleController.SynchronizeModule(htmlContent.ModuleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   UpdateWorkFlow updates the currently active Workflow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="WorkFlowType">The type of workflow (Module | Page | Site).</param>
        /// <param name = "WorkflowID">The ID of the Workflow.</param>
        /// <param name="ObjectID">The ID of the object to apply the update to (depends on WorkFlowType).</param>
        /// <param name="ReplaceExistingSettings">Should existing settings be overwritten?.</param>
        public void UpdateWorkflow(int ObjectID, string WorkFlowType, int WorkflowID, bool ReplaceExistingSettings)
        {
            switch (WorkFlowType)
            {
                case "Module":
                    ModuleController.Instance.UpdateModuleSetting(ObjectID, "WorkflowID", WorkflowID.ToString());
                    break;
                case "Page":
                    TabController.Instance.UpdateTabSetting(ObjectID, "WorkflowID", WorkflowID.ToString());
                    if (ReplaceExistingSettings)
                    {
                        // Get All Modules on the current Tab
                        foreach (var kvp in ModuleController.Instance.GetTabModules(ObjectID))
                        {
                            this.ClearModuleSettings(kvp.Value);
                        }
                    }

                    break;
                case "Site":
                    PortalController.UpdatePortalSetting(ObjectID, "WorkflowID", WorkflowID.ToString());
                    if (ReplaceExistingSettings)
                    {
                        // Get All Tabs aon the Site
                        foreach (var kvp in TabController.Instance.GetTabsByPortal(ObjectID))
                        {
                            TabController.Instance.DeleteTabSetting(kvp.Value.TabID, "WorkFlowID");
                        }

                        // Get All Modules in the current Site
                        foreach (ModuleInfo objModule in ModuleController.Instance.GetModules(ObjectID))
                        {
                            this.ClearModuleSettings(objModule);
                        }
                    }

                    break;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   GetMaximumVersionHistory retrieves the maximum number of versions to store for a module.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "PortalID">The ID of the Portal.</param>
        /// <returns></returns>
        public int GetMaximumVersionHistory(int PortalID)
        {
            int intMaximumVersionHistory = -1;

            // get from portal settings
            intMaximumVersionHistory = int.Parse(PortalController.GetPortalSetting("MaximumVersionHistory", PortalID, "-1"));

            // if undefined at portal level, set portal default
            if (intMaximumVersionHistory == -1)
            {
                intMaximumVersionHistory = 5;

                // default
                PortalController.UpdatePortalSetting(PortalID, "MaximumVersionHistory", intMaximumVersionHistory.ToString());
            }

            return intMaximumVersionHistory;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   UpdateWorkFlowID updates the currently active WorkflowID for the Portal.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "PortalID">The ID of the Portal.</param>
        /// <param name = "MaximumVersionHistory">The MaximumVersionHistory.</param>
        public void UpdateMaximumVersionHistory(int PortalID, int MaximumVersionHistory)
        {
            // data integrity check
            if (MaximumVersionHistory < 0)
            {
                MaximumVersionHistory = 5;

                // default
            }

            // save portal setting
            PortalSettings objPortalSettings = PortalController.Instance.GetCurrentPortalSettings();
            if (PortalSecurity.IsInRole(objPortalSettings.AdministratorRoleName))
            {
                PortalController.UpdatePortalSetting(PortalID, "MaximumVersionHistory", MaximumVersionHistory.ToString());
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   ExportModule implements the IPortable ExportModule Interface.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "moduleId">The Id of the module to be exported.</param>
        /// <returns></returns>
        public string ExportModule(int moduleId)
        {
            string xml = string.Empty;

            ModuleInfo module = ModuleController.Instance.GetModule(moduleId, Null.NullInteger, true);
            int workflowID = this.GetWorkflow(moduleId, module.TabID, module.PortalID).Value;

            HtmlTextInfo content = this.GetTopHtmlText(moduleId, true, workflowID);
            if (content != null)
            {
                xml += "<htmltext>";
                xml += "<content>" + XmlUtils.XMLEncode(this.TokeniseLinks(content.Content, module.PortalID)) + "</content>";
                xml += "</htmltext>";
            }

            return xml;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   ImportModule implements the IPortable ImportModule Interface.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name = "ModuleID">The ID of the Module being imported.</param>
        /// <param name = "Content">The Content being imported.</param>
        /// <param name = "Version">The Version of the Module Content being imported.</param>
        /// <param name = "UserId">The UserID of the User importing the Content.</param>
        public void ImportModule(int ModuleID, string Content, string Version, int UserId)
        {
            ModuleInfo module = ModuleController.Instance.GetModule(ModuleID, Null.NullInteger, true);
            var workflowStateController = new WorkflowStateController();
            int workflowID = this.GetWorkflow(ModuleID, module.TabID, module.PortalID).Value;
            XmlNode xml = Globals.GetContent(Content, "htmltext");

            var htmlContent = new HtmlTextInfo();
            htmlContent.ModuleID = ModuleID;

            // convert Version to System.Version
            var objVersion = new Version(Version);
            if (objVersion >= new Version(5, 1, 0))
            {
                // current module content
                htmlContent.Content = this.DeTokeniseLinks(xml.SelectSingleNode("content").InnerText, module.PortalID);
            }
            else
            {
                // legacy module content
                htmlContent.Content = this.DeTokeniseLinks(xml.SelectSingleNode("desktophtml").InnerText, module.PortalID);
            }

            htmlContent.WorkflowID = workflowID;
            htmlContent.StateID = workflowStateController.GetFirstWorkflowStateID(workflowID);

            // import
            this.UpdateHtmlText(htmlContent, this.GetMaximumVersionHistory(module.PortalID));
        }

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo modInfo, DateTime beginDateUtc)
        {
            var workflowId = this.GetWorkflow(modInfo.ModuleID, modInfo.TabID, modInfo.PortalID).Value;
            var searchDocuments = new List<SearchDocument>();
            var htmlTextInfo = this.GetTopHtmlText(modInfo.ModuleID, true, workflowId);
            var repo = new HtmlModuleSettingsRepository();
            var settings = repo.GetSettings(modInfo);

            if (htmlTextInfo != null &&
                (htmlTextInfo.LastModifiedOnDate.ToUniversalTime() > beginDateUtc &&
                 htmlTextInfo.LastModifiedOnDate.ToUniversalTime() < DateTime.UtcNow))
            {
                var strContent = HtmlUtils.Clean(htmlTextInfo.Content, false);

                // Get the description string
                var description = strContent.Length <= settings.SearchDescLength ? strContent : HtmlUtils.Shorten(strContent, settings.SearchDescLength, "...");

                var searchDoc = new SearchDocument
                {
                    UniqueKey = modInfo.ModuleID.ToString(),
                    PortalId = modInfo.PortalID,
                    Title = modInfo.ModuleTitle,
                    Description = description,
                    Body = strContent,
                    ModifiedTimeUtc = htmlTextInfo.LastModifiedOnDate.ToUniversalTime(),
                };

                if (modInfo.Terms != null && modInfo.Terms.Count > 0)
                {
                    searchDoc.Tags = CollectHierarchicalTags(modInfo.Terms);
                }

                searchDocuments.Add(searchDoc);
            }

            return searchDocuments;
        }

        public string UpgradeModule(string Version)
        {
            switch (Version)
            {
                case "05.01.02":
                    // remove the Code SubDirectory
                    Config.RemoveCodeSubDirectory("HTML");

                    // Once the web.config entry is done we can safely remove the HTML folder
                    var arrPaths = new string[1];
                    arrPaths[0] = "App_Code\\HTML\\";
                    FileSystemUtils.DeleteFiles(arrPaths);
                    break;
                case "06.00.00":
                    DesktopModuleInfo desktopModule = DesktopModuleController.GetDesktopModuleByModuleName("DNN_HTML", Null.NullInteger);
                    desktopModule.Category = "Common";
                    DesktopModuleController.SaveDesktopModule(desktopModule, false, false);
                    break;

                case "06.02.00":
                    this.AddNotificationTypes();
                    break;
            }

            return string.Empty;
        }

        private static void AddHtmlNotification(string subject, string body, UserInfo user)
        {
            var notificationType = NotificationsController.Instance.GetNotificationType("HtmlNotification");
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var sender = UserController.GetUserById(portalSettings.PortalId, portalSettings.AdministratorId);

            var notification = new Notification { NotificationTypeID = notificationType.NotificationTypeId, Subject = subject, Body = body, IncludeDismissAction = true, SenderUserID = sender.UserID };
            NotificationsController.Instance.SendNotification(notification, portalSettings.PortalId, null, new List<UserInfo> { user });
        }

        private static List<string> CollectHierarchicalTags(List<Term> terms)
        {
            Func<List<Term>, List<string>, List<string>> collectTagsFunc = null;
            collectTagsFunc = (ts, tags) =>
            {
                if (ts != null && ts.Count > 0)
                {
                    foreach (var t in ts)
                    {
                        tags.Add(t.Name);
                        tags.AddRange(collectTagsFunc(t.ChildTerms, new List<string>()));
                    }
                }

                return tags;
            };

            return collectTagsFunc(terms, new List<string>());
        }

        private void ClearModuleSettings(ModuleInfo objModule)
        {
            if (objModule.ModuleDefinition.FriendlyName == "Text/HTML")
            {
                ModuleController.Instance.DeleteModuleSetting(objModule.ModuleID, "WorkFlowID");
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        ///   CreateUserNotifications creates HtmlTextUser records and optionally sends email notifications to participants in a Workflow.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objHtmlText">An HtmlTextInfo object.</param>
        private void CreateUserNotifications(HtmlTextInfo objHtmlText)
        {
            var _htmlTextUserController = new HtmlTextUserController();
            HtmlTextUserInfo _htmlTextUser = null;
            UserInfo _user = null;

            // clean up old user notification records
            _htmlTextUserController.DeleteHtmlTextUsers();

            // ensure we have latest htmltext object loaded
            objHtmlText = this.GetHtmlText(objHtmlText.ModuleID, objHtmlText.ItemID);

            // build collection of users to notify
            var objWorkflow = new WorkflowStateController();
            var arrUsers = new ArrayList();

            // if not published
            if (objHtmlText.IsPublished == false)
            {
                arrUsers.Add(objHtmlText.CreatedByUserID); // include content owner
            }

            // if not draft and not published
            if (objHtmlText.StateID != objWorkflow.GetFirstWorkflowStateID(objHtmlText.WorkflowID) && objHtmlText.IsPublished == false)
            {
                // get users from permissions for state
                foreach (WorkflowStatePermissionInfo permission in WorkflowStatePermissionController.GetWorkflowStatePermissions(objHtmlText.StateID))
                {
                    if (permission.AllowAccess)
                    {
                        if (Null.IsNull(permission.UserID))
                        {
                            int roleId = permission.RoleID;
                            RoleInfo objRole = RoleController.Instance.GetRole(objHtmlText.PortalID, r => r.RoleID == roleId);
                            if (objRole != null)
                            {
                                foreach (UserRoleInfo objUserRole in RoleController.Instance.GetUserRoles(objHtmlText.PortalID, null, objRole.RoleName))
                                {
                                    if (!arrUsers.Contains(objUserRole.UserID))
                                    {
                                        arrUsers.Add(objUserRole.UserID);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!arrUsers.Contains(permission.UserID))
                            {
                                arrUsers.Add(permission.UserID);
                            }
                        }
                    }
                }
            }

            // process notifications
            if (arrUsers.Count > 0 || (objHtmlText.IsPublished && objHtmlText.Notify))
            {
                // get tabid from module
                ModuleInfo objModule = ModuleController.Instance.GetModule(objHtmlText.ModuleID, Null.NullInteger, true);

                PortalSettings objPortalSettings = PortalController.Instance.GetCurrentPortalSettings();
                if (objPortalSettings != null)
                {
                    string strResourceFile = string.Format(
                        "{0}/DesktopModules/{1}/{2}/{3}",
                        Globals.ApplicationPath,
                        objModule.DesktopModule.FolderName,
                        Localization.LocalResourceDirectory,
                        Localization.LocalSharedResourceFile);
                    string strSubject = Localization.GetString("NotificationSubject", strResourceFile);
                    string strBody = Localization.GetString("NotificationBody", strResourceFile);
                    strBody = strBody.Replace("[URL]", this.NavigationManager.NavigateURL(objModule.TabID));
                    strBody = strBody.Replace("[STATE]", objHtmlText.StateName);

                    // process user notification collection
                    foreach (int intUserID in arrUsers)
                    {
                        // create user notification record
                        _htmlTextUser = new HtmlTextUserInfo();
                        _htmlTextUser.ItemID = objHtmlText.ItemID;
                        _htmlTextUser.StateID = objHtmlText.StateID;
                        _htmlTextUser.ModuleID = objHtmlText.ModuleID;
                        _htmlTextUser.TabID = objModule.TabID;
                        _htmlTextUser.UserID = intUserID;
                        _htmlTextUserController.AddHtmlTextUser(_htmlTextUser);

                        // send an email notification to a user if the state indicates to do so
                        if (objHtmlText.Notify)
                        {
                            _user = UserController.GetUserById(objHtmlText.PortalID, intUserID);
                            if (_user != null)
                            {
                                AddHtmlNotification(strSubject, strBody, _user);
                            }
                        }
                    }

                    // if published and the published state specifies to notify members of the workflow
                    if (objHtmlText.IsPublished && objHtmlText.Notify)
                    {
                        // send email notification to the author
                        _user = UserController.GetUserById(objHtmlText.PortalID, objHtmlText.CreatedByUserID);
                        if (_user != null)
                        {
                            try
                            {
                                Services.Mail.Mail.SendEmail(objPortalSettings.Email, objPortalSettings.Email, strSubject, strBody);
                            }
                            catch (Exception exc)
                            {
                                Exceptions.LogException(exc);
                            }
                        }
                    }
                }
            }
        }

        private string DeTokeniseLinks(string content, int portalId)
        {
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalRoot = UrlUtils.Combine(Globals.ApplicationPath, portal.HomeDirectory);
            if (!portalRoot.StartsWith("/"))
            {
                portalRoot = "/" + portalRoot;
            }

            if (!portalRoot.EndsWith("/"))
            {
                portalRoot = portalRoot + "/";
            }

            content = Regex.Replace(content, PortalRootToken + "\\/{0,1}", portalRoot, RegexOptions.IgnoreCase);

            return content;
        }

        private string TokeniseLinks(string content, int portalId)
        {
            // Replace any relative portal root reference by a token "{{PortalRoot}}"
            var portal = PortalController.Instance.GetPortal(portalId);
            var portalRoot = UrlUtils.Combine(Globals.ApplicationPath, portal.HomeDirectory);
            if (!portalRoot.StartsWith("/"))
            {
                portalRoot = "/" + portalRoot;
            }

            if (!portalRoot.EndsWith("/"))
            {
                portalRoot = portalRoot + "/";
            }

            // Portal Root regular expression
            var regex = @"(?<url>
                        (?<host>
                        (?<protocol>[A-Za-z]{3,9}:(?:\/\/)?)
                        (?<domain>(?:[\-;:&=\+\$,\w]+@)?[A-Za-z0-9\.\-]+|(?:www\.|[\-;:&=\+\$,\w]+@)[A-Za-z0-9\.\-]+))?
                        (?<portalRoot>" + portalRoot + "))";

            var matchEvaluator = new MatchEvaluator(this.ReplaceWithRootToken);
            var exp = RegexUtils.GetCachedRegex(regex, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
            return exp.Replace(content, matchEvaluator);
        }

        private void AddNotificationTypes()
        {
            var type = new NotificationType { Name = "HtmlNotification", Description = "Html Module Notification" };
            if (NotificationsController.Instance.GetNotificationType(type.Name) == null)
            {
                NotificationsController.Instance.CreateNotificationType(type);
            }
        }
    }
}
