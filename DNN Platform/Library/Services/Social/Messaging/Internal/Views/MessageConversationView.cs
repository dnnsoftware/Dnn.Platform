// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Data;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging.Views
    /// Class:      MessageConversationView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageConversationView class contains details of the latest message in a Conversation.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageConversationView : IHydratable
    {
        private int _messageID = -1;
        private string _displayDate;
        private DateTime _createdOnDate;

        /// <summary>
        /// MessageID - The primary key
        /// </summary>
        public int MessageID
        {
            get
            {
                return this._messageID;
            }
            set
            {
                this._messageID = value;
            }
        }

        /// <summary>
        /// portalID for the message
        /// </summary>
        public int PortalID { get; set; }

        /// <summary>
        /// To list for the message. This information is saved for faster display of To list in the message
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// Message From
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Message Subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Message body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Conversation ID of the Message. Each message has at least one ConversationId. Subsequent Replies to a Message get same ConversationId
        /// </summary>
        public int ConversationId { get; set; }

        /// <summary>
        /// ReplyAllAllowed is a bit value to indicate if the reply to the message can be sent to all the recipients or just the sender
        /// </summary>

        public bool ReplyAllAllowed { get; set; }

        /// <summary>
        /// The UserID of the sender of the message
        /// </summary>
        public int SenderUserID { get; set; }

        /// <summary>
        /// A pretty printed string with the time since the message was created
        /// </summary>
        public string DisplayDate
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayDate))
                {
                    this._displayDate = DateUtils.CalculateDateForDisplay(this._createdOnDate);
                }
                return this._displayDate;
            }
        }

        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        public int KeyID
        {
            get
            {
                return this.MessageID;
            }
            set
            {
                this.MessageID = value;
            }
        }

        /// <summary>
        /// RowNumber of the message in a set
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// Count of Total Attachments in a Conversation. It is calculated by adding attachments in all the threads for a given conversation.
        /// </summary>
        public int AttachmentCount { get; set; }

        /// <summary>
        /// Count of Total New (Unread) Threads in a Conversation. It is calculated by inspecting all the threads in a conversation and counting the ones that are not read yet.
        /// </summary>
        public int NewThreadCount { get; set; }

        /// <summary>
        /// Count of Total Threads in a Conversation.
        /// </summary>
        public int ThreadCount { get; set; }

        /// <summary>
        /// The Sender User Profile URL
        /// </summary>
        public string SenderProfileUrl
        {
            get
            {
                return Globals.UserProfileURL(this.SenderUserID);
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.MessageID = Convert.ToInt32(dr["MessageID"]);
            this.To = Null.SetNullString(dr["To"]);
            this.From = Null.SetNullString(dr["From"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.Body = Null.SetNullString(dr["Body"]);
            this.ConversationId = Null.SetNullInteger(dr["ConversationID"]);
            this.ReplyAllAllowed = Null.SetNullBoolean(dr["ReplyAllAllowed"]);
            this.SenderUserID = Convert.ToInt32(dr["SenderUserID"]);
            this.RowNumber = Convert.ToInt32(dr["RowNumber"]);
            this.AttachmentCount = Convert.ToInt32(dr["AttachmentCount"]);
            this.NewThreadCount = Convert.ToInt32(dr["NewThreadCount"]);
            this.ThreadCount = Convert.ToInt32(dr["ThreadCount"]);
            this._createdOnDate = Null.SetNullDateTime(dr["CreatedOnDate"]);
        }
    }
}
