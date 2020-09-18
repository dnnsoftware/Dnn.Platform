// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Journal
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Search.Entities;
    using DotNetNuke.Services.Search.Internals;

    internal class JournalControllerImpl : IJournalController
    {
        private const string AllowResizePhotosSetting = "Journal_AllowResizePhotos";
        private const string AllowPhotosSetting = "Journal_AllowPhotos";
        private const string EditorEnabledSetting = "Journal_EditorEnabled";

        private static readonly string[] InvalidSecuritySetsWithoutId = new[] { "R", "U", "F", "P" };
        private static readonly char[] ValidSecurityDescriptors = new[] { 'E', 'C', 'R', 'U', 'F', 'P' };
        private readonly IJournalDataService _dataService;

        public JournalControllerImpl()
        {
            this._dataService = JournalDataService.Instance;
        }

        // Journal Items
        public void SaveJournalItem(JournalItem journalItem, int tabId, int moduleId)
        {
            if (journalItem.UserId < 1)
            {
                throw new ArgumentException("journalItem.UserId must be for a real user");
            }

            UserInfo currentUser = UserController.GetUserById(journalItem.PortalId, journalItem.UserId);
            if (currentUser == null)
            {
                throw new Exception("Unable to locate the current user");
            }

            string xml = null;
            var portalSecurity = PortalSecurity.Instance;
            if (!string.IsNullOrEmpty(journalItem.Title))
            {
                journalItem.Title = portalSecurity.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);
            }

            if (!string.IsNullOrEmpty(journalItem.Summary))
            {
                journalItem.Summary = portalSecurity.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.Body))
            {
                journalItem.Body = portalSecurity.InputFilter(journalItem.Body, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.Body))
            {
                var xDoc = new XmlDocument { XmlResolver = null };
                XmlElement xnode = xDoc.CreateElement("items");
                XmlElement xnode2 = xDoc.CreateElement("item");
                xnode2.AppendChild(this.CreateElement(xDoc, "id", "-1"));
                xnode2.AppendChild(this.CreateCDataElement(xDoc, "body", journalItem.Body));
                xnode.AppendChild(xnode2);
                xDoc.AppendChild(xnode);
                XmlDeclaration xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
                xDec.Encoding = "UTF-16";
                xDec.Standalone = "yes";
                XmlElement root = xDoc.DocumentElement;
                xDoc.InsertBefore(xDec, root);
                journalItem.JournalXML = xDoc;
                xml = journalItem.JournalXML.OuterXml;
            }

            if (journalItem.ItemData != null)
            {
                if (!string.IsNullOrEmpty(journalItem.ItemData.Title))
                {
                    journalItem.ItemData.Title = portalSecurity.InputFilter(journalItem.ItemData.Title, PortalSecurity.FilterFlag.NoMarkup);
                }

                if (!string.IsNullOrEmpty(journalItem.ItemData.Description))
                {
                    journalItem.ItemData.Description = portalSecurity.InputFilter(journalItem.ItemData.Description, PortalSecurity.FilterFlag.NoScripting);
                }

                if (!string.IsNullOrEmpty(journalItem.ItemData.Url))
                {
                    journalItem.ItemData.Url = portalSecurity.InputFilter(journalItem.ItemData.Url, PortalSecurity.FilterFlag.NoScripting);
                }

                if (!string.IsNullOrEmpty(journalItem.ItemData.ImageUrl))
                {
                    journalItem.ItemData.ImageUrl = portalSecurity.InputFilter(journalItem.ItemData.ImageUrl, PortalSecurity.FilterFlag.NoScripting);
                }
            }

            string journalData = journalItem.ItemData.ToJson();
            if (journalData == "null")
            {
                journalData = null;
            }

            this.PrepareSecuritySet(journalItem, currentUser);

            journalItem.JournalId = this._dataService.Journal_Save(
                journalItem.PortalId,
                journalItem.UserId,
                journalItem.ProfileId,
                journalItem.SocialGroupId,
                journalItem.JournalId,
                journalItem.JournalTypeId,
                journalItem.Title,
                journalItem.Summary,
                journalItem.Body,
                journalData,
                xml,
                journalItem.ObjectKey,
                journalItem.AccessKey,
                journalItem.SecuritySet,
                journalItem.CommentsDisabled,
                journalItem.CommentsHidden);

            var updatedJournalItem = this.GetJournalItem(journalItem.PortalId, journalItem.UserId, journalItem.JournalId);
            journalItem.DateCreated = updatedJournalItem.DateCreated;
            journalItem.DateUpdated = updatedJournalItem.DateUpdated;
            var cnt = new Content();

            if (journalItem.ContentItemId > 0)
            {
                cnt.UpdateContentItem(journalItem, tabId, moduleId);
                this._dataService.Journal_UpdateContentItemId(journalItem.JournalId, journalItem.ContentItemId);
            }
            else
            {
                ContentItem ci = cnt.CreateContentItem(journalItem, tabId, moduleId);
                this._dataService.Journal_UpdateContentItemId(journalItem.JournalId, ci.ContentItemId);
                journalItem.ContentItemId = ci.ContentItemId;
            }

            if (journalItem.SocialGroupId > 0)
            {
                try
                {
                    this.UpdateGroupStats(journalItem.PortalId, journalItem.SocialGroupId);
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.LogException(exc);
                }
            }
        }

        public void UpdateJournalItem(JournalItem journalItem, int tabId, int moduleId)
        {
            if (journalItem.UserId < 1)
            {
                throw new ArgumentException("journalItem.UserId must be for a real user");
            }

            UserInfo currentUser = UserController.GetUserById(journalItem.PortalId, journalItem.UserId);
            if (currentUser == null)
            {
                throw new Exception("Unable to locate the current user");
            }

            string xml = null;
            var portalSecurity = PortalSecurity.Instance;
            if (!string.IsNullOrEmpty(journalItem.Title))
            {
                journalItem.Title = portalSecurity.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);
            }

            if (!string.IsNullOrEmpty(journalItem.Summary))
            {
                journalItem.Summary = portalSecurity.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.Body))
            {
                journalItem.Body = portalSecurity.InputFilter(journalItem.Body, PortalSecurity.FilterFlag.NoScripting);
            }

            if (!string.IsNullOrEmpty(journalItem.Body))
            {
                var xDoc = new XmlDocument { XmlResolver = null };
                XmlElement xnode = xDoc.CreateElement("items");
                XmlElement xnode2 = xDoc.CreateElement("item");
                xnode2.AppendChild(this.CreateElement(xDoc, "id", "-1"));
                xnode2.AppendChild(this.CreateCDataElement(xDoc, "body", journalItem.Body));
                xnode.AppendChild(xnode2);
                xDoc.AppendChild(xnode);
                XmlDeclaration xDec = xDoc.CreateXmlDeclaration("1.0", null, null);
                xDec.Encoding = "UTF-16";
                xDec.Standalone = "yes";
                XmlElement root = xDoc.DocumentElement;
                xDoc.InsertBefore(xDec, root);
                journalItem.JournalXML = xDoc;
                xml = journalItem.JournalXML.OuterXml;
            }

            if (journalItem.ItemData != null)
            {
                if (!string.IsNullOrEmpty(journalItem.ItemData.Title))
                {
                    journalItem.ItemData.Title = portalSecurity.InputFilter(journalItem.ItemData.Title, PortalSecurity.FilterFlag.NoMarkup);
                }

                if (!string.IsNullOrEmpty(journalItem.ItemData.Description))
                {
                    journalItem.ItemData.Description =
                        portalSecurity.InputFilter(journalItem.ItemData.Description, PortalSecurity.FilterFlag.NoScripting);
                }

                if (!string.IsNullOrEmpty(journalItem.ItemData.Url))
                {
                    journalItem.ItemData.Url = portalSecurity.InputFilter(journalItem.ItemData.Url, PortalSecurity.FilterFlag.NoScripting);
                }

                if (!string.IsNullOrEmpty(journalItem.ItemData.ImageUrl))
                {
                    journalItem.ItemData.ImageUrl = portalSecurity.InputFilter(journalItem.ItemData.ImageUrl, PortalSecurity.FilterFlag.NoScripting);
                }
            }

            string journalData = journalItem.ItemData.ToJson();
            if (journalData == "null")
            {
                journalData = null;
            }

            this.PrepareSecuritySet(journalItem, currentUser);

            journalItem.JournalId = this._dataService.Journal_Update(
                journalItem.PortalId,
                journalItem.UserId,
                journalItem.ProfileId,
                journalItem.SocialGroupId,
                journalItem.JournalId,
                journalItem.JournalTypeId,
                journalItem.Title,
                journalItem.Summary,
                journalItem.Body,
                journalData,
                xml,
                journalItem.ObjectKey,
                journalItem.AccessKey,
                journalItem.SecuritySet,
                journalItem.CommentsDisabled,
                journalItem.CommentsHidden);

            var updatedJournalItem = this.GetJournalItem(journalItem.PortalId, journalItem.UserId, journalItem.JournalId);
            journalItem.DateCreated = updatedJournalItem.DateCreated;
            journalItem.DateUpdated = updatedJournalItem.DateUpdated;

            var cnt = new Content();
            if (journalItem.ContentItemId > 0)
            {
                cnt.UpdateContentItem(journalItem, tabId, moduleId);
                this._dataService.Journal_UpdateContentItemId(journalItem.JournalId, journalItem.ContentItemId);
            }
            else
            {
                ContentItem ci = cnt.CreateContentItem(journalItem, tabId, moduleId);
                this._dataService.Journal_UpdateContentItemId(journalItem.JournalId, ci.ContentItemId);
                journalItem.ContentItemId = ci.ContentItemId;
            }

            if (journalItem.SocialGroupId > 0)
            {
                try
                {
                    this.UpdateGroupStats(journalItem.PortalId, journalItem.SocialGroupId);
                }
                catch (Exception exc)
                {
                    Exceptions.Exceptions.LogException(exc);
                }
            }
        }

        public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId)
        {
            return this.GetJournalItem(portalId, currentUserId, journalId, false, false);
        }

        public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems)
        {
            return this.GetJournalItem(portalId, currentUserId, journalId, includeAllItems, false);
        }

        public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted)
        {
            return this.GetJournalItem(portalId, currentUserId, journalId, includeAllItems, isDeleted, false);
        }

        public JournalItem GetJournalItem(int portalId, int currentUserId, int journalId, bool includeAllItems, bool isDeleted, bool securityCheck)
        {
            return CBO.FillObject<JournalItem>(this._dataService.Journal_Get(portalId, currentUserId, journalId, includeAllItems, isDeleted, securityCheck));
        }

        public JournalItem GetJournalItemByKey(int portalId, string objectKey)
        {
            return this.GetJournalItemByKey(portalId, objectKey, false, false);
        }

        public JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems)
        {
            return this.GetJournalItemByKey(portalId, objectKey, includeAllItems, false);
        }

        public JournalItem GetJournalItemByKey(int portalId, string objectKey, bool includeAllItems, bool isDeleted)
        {
            if (string.IsNullOrEmpty(objectKey))
            {
                return null;
            }

            return CBO.FillObject<JournalItem>(this._dataService.Journal_GetByKey(portalId, objectKey, includeAllItems, isDeleted));
        }

        public IFileInfo SaveJourmalFile(ModuleInfo module, UserInfo userInfo, string fileName, Stream fileContent)
        {
            var userFolder = FolderManager.Instance.GetUserFolder(userInfo);

            if (this.IsImageFile(fileName) && this.IsResizePhotosEnabled(module))
            {
                using (var stream = this.GetJournalImageContent(fileContent))
                {
                    return FileManager.Instance.AddFile(userFolder, fileName, stream, true);
                }
            }

            // todo: deal with the case where the exact file name already exists.
            return FileManager.Instance.AddFile(userFolder, fileName, fileContent, true);
        }

        public void SaveJournalItem(JournalItem journalItem, ModuleInfo module)
        {
            var tabId = module == null ? Null.NullInteger : module.TabID;
            var tabModuleId = module == null ? Null.NullInteger : module.TabModuleID;

            this.SaveJournalItem(journalItem, tabId, tabModuleId);
        }

        public void UpdateJournalItem(JournalItem journalItem, ModuleInfo module)
        {
            var tabId = module == null ? Null.NullInteger : module.TabID;
            var tabModuleId = module == null ? Null.NullInteger : module.TabModuleID;

            this.UpdateJournalItem(journalItem, tabId, tabModuleId);
        }

        public void DisableComments(int portalId, int journalId)
        {
            this._dataService.Journal_Comments_ToggleDisable(portalId, journalId, true);
        }

        public void EnableComments(int portalId, int journalId)
        {
            this._dataService.Journal_Comments_ToggleDisable(portalId, journalId, false);
        }

        public void HideComments(int portalId, int journalId)
        {
            this._dataService.Journal_Comments_ToggleHidden(portalId, journalId, true);
        }

        public void ShowComments(int portalId, int journalId)
        {
            this._dataService.Journal_Comments_ToggleHidden(portalId, journalId, false);
        }

        // Delete Journal Items

        /// <summary>
        /// HARD deletes journal items.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="journalId"></param>
        public void DeleteJournalItem(int portalId, int currentUserId, int journalId)
        {
            this.DeleteJournalItem(portalId, currentUserId, journalId, false);
        }

        /// <summary>
        /// HARD deletes journal items based on item key.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="objectKey"></param>
        public void DeleteJournalItemByKey(int portalId, string objectKey)
        {
            this._dataService.Journal_DeleteByKey(portalId, objectKey);
        }

        /// <summary>
        /// HARD deletes journal items based on group Id.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="groupId"></param>
        public void DeleteJournalItemByGroupId(int portalId, int groupId)
        {
            this._dataService.Journal_DeleteByGroupId(portalId, groupId);
        }

        /// <summary>
        /// SOFT deletes journal items.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="currentUserId"></param>
        /// <param name="journalId"></param>
        public void SoftDeleteJournalItem(int portalId, int currentUserId, int journalId)
        {
            this.DeleteJournalItem(portalId, currentUserId, journalId, true);
        }

        /// <summary>
        /// SOFT deletes journal items based on item key.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="objectKey"></param>
        public void SoftDeleteJournalItemByKey(int portalId, string objectKey)
        {
            this._dataService.Journal_SoftDeleteByKey(portalId, objectKey);
        }

        /// <summary>
        /// SOFT deletes journal items based on group Id.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="groupId"></param>
        public void SoftDeleteJournalItemByGroupId(int portalId, int groupId)
        {
            this._dataService.Journal_SoftDeleteByGroupId(portalId, groupId);
        }

        // Journal Comments
        public IList<CommentInfo> GetCommentsByJournalIds(List<int> journalIdList)
        {
            var journalIds = journalIdList.Aggregate(string.Empty, (current, journalId) => current + journalId + ";");
            return CBO.FillCollection<CommentInfo>(this._dataService.Journal_Comment_ListByJournalIds(journalIds));
        }

        public void LikeJournalItem(int journalId, int userId, string displayName)
        {
            this._dataService.Journal_Like(journalId, userId, displayName);
        }

        public void SaveComment(CommentInfo comment)
        {
            var portalSecurity = PortalSecurity.Instance;
            if (!string.IsNullOrEmpty(comment.Comment))
            {
                comment.Comment =
                    portalSecurity.InputFilter(comment.Comment, PortalSecurity.FilterFlag.NoScripting);
            }

            // TODO: enable once the profanity filter is working properly.
            // objCommentInfo.Comment = portalSecurity.Remove(objCommentInfo.Comment, DotNetNuke.Security.PortalSecurity.ConfigType.ListController, "ProfanityFilter", DotNetNuke.Security.PortalSecurity.FilterScope.PortalList);
            string xml = null;
            if (comment.CommentXML != null)
            {
                xml = comment.CommentXML.OuterXml;
            }

            comment.CommentId = this._dataService.Journal_Comment_Save(comment.JournalId, comment.CommentId, comment.UserId, comment.Comment, xml, Null.NullDate);

            var newComment = this.GetComment(comment.CommentId);
            comment.DateCreated = newComment.DateCreated;
            comment.DateUpdated = newComment.DateUpdated;
        }

        public CommentInfo GetComment(int commentId)
        {
            return CBO.FillObject<CommentInfo>(this._dataService.Journal_Comment_Get(commentId));
        }

        public void DeleteComment(int journalId, int commentId)
        {
            this._dataService.Journal_Comment_Delete(journalId, commentId);

            // UNDONE: update the parent journal item and content item so this comment gets removed from search index
        }

        public void LikeComment(int journalId, int commentId, int userId, string displayName)
        {
            this._dataService.Journal_Comment_Like(journalId, commentId, userId, displayName);
        }

        public JournalTypeInfo GetJournalType(string journalType)
        {
            return CBO.FillObject<JournalTypeInfo>(this._dataService.Journal_Types_Get(journalType));
        }

        public JournalTypeInfo GetJournalTypeById(int journalTypeId)
        {
            return CBO.FillObject<JournalTypeInfo>(this._dataService.Journal_Types_GetById(journalTypeId));
        }

        public IEnumerable<JournalTypeInfo> GetJournalTypes(int portalId)
        {
            return CBO.GetCachedObject<IEnumerable<JournalTypeInfo>>(
                                            new CacheItemArgs(
                                                string.Format(DataCache.JournalTypesCacheKey, portalId),
                                                DataCache.JournalTypesTimeOut,
                                                DataCache.JournalTypesCachePriority,
                                                portalId),
                                            c => CBO.FillCollection<JournalTypeInfo>(this._dataService.Journal_Types_List(portalId)));
        }

        [Obsolete("Deprecated in DNN 7.2.2. Use SaveJournalItem(JournalItem, ModuleInfo). Scheduled removal in v10.0.0.")]
        public void SaveJournalItem(JournalItem journalItem, int tabId)
        {
            this.SaveJournalItem(journalItem, tabId, Null.NullInteger);
        }

        [Obsolete("Deprecated in DNN 7.2.2. Use UpdateJournalItem(JournalItem, ModuleInfo). Scheduled removal in v10.0.0.")]
        public void UpdateJournalItem(JournalItem journalItem, int tabId)
        {
            this.UpdateJournalItem(journalItem, tabId, Null.NullInteger);
        }

        private XmlElement CreateElement(XmlDocument xDoc, string name, string value)
        {
            XmlElement xnode = xDoc.CreateElement(name);
            XmlText xtext = xDoc.CreateTextNode(value);
            xnode.AppendChild(xtext);
            return xnode;
        }

        private XmlElement CreateCDataElement(XmlDocument xDoc, string name, string value)
        {
            XmlElement xnode = xDoc.CreateElement(name);
            XmlCDataSection xdata = xDoc.CreateCDataSection(value);
            xnode.AppendChild(xdata);
            return xnode;
        }

        private void UpdateGroupStats(int portalId, int groupId)
        {
            RoleInfo role = RoleController.Instance.GetRole(portalId, r => r.RoleID == groupId);
            if (role == null)
            {
                return;
            }

            for (var i = 0; i < role.Settings.Keys.Count; i++)
            {
                var key = role.Settings.Keys.ElementAt(i);
                if (key.StartsWith("stat_"))
                {
                    role.Settings[key] = "0";
                }
            }

            using (IDataReader dr = this._dataService.Journal_GetStatsForGroup(portalId, groupId))
            {
                while (dr.Read())
                {
                    string settingName = "stat_" + dr["JournalType"];
                    if (role.Settings.ContainsKey(settingName))
                    {
                        role.Settings[settingName] = dr["JournalTypeCount"].ToString();
                    }
                    else
                    {
                        role.Settings.Add(settingName, dr["JournalTypeCount"].ToString());
                    }
                }

                dr.Close();
            }

            RoleController.Instance.UpdateRoleSettings(role, true);
        }

        private void DeleteJournalItem(int portalId, int currentUserId, int journalId, bool softDelete)
        {
            var ji = this.GetJournalItem(portalId, currentUserId, journalId, !softDelete);
            if (ji == null)
            {
                return;
            }

            var groupId = ji.SocialGroupId;

            if (softDelete)
            {
                this._dataService.Journal_SoftDelete(journalId);
            }
            else
            {
                this._dataService.Journal_Delete(journalId);
            }

            if (groupId > 0)
            {
                this.UpdateGroupStats(portalId, groupId);
            }

            // queue remove journal from search index
            var document = new SearchDocumentToDelete
            {
                PortalId = portalId,
                AuthorUserId = currentUserId,
                UniqueKey = ji.ContentItemId.ToString("D"),

                // QueryString = "journalid=" + journalId,
                SearchTypeId = SearchHelper.Instance.GetSearchTypeByName("module").SearchTypeId,
            };

            if (groupId > 0)
            {
                document.RoleId = groupId;
            }

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        private Stream GetJournalImageContent(Stream fileContent)
        {
            Image image = new Bitmap(fileContent);
            int thumbnailWidth = 400;
            int thumbnailHeight = 400;
            this.GetThumbnailSize(image.Width, image.Height, ref thumbnailWidth, ref thumbnailHeight);
            var thumbnail = image.GetThumbnailImage(thumbnailWidth, thumbnailHeight, this.ThumbnailCallback, IntPtr.Zero);
            var result = new MemoryStream();
            thumbnail.Save(result, image.RawFormat);
            return result;
        }

        private void GetThumbnailSize(int imageWidth, int imageHeight, ref int thumbnailWidth, ref int thumbnailHeight)
        {
            if (imageWidth >= imageHeight)
            {
                thumbnailWidth = Math.Min(imageWidth, thumbnailWidth);
                thumbnailHeight = this.GetMinorSize(imageHeight, imageWidth, thumbnailWidth);
            }
            else
            {
                thumbnailHeight = Math.Min(imageHeight, thumbnailHeight);
                thumbnailWidth = this.GetMinorSize(imageWidth, imageHeight, thumbnailHeight);
            }
        }

        private int GetMinorSize(int imageMinorSize, int imageMajorSize, int thumbnailMajorSize)
        {
            if (imageMajorSize == thumbnailMajorSize)
            {
                return imageMinorSize;
            }

            double calculated = (Convert.ToDouble(imageMinorSize) * Convert.ToDouble(thumbnailMajorSize)) / Convert.ToDouble(imageMajorSize);
            return Convert.ToInt32(Math.Round(calculated));
        }

        private bool IsImageFile(string fileName)
        {
            return (Globals.glbImageFileTypes + ",").IndexOf(Path.GetExtension(fileName).Replace(".", string.Empty) + ",", StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        private bool ThumbnailCallback()
        {
            return true;
        }

        private bool IsResizePhotosEnabled(ModuleInfo module)
        {
            return this.GetBooleanSetting(AllowResizePhotosSetting, false, module) &&
                   this.GetBooleanSetting(AllowPhotosSetting, true, module) &&
                   this.GetBooleanSetting(EditorEnabledSetting, true, module);
        }

        private bool GetBooleanSetting(string settingName, bool defaultValue, ModuleInfo module)
        {
            if (module.ModuleSettings.Contains(settingName))
            {
                return Convert.ToBoolean(module.ModuleSettings[settingName].ToString());
            }

            return defaultValue;
        }

        // none of the parameters should be null; checked before calling this method
        private void PrepareSecuritySet(JournalItem journalItem, UserInfo currentUser)
        {
            var originalSecuritySet =
                journalItem.SecuritySet = (journalItem.SecuritySet ?? string.Empty).ToUpperInvariant();

            if (string.IsNullOrEmpty(journalItem.SecuritySet))
            {
                journalItem.SecuritySet = "E,";
            }
            else if (!journalItem.SecuritySet.EndsWith(","))
            {
                journalItem.SecuritySet += ",";
                originalSecuritySet = journalItem.SecuritySet;
            }

            if (journalItem.SecuritySet == "F,")
            {
                journalItem.SecuritySet = "F" + journalItem.UserId + ",";
                if (journalItem.ProfileId > 0)
                {
                    journalItem.SecuritySet += "P" + journalItem.ProfileId + ",";
                }
            }
            else if (journalItem.SecuritySet == "U,")
            {
                journalItem.SecuritySet += "U" + journalItem.UserId + ",";
            }
            else if (journalItem.SecuritySet == "R,")
            {
                if (journalItem.SocialGroupId > 0)
                {
                    journalItem.SecuritySet += "R" + journalItem.SocialGroupId + ",";
                }
            }

            if (journalItem.ProfileId > 0 && journalItem.UserId != journalItem.ProfileId)
            {
                if (!journalItem.SecuritySet.Contains("P" + journalItem.ProfileId + ","))
                {
                    journalItem.SecuritySet += "P" + journalItem.ProfileId + ",";
                }

                if (!journalItem.SecuritySet.Contains("U" + journalItem.UserId + ","))
                {
                    journalItem.SecuritySet += "U" + journalItem.UserId + ",";
                }
            }

            if (!journalItem.SecuritySet.Contains("U" + journalItem.UserId + ","))
            {
                journalItem.SecuritySet += "U" + journalItem.UserId + ",";
            }

            // if the post is marked as private, we shouldn't make it visible to the group.
            if (journalItem.SocialGroupId > 0 && originalSecuritySet.Contains("U,"))
            {
                var item = journalItem;
                var role = RoleController.Instance.GetRole(
                    journalItem.PortalId,
                    r => r.SecurityMode != SecurityMode.SecurityRole && r.RoleID == item.SocialGroupId);

                if (role != null && !role.IsPublic)
                {
                    journalItem.SecuritySet = journalItem.SecuritySet.Replace("E,", string.Empty).Replace("C,", string.Empty);
                }
            }

            // clean up and remove duplicates
            var parts = journalItem.SecuritySet
                .Replace(" ", string.Empty)
                .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .Except(InvalidSecuritySetsWithoutId)
                .Where(p => p.IndexOfAny(ValidSecurityDescriptors) >= 0);

            // TODO: validate existence and visibility/accessability of all Roles added to the set (if any)
            journalItem.SecuritySet = string.Join(",", parts);
        }
    }
}
