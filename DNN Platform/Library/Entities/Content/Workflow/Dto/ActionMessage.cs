// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Dto
{
    /// <summary>
    /// This dto class represents the message that will be sent as notification
    /// of a specific action on the Workflow (Complete/Discard state, Complete/Discard workflow).
    /// </summary>
    public class ActionMessage
    {
        /// <summary>
        /// Gets or sets subject of the message.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets body of the message.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether indicates if the message is going to be toasted or not. By default, it is False.
        /// </summary>
        public bool SendToast { get; set; }
    }
}
