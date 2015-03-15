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
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Util;

namespace log4net.Repository.Hierarchy
{
	#region LoggerCreationEvent

	/// <summary>
	/// Delegate used to handle logger creation event notifications.
	/// </summary>
	/// <param name="sender">The <see cref="Hierarchy"/> in which the <see cref="Logger"/> has been created.</param>
	/// <param name="e">The <see cref="LoggerCreationEventArgs"/> event args that hold the <see cref="Logger"/> instance that has been created.</param>
	/// <remarks>
	/// <para>
	/// Delegate used to handle logger creation event notifications.
	/// </para>
	/// </remarks>
	public delegate void LoggerCreationEventHandler(object sender, LoggerCreationEventArgs e);

	/// <summary>
	/// Provides data for the <see cref="Hierarchy.LoggerCreatedEvent"/> event.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Hierarchy.LoggerCreatedEvent"/> event is raised every time a
	/// <see cref="Logger"/> is created.
	/// </para>
	/// </remarks>
	public class LoggerCreationEventArgs : EventArgs
	{
		/// <summary>
		/// The <see cref="Logger"/> created
		/// </summary>
		private Logger m_log;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="log">The <see cref="Logger"/> that has been created.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="LoggerCreationEventArgs" /> event argument 
		/// class,with the specified <see cref="Logger"/>.
		/// </para>
		/// </remarks>
		public LoggerCreationEventArgs(Logger log)
		{
			m_log = log;
		}

		/// <summary>
		/// Gets the <see cref="Logger"/> that has been created.
		/// </summary>
		/// <value>
		/// The <see cref="Logger"/> that has been created.
		/// </value>
		/// <remarks>
		/// <para>
		/// The <see cref="Logger"/> that has been created.
		/// </para>
		/// </remarks>
		public Logger Logger
		{
			get { return m_log; }
		}
	}

	#endregion LoggerCreationEvent

	/// <summary>
	/// Hierarchical organization of loggers
	/// </summary>
	/// <remarks>
	/// <para>
	/// <i>The casual user should not have to deal with this class
	/// directly.</i>
	/// </para>
	/// <para>
	/// This class is specialized in retrieving loggers by name and
	/// also maintaining the logger hierarchy. Implements the 
	/// <see cref="ILoggerRepository"/> interface.
	/// </para>
	/// <para>
	/// The structure of the logger hierarchy is maintained by the
	/// <see cref="GetLogger(string)"/> method. The hierarchy is such that children
	/// link to their parent but parents do not have any references to their
	/// children. Moreover, loggers can be instantiated in any order, in
	/// particular descendant before ancestor.
	/// </para>
	/// <para>
	/// In case a descendant is created before a particular ancestor,
	/// then it creates a provision node for the ancestor and adds itself
	/// to the provision node. Other descendants of the same ancestor add
	/// themselves to the previously created provision node.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class Hierarchy : LoggerRepositorySkeleton, IBasicRepositoryConfigurator, IXmlRepositoryConfigurator
	{
		#region Public Events

		/// <summary>
		/// Event used to notify that a logger has been created.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Event raised when a logger is created.
		/// </para>
		/// </remarks>
		public event LoggerCreationEventHandler LoggerCreatedEvent
		{
			add { m_loggerCreatedEvent += value; }
			remove { m_loggerCreatedEvent -= value; }
		}

		#endregion Public Events

		#region Public Instance Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="Hierarchy" /> class.
		/// </para>
		/// </remarks>
		public Hierarchy() : this(new DefaultLoggerFactory())
		{
		}

		/// <summary>
		/// Construct with properties
		/// </summary>
		/// <param name="properties">The properties to pass to this repository.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="Hierarchy" /> class.
		/// </para>
		/// </remarks>
		public Hierarchy(PropertiesDictionary properties) : this(properties, new DefaultLoggerFactory())
		{
		}

		/// <summary>
		/// Construct with a logger factory
		/// </summary>
		/// <param name="loggerFactory">The factory to use to create new logger instances.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="Hierarchy" /> class with 
		/// the specified <see cref="ILoggerFactory" />.
		/// </para>
		/// </remarks>
		public Hierarchy(ILoggerFactory loggerFactory) : this(new PropertiesDictionary(), loggerFactory)
		{
		}

		/// <summary>
		/// Construct with properties and a logger factory
		/// </summary>
		/// <param name="properties">The properties to pass to this repository.</param>
		/// <param name="loggerFactory">The factory to use to create new logger instances.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="Hierarchy" /> class with 
		/// the specified <see cref="ILoggerFactory" />.
		/// </para>
		/// </remarks>
		public Hierarchy(PropertiesDictionary properties, ILoggerFactory loggerFactory) : base(properties)
		{
			if (loggerFactory == null)
			{
				throw new ArgumentNullException("loggerFactory");
			}

			m_defaultFactory = loggerFactory;

			m_ht = System.Collections.Hashtable.Synchronized(new System.Collections.Hashtable());
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Has no appender warning been emitted
		/// </summary>
		/// <remarks>
		/// <para>
		/// Flag to indicate if we have already issued a warning
		/// about not having an appender warning.
		/// </para>
		/// </remarks>
		public bool EmittedNoAppenderWarning
		{
			get { return m_emittedNoAppenderWarning; }
			set { m_emittedNoAppenderWarning = value; }
		}

		/// <summary>
		/// Get the root of this hierarchy
		/// </summary>
		/// <remarks>
		/// <para>
		/// Get the root of this hierarchy.
		/// </para>
		/// </remarks>
		public Logger Root
		{
			get 
			{ 
				if (m_root == null)
				{
					lock(this)
					{
						if (m_root == null)
						{
							// Create the root logger
							Logger root = m_defaultFactory.CreateLogger(this, null);
							root.Hierarchy = this;

							// Store root
							m_root = root;
						}
					}
				}
				return m_root; 
			}
		}

		/// <summary>
		/// Gets or sets the default <see cref="ILoggerFactory" /> instance.
		/// </summary>
		/// <value>The default <see cref="ILoggerFactory" /></value>
		/// <remarks>
		/// <para>
		/// The logger factory is used to create logger instances.
		/// </para>
		/// </remarks>
		public ILoggerFactory LoggerFactory
		{
			get { return m_defaultFactory; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				m_defaultFactory = value;
			}
		}

		#endregion Public Instance Properties

		#region Override Implementation of LoggerRepositorySkeleton

		/// <summary>
		/// Test if a logger exists
		/// </summary>
		/// <param name="name">The name of the logger to lookup</param>
		/// <returns>The Logger object with the name specified</returns>
		/// <remarks>
		/// <para>
		/// Check if the named logger exists in the hierarchy. If so return
		/// its reference, otherwise returns <c>null</c>.
		/// </para>
		/// </remarks>
		override public ILogger Exists(string name) 
		{	
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			return m_ht[new LoggerKey(name)] as Logger;
		}

		/// <summary>
		/// Returns all the currently defined loggers in the hierarchy as an Array
		/// </summary>
		/// <returns>All the defined loggers</returns>
		/// <remarks>
		/// <para>
		/// Returns all the currently defined loggers in the hierarchy as an Array.
		/// The root logger is <b>not</b> included in the returned
		/// enumeration.
		/// </para>
		/// </remarks>
		override public ILogger[] GetCurrentLoggers() 
		{
			// The accumulation in loggers is necessary because not all elements in
			// ht are Logger objects as there might be some ProvisionNodes
			// as well.
			System.Collections.ArrayList loggers = new System.Collections.ArrayList(m_ht.Count);
	
			// Iterate through m_ht values
			foreach(object node in m_ht.Values)
			{
				if (node is Logger) 
				{
					loggers.Add(node);
				}
			}
			return (Logger[])loggers.ToArray(typeof(Logger));
		}

		/// <summary>
		/// Return a new logger instance named as the first parameter using
		/// the default factory.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Return a new logger instance named as the first parameter using
		/// the default factory.
		/// </para>
		/// <para>
		/// If a logger of that name already exists, then it will be
		/// returned.  Otherwise, a new logger will be instantiated and
		/// then linked with its existing ancestors as well as children.
		/// </para>
		/// </remarks>
		/// <param name="name">The name of the logger to retrieve</param>
		/// <returns>The logger object with the name specified</returns>
		override public ILogger GetLogger(string name) 
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}

			return GetLogger(name, m_defaultFactory);
		}

		/// <summary>
		/// Shutting down a hierarchy will <i>safely</i> close and remove
		/// all appenders in all loggers including the root logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Shutting down a hierarchy will <i>safely</i> close and remove
		/// all appenders in all loggers including the root logger.
		/// </para>
		/// <para>
		/// Some appenders need to be closed before the
		/// application exists. Otherwise, pending logging events might be
		/// lost.
		/// </para>
		/// <para>
		/// The <c>Shutdown</c> method is careful to close nested
		/// appenders before closing regular appenders. This is allows
		/// configurations where a regular appender is attached to a logger
		/// and again to a nested appender.
		/// </para>
		/// </remarks>
		override public void Shutdown() 
		{
			LogLog.Debug(declaringType, "Shutdown called on Hierarchy ["+this.Name+"]");

			// begin by closing nested appenders
			Root.CloseNestedAppenders();

			lock(m_ht) 
			{
				ILogger[] currentLoggers = this.GetCurrentLoggers();

				foreach(Logger logger in currentLoggers)
				{
					logger.CloseNestedAppenders();
				}

				// then, remove all appenders
				Root.RemoveAllAppenders();

				foreach(Logger logger in currentLoggers)
				{
					logger.RemoveAllAppenders();
				}
			}

			base.Shutdown();
		}

		/// <summary>
		/// Reset all values contained in this hierarchy instance to their default.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Reset all values contained in this hierarchy instance to their
		/// default.  This removes all appenders from all loggers, sets
		/// the level of all non-root loggers to <c>null</c>,
		/// sets their additivity flag to <c>true</c> and sets the level
		/// of the root logger to <see cref="Level.Debug"/>. Moreover,
		/// message disabling is set its default "off" value.
		/// </para>
		/// <para>
		/// Existing loggers are not removed. They are just reset.
		/// </para>
		/// <para>
		/// This method should be used sparingly and with care as it will
		/// block all logging until it is completed.
		/// </para>
		/// </remarks>
		override public void ResetConfiguration() 
		{
			Root.Level = LevelMap.LookupWithDefault(Level.Debug);
			Threshold = LevelMap.LookupWithDefault(Level.All);
	
			// the synchronization is needed to prevent hashtable surprises
			lock(m_ht) 
			{	
				Shutdown(); // nested locks are OK	
	
				foreach(Logger l in this.GetCurrentLoggers())
				{
					l.Level = null;
					l.Additivity = true;
				}
			}

			base.ResetConfiguration();

			// Notify listeners
			OnConfigurationChanged(null);
		}

		/// <summary>
		/// Log the logEvent through this hierarchy.
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
		override public void Log(LoggingEvent logEvent)
		{
			if (logEvent == null)
			{
				throw new ArgumentNullException("logEvent");
			}

			this.GetLogger(logEvent.LoggerName, m_defaultFactory).Log(logEvent);
		}

		/// <summary>
		/// Returns all the Appenders that are currently configured
		/// </summary>
		/// <returns>An array containing all the currently configured appenders</returns>
		/// <remarks>
		/// <para>
		/// Returns all the <see cref="log4net.Appender.IAppender"/> instances that are currently configured.
		/// All the loggers are searched for appenders. The appenders may also be containers
		/// for appenders and these are also searched for additional loggers.
		/// </para>
		/// <para>
		/// The list returned is unordered but does not contain duplicates.
		/// </para>
		/// </remarks>
		override public Appender.IAppender[] GetAppenders()
		{
			System.Collections.ArrayList appenderList = new System.Collections.ArrayList();

			CollectAppenders(appenderList, Root);

			foreach(Logger logger in GetCurrentLoggers())
			{
				CollectAppenders(appenderList, logger);
			}

			return (Appender.IAppender[])appenderList.ToArray(typeof(Appender.IAppender));
		}

		#endregion Override Implementation of LoggerRepositorySkeleton

        #region Private Static Methods

        /// <summary>
		/// Collect the appenders from an <see cref="IAppenderAttachable"/>.
		/// The appender may also be a container.
		/// </summary>
		/// <param name="appenderList"></param>
		/// <param name="appender"></param>
		private static void CollectAppender(System.Collections.ArrayList appenderList, Appender.IAppender appender)
		{
			if (!appenderList.Contains(appender))
			{
				appenderList.Add(appender);

				IAppenderAttachable container = appender as IAppenderAttachable;
				if (container != null)
				{
					CollectAppenders(appenderList, container);
				}
			}
		}

		/// <summary>
		/// Collect the appenders from an <see cref="IAppenderAttachable"/> container
		/// </summary>
		/// <param name="appenderList"></param>
		/// <param name="container"></param>
		private static void CollectAppenders(System.Collections.ArrayList appenderList, IAppenderAttachable container)
		{
			foreach(Appender.IAppender appender in container.Appenders)
			{
				CollectAppender(appenderList, appender);
			}
        }

        #endregion

        #region Implementation of IBasicRepositoryConfigurator

        /// <summary>
		/// Initialize the log4net system using the specified appender
		/// </summary>
		/// <param name="appender">the appender to use to log all logging events</param>
		void IBasicRepositoryConfigurator.Configure(IAppender appender)
		{
			BasicRepositoryConfigure(appender);
		}

        /// <summary>
        /// Initialize the log4net system using the specified appenders
        /// </summary>
        /// <param name="appenders">the appenders to use to log all logging events</param>
        void IBasicRepositoryConfigurator.Configure(params IAppender[] appenders)
        {
            BasicRepositoryConfigure(appenders);
        }

		/// <summary>
		/// Initialize the log4net system using the specified appenders
		/// </summary>
		/// <param name="appenders">the appenders to use to log all logging events</param>
		/// <remarks>
		/// <para>
		/// This method provides the same functionality as the 
		/// <see cref="IBasicRepositoryConfigurator.Configure(IAppender)"/> method implemented
		/// on this object, but it is protected and therefore can be called by subclasses.
		/// </para>
		/// </remarks>
		protected void BasicRepositoryConfigure(params IAppender[] appenders)
		{
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
            {
                foreach (IAppender appender in appenders)
                {
                    Root.AddAppender(appender);
                }
            }

		    Configured = true;

            ConfigurationMessages = configurationMessages;

			// Notify listeners
            OnConfigurationChanged(new ConfigurationChangedEventArgs(configurationMessages));
		}

	    #endregion Implementation of IBasicRepositoryConfigurator

		#region Implementation of IXmlRepositoryConfigurator

		/// <summary>
		/// Initialize the log4net system using the specified config
		/// </summary>
		/// <param name="element">the element containing the root of the config</param>
		void IXmlRepositoryConfigurator.Configure(System.Xml.XmlElement element)
		{
			XmlRepositoryConfigure(element);
		}

		/// <summary>
		/// Initialize the log4net system using the specified config
		/// </summary>
		/// <param name="element">the element containing the root of the config</param>
		/// <remarks>
		/// <para>
		/// This method provides the same functionality as the 
		/// <see cref="IBasicRepositoryConfigurator.Configure(IAppender)"/> method implemented
		/// on this object, but it is protected and therefore can be called by subclasses.
		/// </para>
		/// </remarks>
		protected void XmlRepositoryConfigure(System.Xml.XmlElement element)
		{
            ArrayList configurationMessages = new ArrayList();

            using (new LogLog.LogReceivedAdapter(configurationMessages))
		    {
		        XmlHierarchyConfigurator config = new XmlHierarchyConfigurator(this);
                config.Configure(element);
		    }

		    Configured = true;

            ConfigurationMessages = configurationMessages;

			// Notify listeners
            OnConfigurationChanged(new ConfigurationChangedEventArgs(configurationMessages));
		}

		#endregion Implementation of IXmlRepositoryConfigurator

		#region Public Instance Methods

		/// <summary>
		/// Test if this hierarchy is disabled for the specified <see cref="Level"/>.
		/// </summary>
		/// <param name="level">The level to check against.</param>
		/// <returns>
		/// <c>true</c> if the repository is disabled for the level argument, <c>false</c> otherwise.
		/// </returns>
		/// <remarks>
		/// <para>
		/// If this hierarchy has not been configured then this method will
		/// always return <c>true</c>.
		/// </para>
		/// <para>
		/// This method will return <c>true</c> if this repository is
		/// disabled for <c>level</c> object passed as parameter and
		/// <c>false</c> otherwise.
		/// </para>
		/// <para>
		/// See also the <see cref="ILoggerRepository.Threshold"/> property.
		/// </para>
		/// </remarks>
		public bool IsDisabled(Level level) 
		{
			// Cast level to object for performance
			if ((object)level == null)
			{
				throw new ArgumentNullException("level");
			}

			if (Configured)
			{
				return Threshold > level;
			}
			else
			{
				// If not configured the hierarchy is effectively disabled
				return true;
			}
		}

		/// <summary>
		/// Clear all logger definitions from the internal hashtable
		/// </summary>
		/// <remarks>
		/// <para>
		/// This call will clear all logger definitions from the internal
		/// hashtable. Invoking this method will irrevocably mess up the
		/// logger hierarchy.
		/// </para>
		/// <para>
		/// You should <b>really</b> know what you are doing before
		/// invoking this method.
		/// </para>
		/// </remarks>
		public void Clear() 
		{
			m_ht.Clear();
		}

		/// <summary>
		/// Return a new logger instance named as the first parameter using
		/// <paramref name="factory"/>.
		/// </summary>
		/// <param name="name">The name of the logger to retrieve</param>
		/// <param name="factory">The factory that will make the new logger instance</param>
		/// <returns>The logger object with the name specified</returns>
		/// <remarks>
		/// <para>
		/// If a logger of that name already exists, then it will be
		/// returned. Otherwise, a new logger will be instantiated by the
		/// <paramref name="factory"/> parameter and linked with its existing
		/// ancestors as well as children.
		/// </para>
		/// </remarks>
		public Logger GetLogger(string name, ILoggerFactory factory) 
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			if (factory == null)
			{
				throw new ArgumentNullException("factory");
			}

			LoggerKey key = new LoggerKey(name);   
 
			// Synchronize to prevent write conflicts. Read conflicts (in
			// GetEffectiveLevel() method) are possible only if variable
			// assignments are non-atomic.

			lock(m_ht) 
			{
				Logger logger = null;

				Object node = m_ht[key];
				if (node == null) 
				{
					logger = factory.CreateLogger(this, name);
					logger.Hierarchy = this;
					m_ht[key] = logger;	  
					UpdateParents(logger);
					OnLoggerCreationEvent(logger);
					return logger;
				} 
				
				Logger nodeLogger = node as Logger;
				if (nodeLogger != null) 
				{
					return nodeLogger;
				} 
				
				ProvisionNode nodeProvisionNode = node as ProvisionNode;
				if (nodeProvisionNode != null) 
				{
					logger = factory.CreateLogger(this, name);
					logger.Hierarchy = this; 
					m_ht[key] = logger;
					UpdateChildren(nodeProvisionNode, logger);
					UpdateParents(logger);	
					OnLoggerCreationEvent(logger);
					return logger;
				}

				// It should be impossible to arrive here but let's keep the compiler happy.
				return null;
			}
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Sends a logger creation event to all registered listeners
		/// </summary>
		/// <param name="logger">The newly created logger</param>
		/// <remarks>
		/// Raises the logger creation event.
		/// </remarks>
		protected virtual void OnLoggerCreationEvent(Logger logger) 
		{
			LoggerCreationEventHandler handler = m_loggerCreatedEvent;
			if (handler != null)
			{
				handler(this, new LoggerCreationEventArgs(logger));
			}
		}

		#endregion Protected Instance Methods

		#region Private Instance Methods

		/// <summary>
		/// Updates all the parents of the specified logger
		/// </summary>
		/// <param name="log">The logger to update the parents for</param>
		/// <remarks>
		/// <para>
		/// This method loops through all the <i>potential</i> parents of
		/// <paramref name="log"/>. There 3 possible cases:
		/// </para>
		/// <list type="number">
		///		<item>
		///			<term>No entry for the potential parent of <paramref name="log"/> exists</term>
		///			<description>
		///			We create a ProvisionNode for this potential 
		///			parent and insert <paramref name="log"/> in that provision node.
		///			</description>
		///		</item>
		///		<item>
		///			<term>The entry is of type Logger for the potential parent.</term>
		///			<description>
		///			The entry is <paramref name="log"/>'s nearest existing parent. We 
		///			update <paramref name="log"/>'s parent field with this entry. We also break from 
		///			he loop because updating our parent's parent is our parent's 
		///			responsibility.
		///			</description>
		///		</item>
		///		<item>
		///			<term>The entry is of type ProvisionNode for this potential parent.</term>
		///			<description>
		///			We add <paramref name="log"/> to the list of children for this 
		///			potential parent.
		///			</description>
		///		</item>
		/// </list>
		/// </remarks>
		private void UpdateParents(Logger log) 
		{
			string name = log.Name;
			int length = name.Length;
			bool parentFound = false;
	
			// if name = "w.x.y.z", loop through "w.x.y", "w.x" and "w", but not "w.x.y.z" 
			for(int i = name.LastIndexOf('.', length-1); i >= 0; i = name.LastIndexOf('.', i-1))  
			{
				string substr = name.Substring(0, i);

				LoggerKey key = new LoggerKey(substr); // simple constructor
				Object node = m_ht[key];
				// Create a provision node for a future parent.
				if (node == null) 
				{
					ProvisionNode pn = new ProvisionNode(log);
					m_ht[key] = pn;
				} 
				else
				{
					Logger nodeLogger = node as Logger;
					if (nodeLogger != null)
					{
						parentFound = true;
						log.Parent = nodeLogger;
						break; // no need to update the ancestors of the closest ancestor
					}
					else
					{
						ProvisionNode nodeProvisionNode = node as ProvisionNode;
						if (nodeProvisionNode != null)
						{
							nodeProvisionNode.Add(log);
						}
						else
						{
							LogLog.Error(declaringType, "Unexpected object type ["+node.GetType()+"] in ht.", new LogException());
						}
					} 
				} 
			}

			// If we could not find any existing parents, then link with root.
			if (!parentFound) 
			{
				log.Parent = this.Root;
			}
		}

		/// <summary>
		/// Replace a <see cref="ProvisionNode"/> with a <see cref="Logger"/> in the hierarchy.
		/// </summary>
		/// <param name="pn"></param>
		/// <param name="log"></param>
		/// <remarks>
		/// <para>
		/// We update the links for all the children that placed themselves
		/// in the provision node 'pn'. The second argument 'log' is a
		/// reference for the newly created Logger, parent of all the
		/// children in 'pn'.
		/// </para>
		/// <para>
		/// We loop on all the children 'c' in 'pn'.
		/// </para>
		/// <para>
		/// If the child 'c' has been already linked to a child of
		/// 'log' then there is no need to update 'c'.
		/// </para>
		/// <para>
		/// Otherwise, we set log's parent field to c's parent and set
		/// c's parent field to log.
		/// </para>
		/// </remarks>
		private static void UpdateChildren(ProvisionNode pn, Logger log) 
		{
			for(int i = 0; i < pn.Count; i++) 
			{
				Logger childLogger = (Logger)pn[i];

				// Unless this child already points to a correct (lower) parent,
				// make log.Parent point to childLogger.Parent and childLogger.Parent to log.
				if (!childLogger.Parent.Name.StartsWith(log.Name)) 
				{
					log.Parent = childLogger.Parent;
					childLogger.Parent = log;	  
				}
			}
		}

		/// <summary>
		/// Define or redefine a Level using the values in the <see cref="LevelEntry"/> argument
		/// </summary>
		/// <param name="levelEntry">the level values</param>
		/// <remarks>
		/// <para>
		/// Define or redefine a Level using the values in the <see cref="LevelEntry"/> argument
		/// </para>
		/// <para>
		/// Supports setting levels via the configuration file.
		/// </para>
		/// </remarks>
		internal void AddLevel(LevelEntry levelEntry)
		{
			if (levelEntry == null) throw new ArgumentNullException("levelEntry");
			if (levelEntry.Name == null) throw new ArgumentNullException("levelEntry.Name");

			// Lookup replacement value
			if (levelEntry.Value == -1)
			{
				Level previousLevel = LevelMap[levelEntry.Name];
				if (previousLevel == null)
				{
					throw new InvalidOperationException("Cannot redefine level ["+levelEntry.Name+"] because it is not defined in the LevelMap. To define the level supply the level value.");
				}

				levelEntry.Value = previousLevel.Value;
			}

			LevelMap.Add(levelEntry.Name, levelEntry.Value, levelEntry.DisplayName);
		}

		/// <summary>
		/// A class to hold the value, name and display name for a level
		/// </summary>
		/// <remarks>
		/// <para>
		/// A class to hold the value, name and display name for a level
		/// </para>
		/// </remarks>
		internal class LevelEntry
		{
			private int m_levelValue = -1;
			private string m_levelName = null;
			private string m_levelDisplayName = null;

			/// <summary>
			/// Value of the level
			/// </summary>
			/// <remarks>
			/// <para>
			/// If the value is not set (defaults to -1) the value will be looked
			/// up for the current level with the same name.
			/// </para>
			/// </remarks>
			public int Value
			{
				get { return m_levelValue; }
				set { m_levelValue = value; }
			}

			/// <summary>
			/// Name of the level
			/// </summary>
			/// <value>
			/// The name of the level
			/// </value>
			/// <remarks>
			/// <para>
			/// The name of the level.
			/// </para>
			/// </remarks>
			public string Name
			{
				get { return m_levelName; }
				set { m_levelName = value; }
			}

			/// <summary>
			/// Display name for the level
			/// </summary>
			/// <value>
			/// The display name of the level
			/// </value>
			/// <remarks>
			/// <para>
			/// The display name of the level.
			/// </para>
			/// </remarks>
			public string DisplayName
			{
				get { return m_levelDisplayName; }
				set { m_levelDisplayName = value; }
			}

			/// <summary>
			/// Override <c>Object.ToString</c> to return sensible debug info
			/// </summary>
			/// <returns>string info about this object</returns>
			public override string ToString()
			{
				return "LevelEntry(Value="+m_levelValue+", Name="+m_levelName+", DisplayName="+m_levelDisplayName+")";
			}
		}

		/// <summary>
		/// Set a Property using the values in the <see cref="LevelEntry"/> argument
		/// </summary>
		/// <param name="propertyEntry">the property value</param>
		/// <remarks>
		/// <para>
		/// Set a Property using the values in the <see cref="LevelEntry"/> argument.
		/// </para>
		/// <para>
		/// Supports setting property values via the configuration file.
		/// </para>
		/// </remarks>
		internal void AddProperty(PropertyEntry propertyEntry)
		{
			if (propertyEntry == null) throw new ArgumentNullException("propertyEntry");
			if (propertyEntry.Key == null) throw new ArgumentNullException("propertyEntry.Key");

			Properties[propertyEntry.Key] = propertyEntry.Value;
		}

		#endregion Private Instance Methods

		#region Private Instance Fields

		private ILoggerFactory m_defaultFactory;

		private System.Collections.Hashtable m_ht;
		private Logger m_root;
  
		private bool m_emittedNoAppenderWarning = false;

	    private event LoggerCreationEventHandler m_loggerCreatedEvent;

		#endregion Private Instance Fields

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the Hierarchy class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(Hierarchy);

	    #endregion Private Static Fields
	}
}
