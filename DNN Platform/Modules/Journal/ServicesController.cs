// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.Journal.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Journal.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [SupportedModules("Journal")]
    public class ServicesController : DnnApiController
    {
        private const int MentionNotificationLength = 100;
        private const string MentionNotificationSuffix = "...";
        private const string MentionIdentityChar = "@";

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServicesController));

        private static readonly string[] AcceptedFileExtensions = { "jpg", "png", "gif", "jpe", "jpeg", "tiff", "bmp" };

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage Create(CreateDTO postData)
        {
            try
            {
                int userId = this.UserInfo.UserID;
                IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();

                if (postData.ProfileId == -1)
                {
                    postData.ProfileId = userId;
                }

                this.checkProfileAccess(postData.ProfileId, this.UserInfo);

                this.checkGroupAccess(postData);

                var journalItem = this.prepareJournalItem(postData, mentionedUsers);

                JournalController.Instance.SaveJournalItem(journalItem, this.ActiveModule);

                var originalSummary = journalItem.Summary;
                this.SendMentionNotifications(mentionedUsers, journalItem, originalSummary);

                return this.Request.CreateResponse(HttpStatusCode.OK, journalItem);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage Delete(JournalIdDTO postData)
        {
            try
            {
                var jc = JournalController.Instance;
                var ji = jc.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);

                if (ji == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
                }

                if (ji.UserId == this.UserInfo.UserID || ji.ProfileId == this.UserInfo.UserID || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    jc.DeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage SoftDelete(JournalIdDTO postData)
        {
            try
            {
                var jc = JournalController.Instance;
                var ji = jc.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);

                if (ji == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
                }

                if (ji.UserId == this.UserInfo.UserID || ji.ProfileId == this.UserInfo.UserID || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    jc.SoftDeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize]
        public HttpResponseMessage PreviewUrl(PreviewDTO postData)
        {
            try
            {
                var link = Utilities.GetLinkData(postData.Url);
                return this.Request.CreateResponse(HttpStatusCode.OK, link);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage GetListForProfile(GetListForProfileDTO postData)
        {
            try
            {
                var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, postData.ProfileId, postData.GroupId, this.UserInfo);
                return this.Request.CreateResponse(HttpStatusCode.OK, jp.GetList(postData.RowIndex, postData.MaxRows), "text/html");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                throw new HttpException(500, exc.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage Like(JournalIdDTO postData)
        {
            try
            {
                JournalController.Instance.LikeJournalItem(postData.JournalId, this.UserInfo.UserID, this.UserInfo.DisplayName);
                var ji = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
                var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, ji.ProfileId, -1, this.UserInfo);
                var isLiked = false;
                var likeList = jp.GetLikeListHTML(ji, ref isLiked);
                likeList = Utilities.LocalizeControl(likeList);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { LikeList = likeList, Liked = isLiked });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage CommentSave(CommentSaveDTO postData)
        {
            try
            {
                var comment = Utilities.RemoveHTML(HttpUtility.UrlDecode(postData.Comment));

                IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();
                var originalComment = comment;
                comment = this.ParseMentions(comment, postData.Mentions, ref mentionedUsers);
                var ci = new CommentInfo { JournalId = postData.JournalId, Comment = comment };
                ci.UserId = this.UserInfo.UserID;
                ci.DisplayName = this.UserInfo.DisplayName;
                JournalController.Instance.SaveComment(ci);

                var ji = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
                var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, ji.ProfileId, -1, this.UserInfo);

                this.SendMentionNotifications(mentionedUsers, ji, originalComment, "Comment");

                return this.Request.CreateResponse(HttpStatusCode.OK, jp.GetCommentRow(ji, ci), "text/html");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage CommentDelete(CommentDeleteDTO postData)
        {
            try
            {
                var ci = JournalController.Instance.GetComment(postData.CommentId);
                if (ci == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "delete failed");
                }

                var ji = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);

                if (ji == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
                }

                if (ci.UserId == this.UserInfo.UserID || ji.UserId == this.UserInfo.UserID || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
                {
                    JournalController.Instance.DeleteComment(postData.JournalId, postData.CommentId);
                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        [DnnAuthorize(DenyRoles = "Unverified Users")]
        public HttpResponseMessage GetSuggestions(string keyword)
        {
            try
            {
                var findedUsers = new List<SuggestDTO>();
                var relations = RelationshipController.Instance.GetUserRelationships(this.UserInfo);
                foreach (var ur in relations)
                {
                    var targetUserId = ur.UserId == this.UserInfo.UserID ? ur.RelatedUserId : ur.UserId;
                    var targetUser = UserController.GetUserById(this.PortalSettings.PortalId, targetUserId);
                    var relationship = RelationshipController.Instance.GetRelationship(ur.RelationshipId);
                    if (ur.Status == RelationshipStatus.Accepted && targetUser != null
                        && ((relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Followers && ur.RelatedUserId == this.UserInfo.UserID)
                                || relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Friends)
                        && (targetUser.DisplayName.ToLowerInvariant().Contains(keyword.ToLowerInvariant())
                                || targetUser.DisplayName.ToLowerInvariant().Contains(keyword.Replace("-", " ").ToLowerInvariant()))
                        && findedUsers.All(s => s.userId != targetUser.UserID))
                    {
                        findedUsers.Add(new SuggestDTO
                        {
                            displayName = targetUser.DisplayName.Replace(" ", "-"),
                            userId = targetUser.UserID,
                            avatar = targetUser.Profile.PhotoURL,
                            key = keyword,
                        });
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, findedUsers.Cast<object>().Take(5));
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private static bool IsImageFile(string relativePath)
        {
            if (relativePath == null)
            {
                return false;
            }

            if (relativePath.Contains("?"))
            {
                relativePath = relativePath.Substring(
                    0,
                    relativePath.IndexOf("?", StringComparison.InvariantCultureIgnoreCase));
            }

            var extension = relativePath.Substring(relativePath.LastIndexOf(
                ".",
                StringComparison.Ordinal) + 1).ToLowerInvariant();
            return AcceptedFileExtensions.Contains(extension);
        }

        private static bool IsAllowedLink(string url)
        {
            return !string.IsNullOrEmpty(url) && !url.Contains("//");
        }

        // Check if a user can post content on a specific profile's page
        private void checkProfileAccess(int profileId, UserInfo currentUser)
        {
            if (profileId != currentUser.UserID)
            {
                var profileUser = UserController.Instance.GetUser(this.PortalSettings.PortalId, profileId);
                if (profileUser == null || (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !Utilities.AreFriends(profileUser, currentUser)))
                {
                    throw new ArgumentException("you have no permission to post journal on current profile page.");
                }
            }
        }

        private void checkGroupAccess(CreateDTO postData)
        {
            if (postData.GroupId > 0)
            {
                postData.ProfileId = -1;

                RoleInfo roleInfo = RoleController.Instance.GetRoleById(this.ActiveModule.OwnerPortalID, postData.GroupId);
                if (roleInfo != null)
                {
                    if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !this.UserInfo.IsInRole(roleInfo.RoleName))
                    {
                        throw new ArgumentException("you have no permission to post journal on current group.");
                    }

                    if (!roleInfo.IsPublic)
                    {
                        postData.SecuritySet = "R";
                    }
                }
            }
        }

        private JournalItem prepareJournalItem(CreateDTO postData, IDictionary<string, UserInfo> mentionedUsers)
        {
            var journalTypeId = 1;
            switch (postData.JournalType)
            {
                case "link":
                    journalTypeId = 2;
                    break;
                case "photo":
                    journalTypeId = 3;
                    break;
                case "file":
                    journalTypeId = 4;
                    break;
            }

            var ji = new JournalItem
            {
                JournalId = -1,
                JournalTypeId = journalTypeId,
                PortalId = this.ActiveModule.OwnerPortalID,
                UserId = this.UserInfo.UserID,
                SocialGroupId = postData.GroupId,
                ProfileId = postData.ProfileId,
                Summary = postData.Text ?? string.Empty,
                SecuritySet = postData.SecuritySet,
            };
            ji.Title = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(ji.Title));
            ji.Summary = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(ji.Summary));

            var ps = PortalSecurity.Instance;

            ji.Title = ps.InputFilter(ji.Title, PortalSecurity.FilterFlag.NoScripting);
            ji.Title = Utilities.RemoveHTML(ji.Title);
            ji.Title = ps.InputFilter(ji.Title, PortalSecurity.FilterFlag.NoMarkup);

            ji.Summary = ps.InputFilter(ji.Summary, PortalSecurity.FilterFlag.NoScripting);
            ji.Summary = Utilities.RemoveHTML(ji.Summary);
            ji.Summary = ps.InputFilter(ji.Summary, PortalSecurity.FilterFlag.NoMarkup);

            // parse the mentions context in post data
            var originalSummary = ji.Summary;

            ji.Summary = this.ParseMentions(ji.Summary, postData.Mentions, ref mentionedUsers);

            if (ji.Summary.Length > 2000)
            {
                ji.Body = ji.Summary;
                ji.Summary = null;
            }

            if (!string.IsNullOrEmpty(postData.ItemData))
            {
                ji.ItemData = postData.ItemData.FromJson<ItemData>();
                var originalImageUrl = ji.ItemData.ImageUrl;
                if (!IsImageFile(ji.ItemData.ImageUrl))
                {
                    ji.ItemData.ImageUrl = string.Empty;
                }

                ji.ItemData.Description = HttpUtility.UrlDecode(ji.ItemData.Description);

                if (!IsAllowedLink(ji.ItemData.Url))
                {
                    ji.ItemData.Url = string.Empty;
                }

                if (!string.IsNullOrEmpty(ji.ItemData.Url) && ji.ItemData.Url.StartsWith("fileid="))
                {
                    var fileId = Convert.ToInt32(ji.ItemData.Url.Replace("fileid=", string.Empty).Trim());
                    var file = FileManager.Instance.GetFile(fileId);

                    if (!this.IsCurrentUserFile(file))
                    {
                        throw new ArgumentException("you have no permission to attach files not belongs to you.");
                    }

                    ji.ItemData.Title = file.FileName;
                    ji.ItemData.Url = Globals.LinkClick(ji.ItemData.Url, Null.NullInteger, Null.NullInteger);

                    if (string.IsNullOrEmpty(ji.ItemData.ImageUrl) &&
                        originalImageUrl.ToLowerInvariant().StartsWith("/linkclick.aspx?") &&
                        AcceptedFileExtensions.Contains(file.Extension.ToLowerInvariant()))
                    {
                        ji.ItemData.ImageUrl = originalImageUrl;
                    }
                }
            }

            return ji;
        }

        private string ParseMentions(string content, IList<MentionDTO> mentions, ref IDictionary<string, UserInfo> mentionedUsers)
        {
            if (mentions == null || mentions.Count == 0)
            {
                return content;
            }

            foreach (var mention in mentions)
            {
                var user = UserController.GetUserById(this.PortalSettings.PortalId, mention.UserId);

                if (user != null)
                {
                    var relationship = RelationshipController.Instance.GetFollowingRelationship(this.UserInfo, user) ??
                                       RelationshipController.Instance.GetFriendRelationship(this.UserInfo, user);
                    if (relationship != null && relationship.Status == RelationshipStatus.Accepted)
                    {
                        var userLink = string.Format(
                            "<a href=\"{0}\" class=\"userLink\" target=\"_blank\">{1}</a>",
                            Globals.UserProfileURL(user.UserID),
                            MentionIdentityChar + user.DisplayName);
                        content = content.Replace(MentionIdentityChar + mention.DisplayName, userLink);

                        mentionedUsers.Add(mention.DisplayName, user);
                    }
                }
            }

            return content;
        }

        private void SendMentionNotifications(IDictionary<string, UserInfo> mentionedUsers, JournalItem item, string originalSummary, string type = "Post")
        {
            // send notification to the mention users
            var subjectTemplate = Utilities.GetSharedResource("Notification_Mention.Subject");
            var bodyTemplate = Utilities.GetSharedResource("Notification_Mention.Body");
            var mentionType = Utilities.GetSharedResource("Notification_MentionType_" + type);
            var notificationType = DotNetNuke.Services.Social.Notifications.NotificationsController.Instance.GetNotificationType("JournalMention");

            foreach (var key in mentionedUsers.Keys)
            {
                var mentionUser = mentionedUsers[key];
                var mentionText = originalSummary.Substring(originalSummary.IndexOf(MentionIdentityChar + key, StringComparison.InvariantCultureIgnoreCase));
                if (mentionText.Length > MentionNotificationLength)
                {
                    mentionText = mentionText.Substring(0, MentionNotificationLength) + MentionNotificationSuffix;
                }

                var notification = new Notification
                {
                    Subject = string.Format(subjectTemplate, this.UserInfo.DisplayName, mentionType),
                    Body = string.Format(bodyTemplate, mentionText),
                    NotificationTypeID = notificationType.NotificationTypeId,
                    SenderUserID = this.UserInfo.UserID,
                    IncludeDismissAction = true,
                    Context = string.Format("{0}_{1}", this.UserInfo.UserID, item.JournalId),
                };

                Services.Social.Notifications.NotificationsController.Instance.SendNotification(notification, this.PortalSettings.PortalId, null, new List<UserInfo> { mentionUser });
            }
        }

        private bool IsCurrentUserFile(IFileInfo file)
        {
            if (file == null)
            {
                return false;
            }

            var userFolders = this.GetUserFolders();

            return userFolders.Any(f => file.FolderId == f.FolderID);
        }

        private IList<IFolderInfo> GetUserFolders()
        {
            var folders = new List<IFolderInfo>();

            var userFolder = FolderManager.Instance.GetUserFolder(this.UserInfo);
            folders.Add(userFolder);
            folders.AddRange(this.GetSubFolders(userFolder));

            return folders;
        }

        private IList<IFolderInfo> GetSubFolders(IFolderInfo parentFolder)
        {
            var folders = new List<IFolderInfo>();
            foreach (var folder in FolderManager.Instance.GetFolders(parentFolder))
            {
                folders.Add(folder);
                folders.AddRange(this.GetSubFolders(folder));
            }

            return folders;
        }

        public class CreateDTO
        {
            public string Text { get; set; }

            public int ProfileId { get; set; }

            public string JournalType { get; set; }

            public string ItemData { get; set; }

            public string SecuritySet { get; set; }

            public int GroupId { get; set; }

            public IList<MentionDTO> Mentions { get; set; }
        }

        public class MentionDTO
        {
            public string DisplayName { get; set; }

            public int UserId { get; set; }
        }

        public class JournalIdDTO
        {
            public int JournalId { get; set; }
        }

        public class PreviewDTO
        {
            public string Url { get; set; }
        }

        public class GetListForProfileDTO
        {
            public int ProfileId { get; set; }

            public int GroupId { get; set; }

            public int RowIndex { get; set; }

            public int MaxRows { get; set; }
        }

        public class CommentSaveDTO
        {
            public int JournalId { get; set; }

            public string Comment { get; set; }

            public IList<MentionDTO> Mentions { get; set; }
        }

        public class CommentDeleteDTO
        {
            public int JournalId { get; set; }

            public int CommentId { get; set; }
        }

        public class SuggestDTO
        {
            public string displayName { get; set; }

            public int userId { get; set; }

            public string avatar { get; set; }

            public string key { get; set; }
        }
    }
}
