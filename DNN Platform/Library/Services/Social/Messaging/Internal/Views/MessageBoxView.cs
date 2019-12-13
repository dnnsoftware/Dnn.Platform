// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging.Views
    /// Class:      MessageBoxView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageBoxView contains the View of Inbox, Sent or Archived messages
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageBoxView 
    {
        /// <summary>
        /// Total Number of Coversations
        /// </summary>
        public int TotalConversations { get; set; }

        /// <summary>
        /// Total Number of New (Unread) Threads
        /// </summary>
        public int TotalNewThreads { get; set; }

        /// <summary>
        /// List of Conversations
        /// </summary>
        public List<MessageConversationView> Conversations { get; set; }
    }
}
