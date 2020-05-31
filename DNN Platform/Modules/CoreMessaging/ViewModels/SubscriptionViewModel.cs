// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels
{
    using System.Runtime.Serialization;

    [DataContract]
    public class SubscriptionViewModel
    {
        [DataMember(Name = "subscriptionId")]
        public int SubscriptionId { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "subscriptionType")]
        public string SubscriptionType { get; set; }
    }
}
