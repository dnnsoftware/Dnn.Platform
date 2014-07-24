using System;

namespace log4net.Repository.Hierarchy
{
	public class LoggerCreationEventArgs : EventArgs
	{
		private log4net.Repository.Hierarchy.Logger m_log;

		public log4net.Repository.Hierarchy.Logger Logger
		{
			get
			{
				return this.m_log;
			}
		}

		public LoggerCreationEventArgs(log4net.Repository.Hierarchy.Logger log)
		{
			this.m_log = log;
		}
	}
}