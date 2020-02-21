// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Social.Subscriptions.Entities;

namespace DotNetNuke.Services.Social.Subscriptions
{
    /// <summary>
    /// This controller provides permission info about the User Subscription.
    /// </summary>
    public interface ISubscriptionSecurityController
    {
        /// <summary>
        /// Check if the User has permission on the Subscription based on the Subscription ModuleId and TabId.
        /// </summary>
        /// <param name="subscription">Subscription</param>
        /// <returns>True if user has permission, false otherwise</returns>
        bool HasPermission(Subscription subscription);
    }
}
