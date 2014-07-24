using log4net.Core;

namespace log4net.Layout
{
	public class RawUtcTimeStampLayout : IRawLayout
	{
		public RawUtcTimeStampLayout()
		{
		}

		public virtual object Format(LoggingEvent loggingEvent)
		{
			return loggingEvent.TimeStamp.ToUniversalTime();
		}
	}
}