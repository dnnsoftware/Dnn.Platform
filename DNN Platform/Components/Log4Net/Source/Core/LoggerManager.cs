using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Util;
using System;
using System.Reflection;
using System.Security;
using System.Text;

namespace log4net.Core
{
	public sealed class LoggerManager
	{
		private readonly static Type declaringType;

		private static IRepositorySelector s_repositorySelector;

		public static IRepositorySelector RepositorySelector
		{
			get
			{
				return LoggerManager.s_repositorySelector;
			}
			set
			{
				LoggerManager.s_repositorySelector = value;
			}
		}

		static LoggerManager()
		{
			LoggerManager.declaringType = typeof(LoggerManager);
			try
			{
				LoggerManager.RegisterAppDomainEvents();
			}
			catch (SecurityException securityException)
			{
				LogLog.Debug(LoggerManager.declaringType, "Security Exception (ControlAppDomain LinkDemand) while trying to register Shutdown handler with the AppDomain. LoggerManager.Shutdown() will not be called automatically when the AppDomain exits. It must be called programmatically.");
			}
			LogLog.Debug(LoggerManager.declaringType, LoggerManager.GetVersionInfo());
			string appSetting = SystemInfo.GetAppSetting("log4net.RepositorySelector");
			if (appSetting != null && appSetting.Length > 0)
			{
				Type typeFromString = null;
				try
				{
					typeFromString = SystemInfo.GetTypeFromString(appSetting, false, true);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					LogLog.Error(LoggerManager.declaringType, string.Concat("Exception while resolving RepositorySelector Type [", appSetting, "]"), exception);
				}
				if (typeFromString != null)
				{
					object obj = null;
					try
					{
						obj = Activator.CreateInstance(typeFromString);
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						LogLog.Error(LoggerManager.declaringType, string.Concat("Exception while creating RepositorySelector [", typeFromString.FullName, "]"), exception2);
					}
					if (obj == null || !(obj is IRepositorySelector))
					{
						LogLog.Error(LoggerManager.declaringType, string.Concat("RepositorySelector Type [", typeFromString.FullName, "] is not an IRepositorySelector"));
					}
					else
					{
						LoggerManager.s_repositorySelector = (IRepositorySelector)obj;
					}
				}
			}
			if (LoggerManager.s_repositorySelector == null)
			{
				LoggerManager.s_repositorySelector = new DefaultRepositorySelector(typeof(Hierarchy));
			}
		}

		private LoggerManager()
		{
		}

		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(string repository)
		{
			return LoggerManager.CreateRepository(repository);
		}

		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(string repository, Type repositoryType)
		{
			return LoggerManager.CreateRepository(repository, repositoryType);
		}

		[Obsolete("Use CreateRepository instead of CreateDomain")]
		public static ILoggerRepository CreateDomain(Assembly repositoryAssembly, Type repositoryType)
		{
			return LoggerManager.CreateRepository(repositoryAssembly, repositoryType);
		}

		public static ILoggerRepository CreateRepository(string repository)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			return LoggerManager.RepositorySelector.CreateRepository(repository, null);
		}

		public static ILoggerRepository CreateRepository(string repository, Type repositoryType)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			if (repositoryType == null)
			{
				throw new ArgumentNullException("repositoryType");
			}
			return LoggerManager.RepositorySelector.CreateRepository(repository, repositoryType);
		}

		public static ILoggerRepository CreateRepository(Assembly repositoryAssembly, Type repositoryType)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			if (repositoryType == null)
			{
				throw new ArgumentNullException("repositoryType");
			}
			return LoggerManager.RepositorySelector.CreateRepository(repositoryAssembly, repositoryType);
		}

		public static ILogger Exists(string repository, string name)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return LoggerManager.RepositorySelector.GetRepository(repository).Exists(name);
		}

		public static ILogger Exists(Assembly repositoryAssembly, string name)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return LoggerManager.RepositorySelector.GetRepository(repositoryAssembly).Exists(name);
		}

		public static ILoggerRepository[] GetAllRepositories()
		{
			return LoggerManager.RepositorySelector.GetAllRepositories();
		}

		public static ILogger[] GetCurrentLoggers(string repository)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			return LoggerManager.RepositorySelector.GetRepository(repository).GetCurrentLoggers();
		}

		public static ILogger[] GetCurrentLoggers(Assembly repositoryAssembly)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			return LoggerManager.RepositorySelector.GetRepository(repositoryAssembly).GetCurrentLoggers();
		}

		public static ILogger GetLogger(string repository, string name)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return LoggerManager.RepositorySelector.GetRepository(repository).GetLogger(name);
		}

		public static ILogger GetLogger(Assembly repositoryAssembly, string name)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return LoggerManager.RepositorySelector.GetRepository(repositoryAssembly).GetLogger(name);
		}

		public static ILogger GetLogger(string repository, Type type)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return LoggerManager.RepositorySelector.GetRepository(repository).GetLogger(type.FullName);
		}

		public static ILogger GetLogger(Assembly repositoryAssembly, Type type)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			if (type == null)
			{
				throw new ArgumentNullException("type");
			}
			return LoggerManager.RepositorySelector.GetRepository(repositoryAssembly).GetLogger(type.FullName);
		}

		[Obsolete("Use GetRepository instead of GetLoggerRepository")]
		public static ILoggerRepository GetLoggerRepository(string repository)
		{
			return LoggerManager.GetRepository(repository);
		}

		[Obsolete("Use GetRepository instead of GetLoggerRepository")]
		public static ILoggerRepository GetLoggerRepository(Assembly repositoryAssembly)
		{
			return LoggerManager.GetRepository(repositoryAssembly);
		}

		public static ILoggerRepository GetRepository(string repository)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			return LoggerManager.RepositorySelector.GetRepository(repository);
		}

		public static ILoggerRepository GetRepository(Assembly repositoryAssembly)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			return LoggerManager.RepositorySelector.GetRepository(repositoryAssembly);
		}

		private static string GetVersionInfo()
		{
			StringBuilder stringBuilder = new StringBuilder();
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			stringBuilder.Append("log4net assembly [").Append(executingAssembly.FullName).Append("]. ");
			stringBuilder.Append("Loaded from [").Append(SystemInfo.AssemblyLocationInfo(executingAssembly)).Append("]. ");
			stringBuilder.Append("(.NET Runtime [").Append(Environment.Version.ToString()).Append("]");
			stringBuilder.Append(" on ").Append(Environment.OSVersion.ToString());
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}

		private static void OnDomainUnload(object sender, EventArgs e)
		{
			LoggerManager.Shutdown();
		}

		private static void OnProcessExit(object sender, EventArgs e)
		{
			LoggerManager.Shutdown();
		}

		private static void RegisterAppDomainEvents()
		{
			AppDomain.CurrentDomain.ProcessExit += new EventHandler(LoggerManager.OnProcessExit);
			AppDomain.CurrentDomain.DomainUnload += new EventHandler(LoggerManager.OnDomainUnload);
		}

		public static void ResetConfiguration(string repository)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			LoggerManager.RepositorySelector.GetRepository(repository).ResetConfiguration();
		}

		public static void ResetConfiguration(Assembly repositoryAssembly)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			LoggerManager.RepositorySelector.GetRepository(repositoryAssembly).ResetConfiguration();
		}

		public static void Shutdown()
		{
			ILoggerRepository[] allRepositories = LoggerManager.GetAllRepositories();
			for (int i = 0; i < (int)allRepositories.Length; i++)
			{
				allRepositories[i].Shutdown();
			}
		}

		public static void ShutdownRepository(string repository)
		{
			if (repository == null)
			{
				throw new ArgumentNullException("repository");
			}
			LoggerManager.RepositorySelector.GetRepository(repository).Shutdown();
		}

		public static void ShutdownRepository(Assembly repositoryAssembly)
		{
			if (repositoryAssembly == null)
			{
				throw new ArgumentNullException("repositoryAssembly");
			}
			LoggerManager.RepositorySelector.GetRepository(repositoryAssembly).Shutdown();
		}
	}
}