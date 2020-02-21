// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Content.Workflow.Dto
{
    /// <summary>
    /// This Dto class represents the workflow state transaction on complete state or discard state.
    /// </summary>
    public class StateTransaction
    {
        /// <summary>
        /// The content item id that represent the element that is going to change workflow state
        /// </summary>
        public int ContentItemId { get; set; }

        /// <summary>
        /// The current state of the element
        /// </summary>
        public int CurrentStateId { get; set; }

        /// <summary>
        /// This property represents the user that performs the state transaction
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// This property represents the message attached to the state transaction
        /// </summary>
        public StateTransactionMessage Message { get; set; }
    }
}
