using log4net.Appender;
using log4net.Layout;
using log4net.Repository;
using log4net.Util;
using System;
using System.Collections;
using System.Reflection;

namespace log4net.Config
{
	public sealed class BasicConfigurator
	{
		private readonly static Type declaringType;

		static BasicConfigurator()
		{
			BasicConfigurator.declaringType = typeof(BasicConfigurator);
		}

		private BasicConfigurator()
		{
		}

		public static ICollection Configure()
		{
			return BasicConfigurator.Configure(LogManager.GetRepository(Assembly.GetCallingAssembly()));
		}

		public static ICollection Configure(IAppender appender)
		{
			return BasicConfigurator.Configure(new IAppender[] { appender });
		}

		public static ICollection Configure(params IAppender[] appenders)
		{
			ArrayList arrayLists = new ArrayList();
			ILoggerRepository repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				BasicConfigurator.InternalConfigure(repository, appenders);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(ILoggerRepository repository)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				PatternLayout patternLayout = new PatternLayout()
				{
					ConversionPattern = "%timestamp [%thread] %level %logger %ndc - %message%newline"
				};
				patternLayout.ActivateOptions();
				ConsoleAppender consoleAppender = new ConsoleAppender()
				{
					Layout = patternLayout
				};
				consoleAppender.ActivateOptions();
				BasicConfigurator.InternalConfigure(repository, new IAppender[] { consoleAppender });
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		public static ICollection Configure(ILoggerRepository repository, IAppender appender)
		{
			return BasicConfigurator.Configure(repository, new IAppender[] { appender });
		}

		public static ICollection Configure(ILoggerRepository repository, params IAppender[] appenders)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				BasicConfigurator.InternalConfigure(repository, appenders);
			}
			repository.ConfigurationMessages = arrayLists;
			return arrayLists;
		}

		private static void InternalConfigure(ILoggerRepository repository, params IAppender[] appenders)
		{
			IBasicRepositoryConfigurator basicRepositoryConfigurator = repository as IBasicRepositoryConfigurator;
			if (basicRepositoryConfigurator != null)
			{
				basicRepositoryConfigurator.Configure(appenders);
				return;
			}
			LogLog.Warn(BasicConfigurator.declaringType, string.Concat("BasicConfigurator: Repository [", repository, "] does not support the BasicConfigurator"));
		}
	}
}