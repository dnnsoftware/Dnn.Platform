// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      Messages
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Message class describes the content of messages sent via the system
    /// As messaging is system wide process and there may be no portalID context (e.g. a host messaging "all users") the object does not tie to portalID.
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class Message : BaseEntityInfo, IHydratable
    {
        private int _messageID = -1;
        private string _displayDate;

        /// <summary>
        /// Gets a pretty printed string with the time since the message was created.
        /// </summary>
        [XmlAttribute]
        public string DisplayDate
        {
            get
            {
                if (string.IsNullOrEmpty(this._displayDate))
                {
                    this._displayDate = DateUtils.CalculateDateForDisplay(this.CreatedOnDate);
                }

                return this._displayDate;
            }
        }

        /// <summary>
        /// Gets or sets messageID - The primary key.
        /// </summary>
        [XmlAttribute]
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
        /// Gets or sets portalID for the message.
        /// </summary>
        [XmlAttribute]
        public int PortalID { get; set; }

        /// <summary>
        /// Gets or sets to list for the message. This information is saved for faster display of To list in the message.
        /// </summary>
        [XmlAttribute]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets message From.
        /// </summary>
        [XmlAttribute]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets message Subject.
        /// </summary>
        [XmlAttribute]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets message body.
        /// </summary>
        [XmlAttribute]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets conversation ID of the Message. Each message has at least one ConversationId. Subsequent Replies to a Message get same ConversationId.
        /// </summary>
        [XmlAttribute]
        public int ConversationId { get; set; }

        /// <summary>
        /// Gets or sets notificationType ID of the Message. Each message is associated with a notification type.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeID { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether replyAllAllowed is a bit value to indicate if the reply to the message can be sent to all the recipients or just the sender.
        /// </summary>
        [XmlAttribute]
        public bool ReplyAllAllowed { get; set; }

        /// <summary>
        /// Gets or sets the UserID of the sender of the message.
        /// </summary>
        [XmlAttribute]
        public int SenderUserID { get; set; }

        /// <summary>
        /// Gets or sets iHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
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
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            this.MessageID = Convert.ToInt32(dr["MessageID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalId"]);
            this.To = Null.SetNullString(dr["To"]);
            this.From = Null.SetNullString(dr["From"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.Body = Null.SetNullString(dr["Body"]);
            this.ConversationId = Null.SetNullInteger(dr["ConversationID"]);
            this.ReplyAllAllowed = Null.SetNullBoolean(dr["ReplyAllAllowed"]);
            this.SenderUserID = Convert.ToInt32(dr["SenderUserID"]);
            this.NotificationTypeID = Null.SetNullInteger(dr["NotificationTypeID"]);

            // add audit column data
            this.FillInternal(dr);
        }
    }
}
