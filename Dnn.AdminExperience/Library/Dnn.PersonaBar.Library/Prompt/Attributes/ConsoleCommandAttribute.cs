// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Prompt.Attributes;

using System;

using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(9, 7, 0, "Moved to DotNetNuke.Prompt in the core library project")]
[AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CS3015 // Type has no accessible constructors which use only CLS-compliant types
public partial class ConsoleCommandAttribute : Attribute
#pragma warning restore CS3015 // Type has no accessible constructors which use only CLS-compliant types
{
    /// <summary>Initializes a new instance of the <see cref="ConsoleCommandAttribute"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="category"></param>
    /// <param name="description"></param>
    public ConsoleCommandAttribute(string name, string category, string description)
    {
        this.Name = name;
        this.Category = category;
        this.Description = description;
    }

    public string Name { get; set; }

    public string Category { get; set; }

    public string Description { get; set; }
}
