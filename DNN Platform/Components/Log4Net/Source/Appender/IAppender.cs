using log4net.Core;

namespace log4net.Appender
{
	public interface IAppender
	{
		string Name
		{
			get;
			set;
		}

		void Close();

		void DoAppend(LoggingEvent loggingEvent);
	}
}