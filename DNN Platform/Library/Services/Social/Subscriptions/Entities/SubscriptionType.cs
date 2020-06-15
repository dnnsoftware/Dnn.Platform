// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions.Entities
{
    using System;

    /// <summary>
    /// This class represents a Subscription Type.
    /// </summary>
    [Serializable]
    public class SubscriptionType
    {
        /// <summary>
        /// Gets or sets the Subscription Type identifier.
        /// </summary>
        public int SubscriptionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Subscription Name.
        /// </summary>
        public string SubscriptionName { get; set; }

        /// <summary>
        /// Gets or sets the Subscription Friendly Name.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the Desktop Module Id associated with the subscription type.
        /// This is an optional field but it should be set if the Subscription Type belongs to a specific module.
        /// </summary>
        public int DesktopModuleId { get; set; }
    }
}
