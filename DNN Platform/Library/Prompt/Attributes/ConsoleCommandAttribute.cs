// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
        /// Resource key for the category of the command
        /// </summary>
        public string CategoryKey { get; set; }
        /// <summary>
        /// Resource key for the description of the command
        /// </summary>
        public string DescriptionKey { get; set; }

        public ConsoleCommandAttribute(string name, string categoryKey, string descriptionKey)
        {
            Name = name;
            CategoryKey = categoryKey;
            DescriptionKey = descriptionKey;
        }
    }
}
