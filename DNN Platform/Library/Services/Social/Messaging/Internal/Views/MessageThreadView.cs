#region Usings

using System;
using System.Collections.Generic;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      MessageThreadView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageThreadView class contains MessageConversationView and collection of MessageAttachmentView
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageThreadView
    {
        /// <summary>
        /// MessageItemView containing consolidated information about the message
        /// </summary>
        public MessageConversationView Conversation { get; set; }

        /// <summary>
        /// List of attachments
        /// </summary>
        public IList<MessageFileView> Attachments { get; set; }
    }
}
