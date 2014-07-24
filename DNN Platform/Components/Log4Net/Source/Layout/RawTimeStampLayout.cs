using log4net.Core;

namespace log4net.Layout
{
	public class RawTimeStampLayout : IRawLayout
	{
		public RawTimeStampLayout()
		{
		}

		public virtual object Format(LoggingEvent loggingEvent)
		{
			return loggingEvent.TimeStamp;
		}
	}
}