// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Prompt.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConsoleCommandParameterAttribute : Attribute
    {
        /// <summary>
        /// The name used in commands to access this parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether the parameter is required
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// The default value serialized as string
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// The resource key for the description of this parameter
        /// </summary>
        public string DescriptionKey { get; set; }
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required, string defaultValue)
        {
            Name = name;
            Required = required;
            DefaultValue = defaultValue;
            DescriptionKey = descriptionKey;
        }
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required) : this(name, descriptionKey, required, string.Empty) { }
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, string defaultValue) : this(name, descriptionKey, false, defaultValue) { }
        public ConsoleCommandParameterAttribute(string name, string descriptionKey) : this(name, descriptionKey, false, string.Empty) { }
    }
}
