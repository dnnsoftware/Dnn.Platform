// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// This is used to retrieve and keep a list of all commands found in the installation
    /// </summary>
    public interface ICommand
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
