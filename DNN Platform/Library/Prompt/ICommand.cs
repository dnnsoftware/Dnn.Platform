// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Prompt
{
    /// <summary>
    /// This is used to internally retrieve and keep a list of all commands found in the installation
    /// </summary>
    internal interface ICommand
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
        /// Category to which this command belongs. This is used to 
        /// group the list of commands when a user requests this.
        /// </summary>
        string Category { get; set; }
        /// <summary>
        /// Key that is used to lookup the command internally 
        /// (= upper cased command name)
        /// </summary>
        string Key { get; set; }
        /// <summary>
        /// Assembly version of the assembly containing the command
        /// </summary>
        string Version { get; set; }
        /// <summary>
        /// Full name of the class of the command
        /// </summary>
        string TypeFullName { get; set; }
    }
}
