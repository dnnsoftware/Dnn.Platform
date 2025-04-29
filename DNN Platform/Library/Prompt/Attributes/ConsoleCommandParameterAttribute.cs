// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Prompt;

using System;

/// <summary>An attribute decorating a property representing a parameter to a Prompt command.</summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class ConsoleCommandParameterAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.</summary>
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

    /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="descriptionKey"></param>
    /// <param name="required"></param>
    public ConsoleCommandParameterAttribute(string name, string descriptionKey, bool required)
        : this(name, descriptionKey, required, string.Empty)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="descriptionKey"></param>
    /// <param name="defaultValue"></param>
    public ConsoleCommandParameterAttribute(string name, string descriptionKey, string defaultValue)
        : this(name, descriptionKey, false, defaultValue)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="ConsoleCommandParameterAttribute"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="descriptionKey"></param>
    public ConsoleCommandParameterAttribute(string name, string descriptionKey)
        : this(name, descriptionKey, false, string.Empty)
    {
    }

    /// <summary>Gets or sets the name used in commands to access this parameter.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets a value indicating whether whether the parameter is required.</summary>
    public bool Required { get; set; }

    /// <summary>Gets or sets the default value serialized as string.</summary>
    public string DefaultValue { get; set; }

    /// <summary>Gets or sets the resource key for the description of this parameter.</summary>
    public string DescriptionKey { get; set; }
}
