// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Portals.Internal;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Social.Messaging.Data;
    using DotNetNuke.Services.Social.Messaging.Exceptions;
    using DotNetNuke.Services.Social.Messaging.Internal;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Controller class for social Messaging.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class MessagingController
                                : ServiceLocator<IMessagingController, MessagingController>,
                                IMessagingController
    {
        internal const int ConstMaxTo = 2000;
        internal const int ConstMaxSubject = 400;
        internal const int ConstDefaultPageIndex = 0;
        internal const int ConstDefaultPageSize = 10;
        internal const string ConstSortColumnDate = "CreatedOnDate";
        internal const string ConstSortColumnFrom = "From";
        internal const string ConstSortColumnSubject = "Subject";
        internal const bool ConstAscending = true;
        internal const double DefaultMessagingThrottlingInterval = 0.5; // default MessagingThrottlingInterval set to 30 seconds.

        private readonly IDataService _dataService;

        public MessagingController()
            : this(DataService.Instance)
        {
        }

        public MessagingController(IDataService dataService)
        {
            // Argument Contract
            Requires.NotNull("dataService", dataService);

            this._dataService = dataService;
        }

        public virtual void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs)
        {
            this.SendMessage(message, roles, users, fileIDs, UserController.Instance.GetCurrentUserInfo());
        }

        public virtual void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender)
        {
            if (sender == null || sender.UserID <= 0)
            {
                throw new ArgumentException(Localization.GetString("MsgSenderRequiredError", Localization.ExceptionsResourceFile));
            }

            if (message == null)
            {
                throw new ArgumentException(Localization.GetString("MsgMessageRequiredError", Localization.ExceptionsResourceFile));
            }

            if (string.IsNullOrEmpty(message.Subject) && string.IsNullOrEmpty(message.Body))
            {
                throw new ArgumentException(Localization.GetString("MsgSubjectOrBodyRequiredError", Localization.ExceptionsResourceFile));
            }

            if (roles == null && users == null)
            {
                throw new ArgumentException(Localization.GetString("MsgRolesOrUsersRequiredError", Localization.ExceptionsResourceFile));
            }

            if (InternalMessagingController.Instance.DisablePrivateMessage(sender.PortalID) && !this.IsAdminOrHost(sender))
            {
                throw new ArgumentException(Localization.GetString("PrivateMessageDisabledError", Localization.ExceptionsResourceFile));
            }

            if (!string.IsNullOrEmpty(message.Subject) && message.Subject.Length > ConstMaxSubject)
            {
                throw new ArgumentException(string.Format(Localization.GetString("MsgSubjectTooBigError", Localization.ExceptionsResourceFile), ConstMaxSubject, message.Subject.Length));
            }

            if (roles != null && roles.Count > 0 && !this.IsAdminOrHost(sender))
            {
                if (!roles.All(role => sender.Social.Roles.Any(userRoleInfo => role.RoleID == userRoleInfo.RoleID && userRoleInfo.IsOwner)))
                {
                    throw new ArgumentException(Localization.GetString("MsgOnlyHostOrAdminOrUserInGroupCanSendToRoleError", Localization.ExceptionsResourceFile));
                }
            }

            var sbTo = new StringBuilder();
            var replyAllAllowed = true;
            if (roles != null)
            {
                foreach (var role in roles.Where(role => !string.IsNullOrEmpty(role.RoleName)))
                {
                    sbTo.Append(role.RoleName + ",");
                    replyAllAllowed = false;
                }
            }

            if (users != null)
            {
                foreach (var user in users.Where(user => !string.IsNullOrEmpty(user.DisplayName)))
                {
                    sbTo.Append(user.DisplayName + ",");
                }
            }

            if (sbTo.Length == 0)
            {
                throw new ArgumentException(Localization.GetString("MsgEmptyToListFoundError", Localization.ExceptionsResourceFile));
            }

            if (sbTo.Length > ConstMaxTo)
            {
                throw new ArgumentException(string.Format(Localization.GetString("MsgToListTooBigError", Localization.ExceptionsResourceFile), ConstMaxTo, sbTo.Length));
            }

            // Cannot send message if within ThrottlingInterval
            var waitTime = InternalMessagingController.Instance.WaitTimeForNextMessage(sender);
            if (waitTime > 0)
            {
                var interval = this.GetPortalSettingAsDouble("MessagingThrottlingInterval", sender.PortalID, DefaultMessagingThrottlingInterval);
                throw new ThrottlingIntervalNotMetException(string.Format(Localization.GetString("MsgThrottlingIntervalNotMet", Localization.ExceptionsResourceFile), interval));
            }

            // Cannot have attachments if it's not enabled
            if (fileIDs != null && fileIDs.Count > 0 && !InternalMessagingController.Instance.AttachmentsAllowed(sender.PortalID))
            {
                throw new AttachmentsNotAllowed(Localization.GetString("MsgAttachmentsNotAllowed", Localization.ExceptionsResourceFile));
            }

            // Cannot exceed RecipientLimit
            var recipientCount = 0;
            if (users != null)
            {
                recipientCount += users.Count;
            }

            if (roles != null)
            {
                recipientCount += roles.Count;
            }

            if (recipientCount > InternalMessagingController.Instance.RecipientLimit(sender.PortalID))
            {
                throw new RecipientLimitExceededException(Localization.GetString("MsgRecipientLimitExceeded", Localization.ExceptionsResourceFile));
            }

            // Profanity Filter
            var profanityFilterSetting = this.GetPortalSetting("MessagingProfanityFilters", sender.PortalID, "NO");
            if (profanityFilterSetting.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                message.Subject = this.InputFilter(message.Subject);
                message.Body = this.InputFilter(message.Body);
            }

            message.To = sbTo.ToString().Trim(',');
            message.MessageID = Null.NullInteger;
            message.ReplyAllAllowed = replyAllAllowed;
            message.SenderUserID = sender.UserID;
            message.From = sender.DisplayName;

            message.MessageID = this._dataService.SaveMessage(message, PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID), UserController.Instance.GetCurrentUserInfo().UserID);

            // associate attachments
            if (fileIDs != null)
            {
                foreach (var attachment in fileIDs.Select(fileId => new MessageAttachment { MessageAttachmentID = Null.NullInteger, FileID = fileId, MessageID = message.MessageID }))
                {
                    if (this.CanViewFile(attachment.FileID))
                    {
                        this._dataService.SaveMessageAttachment(attachment, UserController.Instance.GetCurrentUserInfo().UserID);
                    }
                }
            }

            // send message to Roles
            if (roles != null)
            {
                var roleIds = string.Empty;
                roleIds = roles
                    .Select(r => r.RoleID)
                    .Aggregate(roleIds, (current, roleId) => current + (roleId + ","))
                    .Trim(',');

                this._dataService.CreateMessageRecipientsForRole(message.MessageID, roleIds, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            // send message to each User - this should be called after CreateMessageRecipientsForRole.
            if (users == null)
            {
                users = new List<UserInfo>();
            }

            foreach (var recipient in from user in users where InternalMessagingController.Instance.GetMessageRecipient(message.MessageID, user.UserID) == null select new MessageRecipient { MessageID = message.MessageID, UserID = user.UserID, Read = false, RecipientID = Null.NullInteger })
            {
                this._dataService.SaveMessageRecipient(recipient, UserController.Instance.GetCurrentUserInfo().UserID);
            }

            if (users.All(u => u.UserID != sender.UserID))
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(message.MessageID, sender.UserID);

                if (recipient == null)
                {
                    // add sender as a recipient of the message
                    recipient = new MessageRecipient { MessageID = message.MessageID, UserID = sender.UserID, Read = false, RecipientID = Null.NullInteger };
                    recipient.RecipientID = this._dataService.SaveMessageRecipient(recipient, UserController.Instance.GetCurrentUserInfo().UserID);
                }

                InternalMessagingController.Instance.MarkMessageAsDispatched(message.MessageID, recipient.RecipientID);
            }

            // Mark the conversation as read by the sender of the message.
            InternalMessagingController.Instance.MarkRead(message.MessageID, sender.UserID);
        }

        internal virtual UserInfo GetCurrentUserInfo()
        {
            return UserController.Instance.GetCurrentUserInfo();
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual int GetPortalSettingAsInteger(string key, int portalId, int defaultValue)
        {
            return PortalController.GetPortalSettingAsInteger(key, portalId, defaultValue);
        }

        internal virtual double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
        {
            return PortalController.GetPortalSettingAsDouble(key, portalId, defaultValue);
        }

        internal virtual string InputFilter(string input)
        {
            var ps = PortalSecurity.Instance;
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }

        internal virtual bool IsAdminOrHost(UserInfo userInfo)
        {
            return userInfo.IsSuperUser || userInfo.IsInRole(PortalController.Instance.GetCurrentPortalSettings().AdministratorRoleName);
        }

        protected override Func<IMessagingController> GetFactory()
        {
            return () => new MessagingController();
        }

        private bool CanViewFile(int fileId)
        {
            var file = FileManager.Instance.GetFile(fileId);
            if (file == null)
            {
                return false;
            }

            var folder = FolderManager.Instance.GetFolder(file.FolderId);
            return folder != null && FolderPermissionController.Instance.CanViewFolder(folder);
        }
    }
}
