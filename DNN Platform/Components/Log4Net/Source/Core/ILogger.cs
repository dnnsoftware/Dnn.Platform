using log4net.Repository;
using System;

namespace log4net.Core
{
	public interface ILogger
	{
		string Name
		{
			get;
		}

		ILoggerRepository Repository
		{
			get;
		}

		bool IsEnabledFor(Level level);

		void Log(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception);

		void Log(LoggingEvent logEvent);
	}
}