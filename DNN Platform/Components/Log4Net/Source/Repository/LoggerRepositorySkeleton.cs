using log4net.Appender;
using log4net.Core;
using log4net.ObjectRenderer;
using log4net.Plugin;
using log4net.Util;
using System;
using System.Collections;

namespace log4net.Repository
{
	public abstract class LoggerRepositorySkeleton : ILoggerRepository
	{
		private string m_name;

		private log4net.ObjectRenderer.RendererMap m_rendererMap;

		private log4net.Plugin.PluginMap m_pluginMap;

		private log4net.Core.LevelMap m_levelMap;

		private Level m_threshold;

		private bool m_configured;

		private ICollection m_configurationMessages;

		private PropertiesDictionary m_properties;

		private readonly static Type declaringType;

		public virtual ICollection ConfigurationMessages
		{
			get
			{
				return this.m_configurationMessages;
			}
			set
			{
				this.m_configurationMessages = value;
			}
		}

		public virtual bool Configured
		{
			get
			{
				return this.m_configured;
			}
			set
			{
				this.m_configured = value;
			}
		}

		public virtual log4net.Core.LevelMap LevelMap
		{
			get
			{
				return this.m_levelMap;
			}
		}

		public virtual string Name
		{
			get
			{
				return this.m_name;
			}
			set
			{
				this.m_name = value;
			}
		}

		public virtual log4net.Plugin.PluginMap PluginMap
		{
			get
			{
				return this.m_pluginMap;
			}
		}

		public PropertiesDictionary Properties
		{
			get
			{
				return this.m_properties;
			}
		}

		public virtual log4net.ObjectRenderer.RendererMap RendererMap
		{
			get
			{
				return this.m_rendererMap;
			}
		}

		public virtual Level Threshold
		{
			get
			{
				return this.m_threshold;
			}
			set
			{
				if (value != null)
				{
					this.m_threshold = value;
					return;
				}
				LogLog.Warn(LoggerRepositorySkeleton.declaringType, "LoggerRepositorySkeleton: Threshold cannot be set to null. Setting to ALL");
				this.m_threshold = Level.All;
			}
		}

		static LoggerRepositorySkeleton()
		{
			LoggerRepositorySkeleton.declaringType = typeof(LoggerRepositorySkeleton);
		}

		protected LoggerRepositorySkeleton() : this(new PropertiesDictionary())
		{
		}

		protected LoggerRepositorySkeleton(PropertiesDictionary properties)
		{
			this.m_properties = properties;
			this.m_rendererMap = new log4net.ObjectRenderer.RendererMap();
			this.m_pluginMap = new log4net.Plugin.PluginMap(this);
			this.m_levelMap = new log4net.Core.LevelMap();
			this.m_configurationMessages = EmptyCollection.Instance;
			this.m_configured = false;
			this.AddBuiltinLevels();
			this.m_threshold = Level.All;
		}

		private void AddBuiltinLevels()
		{
			this.m_levelMap.Add(Level.Off);
			this.m_levelMap.Add(Level.Emergency);
			this.m_levelMap.Add(Level.Fatal);
			this.m_levelMap.Add(Level.Alert);
			this.m_levelMap.Add(Level.Critical);
			this.m_levelMap.Add(Level.Severe);
			this.m_levelMap.Add(Level.Error);
			this.m_levelMap.Add(Level.Warn);
			this.m_levelMap.Add(Level.Notice);
			this.m_levelMap.Add(Level.Info);
			this.m_levelMap.Add(Level.Debug);
			this.m_levelMap.Add(Level.Fine);
			this.m_levelMap.Add(Level.Trace);
			this.m_levelMap.Add(Level.Finer);
			this.m_levelMap.Add(Level.Verbose);
			this.m_levelMap.Add(Level.Finest);
			this.m_levelMap.Add(Level.All);
		}

		public virtual void AddRenderer(Type typeToRender, IObjectRenderer rendererInstance)
		{
			if (typeToRender == null)
			{
				throw new ArgumentNullException("typeToRender");
			}
			if (rendererInstance == null)
			{
				throw new ArgumentNullException("rendererInstance");
			}
			this.m_rendererMap.Put(typeToRender, rendererInstance);
		}

		public abstract ILogger Exists(string name);

		public abstract IAppender[] GetAppenders();

		public abstract ILogger[] GetCurrentLoggers();

		public abstract ILogger GetLogger(string name);

		public abstract void Log(LoggingEvent logEvent);

		protected virtual void OnConfigurationChanged(EventArgs e)
		{
			if (e == null)
			{
				e = EventArgs.Empty;
			}
			LoggerRepositoryConfigurationChangedEventHandler loggerRepositoryConfigurationChangedEventHandler = this.m_configurationChangedEvent;
			if (loggerRepositoryConfigurationChangedEventHandler != null)
			{
				loggerRepositoryConfigurationChangedEventHandler(this, e);
			}
		}

		protected virtual void OnConfigurationReset(EventArgs e)
		{
			if (e == null)
			{
				e = EventArgs.Empty;
			}
			LoggerRepositoryConfigurationResetEventHandler loggerRepositoryConfigurationResetEventHandler = this.m_configurationResetEvent;
			if (loggerRepositoryConfigurationResetEventHandler != null)
			{
				loggerRepositoryConfigurationResetEventHandler(this, e);
			}
		}

		protected virtual void OnShutdown(EventArgs e)
		{
			if (e == null)
			{
				e = EventArgs.Empty;
			}
			LoggerRepositoryShutdownEventHandler loggerRepositoryShutdownEventHandler = this.m_shutdownEvent;
			if (loggerRepositoryShutdownEventHandler != null)
			{
				loggerRepositoryShutdownEventHandler(this, e);
			}
		}

		public void RaiseConfigurationChanged(EventArgs e)
		{
			this.OnConfigurationChanged(e);
		}

		public virtual void ResetConfiguration()
		{
			this.m_rendererMap.Clear();
			this.m_levelMap.Clear();
			this.m_configurationMessages = EmptyCollection.Instance;
			this.AddBuiltinLevels();
			this.Configured = false;
			this.OnConfigurationReset(null);
		}

		public virtual void Shutdown()
		{
			foreach (IPlugin allPlugin in this.PluginMap.AllPlugins)
			{
				allPlugin.Shutdown();
			}
			this.OnShutdown(null);
		}

		public event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged
		{
			add
			{
				this.m_configurationChangedEvent += value;
			}
			remove
			{
				this.m_configurationChangedEvent -= value;
			}
		}

		public event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset
		{
			add
			{
				this.m_configurationResetEvent += value;
			}
			remove
			{
				this.m_configurationResetEvent -= value;
			}
		}

		private event LoggerRepositoryConfigurationChangedEventHandler m_configurationChangedEvent;

		private event LoggerRepositoryConfigurationResetEventHandler m_configurationResetEvent;

		private event LoggerRepositoryShutdownEventHandler m_shutdownEvent;

		public event LoggerRepositoryShutdownEventHandler ShutdownEvent
		{
			add
			{
				this.m_shutdownEvent += value;
			}
			remove
			{
				this.m_shutdownEvent -= value;
			}
		}
	}
}