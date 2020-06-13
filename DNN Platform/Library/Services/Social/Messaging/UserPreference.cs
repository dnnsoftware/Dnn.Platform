// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging
{
    using System;

    /// <summary>
    /// Represents the  Messaging User Preference.
    /// </summary>
    [Serializable]
    public class UserPreference
    {
        /// <summary>
        /// Gets or sets portal where the preference are applied.
        /// </summary>
        public int PortalId { get; set; }

        /// <summary>
        /// Gets or sets user Identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the Email Delivery Frequency used for Messages.
        /// </summary>
        public Frequency MessagesEmailFrequency { get; set; }

        /// <summary>
        /// Gets or sets the Email Delivery Frequency used for Notifications.
        /// </summary>
        public Frequency NotificationsEmailFrequency { get; set; }
    }
}
