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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Framework;
using DotNetNuke.Security;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Social.Messaging.Data;
using DotNetNuke.Services.Social.Messaging.Exceptions;
using DotNetNuke.Services.Social.Messaging.Internal;

#endregion

namespace DotNetNuke.Services.Social.Messaging
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Controller class for social Messaging
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// </history>
    /// -----------------------------------------------------------------------------
    public class MessagingController 
                                : ServiceLocator<IMessagingController, MessagingController>
                                , IMessagingController
    {
        protected override Func<IMessagingController> GetFactory()
        {
            return () => new MessagingController();
        }

        #region Constants

        internal const int ConstMaxTo = 2000;
        internal const int ConstMaxSubject = 400;
        internal const int ConstDefaultPageIndex = 0;
        internal const int ConstDefaultPageSize = 10;
        internal const string ConstSortColumnDate = "CreatedOnDate";
        internal const string ConstSortColumnFrom = "From";
        internal const string ConstSortColumnSubject = "Subject";
        internal const bool ConstAscending = true;

        #endregion

        #region Private Variables

        private readonly IDataService _dataService;

        #endregion

        #region Constructors

        public MessagingController() : this(DataService.Instance)
        {
        }

        public MessagingController(IDataService dataService)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);

            _dataService = dataService;
        }

        #endregion

        #region Public APIs

        public virtual void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs)
        {
            SendMessage(message, roles, users, fileIDs, UserController.GetCurrentUserInfo());
        }

        public virtual void SendMessage(Message message, IList<RoleInfo> roles, IList<UserInfo> users, IList<int> fileIDs, UserInfo sender)
        {
            if (sender == null || sender.UserID <= 0)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgSenderRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (message == null)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgMessageRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (string.IsNullOrEmpty(message.Subject) && string.IsNullOrEmpty(message.Body))
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgSubjectOrBodyRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (roles == null && users == null)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgRolesOrUsersRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (!string.IsNullOrEmpty(message.Subject) && message.Subject.Length > ConstMaxSubject)
            {
                throw new ArgumentException(string.Format(Localization.Localization.GetString("MsgSubjectTooBigError", Localization.Localization.ExceptionsResourceFile), ConstMaxSubject, message.Subject.Length));
            }

            if (roles != null && roles.Count > 0 && !IsAdminOrHost(sender))
            {
                if (!roles.All(role => sender.Social.Roles.Any(userRoleInfo => role.RoleID == userRoleInfo.RoleID)))
                {
                    throw new ArgumentException(Localization.Localization.GetString("MsgOnlyHostOrAdminOrUserInGroupCanSendToRoleError", Localization.Localization.ExceptionsResourceFile));
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
                foreach (var user in users.Where(user => !string.IsNullOrEmpty(user.DisplayName))) sbTo.Append(user.DisplayName + ",");
            }

            if (sbTo.Length == 0)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgEmptyToListFoundError", Localization.Localization.ExceptionsResourceFile));
            }
            
            if (sbTo.Length > ConstMaxTo)
            {
                throw new ArgumentException(string.Format(Localization.Localization.GetString("MsgToListTooBigError", Localization.Localization.ExceptionsResourceFile), ConstMaxTo, sbTo.Length));
            }

            //Cannot send message if within ThrottlingInterval
            var waitTime = InternalMessagingController.Instance.WaitTimeForNextMessage(sender);
            if (waitTime > 0)
            {
                var interval = GetPortalSettingAsInteger("MessagingThrottlingInterval", sender.PortalID, Null.NullInteger);
                throw new ThrottlingIntervalNotMetException(string.Format(Localization.Localization.GetString("MsgThrottlingIntervalNotMet", Localization.Localization.ExceptionsResourceFile), interval));
            }

            //Cannot have attachments if it's not enabled
            if (fileIDs != null && !InternalMessagingController.Instance.AttachmentsAllowed(sender.PortalID))
            {
                throw new AttachmentsNotAllowed(Localization.Localization.GetString("MsgAttachmentsNotAllowed", Localization.Localization.ExceptionsResourceFile));
            }

            //Cannot exceed RecipientLimit
            var recipientCount = 0;
            if (users != null) recipientCount += users.Count;
            if (roles != null) recipientCount += roles.Count;
            if (recipientCount > InternalMessagingController.Instance.RecipientLimit(sender.PortalID))
            {
                throw new RecipientLimitExceededException(Localization.Localization.GetString("MsgRecipientLimitExceeded", Localization.Localization.ExceptionsResourceFile));
            }

            //Profanity Filter
            var profanityFilterSetting = GetPortalSetting("MessagingProfanityFilters", sender.PortalID, "NO");
            if (profanityFilterSetting.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                message.Subject = InputFilter(message.Subject);
                message.Body = InputFilter(message.Body);
            }

            message.To = sbTo.ToString().Trim(',');
            message.MessageID = Null.NullInteger;
            message.ReplyAllAllowed = replyAllAllowed;
            message.SenderUserID = sender.UserID;
            message.From = sender.DisplayName;

            message.MessageID = _dataService.SaveMessage(message, PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID), UserController.GetCurrentUserInfo().UserID);

            //associate attachments
            if (fileIDs != null)
            {
                foreach (var attachment in fileIDs.Select(fileId => new MessageAttachment { MessageAttachmentID = Null.NullInteger, FileID = fileId, MessageID = message.MessageID }))
                {
                    _dataService.SaveMessageAttachment(attachment, UserController.GetCurrentUserInfo().UserID);
                }
            }

            //send message to Roles
            if (roles != null)
            {
                var roleIds = string.Empty;
                roleIds = roles
                    .Select(r => r.RoleID)
                    .Aggregate(roleIds, (current, roleId) => current + (roleId + ","))
                    .Trim(',');

                _dataService.CreateMessageRecipientsForRole(message.MessageID, roleIds, UserController.GetCurrentUserInfo().UserID);
            }

            //send message to each User - this should be called after CreateMessageRecipientsForRole.
            if (users == null)
            {
                users = new List<UserInfo>();
            }

            foreach (var recipient in from user in users where InternalMessagingController.Instance.GetMessageRecipient(message.MessageID, user.UserID) == null select new MessageRecipient { MessageID = message.MessageID, UserID = user.UserID, Read = false, RecipientID = Null.NullInteger })
            {
                _dataService.SaveMessageRecipient(recipient, UserController.GetCurrentUserInfo().UserID);
            }

            if (users.All(u => u.UserID != sender.UserID))
            {
                //add sender as a recipient of the message
                var recipientId = _dataService.SaveMessageRecipient(new MessageRecipient { MessageID = message.MessageID, UserID = sender.UserID, Read = false, RecipientID = Null.NullInteger }, UserController.GetCurrentUserInfo().UserID);
                InternalMessagingController.Instance.MarkMessageAsDispatched(message.MessageID, recipientId);
            }

            // Mark the conversation as read by the sender of the message.
            InternalMessagingController.Instance.MarkRead(message.MessageID, sender.UserID);
        }

        #endregion

        #region Internal Methods

        internal virtual UserInfo GetCurrentUserInfo()
        {
            return UserController.GetCurrentUserInfo();
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual int GetPortalSettingAsInteger(string key, int portalId, int defaultValue)
        {
            return PortalController.GetPortalSettingAsInteger(key, portalId, defaultValue);
        }

        internal virtual string InputFilter(string input)
        {
            var ps = new PortalSecurity();
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }

        internal virtual bool IsAdminOrHost(UserInfo userInfo)
        {
            return userInfo.IsSuperUser || userInfo.IsInRole(TestablePortalSettings.Instance.AdministratorRoleName);
        }

        #endregion
    }
}
