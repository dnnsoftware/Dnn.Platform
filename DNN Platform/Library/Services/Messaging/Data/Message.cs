// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data
{
    using System;
    using System.Data;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;

    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Info class for Messaging.
    /// </summary>
    /// <remarks>
    /// </remarks>
    [Serializable]
    public class Message : IHydratable
    {
        private string _Body;
        private Guid _Conversation;
        private bool _EmailSent;
        private int _FromUserID;
        private string _FromUserName;
        private DateTime _MessageDate;
        private int _MessageID;
        private int _PortalID;
        private int _ReplyTo;
        private MessageStatusType _Status;
        private string _Subject;
        private int _ToRoleId;
        private int _ToUserID;
        private string _ToUserName;

        private bool _allowReply;
        private bool _skipInbox;

        private Guid _EmailSchedulerInstance;
        private DateTime _EmailSentDate;

        public Message()
        {
            this.Conversation = Guid.Empty;
            this.Status = MessageStatusType.Draft;
            this.MessageDate = DateTime.Now;
        }

        public string FromUserName
        {
            get
            {
                return this._FromUserName;
            }

            private set
            {
                this._FromUserName = value;
            }
        }

        public int FromUserID
        {
            get
            {
                return this._FromUserID;
            }

            set
            {
                this._FromUserID = value;
            }
        }

        public int ToRoleID
        {
            get
            {
                return this._ToRoleId;
            }

            set
            {
                this._ToRoleId = value;
            }
        }

        public bool AllowReply
        {
            get
            {
                return this._allowReply;
            }

            set
            {
                this._allowReply = value;
            }
        }

        public bool SkipInbox
        {
            get
            {
                return this._skipInbox;
            }

            set
            {
                this._skipInbox = value;
            }
        }

        public bool EmailSent
        {
            get
            {
                return this._EmailSent;
            }

            set
            {
                this._EmailSent = value;
            }
        }

        public string Body
        {
            get
            {
                return this._Body;
            }

            set
            {
                this._Body = value;
            }
        }

        public DateTime MessageDate
        {
            get
            {
                return this._MessageDate;
            }

            set
            {
                this._MessageDate = value;
            }
        }

        public Guid Conversation
        {
            get
            {
                return this._Conversation;
            }

            set
            {
                this._Conversation = value;
            }
        }

        public int MessageID
        {
            get
            {
                return this._MessageID;
            }

            private set
            {
                this._MessageID = value;
            }
        }

        public int PortalID
        {
            get
            {
                return this._PortalID;
            }

            set
            {
                this._PortalID = value;
            }
        }

        public int ReplyTo
        {
            get
            {
                return this._ReplyTo;
            }

            private set
            {
                this._ReplyTo = value;
            }
        }

        public MessageStatusType Status
        {
            get
            {
                return this._Status;
            }

            set
            {
                this._Status = value;
            }
        }

        public string Subject
        {
            get
            {
                var ps = PortalSecurity.Instance;
                return ps.InputFilter(this._Subject, PortalSecurity.FilterFlag.NoMarkup);
            }

            set
            {
                var ps = PortalSecurity.Instance;
                ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
                this._Subject = ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }

        public int ToUserID
        {
            get
            {
                return this._ToUserID;
            }

            set
            {
                this._ToUserID = value;
            }
        }

        public string ToUserName
        {
            get
            {
                return this._ToUserName;
            }

            private set
            {
                this._ToUserName = value;
            }
        }

        public DateTime EmailSentDate
        {
            get
            {
                return this._EmailSentDate;
            }

            private set
            {
                this._EmailSentDate = value;
            }
        }

        public Guid EmailSchedulerInstance
        {
            get
            {
                return this._EmailSchedulerInstance;
            }

            private set
            {
                this._EmailSchedulerInstance = value;
            }
        }

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

        public Message GetReplyMessage()
        {
            var message = new Message();
            message.AllowReply = this.AllowReply;
            message.Body = string.Format("<br><br><br>On {0} {1} wrote ", this.MessageDate, this.FromUserName) + this.Body;
            message.Conversation = this.Conversation;
            message.FromUserID = this.ToUserID;
            message.ToUserID = this.FromUserID;
            message.ToUserName = this.FromUserName;
            message.PortalID = this.PortalID;
            message.ReplyTo = this.MessageID;
            message.SkipInbox = this.SkipInbox;
            message.Subject = "RE:" + this.Subject;

            return message;
        }

        public void Fill(IDataReader dr)
        {
            this.MessageID = Null.SetNullInteger(dr["MessageID"]);
            this.PortalID = Null.SetNullInteger(dr["PortalID"]);
            this.FromUserID = Null.SetNullInteger(dr["FromUserID"]);
            this.FromUserName = Null.SetNullString(dr["FromUserName"]);
            this.ToUserID = Null.SetNullInteger(dr["ToUserID"]);

            // '_ToUserName = Null.SetNullString(dr.Item("ToUserName"))
            this.ReplyTo = Null.SetNullInteger(dr["ReplyTo"]);
            this.Status = (MessageStatusType)Enum.Parse(typeof(MessageStatusType), Null.SetNullString(dr["Status"]));
            this.Body = Null.SetNullString(dr["Body"]);
            this.Subject = Null.SetNullString(dr["Subject"]);
            this.MessageDate = Null.SetNullDateTime(dr["Date"]);
            this.ToRoleID = Null.SetNullInteger(dr["ToRoleID"]);
            this.AllowReply = Null.SetNullBoolean(dr["AllowReply"]);
            this.SkipInbox = Null.SetNullBoolean(dr["SkipPortal"]);
            this.EmailSent = Null.SetNullBoolean(dr["EmailSent"]);
            this.ToUserName = Null.SetNullString(dr["ToUserName"]);
            string g = Null.SetNullString(dr["Conversation"]);
            this.EmailSentDate = Null.SetNullDateTime(dr["EmailSentDate"]);
            this.EmailSchedulerInstance = Null.SetNullGuid(dr["EmailSchedulerInstance"]);
            this.Conversation = Null.SetNullGuid(dr["Conversation"]);

            // 'Conversation = New Guid(g)
        }
    }
}
