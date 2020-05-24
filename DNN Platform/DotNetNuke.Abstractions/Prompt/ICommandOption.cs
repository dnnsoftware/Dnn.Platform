// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>
    /// This is used in the ICommandHelp to send a list of command parameters to the client for explanatory help
    /// </summary>
    public interface ICommandOption
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Description of what this parameter is for
        /// </summary>
        string Description { get; set; }
        /// <summary>
        /// Default serialized value if it is specified
        /// </summary>
        string DefaultValue { get; set; }
        /// <summary>
        /// Whether the parameter is required
        /// </summary>
        bool Required { get; set; }
    }
}
