﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;

namespace Dnn.PersonaBar.Library.Prompt.Attributes
{
    /// <summary>
    /// Attribute to define the help for the flag parameter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class FlagParameterAttribute : Attribute
    {
        /// <summary>
        /// Name of the flag
        /// </summary>
        public string Flag { get; set; }

        /// <summary>
        /// Type of the flag value expected.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Is flag required or not
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Default value of the flag
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Description of flag
        /// </summary>
        public string Description { get; set; }

        public FlagParameterAttribute(string flag, string description, string type, string defaultValue, bool required)
        {
            this.Flag = flag;
            this.Type = type;
            this.Required = required;
            this.DefaultValue = defaultValue;
            this.Description = description;
        }
        public FlagParameterAttribute(string flag, string description, string type, string defaultValue) : this(flag, description, type, defaultValue, false) { }
        public FlagParameterAttribute(string flag, string description, string type) : this(flag, description, type, string.Empty, false) { }
        public FlagParameterAttribute(string flag, string description, string type, bool required) : this(flag, description, type, string.Empty, required) { }
    }
}
