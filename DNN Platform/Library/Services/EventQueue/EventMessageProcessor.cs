namespace DotNetNuke.Services.EventQueue
{
	/// <summary>
	/// Basic class of EventMessageProcessor.
	/// </summary>
    public abstract class EventMessageProcessorBase
    {
        public abstract bool ProcessMessage(EventMessage message);
    }
}
