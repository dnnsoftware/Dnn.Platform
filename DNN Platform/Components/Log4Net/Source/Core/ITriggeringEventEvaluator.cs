namespace log4net.Core
{
	public interface ITriggeringEventEvaluator
	{
		bool IsTriggeringEvent(LoggingEvent loggingEvent);
	}
}