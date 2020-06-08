// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Social.Messaging
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      Messages
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The Message class describes the content of messages sent via the system
    /// As messaging is system wide process and there may be no portalID context (e.g. a host messaging "all users") the object does not tie to portalID
    /// </summary>
    /// -----------------------------------------------------------------------------
    [Serializable]
    public class Message : BaseEntityInfo, IHydratable
    {
        private int _messageID = -1;
        private string _displayDate;

        /// <summary>
        /// MessageID - The primary key
        /// </summary>
        [XmlAttribute]
        public int MessageID
        {
            get
            {
                return _messageID;
            }
            set
            {
                _messageID = value;
            }
        }

        /// <summary>
        /// portalID for the message
        /// </summary>
        [XmlAttribute]
        public int PortalID { get; set; }

        /// <summary>
        /// To list for the message. This information is saved for faster display of To list in the message
        /// </summary>
        [XmlAttribute]
        public string To { get; set; }

        /// <summary>
        /// Message From
        /// </summary>
        [XmlAttribute]
        public string From { get; set; }

        /// <summary>
        /// Message Subject
        /// </summary>
        [XmlAttribute]
        public string Subject { get; set; }

        /// <summary>
        /// Message body
        /// </summary>
        [XmlAttribute]
        public string Body { get; set; }

        /// <summary>
        /// Conversation ID of the Message. Each message has at least one ConversationId. Subsequent Replies to a Message get same ConversationId
        /// </summary>
        [XmlAttribute]
        public int ConversationId { get; set; }

        /// <summary>
        /// NotificationType ID of the Message. Each message is associated with a notification type.
        /// </summary>
        [XmlAttribute]
        public int NotificationTypeID { get; set; }

        /// <summary>
        /// ReplyAllAllowed is a bit value to indicate if the reply to the message can be sent to all the recipients or just the sender
        /// </summary>
        [XmlAttribute]
        public bool ReplyAllAllowed { get; set; }

        /// <summary>
        /// The UserID of the sender of the message
        /// </summary>
        [XmlAttribute]
        public int SenderUserID { get; set; }

        /// <summary>
        /// A pretty printed string with the time since the message was created
        /// </summary>
        [XmlAttribute]
        public string DisplayDate
        {
            get
            {
                if (string.IsNullOrEmpty(_displayDate))
                {
                    _displayDate = DateUtils.CalculateDateForDisplay(CreatedOnDate);
                }
                return _displayDate;
            }
        }

        /// <summary>
        /// IHydratable.KeyID.
        /// </summary>
        [XmlIgnore]
        public int KeyID
        {
            get
            {
                return MessageID;
            }
            set
            {
                MessageID = value;
            }
        }

        /// <summary>
        /// Fill the object with data from database.
        /// </summary>
        /// <param name="dr">the data reader.</param>
        public void Fill(IDataReader dr)
        {
            MessageID = Convert.ToInt32(dr["MessageID"]);
            PortalID = Null.SetNullInteger(dr["PortalId"]);
            To = Null.SetNullString(dr["To"]);
            From = Null.SetNullString(dr["From"]);
            Subject = Null.SetNullString(dr["Subject"]);
            Body = Null.SetNullString(dr["Body"]);
            ConversationId = Null.SetNullInteger(dr["ConversationID"]);
            ReplyAllAllowed = Null.SetNullBoolean(dr["ReplyAllAllowed"]);
            SenderUserID = Convert.ToInt32(dr["SenderUserID"]);
            NotificationTypeID = Null.SetNullInteger(dr["NotificationTypeID"]);
            //add audit column data
            FillInternal(dr);
        }
    }
}
