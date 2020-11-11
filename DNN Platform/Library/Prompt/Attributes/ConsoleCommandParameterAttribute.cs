// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;

namespace DotNetNuke.Prompt
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ConsoleCommandParameterAttribute : Attribute
    {
        /// <summary>
        /// The name used in commands to access this parameter.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Whether the parameter is required.
        /// </summary>
        public bool Required { get; set; }
        /// <summary>
        /// The default value serialized as string.
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// The resource key for the description of this parameter.
        /// </summary>
        public string DescriptionKey { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="descriptionKey"></param>
        /// <param name="required"></param>
        /// <param name="defaultValue"></param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required, string defaultValue)
        {
            this.Name = name;
            this.Required = required;
            this.DefaultValue = defaultValue;
            this.DescriptionKey = descriptionKey;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="descriptionKey"></param>
        /// <param name="required"></param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required) : this(name, descriptionKey, required, string.Empty)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="descriptionKey"></param>
        /// <param name="defaultValue"></param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey, string defaultValue) : this(name, descriptionKey, false, defaultValue)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="descriptionKey"></param>
        public ConsoleCommandParameterAttribute(string name, string descriptionKey) : this(name, descriptionKey, false, string.Empty)
        {
        }
    }
}
