using log4net.Core;

namespace log4net.Appender
{
	public interface IBulkAppender : IAppender
	{
		void DoAppend(LoggingEvent[] loggingEvents);
	}
}