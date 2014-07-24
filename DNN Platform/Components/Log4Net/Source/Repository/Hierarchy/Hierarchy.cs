using log4net.Appender;
using log4net.Core;
using log4net.Util;
using System;
using System.Collections;
using System.Xml;

namespace log4net.Repository.Hierarchy
{
	public class Hierarchy : LoggerRepositorySkeleton, IBasicRepositoryConfigurator, IXmlRepositoryConfigurator
	{
		private ILoggerFactory m_defaultFactory;

		private Hashtable m_ht;

		private Logger m_root;

		private bool m_emittedNoAppenderWarning;

		private readonly static Type declaringType;

		public bool EmittedNoAppenderWarning
		{
			get
			{
				return this.m_emittedNoAppenderWarning;
			}
			set
			{
				this.m_emittedNoAppenderWarning = value;
			}
		}

		public ILoggerFactory LoggerFactory
		{
			get
			{
				return this.m_defaultFactory;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				this.m_defaultFactory = value;
			}
		}

		public Logger Root
		{
			get
			{
				if (this.m_root == null)
				{
					lock (this)
					{
						if (this.m_root == null)
						{
							Logger logger = this.m_defaultFactory.CreateLogger(this, null);
							logger.Hierarchy = this;
							this.m_root = logger;
						}
					}
				}
				return this.m_root;
			}
		}

		static Hierarchy()
		{
			log4net.Repository.Hierarchy.Hierarchy.declaringType = typeof(log4net.Repository.Hierarchy.Hierarchy);
		}

		public Hierarchy() : this(new DefaultLoggerFactory())
		{
		}

		public Hierarchy(PropertiesDictionary properties) : this(properties, new DefaultLoggerFactory())
		{
		}

		public Hierarchy(ILoggerFactory loggerFactory) : this(new PropertiesDictionary(), loggerFactory)
		{
		}

		public Hierarchy(PropertiesDictionary properties, ILoggerFactory loggerFactory) : base(properties)
		{
			if (loggerFactory == null)
			{
				throw new ArgumentNullException("loggerFactory");
			}
			this.m_defaultFactory = loggerFactory;
			this.m_ht = Hashtable.Synchronized(new Hashtable());
		}

		internal void AddLevel(log4net.Repository.Hierarchy.Hierarchy.LevelEntry levelEntry)
		{
			if (levelEntry == null)
			{
				throw new ArgumentNullException("levelEntry");
			}
			if (levelEntry.Name == null)
			{
				throw new ArgumentNullException("levelEntry.Name");
			}
			if (levelEntry.Value == -1)
			{
				Level item = this.LevelMap[levelEntry.Name];
				if (item == null)
				{
					throw new InvalidOperationException(string.Concat("Cannot redefine level [", levelEntry.Name, "] because it is not defined in the LevelMap. To define the level supply the level value."));
				}
				levelEntry.Value = item.Value;
			}
			this.LevelMap.Add(levelEntry.Name, levelEntry.Value, levelEntry.DisplayName);
		}

		internal void AddProperty(PropertyEntry propertyEntry)
		{
			if (propertyEntry == null)
			{
				throw new ArgumentNullException("propertyEntry");
			}
			if (propertyEntry.Key == null)
			{
				throw new ArgumentNullException("propertyEntry.Key");
			}
			base.Properties[propertyEntry.Key] = propertyEntry.Value;
		}

		protected void BasicRepositoryConfigure(params IAppender[] appenders)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				IAppender[] appenderArray = appenders;
				for (int i = 0; i < (int)appenderArray.Length; i++)
				{
					IAppender appender = appenderArray[i];
					this.Root.AddAppender(appender);
				}
			}
			this.Configured = true;
			this.ConfigurationMessages = arrayLists;
			this.OnConfigurationChanged(new ConfigurationChangedEventArgs(arrayLists));
		}

		public void Clear()
		{
			this.m_ht.Clear();
		}

		private static void CollectAppender(ArrayList appenderList, IAppender appender)
		{
			if (!appenderList.Contains(appender))
			{
				appenderList.Add(appender);
				IAppenderAttachable appenderAttachable = appender as IAppenderAttachable;
				if (appenderAttachable != null)
				{
					log4net.Repository.Hierarchy.Hierarchy.CollectAppenders(appenderList, appenderAttachable);
				}
			}
		}

		private static void CollectAppenders(ArrayList appenderList, IAppenderAttachable container)
		{
			foreach (IAppender appender in container.Appenders)
			{
				log4net.Repository.Hierarchy.Hierarchy.CollectAppender(appenderList, appender);
			}
		}

		public override ILogger Exists(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return this.m_ht[new LoggerKey(name)] as Logger;
		}

		public override IAppender[] GetAppenders()
		{
			ArrayList arrayLists = new ArrayList();
			log4net.Repository.Hierarchy.Hierarchy.CollectAppenders(arrayLists, this.Root);
			ILogger[] currentLoggers = this.GetCurrentLoggers();
			for (int i = 0; i < (int)currentLoggers.Length; i++)
			{
				log4net.Repository.Hierarchy.Hierarchy.CollectAppenders(arrayLists, (Logger)currentLoggers[i]);
			}
			return (IAppender[])arrayLists.ToArray(typeof(IAppender));
		}

		public override ILogger[] GetCurrentLoggers()
		{
			ArrayList arrayLists = new ArrayList(this.m_ht.Count);
			foreach (object value in this.m_ht.Values)
			{
				if (!(value is Logger))
				{
					continue;
				}
				arrayLists.Add(value);
			}
			return (Logger[])arrayLists.ToArray(typeof(Logger));
		}

		public override ILogger GetLogger(string name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			return this.GetLogger(name, this.m_defaultFactory);
		}

		public Logger GetLogger(string name, ILoggerFactory factory)
		{
			Logger logger;
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}
			LoggerKey loggerKey = new LoggerKey(name);
			lock (this.m_ht)
			{
				Logger logger1 = null;
				object item = this.m_ht[loggerKey];
				if (item != null)
				{
					Logger logger2 = item as Logger;
					if (logger2 == null)
					{
						ProvisionNode provisionNode = item as ProvisionNode;
						if (provisionNode == null)
						{
							logger = null;
						}
						else
						{
							logger1 = factory.CreateLogger(this, name);
							logger1.Hierarchy = this;
							this.m_ht[loggerKey] = logger1;
							log4net.Repository.Hierarchy.Hierarchy.UpdateChildren(provisionNode, logger1);
							this.UpdateParents(logger1);
							this.OnLoggerCreationEvent(logger1);
							logger = logger1;
						}
					}
					else
					{
						logger = logger2;
					}
				}
				else
				{
					logger1 = factory.CreateLogger(this, name);
					logger1.Hierarchy = this;
					this.m_ht[loggerKey] = logger1;
					this.UpdateParents(logger1);
					this.OnLoggerCreationEvent(logger1);
					logger = logger1;
				}
			}
			return logger;
		}

		public bool IsDisabled(Level level)
		{
			if (level == null)
			{
				throw new ArgumentNullException("level");
			}
			if (!this.Configured)
			{
				return true;
			}
			return this.Threshold > level;
		}

		public override void Log(LoggingEvent logEvent)
		{
			if (logEvent == null)
			{
				throw new ArgumentNullException("logEvent");
			}
			this.GetLogger(logEvent.LoggerName, this.m_defaultFactory).Log(logEvent);
		}

		void log4net.Repository.IBasicRepositoryConfigurator.Configure(IAppender appender)
		{
			this.BasicRepositoryConfigure(new IAppender[] { appender });
		}

		void log4net.Repository.IBasicRepositoryConfigurator.Configure(params IAppender[] appenders)
		{
			this.BasicRepositoryConfigure(appenders);
		}

		void log4net.Repository.IXmlRepositoryConfigurator.Configure(XmlElement element)
		{
			this.XmlRepositoryConfigure(element);
		}

		protected virtual void OnLoggerCreationEvent(Logger logger)
		{
			LoggerCreationEventHandler loggerCreationEventHandler = this.m_loggerCreatedEvent;
			if (loggerCreationEventHandler != null)
			{
				loggerCreationEventHandler(this, new LoggerCreationEventArgs(logger));
			}
		}

		public override void ResetConfiguration()
		{
			this.Root.Level = this.LevelMap.LookupWithDefault(Level.Debug);
			this.Threshold = this.LevelMap.LookupWithDefault(Level.All);
			lock (this.m_ht)
			{
				this.Shutdown();
				ILogger[] currentLoggers = this.GetCurrentLoggers();
				for (int i = 0; i < (int)currentLoggers.Length; i++)
				{
					Logger logger = (Logger)currentLoggers[i];
					logger.Level = null;
					logger.Additivity = true;
				}
			}
			base.ResetConfiguration();
			this.OnConfigurationChanged(null);
		}

		public override void Shutdown()
		{
			LogLog.Debug(log4net.Repository.Hierarchy.Hierarchy.declaringType, string.Concat("Shutdown called on Hierarchy [", this.Name, "]"));
			this.Root.CloseNestedAppenders();
			lock (this.m_ht)
			{
				ILogger[] currentLoggers = this.GetCurrentLoggers();
				ILogger[] loggerArray = currentLoggers;
				for (int i = 0; i < (int)loggerArray.Length; i++)
				{
					((Logger)loggerArray[i]).CloseNestedAppenders();
				}
				this.Root.RemoveAllAppenders();
				ILogger[] loggerArray1 = currentLoggers;
				for (int j = 0; j < (int)loggerArray1.Length; j++)
				{
					((Logger)loggerArray1[j]).RemoveAllAppenders();
				}
			}
			base.Shutdown();
		}

		private static void UpdateChildren(ProvisionNode pn, Logger log)
		{
			for (int i = 0; i < pn.Count; i++)
			{
				Logger item = (Logger)pn[i];
				if (!item.Parent.Name.StartsWith(log.Name))
				{
					log.Parent = item.Parent;
					item.Parent = log;
				}
			}
		}

		private void UpdateParents(Logger log)
		{
			string name = log.Name;
			int length = name.Length;
			bool flag = false;
			for (int i = name.LastIndexOf('.', length - 1); i >= 0; i = name.LastIndexOf('.', i - 1))
			{
				LoggerKey loggerKey = new LoggerKey(name.Substring(0, i));
				object item = this.m_ht[loggerKey];
				if (item != null)
				{
					Logger logger = item as Logger;
					if (logger == null)
					{
						ProvisionNode provisionNode = item as ProvisionNode;
						if (provisionNode == null)
						{
							LogLog.Error(log4net.Repository.Hierarchy.Hierarchy.declaringType, string.Concat("Unexpected object type [", item.GetType(), "] in ht."), new LogException());
						}
						else
						{
							provisionNode.Add(log);
						}
					}
					else
					{
						flag = true;
						log.Parent = logger;
						break;
					}
				}
				else
				{
					ProvisionNode provisionNode1 = new ProvisionNode(log);
					this.m_ht[loggerKey] = provisionNode1;
				}
			}
			if (!flag)
			{
				log.Parent = this.Root;
			}
		}

		protected void XmlRepositoryConfigure(XmlElement element)
		{
			ArrayList arrayLists = new ArrayList();
			using (LogLog.LogReceivedAdapter logReceivedAdapter = new LogLog.LogReceivedAdapter(arrayLists))
			{
				(new XmlHierarchyConfigurator(this)).Configure(element);
			}
			this.Configured = true;
			this.ConfigurationMessages = arrayLists;
			this.OnConfigurationChanged(new ConfigurationChangedEventArgs(arrayLists));
		}

		public event LoggerCreationEventHandler LoggerCreatedEvent
		{
			add
			{
				this.m_loggerCreatedEvent += value;
			}
			remove
			{
				this.m_loggerCreatedEvent -= value;
			}
		}

		private event LoggerCreationEventHandler m_loggerCreatedEvent;

		internal class LevelEntry
		{
			private int m_levelValue;

			private string m_levelName;

			private string m_levelDisplayName;

			public string DisplayName
			{
				get
				{
					return this.m_levelDisplayName;
				}
				set
				{
					this.m_levelDisplayName = value;
				}
			}

			public string Name
			{
				get
				{
					return this.m_levelName;
				}
				set
				{
					this.m_levelName = value;
				}
			}

			public int Value
			{
				get
				{
					return this.m_levelValue;
				}
				set
				{
					this.m_levelValue = value;
				}
			}

			public LevelEntry()
			{
			}

			public override string ToString()
			{
				object[] mLevelValue = new object[] { "LevelEntry(Value=", this.m_levelValue, ", Name=", this.m_levelName, ", DisplayName=", this.m_levelDisplayName, ")" };
				return string.Concat(mLevelValue);
			}
		}
	}
}