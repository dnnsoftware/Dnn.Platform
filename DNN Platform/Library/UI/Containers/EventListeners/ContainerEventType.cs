// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Containers.EventListeners
{
    /// <summary>ContainerEventType provides a custom enum for Container event types.</summary>
    public enum ContainerEventType
    {
        /// <summary>When initializing the container.</summary>
        OnContainerInit = 0,

        /// <summary>When loading the container.</summary>
        OnContainerLoad = 1,

        /// <summary>Just before rendering the container.</summary>
        OnContainerPreRender = 2,

        /// <summary>When unloading the container.</summary>
        OnContainerUnLoad = 3,
    }
}
