// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal.Views
{
    using System;
    using System.Collections.Generic;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Entities.Messaging
    /// Class:      MessageThreadView
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The MessageThreadView class contains MessageConversationView and collection of MessageAttachmentView.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public class MessageThreadView
    {
        /// <summary>
        /// Gets or sets messageItemView containing consolidated information about the message.
        /// </summary>
        public MessageConversationView Conversation { get; set; }

        /// <summary>
        /// Gets or sets list of attachments.
        /// </summary>
        public IList<MessageFileView> Attachments { get; set; }
    }
}
