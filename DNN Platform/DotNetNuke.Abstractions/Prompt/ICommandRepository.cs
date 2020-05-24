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
        /// List of all commands
        /// </summary>
        /// <returns></returns>
        SortedDictionary<string, ICommand> GetCommands();
        /// <summary>
        /// Get help for the specified command
        /// </summary>
        /// <param name="consoleCommand">Command to get help for</param>
        /// <returns></returns>
        ICommandHelp GetCommandHelp(IConsoleCommand consoleCommand);
    }
}
