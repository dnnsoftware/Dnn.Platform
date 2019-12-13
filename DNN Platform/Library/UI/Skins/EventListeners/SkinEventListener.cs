namespace DotNetNuke.UI.Skins.EventListeners
{
    public class SkinEventListener
    {
        public SkinEventListener(SkinEventType type, SkinEventHandler e)
        {
            EventType = type;
            SkinEvent = e;
        }

        public SkinEventType EventType { get; private set; }
        public SkinEventHandler SkinEvent { get; private set; }
    }
}
