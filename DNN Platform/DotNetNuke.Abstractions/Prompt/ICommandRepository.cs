// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 

using System.Collections.Generic;

namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// The repository handles retrieving of commands from the entire DNN installation
    /// </summary>
    public interface ICommandRepository
    {
        /// <summary>
        /// Gets a list of all found commands.
        /// </summary>
        /// <returns>List of ICommand</returns>
        IEnumerable<ICommand> GetCommands();
        /// <summary>
        /// Get the command. Returns null if no command found for the name.
        /// </summary>
        /// <param name="commandName">Name of the command (commonly in verb-noun format)</param>
        /// <returns>An IConsoleCommand or null</returns>
        IConsoleCommand GetCommand(string commandName);
        /// <summary>
        /// Get help for the specified command
        /// </summary>
        /// <param name="consoleCommand">Command to get help for</param>
        /// <returns>ICommandHelp class that can be used by the client to compile a help text</returns>
        ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand);
    }
}
