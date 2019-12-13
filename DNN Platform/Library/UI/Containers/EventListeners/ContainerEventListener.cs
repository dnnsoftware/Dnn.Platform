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
