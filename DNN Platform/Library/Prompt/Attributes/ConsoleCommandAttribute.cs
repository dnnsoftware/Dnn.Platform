// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt
{
    using System;

    /// <summary>An attribute decorating a Prompt command.</summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="ConsoleCommandAttribute"/> class.</summary>
        /// <param name="name">The command name.</param>
        /// <param name="categoryKey">The resource key for the command category.</param>
        /// <param name="descriptionKey">The resource key for the command description.</param>
        public ConsoleCommandAttribute(string name, string categoryKey, string descriptionKey)
        {
            this.Name = name;
            this.CategoryKey = categoryKey;
            this.DescriptionKey = descriptionKey;
        }

        /// <summary>Gets or sets name used in the UI for the command. By convention we encourage you to use "verb-noun" as with Powershell (e.g. "add-module").</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets resource key for the category of the command.</summary>
        public string CategoryKey { get; set; }

        /// <summary>Gets or sets resource key for the description of the command.</summary>
        public string DescriptionKey { get; set; }
    }
}
