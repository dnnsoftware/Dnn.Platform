namespace log4net.Core
{
	public abstract class LoggerWrapperImpl : ILoggerWrapper
	{
		private readonly ILogger m_logger;

		public virtual ILogger Logger
		{
			get
			{
				return this.m_logger;
			}
		}

		protected LoggerWrapperImpl(ILogger logger)
		{
			this.m_logger = logger;
		}
	}
}