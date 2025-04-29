// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels;

using System.Collections.Generic;

/// <summary>The view model for a single notification.</summary>
public class NotificationViewModel
{
    /// <summary>Gets or sets the id of the notification.</summary>
    public int NotificationId { get; set; }

    /// <summary>Gets or sets the notification subject.</summary>
    public string Subject { get; set; }

    /// <summary>Gets or sets from who this notification comes from.</summary>
    public string From { get; set; }

    /// <summary>Gets or sets the body of the notification.</summary>
    public string Body { get; set; }

    /// <summary>Gets or sets the avatar of the sender.</summary>
    public string SenderAvatar { get; set; }

    /// <summary>Gets or sets the profile url of the sender.</summary>
    public string SenderProfileUrl { get; set; }

    /// <summary>Gets or sets the display name of the sender.</summary>
    public string SenderDisplayName { get; set; }

    /// <summary>Gets or sets the date of the notification.</summary>
    public string DisplayDate { get; set; }

    /// <summary>Gets or sets a list of possible actions on the notification.</summary>
    public IList<NotificationActionViewModel> Actions { get; set; }
}
