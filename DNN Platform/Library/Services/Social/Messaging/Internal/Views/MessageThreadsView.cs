// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      MessageThreadsView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageThreadsView class contains collection of MessageThreadView and other meta data.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageThreadsView
    {
        /// <summary>
        /// Gets or sets total Number of Threads.
        /// </summary>
        public int TotalThreads { get; set; }

        /// <summary>
        /// Gets or sets total Number of New (Unread) Threads.
        /// </summary>
        public int TotalNewThreads { get; set; }

        /// <summary>
        /// Gets or sets total Number of Archived Threads.
        /// </summary>
        public int TotalArchivedThreads { get; set; }

        /// <summary>
        /// Gets or sets list of Conversations.
        /// </summary>
        public List<MessageThreadView> Conversations { get; set; }
    }
}
