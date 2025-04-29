// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels;

/// <summary>The view model for a notification action.</summary>
public class NotificationActionViewModel
{
    /// <summary>Gets or sets the name of the action.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the description of the action.</summary>
    public string Description { get; set; }

    /// <summary>Gets or sets the confirmation message.</summary>
    public string Confirm { get; set; }

    /// <summary>Gets or sets the API call.</summary>
    public string APICall { get; set; }
}
