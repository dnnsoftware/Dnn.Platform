// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// This is used to send the result back to the client when a user asks help for a command
    /// </summary>
    public interface ICommandHelp
    {
        /// <summary>
        /// Name of the command
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Description of the command
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Command parameter list, each with their own help text
        /// </summary>
        IEnumerable<ICommandOption> Options { get; set; }
        /// <summary>
        /// Html formatted block of text that describes what this command does
        /// </summary>
        string ResultHtml { get; set; }
        /// <summary>
        /// Any error produced while trying to retrieve help for this command
        /// </summary>
        string Error { get; set; }
    }
}
