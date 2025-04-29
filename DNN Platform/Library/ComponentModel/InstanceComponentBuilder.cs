// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ComponentModel;

internal class InstanceComponentBuilder : IComponentBuilder
{
    private readonly object instance;
    private readonly string name;

    /// <summary>Initializes a new instance of the <see cref="InstanceComponentBuilder"/> class.</summary>
    /// <param name="name"></param>
    /// <param name="instance"></param>
    public InstanceComponentBuilder(string name, object instance)
    {
        this.name = name;
        this.instance = instance;
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
        return this.instance;
    }
}
