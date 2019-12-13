using DotNetNuke.Services.Social.Messaging;
using DotNetNuke.Tests.Utilities;

namespace DotNetNuke.Tests.Core.Controllers.Messaging.Builders
{
    internal class UserPreferenceBuilder
    {
        private int userId;
        private int portalId;
        private Frequency messagesEmailFrequency;
        private Frequency notificationsEmailFrequency;

        internal UserPreferenceBuilder()
        {
            userId = Constants.USER_InValidId;
            portalId = Constants.PORTAL_ValidPortalId;
            messagesEmailFrequency = Frequency.Instant;
            notificationsEmailFrequency = Frequency.Hourly;
        }

        internal UserPreference Build()
        {
            return new UserPreference
            {
                UserId = userId,
                PortalId = portalId,
                MessagesEmailFrequency = messagesEmailFrequency,
                NotificationsEmailFrequency = notificationsEmailFrequency
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
