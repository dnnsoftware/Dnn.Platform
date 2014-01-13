/*
' Copyright (c) 2011 DotNetNuke Corporation
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Collections.Generic;
//using System.Xml;
using System.Data.SqlTypes;
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Search;
using DotNetNuke.Services.Search.Controllers;
using DotNetNuke.Services.Search.Entities;

namespace DotNetNuke.Modules.Journal.Components {

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Controller class for Journal
    /// </summary>
    /// -----------------------------------------------------------------------------

    //uncomment the interfaces to add the support.
    public class FeatureController : ModuleSearchBase, IModuleSearchController
    {
        #region Optional Interfaces

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int ModuleID) {
            //string strXML = "";

            //List<JournalInfo> colJournals = GetJournals(ModuleID);
            //if (colJournals.Count != 0)
            //{
            //    strXML += "<Journals>";

            //    foreach (JournalInfo objJournal in colJournals)
            //    {
            //        strXML += "<Journal>";
            //        strXML += "<content>" + DotNetNuke.Common.Utilities.XmlUtils.XMLEncode(objJournal.Content) + "</content>";
            //        strXML += "</Journal>";
            //    }
            //    strXML += "</Journals>";
            //}

            //return strXML;

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be imported</param>
        /// <param name="Content">The content to be imported</param>
        /// <param name="Version">The version of the module to be imported</param>
        /// <param name="UserId">The Id of the user performing the import</param>
        /// -----------------------------------------------------------------------------
        public void ImportModule(int ModuleID, string Content, string Version, int UserId) {
            //XmlNode xmlJournals = DotNetNuke.Common.Globals.GetContent(Content, "Journals");
            //foreach (XmlNode xmlJournal in xmlJournals.SelectNodes("Journal"))
            //{
            //    JournalInfo objJournal = new JournalInfo();
            //    objJournal.ModuleId = ModuleID;
            //    objJournal.Content = xmlJournal.SelectSingleNode("content").InnerText;
            //    objJournal.CreatedByUser = UserID;
            //    AddJournal(objJournal);
            //}

            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// UpgradeModule implements the IUpgradeable Interface
        /// </summary>
        /// <param name="Version">The current version of the module</param>
        /// -----------------------------------------------------------------------------
        public string UpgradeModule(string Version) {
            throw new System.NotImplementedException("The method or operation is not implemented.");
        }

        #endregion

        #region Implement ModuleSearchBase

        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDate)
        {
            var searchDocuments = new Dictionary<int, SearchDocument>();
            int lastJournalId = Null.NullInteger;
            if (beginDate == DateTime.MinValue)
            {
                beginDate = SqlDateTime.MinValue.Value;
            }
            try
            {
                while (true)
                {
                    var reader = DataProvider.Instance()
                                             .ExecuteReader("Journal_GetSearchItems", moduleInfo.PortalID,
                                                            moduleInfo.TabModuleID, beginDate.ToUniversalTime(), lastJournalId,
                                                            Constants.SearchBatchSize);
                    int rowsAffected = 0;

                    while (reader.Read())
                    {
                        var journalId = Convert.ToInt32(reader["JournalId"]);
                        var journalTypeId = reader["JournalTypeId"].ToString();
                        var userId = Convert.ToInt32(reader["UserId"]);
                        var dateUpdated = Convert.ToDateTime(reader["DateUpdated"]);
                        var profileId = reader["ProfileId"].ToString();
                        var groupId = reader["GroupId"].ToString();
                        var title = reader["Title"].ToString();
                        var summary = reader["Summary"].ToString();
                        var securityKey = reader["SecurityKey"].ToString();

                        if (searchDocuments.ContainsKey(journalId))
                        {
                            searchDocuments[journalId].UniqueKey +=
                                string.Format(",{0}", securityKey);
                        }
                        else
                        {
                            var searchDocument = new SearchDocument()
                                                     {
                                                         UniqueKey = journalId + "_" + securityKey,
                                                         Body = summary,
                                                         ModifiedTimeUtc = dateUpdated,
                                                         Title = title,
                                                         AuthorUserId = userId
                                                     };

                            searchDocuments.Add(journalId, searchDocument);
                        }

                        if (journalId > lastJournalId)
                        {
                            lastJournalId = journalId;
                        }
                        rowsAffected++;
                    }

                    //close the reader
                    reader.Close();

                    if (rowsAffected == 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return searchDocuments.Values.ToList();
        }

        #endregion

        #region Implement IModuleSearchController

        public bool HasViewPermission(SearchResult searchResult)
        {
            var securityKeys = searchResult.UniqueKey.Split('_')[1].Split(',');
            var userInfo = UserController.GetCurrentUserInfo();
            var targetUser = UserController.GetUserById(searchResult.PortalId, searchResult.AuthorUserId);
            var selfKey = string.Format("U{0}", userInfo.UserID);

            if (targetUser == null)
            {
                return false;
            }

            if (securityKeys.Contains("E") || securityKeys.Contains(selfKey))
            {
                return true;
            }

            //do not show items in private group
            if (securityKeys.Any(s => s.StartsWith("R")))
            {
                var groupId = Convert.ToInt32(securityKeys.First(s => s.StartsWith("R")).Substring(1));
                var role = new RoleController().GetRole(groupId, searchResult.PortalId);
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
                return targetUser.Social.Friend != null && targetUser.Social.Friend.Status == RelationshipStatus.Accepted;
            }

            return false;
        }

        public string GetDocUrl(SearchResult searchResult)
        {
            var url = string.Empty;
            var userInfo = UserController.GetCurrentUserInfo();
            var portalSettings = PortalController.GetCurrentPortalSettings();
            var journalId = Convert.ToInt32(searchResult.UniqueKey.Split('_')[0]);
            var journalItem = JournalController.Instance.GetJournalItem(searchResult.PortalId, userInfo.UserID,
                                                                        journalId);

            if (journalItem == null)
            {
                return url;
            }

            if (journalItem.SocialGroupId > 0)
            {
                var tabModuleId = new ContentController().GetContentItem(journalItem.ContentItemId).ModuleID;
                var module = new ModuleController().GetTabModule(tabModuleId);

                url = Globals.NavigateURL(module.TabID, string.Empty, "GroupId=" + journalItem.SocialGroupId,
                                          "jid=" + journalId);
            }
            else if (journalItem.ProfileId > 0)
            {
                url = Globals.NavigateURL(portalSettings.UserTabId, string.Empty, string.Format("userId={0}", journalItem.ProfileId), "jid=" + journalId);
            }
            else
            {
                url = Globals.NavigateURL(portalSettings.UserTabId, string.Empty, string.Format("userId={0}", journalItem.UserId), "jid=" + journalId);
            }

            return url;
        }

        #endregion
    }

}
