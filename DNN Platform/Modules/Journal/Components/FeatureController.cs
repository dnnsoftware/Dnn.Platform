﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Modules.Journal.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Search.Controllers;
    using DotNetNuke.Services.Search.Entities;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The Controller class for Journal.</summary>
    public class FeatureController : ModuleSearchBase, IModuleSearchResultController
    {
        /// <summary>Initializes a new instance of the <see cref="FeatureController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.0. Please use overload with INavigationManager. Scheduled removal in v12.0.0.")]
        public FeatureController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="FeatureController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public FeatureController(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();
        }

        protected INavigationManager NavigationManager { get; }

        /// <inheritdoc cref="IPortable.ExportModule" />
        public string ExportModule(int moduleID)
        {
            // string strXML = "";

            // List<JournalInfo> colJournals = GetJournals(ModuleID);
            // if (colJournals.Count != 0)
            // {
            //    strXML += "<Journals>";

            // foreach (JournalInfo objJournal in colJournals)
            //    {
            //        strXML += "<Journal>";
            //        strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objJournal.Content) + "</content>";
            //        strXML += "</Journal>";
            //    }
            //    strXML += "</Journals>";
            // }

            // return strXML;
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <inheritdoc cref="IPortable.ImportModule" />
        public void ImportModule(int moduleID, string content, string version, int userId)
        {
            // XmlNode xmlJournals = DotNetNuke.Common.Globals.GetContent(Content, "Journals");
            // foreach (XmlNode xmlJournal in xmlJournals.SelectNodes("Journal"))
            // {
            //    JournalInfo objJournal = new JournalInfo();
            //    objJournal.ModuleId = ModuleID;
            //    objJournal.Content = xmlJournal.SelectSingleNode("content").InnerText;
            //    objJournal.CreatedByUser = UserID;
            //    AddJournal(objJournal);
            // }
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <inheritdoc cref="IUpgradeable.UpgradeModule" />
        public string UpgradeModule(string version)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <inheritdoc/>
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            var searchDocuments = new Dictionary<string, SearchDocument>();
            var lastJournalId = Null.NullInteger;
            try
            {
                while (true)
                {
                    using (var reader = DataProvider.Instance().ExecuteReader("Journal_GetSearchItems", moduleInfo.PortalID, moduleInfo.TabModuleID, beginDateUtc, lastJournalId, Constants.SearchBatchSize))
                    {
                        var journalIds = new Dictionary<int, int>();

                        while (reader.Read())
                        {
                            var journalId = Convert.ToInt32(reader["JournalId"]);

                            // var journalTypeId = reader["JournalTypeId"].ToString();
                            var userId = Convert.ToInt32(reader["UserId"]);
                            var dateUpdated = Convert.ToDateTime(reader["DateUpdated"]);
                            var profileId = reader["ProfileId"].ToString();
                            var groupId = reader["GroupId"].ToString();
                            var title = reader["Title"].ToString();
                            var summary = reader["Summary"].ToString();
                            var securityKey = reader["SecurityKey"].ToString();
                            var tabId = reader["TabId"].ToString();
                            var tabModuleId = reader["ModuleId"].ToString();

                            var key = string.Format("JI_{0}", journalId);
                            if (searchDocuments.ContainsKey(key))
                            {
                                searchDocuments[key].UniqueKey +=
                                    string.Format(",{0}", securityKey);
                            }
                            else
                            {
                                var searchDocument = new SearchDocument
                                {
                                    UniqueKey = string.Format("JI_{0}_{1}", journalId, securityKey),
                                    PortalId = moduleInfo.PortalID,
                                    Body = summary,
                                    ModifiedTimeUtc = dateUpdated,
                                    Title = title,
                                    AuthorUserId = userId,
                                    Keywords = new Dictionary<string, string>
                                    {
                                        { "TabId", tabId },
                                        { "TabModuleId", tabModuleId },
                                        { "ProfileId", profileId },
                                        { "GroupId", groupId },
                                    },
                                };

                                searchDocuments.Add(key, searchDocument);
                            }

                            if (journalId > lastJournalId)
                            {
                                lastJournalId = journalId;
                            }

                            if (!journalIds.ContainsKey(journalId))
                            {
                                journalIds.Add(journalId, userId);
                            }
                        }

                        if (journalIds.Count == 0)
                        {
                            break;
                        }

                        // index comments for this journal
                        this.AddCommentItems(journalIds, searchDocuments);
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return searchDocuments.Values.ToList();
        }

        /// <inheritdoc/>
        public bool HasViewPermission(SearchResult searchResult)
        {
            if (!searchResult.UniqueKey.StartsWith("JI_", StringComparison.InvariantCultureIgnoreCase))
            {
                if (HttpContext.Current == null || PortalSettings.Current == null)
                {
                    return false;
                }

                var portalSettings = PortalSettings.Current;
                var currentUser = UserController.Instance.GetCurrentUserInfo();
                var isAdmin = currentUser.IsInRole(RoleController.Instance.GetRoleById(portalSettings.PortalId, portalSettings.AdministratorRoleId).RoleName);
                if (!HttpContext.Current.Request.IsAuthenticated || (!currentUser.IsSuperUser && !isAdmin && currentUser.IsInRole("Unverified Users")))
                {
                    return false;
                }

                return true;
            }

            var securityKeys = searchResult.UniqueKey.Split('_')[2].Split(',');
            var userInfo = UserController.Instance.GetCurrentUserInfo();

            var selfKey = string.Format("U{0}", userInfo.UserID);

            if (securityKeys.Contains("E") || securityKeys.Contains(selfKey))
            {
                return true;
            }

            // do not show items in private group
            if (securityKeys.Any(s => s.StartsWith("R")))
            {
                var groupId = Convert.ToInt32(securityKeys.First(s => s.StartsWith("R")).Substring(1));
                var role = RoleController.Instance.GetRoleById(searchResult.PortalId, groupId);
                if (role != null && !role.IsPublic && !userInfo.IsInRole(role.RoleName))
                {
                    return false;
                }
            }

            if (securityKeys.Contains("C"))
            {
                return userInfo.UserID > 0;
            }

            if (securityKeys.Any(s => s.StartsWith("F")))
            {
                var targetUser = UserController.GetUserById(searchResult.PortalId, searchResult.AuthorUserId);

                return targetUser != null && targetUser.Social.Friend != null && targetUser.Social.Friend.Status == RelationshipStatus.Accepted;
            }

            return false;
        }

        /// <inheritdoc/>
        public string GetDocUrl(SearchResult searchResult)
        {
            if (!searchResult.UniqueKey.StartsWith("JI_", StringComparison.InvariantCultureIgnoreCase))
            {
                return string.Empty;
            }

            string url;
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();
            var journalId = Convert.ToInt32(searchResult.UniqueKey.Split('_')[1]);
            var groupId = Convert.ToInt32(searchResult.Keywords["GroupId"]);
            var tabId = Convert.ToInt32(searchResult.Keywords["TabId"]);

            // var tabModuleId = Convert.ToInt32(searchResult.Keywords["TabModuleId"]);
            var profileId = Convert.ToInt32(searchResult.Keywords["ProfileId"]);

            if (groupId > 0 && tabId > 0)
            {
                url = this.NavigationManager.NavigateURL(tabId, string.Empty, "GroupId=" + groupId, "jid=" + journalId);
            }
            else if (tabId == portalSettings.UserTabId)
            {
                url = this.NavigationManager.NavigateURL(portalSettings.UserTabId, string.Empty, string.Format("userId={0}", profileId), "jid=" + journalId);
            }
            else
            {
                url = this.NavigationManager.NavigateURL(tabId, string.Empty, "jid=" + journalId);
            }

            return url;
        }

        private void AddCommentItems(Dictionary<int, int> journalIds, IDictionary<string, SearchDocument> searchDocuments)
        {
            var comments = JournalController.Instance.GetCommentsByJournalIds(journalIds.Keys.ToList());
            foreach (var commentInfo in comments)
            {
                var journalResult = searchDocuments[string.Format("JI_{0}", commentInfo.JournalId)];
                if (journalResult != null)
                {
                    journalResult.Body += string.Format(" {0}", commentInfo.Comment);
                    if (commentInfo.DateCreated > journalResult.ModifiedTimeUtc)
                    {
                        journalResult.ModifiedTimeUtc = commentInfo.DateUpdated;
                    }
                }
            }
        }
    }
}
