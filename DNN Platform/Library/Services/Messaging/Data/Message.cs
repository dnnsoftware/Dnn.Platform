#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Services.Messaging.Data
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///   The Info class for Messaging
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

        #region "Constructors"

        public Message()
        {
            Conversation = Guid.Empty;
            Status = MessageStatusType.Draft;
            MessageDate = DateTime.Now;
        }

        #endregion

        #region "Public Properties"

        private Guid _EmailSchedulerInstance;
        private DateTime _EmailSentDate;

        public string FromUserName
        {
            get
            {
                return _FromUserName;
            }
            private set
            {
                _FromUserName = value;
            }
        }


        public int FromUserID
        {
            get
            {
                return _FromUserID;
            }
            set
            {
                _FromUserID = value;
            }
        }


        public int ToRoleID
        {
            get
            {
                return _ToRoleId;
            }
            set
            {
                _ToRoleId = value;
            }
        }


        public bool AllowReply
        {
            get
            {
                return _allowReply;
            }
            set
            {
                _allowReply = value;
            }
        }


        public bool SkipInbox
        {
            get
            {
                return _skipInbox;
            }
            set
            {
                _skipInbox = value;
            }
        }

        public bool EmailSent
        {
            get
            {
                return _EmailSent;
            }
            set
            {
                _EmailSent = value;
            }
        }


        public string Body
        {
            get
            {
                return _Body;
            }
            set
            {
                _Body = value;
            }
        }

        public DateTime MessageDate
        {
            get
            {
                return _MessageDate;
            }
            set
            {
                _MessageDate = value;
            }
        }

        public Guid Conversation
        {
            get
            {
                return _Conversation;
            }
            set
            {
                _Conversation = value;
            }
        }

        public int MessageID
        {
            get
            {
                return _MessageID;
            }
            private set
            {
                _MessageID = value;
            }
        }


        public int PortalID
        {
            get
            {
                return _PortalID;
            }
            set
            {
                _PortalID = value;
            }
        }

        public int ReplyTo
        {
            get
            {
                return _ReplyTo;
            }
            private set
            {
                _ReplyTo = value;
            }
        }

        public MessageStatusType Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
            }
        }

        public string Subject
        {
            get
            {
                var ps = PortalSecurity.Instance;
                return ps.InputFilter(_Subject, PortalSecurity.FilterFlag.NoMarkup);
            }
            set
            {
                var ps = PortalSecurity.Instance;
                ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
                _Subject = ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            }
        }


        public int ToUserID
        {
            get
            {
                return _ToUserID;
            }
            set
            {
                _ToUserID = value;
            }
        }

        public string ToUserName
        {
            get
            {
                return _ToUserName;
            }
            private set
            {
                _ToUserName = value;
            }
        }


        public DateTime EmailSentDate
        {
            get
            {
                return _EmailSentDate;
            }
            private set
            {
                _EmailSentDate = value;
            }
        }


        public Guid EmailSchedulerInstance
        {
            get
            {
                return _EmailSchedulerInstance;
            }
            private set
            {
                _EmailSchedulerInstance = value;
            }
        }

        #endregion

        #region "Public Methods"

        public Message GetReplyMessage()
        {
            var message = new Message();
            message.AllowReply = AllowReply;
            message.Body = string.Format("<br><br><br>On {0} {1} wrote ", MessageDate, FromUserName) + Body;
            message.Conversation = Conversation;
            message.FromUserID = ToUserID;
            message.ToUserID = FromUserID;
            message.ToUserName = FromUserName;
            message.PortalID = PortalID;
            message.ReplyTo = MessageID;
            message.SkipInbox = SkipInbox;
            message.Subject = "RE:" + Subject;

            return message;
        }

        #endregion

        #region "IHydratable Implementation"

        public void Fill(IDataReader dr)
        {
            MessageID = Null.SetNullInteger(dr["MessageID"]);
            PortalID = Null.SetNullInteger(dr["PortalID"]);
            FromUserID = Null.SetNullInteger(dr["FromUserID"]);
            FromUserName = Null.SetNullString(dr["FromUserName"]);
            ToUserID = Null.SetNullInteger(dr["ToUserID"]);
            //'_ToUserName = Null.SetNullString(dr.Item("ToUserName"))
            ReplyTo = Null.SetNullInteger(dr["ReplyTo"]);
            Status = (MessageStatusType) Enum.Parse(typeof (MessageStatusType), Null.SetNullString(dr["Status"]));
            Body = Null.SetNullString(dr["Body"]);
            Subject = Null.SetNullString(dr["Subject"]);
            MessageDate = Null.SetNullDateTime(dr["Date"]);
            ToRoleID = Null.SetNullInteger(dr["ToRoleID"]);
            AllowReply = Null.SetNullBoolean(dr["AllowReply"]);
            SkipInbox = Null.SetNullBoolean(dr["SkipPortal"]);
            EmailSent = Null.SetNullBoolean(dr["EmailSent"]);
            ToUserName = Null.SetNullString(dr["ToUserName"]);
            string g = Null.SetNullString(dr["Conversation"]);
            EmailSentDate = Null.SetNullDateTime(dr["EmailSentDate"]);
            EmailSchedulerInstance = Null.SetNullGuid(dr["EmailSchedulerInstance"]);
            Conversation = Null.SetNullGuid(dr["Conversation"]);


            //'Conversation = New Guid(g)
        }

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

        #endregion
    }
}
