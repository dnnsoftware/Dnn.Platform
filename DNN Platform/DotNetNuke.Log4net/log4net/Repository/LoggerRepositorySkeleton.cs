#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;
using System.Collections;
using log4net.ObjectRenderer;
using log4net.Core;
using log4net.Util;
using log4net.Plugin;

namespace log4net.Repository
{
	/// <summary>
	/// Base implementation of <see cref="ILoggerRepository"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Default abstract implementation of the <see cref="ILoggerRepository"/> interface.
	/// </para>
	/// <para>
	/// Skeleton implementation of the <see cref="ILoggerRepository"/> interface.
	/// All <see cref="ILoggerRepository"/> types can extend this type.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public abstract class LoggerRepositorySkeleton : ILoggerRepository
	{
		#region Member Variables

		private string m_name;
		private RendererMap m_rendererMap;
		private PluginMap m_pluginMap;
		private LevelMap m_levelMap;
		private Level m_threshold;
		private bool m_configured;
        private ICollection m_configurationMessages;
		private event LoggerRepositoryShutdownEventHandler m_shutdownEvent;
		private event LoggerRepositoryConfigurationResetEventHandler m_configurationResetEvent;
		private event LoggerRepositoryConfigurationChangedEventHandler m_configurationChangedEvent;
		private PropertiesDictionary m_properties;

		#endregion

		#region Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes the repository with default (empty) properties.
		/// </para>
		/// </remarks>
		protected LoggerRepositorySkeleton() : this(new PropertiesDictionary())
		{
		}

		/// <summary>
		/// Construct the repository using specific properties
		/// </summary>
		/// <param name="properties">the properties to set for this repository</param>
		/// <remarks>
		/// <para>
		/// Initializes the repository with specified properties.
		/// </para>
		/// </remarks>
		protected LoggerRepositorySkeleton(PropertiesDictionary properties)
		{
			m_properties = properties;
			m_rendererMap = new RendererMap();
			m_pluginMap = new PluginMap(this);
			m_levelMap = new LevelMap();
            m_configurationMessages = EmptyCollection.Instance;
			m_configured = false;

			AddBuiltinLevels();

			// Don't disable any levels by default.
			m_threshold = Level.All;
		}

		#endregion

		#region Implementation of ILoggerRepository

		/// <summary>
		/// The name of the repository
		/// </summary>
		/// <value>
		/// The string name of the repository
		/// </value>
		/// <remarks>
		/// <para>
		/// The name of this repository. The name is
		/// used to store and lookup the repositories 
		/// stored by the <see cref="IRepositorySelector"/>.
		/// </para>
		/// </remarks>
		virtual public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// The threshold for all events in this repository
		/// </summary>
		/// <value>
		/// The threshold for all events in this repository
		/// </value>
		/// <remarks>
		/// <para>
		/// The threshold for all events in this repository
		/// </para>
		/// </remarks>
		virtual public Level Threshold
		{
			get { return m_threshold; }
			set
			{ 
				if (value != null)
				{
					m_threshold = value; 
				}
				else
				{
					// Must not set threshold to null
					LogLog.Warn(declaringType, "LoggerRepositorySkeleton: Threshold cannot be set to null. Setting to ALL");
					m_threshold = Level.All;
				}
			}
		}

		/// <summary>
		/// RendererMap accesses the object renderer map for this repository.
		/// </summary>
		/// <value>
		/// RendererMap accesses the object renderer map for this repository.
		/// </value>
		/// <remarks>
		/// <para>
		/// RendererMap accesses the object renderer map for this repository.
		/// </para>
		/// <para>
		/// The RendererMap holds a mapping between types and
		/// <see cref="IObjectRenderer"/> objects.
		/// </para>
		/// </remarks>
		virtual public RendererMap RendererMap
		{
			get { return m_rendererMap; }
		}

		/// <summary>
		/// The plugin map for this repository.
		/// </summary>
		/// <value>
		/// The plugin map for this repository.
		/// </value>
		/// <remarks>
		/// <para>
		/// The plugin map holds the <see cref="IPlugin"/> instances
		/// that have been attached to this repository.
		/// </para>
		/// </remarks>
		virtual public PluginMap PluginMap
		{
			get { return m_pluginMap; }
		}

		/// <summary>
		/// Get the level map for the Repository.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Get the level map for the Repository.
		/// </para>
		/// <para>
		/// The level map defines the mappings between
		/// level names and <see cref="Level"/> objects in
		/// this repository.
		/// </para>
		/// </remarks>
		virtual public LevelMap LevelMap
		{
			get { return m_levelMap; }
		}

		/// <summary>
		/// Test if logger exists
		/// </summary>
		/// <param name="name">The name of the logger to lookup</param>
		/// <returns>The Logger object with the name specified</returns>
		/// <remarks>
		/// <para>
		/// Check if the named logger exists in the repository. If so return
		/// its reference, otherwise returns <c>null</c>.
		/// </para>
		/// </remarks>
		abstract public ILogger Exists(string name);

		/// <summary>
		/// Returns all the currently defined loggers in the repository
		/// </summary>
		/// <returns>All the defined loggers</returns>
		/// <remarks>
		/// <para>
		/// Returns all the currently defined loggers in the repository as an Array.
		/// </para>
		/// </remarks>
		abstract public ILogger[] GetCurrentLoggers();

		/// <summary>
		/// Return a new logger instance
		/// </summary>
		/// <param name="name">The name of the logger to retrieve</param>
		/// <returns>The logger object with the name specified</returns>
		/// <remarks>
		/// <para>
		/// Return a new logger instance.
		/// </para>
		/// <para>
		/// If a logger of that name already exists, then it will be
		/// returned. Otherwise, a new logger will be instantiated and
		/// then linked with its existing ancestors as well as children.
		/// </para>
		/// </remarks>
		abstract public ILogger GetLogger(string name);

		/// <summary>
		/// Shutdown the repository
		/// </summary>
		/// <remarks>
		/// <para>
		/// Shutdown the repository. Can be overridden in a subclass.
		/// This base class implementation notifies the <see cref="ShutdownEvent"/>
		/// listeners and all attached plugins of the shutdown event.
		/// </para>
		/// </remarks>
		virtual public void Shutdown() 
		{
			// Shutdown attached plugins
			foreach(IPlugin plugin in PluginMap.AllPlugins)
			{
				plugin.Shutdown();
			}

			// Notify listeners
			OnShutdown(null);
		}

		/// <summary>
		/// Reset the repositories configuration to a default state
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in this instance to their
		/// default state.
		/// </para>
		/// <para>
		/// Existing loggers are not removed. They are just reset.
		/// </para>
		/// <para>
		/// This method should be used sparingly and with care as it will
		/// block all logging until it is completed.
		/// </para>
		/// </remarks>
		virtual public void ResetConfiguration() 
		{
			// Clear internal data structures
			m_rendererMap.Clear();
			m_levelMap.Clear();
            m_configurationMessages = EmptyCollection.Instance;

			// Add the predefined levels to the map
			AddBuiltinLevels();

			Configured = false;

			// Notify listeners
			OnConfigurationReset(null);
		}

		/// <summary>
		/// Log the logEvent through this repository.
		/// </summary>
		/// <param name="logEvent">the event to log</param>
		/// <remarks>
		/// <para>
		/// This method should not normally be used to log.
		/// The <see cref="ILog"/> interface should be used 
		/// for routine logging. This interface can be obtained
		/// using the <see cref="log4net.LogManager.GetLogger(string)"/> method.
		/// </para>
		/// <para>
		/// The <c>logEvent</c> is delivered to the appropriate logger and
		/// that logger is then responsible for logging the event.
		/// </para>
		/// </remarks>
		abstract public void Log(LoggingEvent logEvent);

		/// <summary>
		/// Flag indicates if this repository has been configured.
		/// </summary>
		/// <value>
		/// Flag indicates if this repository has been configured.
		/// </value>
		/// <remarks>
		/// <para>
		/// Flag indicates if this repository has been configured.
		/// </para>
		/// </remarks>
		virtual public bool Configured 
		{ 
			get { return m_configured; }
			set { m_configured = value; }
		}

        /// <summary>
        /// Contains a list of internal messages captures during the 
        /// last configuration.
        /// </summary>
	    virtual public ICollection ConfigurationMessages
	    {
            get { return m_configurationMessages; }
            set { m_configurationMessages = value; }
	    }

	    /// <summary>
		/// Event to notify that the repository has been shutdown.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has been shutdown.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when the repository has been shutdown.
		/// </para>
		/// </remarks>
		public event LoggerRepositoryShutdownEventHandler ShutdownEvent
		{
			add { m_shutdownEvent += value; }
			remove { m_shutdownEvent -= value; }
		}

		/// <summary>
		/// Event to notify that the repository has had its configuration reset.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has had its configuration reset.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when the repository's configuration has been
		/// reset to default.
		/// </para>
		/// </remarks>
		public event LoggerRepositoryConfigurationResetEventHandler ConfigurationReset
		{
			add { m_configurationResetEvent += value; }
			remove { m_configurationResetEvent -= value; }
		}

		/// <summary>
		/// Event to notify that the repository has had its configuration changed.
		/// </summary>
		/// <value>
		/// Event to notify that the repository has had its configuration changed.
		/// </value>
		/// <remarks>
		/// <para>
		/// Event raised when the repository's configuration has been changed.
		/// </para>
		/// </remarks>
		public event LoggerRepositoryConfigurationChangedEventHandler ConfigurationChanged
		{
			add { m_configurationChangedEvent += value; }
			remove { m_configurationChangedEvent -= value; }
		}

		/// <summary>
		/// Repository specific properties
		/// </summary>
		/// <value>
		/// Repository specific properties
		/// </value>
		/// <remarks>
		/// These properties can be specified on a repository specific basis
		/// </remarks>
		public PropertiesDictionary Properties 
		{ 
			get { return m_properties; } 
		}

		/// <summary>
		/// Returns all the Appenders that are configured as an Array.
		/// </summary>
		/// <returns>All the Appenders</returns>
		/// <remarks>
		/// <para>
		/// Returns all the Appenders that are configured as an Array.
		/// </para>
		/// </remarks>
		abstract public log4net.Appender.IAppender[] GetAppenders();

		#endregion

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the LoggerRepositorySkeleton class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(LoggerRepositorySkeleton);

	    #endregion Private Static Fields

		private void AddBuiltinLevels()
		{
			// Add the predefined levels to the map
			m_levelMap.Add(Level.Off);

			// Unrecoverable errors
			m_levelMap.Add(Level.Emergency);
			m_levelMap.Add(Level.Fatal);
			m_levelMap.Add(Level.Alert); 

			// Recoverable errors
			m_levelMap.Add(Level.Critical); 
			m_levelMap.Add(Level.Severe); 
			m_levelMap.Add(Level.Error); 
			m_levelMap.Add(Level.Warn);

			// Information
			m_levelMap.Add(Level.Notice); 
			m_levelMap.Add(Level.Info); 

			// Debug
			m_levelMap.Add(Level.Debug);
			m_levelMap.Add(Level.Fine);
			m_levelMap.Add(Level.Trace);
			m_levelMap.Add(Level.Finer);
			m_levelMap.Add(Level.Verbose);
			m_levelMap.Add(Level.Finest);

			m_levelMap.Add(Level.All);
		}

		/// <summary>
		/// Adds an object renderer for a specific class. 
		/// </summary>
		/// <param name="typeToRender">The type that will be rendered by the renderer supplied.</param>
		/// <param name="rendererInstance">The object renderer used to render the object.</param>
		/// <remarks>
		/// <para>
		/// Adds an object renderer for a specific class. 
		/// </para>
		/// </remarks>
		virtual public void AddRenderer(Type typeToRender, IObjectRenderer rendererInstance) 
		{
			if (typeToRender == null)
			{
				throw new ArgumentNullException("typeToRender");
			}
			if (rendererInstance == null)
			{
				throw new ArgumentNullException("rendererInstance");
			}

			m_rendererMap.Put(typeToRender, rendererInstance);
		}

		/// <summary>
		/// Notify the registered listeners that the repository is shutting down
		/// </summary>
		/// <param name="e">Empty EventArgs</param>
		/// <remarks>
		/// <para>
		/// Notify any listeners that this repository is shutting down.
		/// </para>
		/// </remarks>
		protected virtual void OnShutdown(EventArgs e)
		{
			if (e == null)
			{
				e = EventArgs.Empty;
			}

			LoggerRepositoryShutdownEventHandler handler = m_shutdownEvent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Notify the registered listeners that the repository has had its configuration reset
		/// </summary>
		/// <param name="e">Empty EventArgs</param>
		/// <remarks>
		/// <para>
		/// Notify any listeners that this repository's configuration has been reset.
		/// </para>
		/// </remarks>
		protected virtual void OnConfigurationReset(EventArgs e)
		{
			if (e == null)
			{
				e = EventArgs.Empty;
			}

			LoggerRepositoryConfigurationResetEventHandler handler = m_configurationResetEvent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Notify the registered listeners that the repository has had its configuration changed
		/// </summary>
		/// <param name="e">Empty EventArgs</param>
		/// <remarks>
		/// <para>
		/// Notify any listeners that this repository's configuration has changed.
		/// </para>
		/// </remarks>
		protected virtual void OnConfigurationChanged(EventArgs e)
		{
			if (e == null)
			{
				e = EventArgs.Empty;
			}

			LoggerRepositoryConfigurationChangedEventHandler handler = m_configurationChangedEvent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		/// <summary>
		/// Raise a configuration changed event on this repository
		/// </summary>
		/// <param name="e">EventArgs.Empty</param>
		/// <remarks>
		/// <para>
		/// Applications that programmatically change the configuration of the repository should
		/// raise this event notification to notify listeners.
		/// </para>
		/// </remarks>
		public void RaiseConfigurationChanged(EventArgs e)
		{
			OnConfigurationChanged(e);
		}
	}
}
