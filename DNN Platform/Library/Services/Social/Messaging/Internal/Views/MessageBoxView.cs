// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    using System;
    using System.Collections.Generic;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging.Views
    /// Class:      MessageBoxView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageBoxView contains the View of Inbox, Sent or Archived messages.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageBoxView
    {
        /// <summary>
        /// Gets or sets total Number of Coversations.
        /// </summary>
        public int TotalConversations { get; set; }

        /// <summary>
        /// Gets or sets total Number of New (Unread) Threads.
        /// </summary>
        public int TotalNewThreads { get; set; }

        /// <summary>
        /// Gets or sets list of Conversations.
        /// </summary>
        public List<MessageConversationView> Conversations { get; set; }
    }
}
