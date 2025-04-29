// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels;

using System.Runtime.Serialization;

/// <summary>The view model for a subscription.</summary>
[DataContract]
public class SubscriptionViewModel
{
    /// <summary>Gets or sets the id of the subscription.</summary>
    [DataMember(Name = "subscriptionId")]
    public int SubscriptionId { get; set; }

    /// <summary>Gets or sets the description of the subscription.</summary>
    [DataMember(Name = "description")]
    public string Description { get; set; }

    /// <summary>Gets or sets the subscription type.</summary>
    [DataMember(Name = "subscriptionType")]
    public string SubscriptionType { get; set; }
}
