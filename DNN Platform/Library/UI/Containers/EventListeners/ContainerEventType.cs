// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.Containers.EventListeners;

/// <summary>ContainerEventType provides a custom enum for Container event types.</summary>
public enum ContainerEventType
{
    OnContainerInit = 0,
    OnContainerLoad = 1,
    OnContainerPreRender = 2,
    OnContainerUnLoad = 3,
}
