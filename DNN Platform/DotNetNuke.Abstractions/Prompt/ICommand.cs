// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.Prompt
{
    /// <summary>This contains information about a Prompt command available in the installation.</summary>
    public interface ICommand
    {
        /// <summary>Gets or sets the name of the command.</summary>
        string Name { get; set; }

        /// <summary>Gets or sets the description of the command.</summary>
        string Description { get; set; }

        /// <summary>Gets or sets the category to which this command belongs.</summary>
        /// <remarks>This is used to group the list of commands when a user requests this.</remarks>
        string Category { get; set; }

        /// <summary>Gets or sets the key that is used to look up the command internally (i.e. the upper-cased command name).</summary>
        string Key { get; set; }

        /// <summary>Gets or sets the assembly version of the assembly containing the command.</summary>
        string Version { get; set; }

        /// <summary>Gets or sets the full name of the class of the command.</summary>
        string TypeFullName { get; set; }
    }
}
