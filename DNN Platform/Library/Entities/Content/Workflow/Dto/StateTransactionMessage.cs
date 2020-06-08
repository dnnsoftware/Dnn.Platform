// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
