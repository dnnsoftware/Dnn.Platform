using log4net.Util;
using System;

namespace log4net.Core
{
	public struct LoggingEventData
	{
		public string LoggerName;

		public log4net.Core.Level Level;

		public string Message;

		public string ThreadName;

		public DateTime TimeStamp;

		public log4net.Core.LocationInfo LocationInfo;

		public string UserName;

		public string Identity;

		public string ExceptionString;

		public string Domain;

		public PropertiesDictionary Properties;
	}
}