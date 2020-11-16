// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

namespace DotNetNuke.Prompt
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConsoleCommandAttribute : Attribute
    {
        /// <summary>
        /// Name used in the UI for the command. By convention we encourage you to use "verb-noun" as with Powershell (e.g. "add-module").
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Resource key for the category of the command.
        /// </summary>
        public string CategoryKey { get; set; }
        /// <summary>
        /// Resource key for the description of the command.
        /// </summary>
        public string DescriptionKey { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandAttribute"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="categoryKey"></param>
        /// <param name="descriptionKey"></param>
        public ConsoleCommandAttribute(string name, string categoryKey, string descriptionKey)
        {
            this.Name = name;
            this.CategoryKey = categoryKey;
            this.DescriptionKey = descriptionKey;
        }
    }
}
