using System;

namespace log4net.Util
{
	public class LogReceivedEventArgs : EventArgs
	{
		private readonly log4net.Util.LogLog loglog;

		public log4net.Util.LogLog LogLog
		{
			get
			{
				return this.loglog;
			}
		}

		public LogReceivedEventArgs(log4net.Util.LogLog loglog)
		{
			this.loglog = loglog;
		}
	}
}