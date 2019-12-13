// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
