// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data;

using System;
using System.Data;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;

/// <summary>  The Info class for Messaging.</summary>
[Serializable]
public class Message : IHydratable
{
    private string body;
    private Guid conversation;
    private bool emailSent;
    private int fromUserID;
    private string fromUserName;
    private DateTime messageDate;
    private int messageID;
    private int portalID;
    private int replyTo;
    private MessageStatusType status;
    private string subject;
    private int toRoleId;
    private int toUserID;
    private string toUserName;

    private bool allowReply;
    private bool skipInbox;

    private Guid emailSchedulerInstance;
    private DateTime emailSentDate;

    /// <summary>Initializes a new instance of the <see cref="Message"/> class.</summary>
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
            return this.fromUserName;
        }

        private set
        {
            this.fromUserName = value;
        }
    }

    public int FromUserID
    {
        get
        {
            return this.fromUserID;
        }

        set
        {
            this.fromUserID = value;
        }
    }

    public int ToRoleID
    {
        get
        {
            return this.toRoleId;
        }

        set
        {
            this.toRoleId = value;
        }
    }

    public bool AllowReply
    {
        get
        {
            return this.allowReply;
        }

        set
        {
            this.allowReply = value;
        }
    }

    public bool SkipInbox
    {
        get
        {
            return this.skipInbox;
        }

        set
        {
            this.skipInbox = value;
        }
    }

    public bool EmailSent
    {
        get
        {
            return this.emailSent;
        }

        set
        {
            this.emailSent = value;
        }
    }

    public string Body
    {
        get
        {
            return this.body;
        }

        set
        {
            this.body = value;
        }
    }

    public DateTime MessageDate
    {
        get
        {
            return this.messageDate;
        }

        set
        {
            this.messageDate = value;
        }
    }

    public Guid Conversation
    {
        get
        {
            return this.conversation;
        }

        set
        {
            this.conversation = value;
        }
    }

    public int MessageID
    {
        get
        {
            return this.messageID;
        }

        private set
        {
            this.messageID = value;
        }
    }

    public int PortalID
    {
        get
        {
            return this.portalID;
        }

        set
        {
            this.portalID = value;
        }
    }

    public int ReplyTo
    {
        get
        {
            return this.replyTo;
        }

        private set
        {
            this.replyTo = value;
        }
    }

    public MessageStatusType Status
    {
        get
        {
            return this.status;
        }

        set
        {
            this.status = value;
        }
    }

    public string Subject
    {
        get
        {
            var ps = PortalSecurity.Instance;
            return ps.InputFilter(this.subject, PortalSecurity.FilterFlag.NoMarkup);
        }

        set
        {
            var ps = PortalSecurity.Instance;
            ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
            this.subject = ps.InputFilter(value, PortalSecurity.FilterFlag.NoMarkup);
        }
    }

    public int ToUserID
    {
        get
        {
            return this.toUserID;
        }

        set
        {
            this.toUserID = value;
        }
    }

    public string ToUserName
    {
        get
        {
            return this.toUserName;
        }

        private set
        {
            this.toUserName = value;
        }
    }

    public DateTime EmailSentDate
    {
        get
        {
            return this.emailSentDate;
        }

        private set
        {
            this.emailSentDate = value;
        }
    }

    public Guid EmailSchedulerInstance
    {
        get
        {
            return this.emailSchedulerInstance;
        }

        private set
        {
            this.emailSchedulerInstance = value;
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
