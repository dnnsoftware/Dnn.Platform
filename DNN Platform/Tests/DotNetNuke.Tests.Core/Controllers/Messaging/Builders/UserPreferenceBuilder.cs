// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Tests.Utilities;

    internal class UserPreferenceBuilder
    {
        private int userId;
        private int portalId;
        private Frequency messagesEmailFrequency;
        private Frequency notificationsEmailFrequency;

        internal UserPreferenceBuilder()
        {
            this.userId = Constants.USER_InValidId;
            this.portalId = Constants.PORTAL_ValidPortalId;
            this.messagesEmailFrequency = Frequency.Instant;
            this.notificationsEmailFrequency = Frequency.Hourly;
        }

        internal UserPreference Build()
        {
            return new UserPreference
            {
                UserId = this.userId,
                PortalId = this.portalId,
                MessagesEmailFrequency = this.messagesEmailFrequency,
                NotificationsEmailFrequency = this.notificationsEmailFrequency,
            };
        }

        internal UserPreferenceBuilder WithUserId(int userId)
        {
            this.userId = userId;
            return this;
        }

        internal UserPreferenceBuilder WithPortalId(int portalId)
        {
            this.portalId = portalId;
            return this;
        }
    }
}
