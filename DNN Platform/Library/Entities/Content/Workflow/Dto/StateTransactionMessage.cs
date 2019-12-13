﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Content.Workflow.Dto
{
    /// <summary>
    /// This class represents the message that will be notified on workflow state transaction
    /// </summary>
    public class StateTransactionMessage
    {
        public StateTransactionMessage()
        {
            Params = new string[]{};
        }

        /// <summary>
        /// Params of the message
        /// </summary>
        public string[] Params { get; set; }

        /// <summary>
        /// User comment
        /// </summary>
        public string UserComment { get; set; }
    }
}
