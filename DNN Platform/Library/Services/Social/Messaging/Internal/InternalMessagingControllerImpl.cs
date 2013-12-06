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
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Portals.Internal;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Services.Social.Messaging.Data;
using DotNetNuke.Services.Social.Messaging.Exceptions;
using DotNetNuke.Services.Social.Messaging.Internal.Views;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Internal
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
    internal class InternalMessagingControllerImpl : IInternalMessagingController
    {
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

        public InternalMessagingControllerImpl()
            : this(DataService.Instance)
        {
        }

        public InternalMessagingControllerImpl(IDataService dataService)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);

            _dataService = dataService;
        }

        #endregion

        #region CRUD APIs

        public virtual void DeleteMessageRecipient(int messageId, int userId)
        {
            _dataService.DeleteMessageRecipientByMessageAndUser(messageId, userId);
        }

        public virtual Message GetMessage(int messageId)
        {
            return CBO.FillObject<Message>(_dataService.GetMessage(messageId));
        }

        public virtual MessageRecipient GetMessageRecipient(int messageId, int userId)
        {
            return CBO.FillObject<MessageRecipient>(_dataService.GetMessageRecipientByMessageAndUser(messageId, userId));
        }

        public virtual IList<MessageRecipient> GetMessageRecipients(int messageId)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetMessageRecipientsByMessage(messageId));
        }

        public virtual void MarkArchived(int conversationId, int userId)
        {
            _dataService.UpdateMessageArchivedStatus(conversationId, userId, true);
        }

        public virtual void MarkRead(int conversationId, int userId)
        {
            _dataService.UpdateMessageReadStatus(conversationId, userId, true);
        }

        public virtual void MarkUnArchived(int conversationId, int userId)
        {
            _dataService.UpdateMessageArchivedStatus(conversationId, userId, false);
        }

        public virtual void MarkUnRead(int conversationId, int userId)
        {
            _dataService.UpdateMessageReadStatus(conversationId, userId, false);
        }

        #endregion

        #region Reply APIs

        public virtual int ReplyMessage(int conversationId, string body, IList<int> fileIDs)
        {
            return ReplyMessage(conversationId, body, fileIDs, UserController.GetCurrentUserInfo());
        }

        public virtual int ReplyMessage(int conversationId, string body, IList<int> fileIDs, UserInfo sender)
        {
            if (sender == null || sender.UserID <= 0)
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgSenderRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentException(Localization.Localization.GetString("MsgBodyRequiredError", Localization.Localization.ExceptionsResourceFile));
            }

            //Cannot have attachments if it's not enabled
            if (fileIDs != null && !InternalMessagingController.Instance.AttachmentsAllowed(sender.PortalID))
            {
                throw new AttachmentsNotAllowed(Localization.Localization.GetString("MsgAttachmentsNotAllowed", Localization.Localization.ExceptionsResourceFile));
            }

            //Profanity Filter
            var profanityFilterSetting = GetPortalSetting("MessagingProfanityFilters", sender.PortalID, "NO");
            if (profanityFilterSetting.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                body = InputFilter(body);
            }

            //call ReplyMessage
            var messageId = _dataService.CreateMessageReply(conversationId, PortalController.GetEffectivePortalId(sender.PortalID), body, sender.UserID, sender.DisplayName, GetCurrentUserInfo().UserID);
            if (messageId == -1) //Parent message was not found or Recipient was not found in the message
            {
                throw new MessageOrRecipientNotFoundException(Localization.Localization.GetString("MsgMessageOrRecipientNotFound", Localization.Localization.ExceptionsResourceFile));
            }

            //associate attachments
            if (fileIDs != null)
            {
                foreach (var attachment in fileIDs.Select(fileId => new MessageAttachment { MessageAttachmentID = Null.NullInteger, FileID = fileId, MessageID = messageId }))
                {
                    _dataService.SaveMessageAttachment(attachment, UserController.GetCurrentUserInfo().UserID);
                }
            }

            // Mark reply as read and dispatched by the sender
            var recipient = GetMessageRecipient(messageId, sender.UserID);
            
            MarkMessageAsDispatched(messageId, recipient.RecipientID);
            MarkRead(conversationId, sender.UserID);

            return messageId;
        }


        #endregion

        #region Admin Settings APIs

        /// <summary>How long a user needs to wait before sending the next message.</summary>
        /// <returns>Time in seconds. Returns zero if user is Host, Admin or has never sent a message.</returns>
        /// <param name="sender">Sender's UserInfo</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public virtual int WaitTimeForNextMessage(UserInfo sender)
        {
            Requires.NotNull("sender", sender);

            var waitTime = 0;
            // MessagingThrottlingInterval contains the number of MINUTES to wait before sending the next message
            var interval = GetPortalSettingAsInteger("MessagingThrottlingInterval", sender.PortalID, Null.NullInteger) * 60;
            if (interval > 0 && !IsAdminOrHost(sender))
            {
                var lastSentMessage = GetLastSentMessage(sender);
                if (lastSentMessage != null)
                {
                    waitTime = (int)(interval - GetDateTimeNow().Subtract(lastSentMessage.CreatedOnDate).TotalSeconds);
                }
            }
            return waitTime < 0 ? 0 : waitTime;
        }

        ///<summary>Last message sent by the User</summary>
        ///<returns>Message. Null when no message was sent</returns>
        /// <param name="sender">Sender's UserInfo</param>        
        public virtual Message GetLastSentMessage(UserInfo sender)
        {
            return CBO.FillObject<Message>(_dataService.GetLastSentMessage(sender.UserID, PortalController.GetEffectivePortalId(sender.PortalID)));
        }

        ///<summary>Are attachments allowed</summary>        
        ///<returns>True or False</returns>
        /// <param name="portalId">Portal Id</param>               
        public virtual bool AttachmentsAllowed(int portalId)
        {
            return GetPortalSetting("MessagingAllowAttachments", portalId, "YES") == "YES";
        }

        ///<summary>Maximum number of Recipients allowed</summary>        
        ///<returns>Count. Message to a Role is considered a single Recipient. Each User in the To list is counted as one User each.</returns>
        /// <param name="portalId">Portal Id</param>        
        public virtual int RecipientLimit(int portalId)
        {
            return GetPortalSettingAsInteger("MessagingRecipientLimit", portalId, 5);
        }

        #endregion

        #region Get View APIs

        public virtual MessageBoxView GetArchivedMessages(int userId, int afterMessageId, int numberOfRecords)
        {
            var reader = _dataService.GetArchiveBoxView(userId, PortalController.GetEffectivePortalId(GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
            return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
        }

        public virtual MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
            return GetInbox(userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, MessageReadStatus.Any, MessageArchivedStatus.UnArchived);
        }

        public virtual MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus)
        {
            var reader = _dataService.GetInBoxView(userId, PortalController.GetEffectivePortalId(GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, MessageSentStatus.Received);
            return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };         
        }

        public virtual MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, ref int totalRecords)
        {
            return GetMessageThread(conversationId, userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending, ref totalRecords);
        }

        public virtual MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, ref int totalRecords)
        {
            var messageThreadsView = new MessageThreadsView();

            var dr = _dataService.GetMessageThread(conversationId, userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, ref totalRecords);

            try
            {
                while (dr.Read())
                {
                    var messageThreadView = new MessageThreadView { Conversation = new MessageConversationView() };
                    messageThreadView.Conversation.Fill(dr);

                    if (messageThreadView.Conversation.AttachmentCount > 0)
                    {
                        messageThreadView.Attachments = _dataService.GetMessageAttachmentsByMessage(messageThreadView.Conversation.MessageID);
                    }

                    if (messageThreadsView.Conversations == null) messageThreadsView.Conversations = new List<MessageThreadView>();

                    messageThreadsView.Conversations.Add(messageThreadView);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return messageThreadsView;            
        }

        public virtual MessageBoxView GetRecentInbox(int userId)
        {
            return GetRecentInbox(userId, ConstDefaultPageIndex, ConstDefaultPageSize);
        }

        public virtual MessageBoxView GetRecentInbox(int userId, int afterMessageId, int numberOfRecords)
        {
            return GetInbox(userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
        }

        public virtual MessageBoxView GetRecentSentbox(int userId)
        {
            return GetRecentSentbox(userId, ConstDefaultPageIndex, ConstDefaultPageSize);
        }

        public virtual MessageBoxView GetRecentSentbox(int userId, int afterMessageId, int numberOfRecords)
        {
            return GetSentbox(userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
        }

        public virtual MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
            return GetSentbox(userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, MessageReadStatus.Any, MessageArchivedStatus.UnArchived);
        }

        public virtual MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus)
        {
            var reader = _dataService.GetSentBoxView(userId, PortalController.GetEffectivePortalId(GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, sortColumn, sortAscending);
            return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
        }

        #endregion

        #region Counter APIs

        public virtual int CountArchivedMessagesByConversation(int conversationId)
        {
            return _dataService.CountArchivedMessagesByConversation(conversationId);
        }

        public virtual int CountMessagesByConversation(int conversationId)
        {
            return _dataService.CountMessagesByConversation(conversationId);
        }

        public virtual int CountConversations(int userId, int portalId)
        {
            return _dataService.CountTotalConversations(userId, portalId);
        }

        public virtual int CountUnreadMessages(int userId, int portalId)
        {
            return _dataService.CountNewThreads(userId, portalId);
        }

        public virtual int CountSentMessages(int userId, int portalId)
        {
            return _dataService.CountSentMessages(userId, portalId);
        }

        public virtual int CountArchivedMessages(int userId, int portalId)
        {
            return _dataService.CountArchivedMessages(userId, portalId);
        }
        
        #endregion

        #region Internal Methods

        internal virtual DateTime GetDateTimeNow()
        {
            return DateTime.UtcNow;
        }

        internal virtual UserInfo GetCurrentUserInfo()
        {
            return UserController.GetCurrentUserInfo();
        }

        internal virtual int GetPortalSettingAsInteger(string key, int portalId, int defaultValue)
        {
            return PortalController.GetPortalSettingAsInteger(key, portalId, defaultValue);
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual bool IsAdminOrHost(UserInfo userInfo)
        {
            return userInfo.IsSuperUser || userInfo.IsInRole(TestablePortalSettings.Instance.AdministratorRoleName);
        }

        internal virtual string InputFilter(string input)
        {
            var ps = new PortalSecurity();
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }

        #endregion

        #region Upgrade APIs

        public void ConvertLegacyMessages(int pageIndex, int pageSize)
        {
            _dataService.ConvertLegacyMessages(pageIndex, pageSize);
        }

        public int CountLegacyMessages()
        {
            var totalRecords = 0;
            var dr = _dataService.CountLegacyMessages();

            try
            {
                while (dr.Read())
                {
                    totalRecords = Convert.ToInt32(dr["TotalRecords"]);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return totalRecords;
        }

        #endregion

        #region Queued email API's

        public IList<MessageRecipient> GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetNextMessagesForInstantDispatch(schedulerInstance, batchSize));
        }

        public IList<MessageRecipient> GetNextMessagesForDigestDispatch(Frequency frequency, Guid schedulerInstance, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(_dataService.GetNextMessagesForDigestDispatch(Convert.ToInt32(frequency), schedulerInstance, batchSize));            
        }


        public virtual void MarkMessageAsDispatched(int messageId, int recipientId)
        {
            _dataService.MarkMessageAsDispatched(messageId, recipientId);
        }

        public virtual void MarkMessageAsSent(int messageId, int recipientId)
        {
            _dataService.MarkMessageAsSent(messageId, recipientId);
        }

        #endregion
    }
}
