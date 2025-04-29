// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers.EventListeners;

using System;

/// <summary>ContainerEventArgs provides a custom EventARgs class for Container Events.</summary>
public class ContainerEventArgs : EventArgs
{
    private readonly Container container;

    /// <summary>Initializes a new instance of the <see cref="ContainerEventArgs"/> class.</summary>
    /// <param name="container"></param>
    public ContainerEventArgs(Container container)
    {
        this.container = container;
    }

    public Container Container
    {
        get
        {
            return this.container;
        }
    }
}
