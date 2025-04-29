// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels;

/// <summary>The view model for totals (messages and notifications).</summary>
public class TotalsViewModel
{
    /// <summary>Gets or sets the number of unread messages.</summary>
    public int TotalUnreadMessages { get; set; }

    /// <summary>Gets or sets the number of new notifications.</summary>
    public int TotalNotifications { get; set; }
}
