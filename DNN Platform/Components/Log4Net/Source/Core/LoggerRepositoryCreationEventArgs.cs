using log4net.Repository;
using System;

namespace log4net.Core
{
	public class LoggerRepositoryCreationEventArgs : EventArgs
	{
		private ILoggerRepository m_repository;

		public ILoggerRepository LoggerRepository
		{
			get
			{
				return this.m_repository;
			}
		}

		public LoggerRepositoryCreationEventArgs(ILoggerRepository repository)
		{
			this.m_repository = repository;
		}
	}
}