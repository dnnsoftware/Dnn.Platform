// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail;

using System.Collections.Generic;
using System.Text;

/// <summary>Information about an email to be sent.</summary>
public class MailInfo
{
    /// <summary>Gets or sets From email address.</summary>
    public string From { get; set; }

    /// <summary>Gets or sets sender email address.</summary>
    public string Sender { get; set; }

    /// <summary>Gets or sets To email address.</summary>
    public string To { get; set; }

    /// <summary>Gets or sets From Name.</summary>
    public string FromName { get; set; }

    /// <summary>Gets or sets CC email address.</summary>
    public string CC { get; set; }

    /// <summary>Gets or sets BCC email address.</summary>
    public string BCC { get; set; }

    /// <summary>Gets or sets Reply To email address.</summary>
    public string ReplyTo { get; set; }

    /// <summary>Gets or sets Body of email.</summary>
    public string Body { get; set; }

    /// <summary>Gets or sets Subject of email.</summary>
    public string Subject { get; set; }

    /// <summary>Gets or sets Priority.</summary>
    public MailPriority Priority { get; set; }

    /// <summary>Gets or sets Body Encoding.</summary>
    public Encoding BodyEncoding { get; set; }

    /// <summary>Gets or sets Body Format.</summary>
    public MailFormat BodyFormat { get; set; }

    /// <summary>Gets or sets mail attachments.</summary>
    public ICollection<MailAttachment> Attachments { get; set; }
}
