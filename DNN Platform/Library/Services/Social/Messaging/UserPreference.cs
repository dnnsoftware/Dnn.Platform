using System;

namespace DotNetNuke.Services.Social.Messaging
{
    /// <summary>
    /// Represents the  Messaging User Preference
    /// </summary>
    [Serializable]
    public class UserPreference
    {
        /// <summary>
        /// Portal where the preference are applied
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// User Identifier
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The Email Delivery Frequency used for Messages
        /// </summary>
        public Frequency MessagesEmailFrequency { get; set; }

        /// <summary>
        /// The Email Delivery Frequency used for Notifications
        /// </summary>
        public Frequency NotificationsEmailFrequency { get; set; }
    }
}
