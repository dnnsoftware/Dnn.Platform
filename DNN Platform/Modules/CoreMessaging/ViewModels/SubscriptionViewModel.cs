// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
