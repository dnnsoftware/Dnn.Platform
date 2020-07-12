// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    using System.Collections.Generic;

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
