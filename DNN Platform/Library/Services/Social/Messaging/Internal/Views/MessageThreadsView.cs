#region Usings

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      MessageThreadsView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageThreadsView class contains collection of MessageThreadView and other meta data
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageThreadsView
    {
        /// <summary>
        /// Total Number of Threads
        /// </summary>
        public int TotalThreads { get; set; }

        /// <summary>
        /// Total Number of New (Unread) Threads
        /// </summary>
        public int TotalNewThreads { get; set; }

        /// <summary>
        /// Total Number of Archived Threads
        /// </summary>
        public int TotalArchivedThreads { get; set; }

        /// <summary>
        /// List of Conversations
        /// </summary>
        public List<MessageThreadView> Conversations { get; set; }
    }
}
