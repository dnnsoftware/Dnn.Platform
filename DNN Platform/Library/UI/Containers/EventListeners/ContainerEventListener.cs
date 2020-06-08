// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Containers.EventListeners
{
    public class ContainerEventListener
    {
        private readonly ContainerEventHandler _ContainerEvent;
        private readonly ContainerEventType _Type;

        public ContainerEventListener(ContainerEventType type, ContainerEventHandler e)
        {
            _Type = type;
            _ContainerEvent = e;
        }

        public ContainerEventType EventType
        {
            get
            {
                return _Type;
            }
        }

        public ContainerEventHandler ContainerEvent
        {
            get
            {
                return _ContainerEvent;
            }
        }
    }
}
