// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Subscriptions
{
    using DotNetNuke.Services.Social.Subscriptions.Entities;

    /// <summary>
    /// This controller provides permission info about the User Subscription.
    /// </summary>
    public interface ISubscriptionSecurityController
    {
        /// <summary>
        /// Check if the User has permission on the Subscription based on the Subscription ModuleId and TabId.
        /// </summary>
        /// <param name="subscription">Subscription.</param>
        /// <returns>True if user has permission, false otherwise.</returns>
        bool HasPermission(Subscription subscription);
    }
}
