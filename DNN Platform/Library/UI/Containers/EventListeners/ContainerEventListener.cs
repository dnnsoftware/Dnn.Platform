// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Containers.EventListeners;

public class ContainerEventListener
{
    private readonly ContainerEventHandler containerEvent;
    private readonly ContainerEventType type;

    /// <summary>Initializes a new instance of the <see cref="ContainerEventListener"/> class.</summary>
    /// <param name="type"></param>
    /// <param name="e"></param>
    public ContainerEventListener(ContainerEventType type, ContainerEventHandler e)
    {
        this.type = type;
        this.containerEvent = e;
    }

    public ContainerEventType EventType
    {
        get
        {
            return this.type;
        }
    }

    public ContainerEventHandler ContainerEvent
    {
        get
        {
            return this.containerEvent;
        }
    }
}
