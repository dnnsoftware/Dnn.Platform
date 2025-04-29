// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.ComponentModel;

using System;

using DotNetNuke.Framework;

internal class TransientComponentBuilder : IComponentBuilder
{
    private readonly string name;
    private readonly Type type;

    /// <summary>Initializes a new instance of the <see cref="TransientComponentBuilder"/> class.</summary>
    /// <param name="name">The name of the component.</param>
    /// <param name="type">The type of the component.</param>
    public TransientComponentBuilder(string name, Type type)
    {
        this.name = name;
        this.type = type;
    }

    /// <inheritdoc/>
    public string Name
    {
        get
        {
            return this.name;
        }
    }

    /// <inheritdoc/>
    public object BuildComponent()
    {
        return Reflection.CreateObject(this.type);
    }
}
