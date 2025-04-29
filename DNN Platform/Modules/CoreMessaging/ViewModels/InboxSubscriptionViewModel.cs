// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels;

using System.Runtime.Serialization;

/// <summary>The view model for inbox subscriptions.</summary>
public class InboxSubscriptionViewModel
{
    /// <summary>Gets or sets the notification frequency.</summary>
    [DataMember(Name = "notifyFreq")]
    public int NotifyFreq { get; set; }

    /// <summary>Gets or sets the messaging frequency.</summary>
    [DataMember(Name = "msgFreq")]
    public int MsgFreq { get; set; }
}
