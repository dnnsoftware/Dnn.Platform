// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// The repository handles retrieving of commands from the entire DNN installation
    /// </summary>
    public interface ICommandRepository
    {
        /// <summary>
        /// Get help for the specified command
        /// </summary>
        /// <param name="consoleCommand">Command to get help for</param>
        /// <returns></returns>
        ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand);
        /// <summary>
        /// Get the command. Returns null if no command found for the name.
        /// </summary>
        /// <param name="commandName">Name of the command (commonly in verb-noun format)</param>
        /// <returns></returns>
        IConsoleCommand GetCommand(string commandName);
    }
}
