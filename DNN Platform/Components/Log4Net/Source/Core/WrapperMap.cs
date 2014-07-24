using log4net.Repository;
using System;
using System.Collections;

namespace log4net.Core
{
	public class WrapperMap
	{
		private readonly Hashtable m_repositories = new Hashtable();

		private readonly WrapperCreationHandler m_createWrapperHandler;

		private readonly LoggerRepositoryShutdownEventHandler m_shutdownHandler;

		protected Hashtable Repositories
		{
			get
			{
				return this.m_repositories;
			}
		}

		public WrapperMap(WrapperCreationHandler createWrapperHandler)
		{
			this.m_createWrapperHandler = createWrapperHandler;
			this.m_shutdownHandler = new LoggerRepositoryShutdownEventHandler(this.ILoggerRepository_Shutdown);
		}

		protected virtual ILoggerWrapper CreateNewWrapperObject(ILogger logger)
		{
			if (this.m_createWrapperHandler == null)
			{
				return null;
			}
			return this.m_createWrapperHandler(logger);
		}

		public virtual ILoggerWrapper GetWrapper(ILogger logger)
		{
			ILoggerWrapper loggerWrapper;
			if (logger == null)
			{
				return null;
			}
			lock (this)
			{
				Hashtable item = (Hashtable)this.m_repositories[logger.Repository];
				if (item == null)
				{
					item = new Hashtable();
					this.m_repositories[logger.Repository] = item;
					logger.Repository.ShutdownEvent += this.m_shutdownHandler;
				}
				ILoggerWrapper item1 = item[logger] as ILoggerWrapper;
				if (item1 == null)
				{
					item1 = this.CreateNewWrapperObject(logger);
					item[logger] = item1;
				}
				loggerWrapper = item1;
			}
			return loggerWrapper;
		}

		private void ILoggerRepository_Shutdown(object sender, EventArgs e)
		{
			ILoggerRepository loggerRepository = sender as ILoggerRepository;
			if (loggerRepository != null)
			{
				this.RepositoryShutdown(loggerRepository);
			}
		}

		protected virtual void RepositoryShutdown(ILoggerRepository repository)
		{
			lock (this)
			{
				this.m_repositories.Remove(repository);
				repository.ShutdownEvent -= this.m_shutdownHandler;
			}
		}
	}
}