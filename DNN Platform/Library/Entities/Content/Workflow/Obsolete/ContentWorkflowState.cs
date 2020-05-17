﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This entity represents a Workflow State
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]    
    public class ContentWorkflowState 
    {
        /// <summary>
        /// State Id
        /// </summary>
        public int StateID { get; set; }

        /// <summary>
        /// Workflow associated to the state
        /// </summary>
        public int WorkflowID { get; set; }

        /// <summary>
        /// State name
        /// </summary>
        public string StateName { get; set; }

        /// <summary>
        /// State Order
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
