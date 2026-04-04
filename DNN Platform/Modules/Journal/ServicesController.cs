// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using DotNetNuke.Abstractions.Application;
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
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>A web API controller for the Journal module.</summary>
/// <param name="hostSettings">The host settings.</param>
[DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
[SupportedModules("Journal")]
public class ServicesController(IHostSettings hostSettings)
    : DnnApiController
{
    private const string MentionNotificationSuffix = "...";
    private const string MentionIdentityChar = "@";
    private const int MentionNotificationLength = 100;

    private static readonly ILogger Logger = DnnLoggingController.GetLogger<ServicesController>();
    private static readonly string[] AcceptedFileExtensions = ["jpg", "png", "gif", "jpe", "jpeg", "tiff", "bmp",];

    private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

    /// <summary>Initializes a new instance of the <see cref="ServicesController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public ServicesController()
        : this(null)
    {
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage Create(CreateDTO postData)
    {
        try
        {
            var userId = this.UserInfo.UserID;
            IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();

            if (postData.ProfileId == -1)
            {
                postData.ProfileId = userId;
            }

            this.CheckProfileAccess(postData.ProfileId, this.UserInfo);

            this.CheckGroupAccess(postData);

            var journalItem = this.PrepareJournalItem(postData, mentionedUsers);

            JournalController.Instance.SaveJournalItem(journalItem, this.ActiveModule);

            var originalSummary = journalItem.Summary;
            this.SendMentionNotifications(mentionedUsers, journalItem, originalSummary);

            return this.Request.CreateResponse(HttpStatusCode.OK, journalItem);
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerCreateException(exc);
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
            var journalController = JournalController.Instance;
            var journalItem = journalController.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);

            if (journalItem == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
            }

            if (journalItem.UserId != this.UserInfo.UserID &&
                journalItem.ProfileId != this.UserInfo.UserID &&
                !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            journalController.DeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerDeleteException(exc);
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
            var journalController = JournalController.Instance;
            var journalItem = journalController.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
            if (journalItem is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
            }

            if (journalItem.UserId != this.UserInfo.UserID &&
                journalItem.ProfileId != this.UserInfo.UserID &&
                !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            journalController.SoftDeleteJournalItem(this.PortalSettings.PortalId, this.UserInfo.UserID, postData.JournalId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerSoftDeleteException(exc);
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
            return this.Request.CreateResponse(HttpStatusCode.OK, Utilities.GetLinkData(postData.Url));
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerPreviewUrlException(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage GetListForProfile(GetListForProfileDTO postData)
    {
        try
        {
            var journalParser = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, postData.ProfileId, postData.GroupId, this.UserInfo);
            return this.Request.CreateResponse(HttpStatusCode.OK, journalParser.GetList(postData.RowIndex, postData.MaxRows), "text/html");
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerGetListForProfileException(exc);
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
            var journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
            if (journalItem is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            JournalController.Instance.LikeJournalItem(postData.JournalId, this.UserInfo.UserID, this.UserInfo.DisplayName);

            journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
            var journalParser = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, journalItem.ProfileId, -1, this.UserInfo);
            var isLiked = false;
            var likeList = journalParser.GetLikeListHTML(journalItem, ref isLiked);
            likeList = Utilities.LocalizeControl(likeList);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { LikeList = likeList, Liked = isLiked, });
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerLikeException(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    /// <summary>Posts a comment to a journal item.</summary>
    /// <param name="postData">The information about the comment to post.</param>
    /// <returns>A <see cref="HttpResponseMessage"/> indicating the result of the operation.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage CommentSave(CommentSaveDTO postData)
    {
        try
        {
            var journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId, false, false, true);
            if (journalItem == null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            var comment = Utilities.RemoveHTML(HttpUtility.UrlDecode(postData.Comment));

            IDictionary<string, UserInfo> mentionedUsers = new Dictionary<string, UserInfo>();
            var originalComment = comment;
            comment = this.ParseMentions(comment, postData.Mentions, ref mentionedUsers);
            var commentInfo = new CommentInfo
            {
                JournalId = postData.JournalId,
                Comment = comment,
                UserId = this.UserInfo.UserID,
                DisplayName = this.UserInfo.DisplayName,
            };
            JournalController.Instance.SaveComment(commentInfo);

            var jp = new JournalParser(this.PortalSettings, this.ActiveModule.ModuleID, journalItem.ProfileId, -1, this.UserInfo);

            this.SendMentionNotifications(mentionedUsers, journalItem, originalComment, "Comment");

            return this.Request.CreateResponse(HttpStatusCode.OK, jp.GetCommentRow(journalItem, commentInfo), "text/html");
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerCommentSaveException(exc);
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
            var commentInfo = JournalController.Instance.GetComment(postData.CommentId);
            if (commentInfo is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "delete failed");
            }

            var journalItem = JournalController.Instance.GetJournalItem(this.ActiveModule.OwnerPortalID, this.UserInfo.UserID, postData.JournalId);
            if (journalItem is null)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "invalid request");
            }

            if (commentInfo.UserId != this.UserInfo.UserID &&
                journalItem.UserId != this.UserInfo.UserID &&
                !this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "access denied");
            }

            JournalController.Instance.DeleteComment(postData.JournalId, postData.CommentId);
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerCommentDeleteException(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpGet]
    [DnnAuthorize(DenyRoles = "Unverified Users")]
    public HttpResponseMessage GetSuggestions(string keyword)
    {
        try
        {
            var foundUsers = new List<SuggestDTO>();
            var relations = RelationshipController.Instance.GetUserRelationships(this.UserInfo);
            foreach (var ur in relations)
            {
                var targetUserId = ur.UserId == this.UserInfo.UserID ? ur.RelatedUserId : ur.UserId;
                var targetUser = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, targetUserId);
                var relationship = RelationshipController.Instance.GetRelationship(ur.RelationshipId);
                if (ur.Status == RelationshipStatus.Accepted && targetUser != null
                                                             && ((relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Followers && ur.RelatedUserId == this.UserInfo.UserID)
                                                                 || relationship.RelationshipTypeId == (int)DefaultRelationshipTypes.Friends)
                                                             && (targetUser.DisplayName.ToLowerInvariant().Contains(keyword.ToLowerInvariant())
                                                                 || targetUser.DisplayName.ToLowerInvariant().Contains(keyword.Replace("-", " ").ToLowerInvariant()))
                                                             && foundUsers.All(s => s.userId != targetUser.UserID))
                {
                    foundUsers.Add(new SuggestDTO
                    {
                        displayName = targetUser.DisplayName.Replace(" ", "-"),
                        userId = targetUser.UserID,
                        avatar = targetUser.Profile.PhotoURL,
                        key = keyword,
                    });
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, foundUsers.Cast<object>().Take(5));
        }
        catch (Exception exc)
        {
            Logger.ServicesControllerGetSuggestionsException(exc);
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

        var lastDotIndex = relativePath.LastIndexOf(".", StringComparison.Ordinal);
        var extension = relativePath.Substring(lastDotIndex + 1).ToLowerInvariant();
        return AcceptedFileExtensions.Contains(extension);
    }

    private static bool IsAllowedLink(string url)
    {
        return !string.IsNullOrEmpty(url) && !url.Contains("//");
    }

    // Check if a user can post content on a specific profile's page
    private void CheckProfileAccess(int profileId, UserInfo currentUser)
    {
        if (profileId == currentUser.UserID)
        {
            return;
        }

        var profileUser = UserController.Instance.GetUser(this.PortalSettings.PortalId, profileId);
        if (profileUser == null || (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !Utilities.AreFriends(profileUser, currentUser)))
        {
            throw new ArgumentException("you have no permission to post journal on current profile page.");
        }
    }

    private void CheckGroupAccess(CreateDTO postData)
    {
        if (postData.GroupId <= 0)
        {
            return;
        }

        postData.ProfileId = -1;

        var roleInfo = RoleController.Instance.GetRoleById(this.ActiveModule.OwnerPortalID, postData.GroupId);
        if (roleInfo == null)
        {
            return;
        }

        if (!this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName) && !this.UserInfo.IsInRole(roleInfo.RoleName))
        {
            throw new ArgumentException("you have no permission to post journal on current group.");
        }

        if (!roleInfo.IsPublic)
        {
            postData.SecuritySet = "R";
        }
    }

    private JournalItem PrepareJournalItem(CreateDTO postData, IDictionary<string, UserInfo> mentionedUsers)
    {
        var journalItem = new JournalItem
        {
            JournalId = -1,
            JournalTypeId = postData.JournalType switch
            {
                "link" => 2,
                "photo" => 3,
                "file" => 4,
                _ => 1,
            },
            PortalId = this.ActiveModule.OwnerPortalID,
            UserId = this.UserInfo.UserID,
            SocialGroupId = postData.GroupId,
            ProfileId = postData.ProfileId,
            Summary = postData.Text ?? string.Empty,
            SecuritySet = postData.SecuritySet,
        };
        journalItem.Title = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(journalItem.Title));
        journalItem.Summary = HttpUtility.HtmlDecode(HttpUtility.UrlDecode(journalItem.Summary));

        var ps = PortalSecurity.Instance;

#pragma warning disable CS0618 // Type or member is obsolete
        journalItem.Title = ps.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoScripting);
        journalItem.Title = Utilities.RemoveHTML(journalItem.Title);
        journalItem.Title = ps.InputFilter(journalItem.Title, PortalSecurity.FilterFlag.NoMarkup);

        journalItem.Summary = ps.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoScripting);
        journalItem.Summary = Utilities.RemoveHTML(journalItem.Summary);
        journalItem.Summary = ps.InputFilter(journalItem.Summary, PortalSecurity.FilterFlag.NoMarkup);
#pragma warning restore CS0618 // Type or member is obsolete

        // parse the mentions context in post data
        journalItem.Summary = this.ParseMentions(journalItem.Summary, postData.Mentions, ref mentionedUsers);

        if (journalItem.Summary.Length > 2000)
        {
            journalItem.Body = journalItem.Summary;
            journalItem.Summary = null;
        }

        if (string.IsNullOrEmpty(postData.ItemData))
        {
            return journalItem;
        }

        journalItem.ItemData = postData.ItemData.FromJson<ItemData>();
        var originalImageUrl = journalItem.ItemData.ImageUrl;
        if (!IsImageFile(journalItem.ItemData.ImageUrl))
        {
            journalItem.ItemData.ImageUrl = string.Empty;
        }

        journalItem.ItemData.Description = HttpUtility.UrlDecode(journalItem.ItemData.Description);

        if (!IsAllowedLink(journalItem.ItemData.Url))
        {
            journalItem.ItemData.Url = string.Empty;
        }

        if (string.IsNullOrEmpty(journalItem.ItemData.Url) || !journalItem.ItemData.Url.StartsWith("fileid="))
        {
            return journalItem;
        }

        var fileId = Convert.ToInt32(journalItem.ItemData.Url.Replace("fileid=", string.Empty).Trim());
        var file = FileManager.Instance.GetFile(fileId);

        if (!this.IsCurrentUserFile(file))
        {
            throw new ArgumentException("you have no permission to attach files not belongs to you.");
        }

        journalItem.ItemData.Title = file.FileName;
        journalItem.ItemData.Url = Globals.LinkClick(journalItem.ItemData.Url, Null.NullInteger, Null.NullInteger);

        if (string.IsNullOrEmpty(journalItem.ItemData.ImageUrl) &&
            originalImageUrl.ToLowerInvariant().StartsWith("/linkclick.aspx?") &&
            AcceptedFileExtensions.Contains(file.Extension.ToLowerInvariant()))
        {
            journalItem.ItemData.ImageUrl = originalImageUrl;
        }

        return journalItem;
    }

    private string ParseMentions(string content, IList<MentionDTO> mentions, ref IDictionary<string, UserInfo> mentionedUsers)
    {
        if (mentions == null || mentions.Count == 0)
        {
            return content;
        }

        foreach (var mention in mentions)
        {
            var user = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, mention.UserId);

            if (user == null)
            {
                continue;
            }

            var relationship = RelationshipController.Instance.GetFollowingRelationship(this.UserInfo, user) ??
                               RelationshipController.Instance.GetFriendRelationship(this.UserInfo, user);
            if (relationship is not { Status: RelationshipStatus.Accepted, })
            {
                continue;
            }

            var userLink =
                $"""<a href="{Globals.UserProfileURL(user.UserID)}" class="userLink" target="_blank">{MentionIdentityChar}{user.DisplayName}</a>""";
            content = content.Replace(MentionIdentityChar + mention.DisplayName, userLink);

            mentionedUsers.Add(mention.DisplayName, user);
        }

        return content;
    }

    private void SendMentionNotifications(IDictionary<string, UserInfo> mentionedUsers, JournalItem item, string originalSummary, string type = "Post")
    {
        // send notification to the mention users
        var subjectTemplate = Utilities.GetSharedResource("Notification_Mention.Subject");
        var bodyTemplate = Utilities.GetSharedResource("Notification_Mention.Body");
        var mentionType = Utilities.GetSharedResource("Notification_MentionType_" + type);
        var notificationType = NotificationsController.Instance.GetNotificationType("JournalMention");

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
                Context = $"{this.UserInfo.UserID}_{item.JournalId}",
            };

            NotificationsController.Instance.SendNotification(notification, this.PortalSettings.PortalId, null, [mentionUser,]);
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
        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public string displayName { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public int userId { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public string avatar { get; set; }

        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Breaking Change")]
        public string key { get; set; }

        // ReSharper restore InconsistentNaming
    }
}
