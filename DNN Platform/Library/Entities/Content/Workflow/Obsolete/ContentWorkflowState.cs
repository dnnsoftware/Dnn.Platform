// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow

// ReSharper enable CheckNamespace
{
    using System;

    /// <summary>
    /// This entity represents a Workflow State.
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]
    public class ContentWorkflowState
    {
        /// <summary>
        /// Gets or sets state Id.
        /// </summary>
        public int StateID { get; set; }

        /// <summary>
        /// Gets or sets workflow associated to the state.
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// Gets or sets state name.
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// Gets or sets state Order.
        /// </summary>
        public int Order { get; set; }

        public bool IsActive { get; set; }

        public bool SendEmail { get; set; }

        public bool SendMessage { get; set; }

        public bool IsDisposalState { get; set; }

        public string OnCompleteMessageSubject { get; set; }

        public string OnCompleteMessageBody { get; set; }

        public string OnDiscardMessageSubject { get; set; }

        public string OnDiscardMessageBody { get; set; }
    }
}
