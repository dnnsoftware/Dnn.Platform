// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Dto
{
    /// <summary>
    /// This Dto class represents the workflow state transaction on complete state or discard state.
    /// </summary>
    public class StateTransaction
    {
        /// <summary>
        /// Gets or sets the content item id that represent the element that is going to change workflow state.
        /// </summary>
        public int ContentItemId { get; set; }

        /// <summary>
        /// Gets or sets the current state of the element.
        /// </summary>
        public int CurrentStateId { get; set; }

        /// <summary>
        /// Gets or sets this property represents the user that performs the state transaction.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets this property represents the message attached to the state transaction.
        /// </summary>
        public StateTransactionMessage Message { get; set; }
    }
}
