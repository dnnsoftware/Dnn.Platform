// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
