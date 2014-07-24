using log4net.Repository;
using log4net.Util;
using System;
using System.Collections;
using System.Reflection;

namespace log4net.Core
{
	public class CompactRepositorySelector : IRepositorySelector
	{
		private const string DefaultRepositoryName = "log4net-default-repository";

		private readonly Hashtable m_name2repositoryMap = new Hashtable();

		private readonly Type m_defaultRepositoryType;

		private readonly static Type declaringType;

		static CompactRepositorySelector()
		{
			CompactRepositorySelector.declaringType = typeof(CompactRepositorySelector);
		}

		public CompactRepositorySelector(Type defaultRepositoryType)
		{
			if (defaultRepositoryType == null)
			{
				throw new ArgumentNullException("defaultRepositoryType");
			}
			if (!typeof(ILoggerRepository).IsAssignableFrom(defaultRepositoryType))
			{
				throw SystemInfo.CreateArgumentOutOfRangeException("defaultRepositoryType", defaultRepositoryType, string.Concat("Parameter: defaultRepositoryType, Value: [", defaultRepositoryType, "] out of range. Argument must implement the ILoggerRepository interface"));
			}
			this.m_defaultRepositoryType = defaultRepositoryType;
			LogLog.Debug(CompactRepositorySelector.declaringType, string.Concat("defaultRepositoryType [", this.m_defaultRepositoryType, "]"));
		}

		public ILoggerRepository CreateRepository(Assembly assembly, Type repositoryType)
		{
			ILoggerRepository loggerRepository;
			if (repositoryType == null)
			{
				repositoryType = this.m_defaultRepositoryType;
			}
			lock (this)
			{
				ILoggerRepository item = this.m_name2repositoryMap["log4net-default-repository"] as ILoggerRepository ?? this.CreateRepository("log4net-default-repository", repositoryType);
				loggerRepository = item;
			}
			return loggerRepository;
		}

		public ILoggerRepository CreateRepository(string repositoryName, Type repositoryType)
		{
			ILoggerRepository loggerRepository;
			if (repositoryName == null)
			{
				throw new ArgumentNullException("repositoryName");
			}
			if (repositoryType == null)
			{
				repositoryType = this.m_defaultRepositoryType;
			}
			lock (this)
			{
				ILoggerRepository item = null;
				item = this.m_name2repositoryMap[repositoryName] as ILoggerRepository;
				if (item != null)
				{
					throw new LogException(string.Concat("Repository [", repositoryName, "] is already defined. Repositories cannot be redefined."));
				}
				Type type = CompactRepositorySelector.declaringType;
				object[] objArray = new object[] { "Creating repository [", repositoryName, "] using type [", repositoryType, "]" };
				LogLog.Debug(type, string.Concat(objArray));
				item = (ILoggerRepository)Activator.CreateInstance(repositoryType);
				item.Name = repositoryName;
				this.m_name2repositoryMap[repositoryName] = item;
				this.OnLoggerRepositoryCreatedEvent(item);
				loggerRepository = item;
			}
			return loggerRepository;
		}

		public bool ExistsRepository(string repositoryName)
		{
			bool flag;
			lock (this)
			{
				flag = this.m_name2repositoryMap.ContainsKey(repositoryName);
			}
			return flag;
		}

		public ILoggerRepository[] GetAllRepositories()
		{
			ILoggerRepository[] loggerRepositoryArray;
			lock (this)
			{
				ICollection values = this.m_name2repositoryMap.Values;
				ILoggerRepository[] loggerRepositoryArray1 = new ILoggerRepository[values.Count];
				values.CopyTo(loggerRepositoryArray1, 0);
				loggerRepositoryArray = loggerRepositoryArray1;
			}
			return loggerRepositoryArray;
		}

		public ILoggerRepository GetRepository(Assembly assembly)
		{
			return this.CreateRepository(assembly, this.m_defaultRepositoryType);
		}

		public ILoggerRepository GetRepository(string repositoryName)
		{
			ILoggerRepository loggerRepository;
			if (repositoryName == null)
			{
				throw new ArgumentNullException("repositoryName");
			}
			lock (this)
			{
				ILoggerRepository item = this.m_name2repositoryMap[repositoryName] as ILoggerRepository;
				if (item == null)
				{
					throw new LogException(string.Concat("Repository [", repositoryName, "] is NOT defined."));
				}
				loggerRepository = item;
			}
			return loggerRepository;
		}

		protected virtual void OnLoggerRepositoryCreatedEvent(ILoggerRepository repository)
		{
			LoggerRepositoryCreationEventHandler loggerRepositoryCreationEventHandler = this.m_loggerRepositoryCreatedEvent;
			if (loggerRepositoryCreationEventHandler != null)
			{
				loggerRepositoryCreationEventHandler(this, new LoggerRepositoryCreationEventArgs(repository));
			}
		}

		public event LoggerRepositoryCreationEventHandler LoggerRepositoryCreatedEvent
		{
			add
			{
				this.m_loggerRepositoryCreatedEvent += value;
			}
			remove
			{
				this.m_loggerRepositoryCreatedEvent -= value;
			}
		}

		private event LoggerRepositoryCreationEventHandler m_loggerRepositoryCreatedEvent;
	}
}