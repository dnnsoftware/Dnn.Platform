// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels;

using System.Collections.Generic;

/// <summary>The view model for a collection of notification.</summary>
public class NotificationsViewModel
{
    /// <summary>Gets or sets the total amount of notifications.</summary>
    public int TotalNotifications { get; set; }

    /// <summary>Gets or sets the notifications.</summary>
    public IList<NotificationViewModel> Notifications { get; set; }
}
