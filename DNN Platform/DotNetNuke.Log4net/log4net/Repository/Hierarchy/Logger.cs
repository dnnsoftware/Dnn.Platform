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
using log4net.Util;
using log4net.Core;

namespace log4net.Repository.Hierarchy
{
	/// <summary>
	/// Implementation of <see cref="ILogger"/> used by <see cref="Hierarchy"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Internal class used to provide implementation of <see cref="ILogger"/>
	/// interface. Applications should use <see cref="LogManager"/> to get
	/// logger instances.
	/// </para>
	/// <para>
	/// This is one of the central classes in the log4net implementation. One of the
	/// distinctive features of log4net are hierarchical loggers and their
	/// evaluation. The <see cref="Hierarchy"/> organizes the <see cref="Logger"/>
	/// instances into a rooted tree hierarchy.
	/// </para>
	/// <para>
	/// The <see cref="Logger"/> class is abstract. Only concrete subclasses of
	/// <see cref="Logger"/> can be created. The <see cref="ILoggerFactory"/>
	/// is used to create instances of this type for the <see cref="Hierarchy"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Aspi Havewala</author>
	/// <author>Douglas de la Torre</author>
	public abstract class Logger : IAppenderAttachable, ILogger
	{
		#region Protected Instance Constructors

		/// <summary>
		/// This constructor created a new <see cref="Logger" /> instance and
		/// sets its name.
		/// </summary>
		/// <param name="name">The name of the <see cref="Logger" />.</param>
		/// <remarks>
		/// <para>
		/// This constructor is protected and designed to be used by
		/// a subclass that is not abstract.
		/// </para>
		/// <para>
		/// Loggers are constructed by <see cref="ILoggerFactory"/> 
		/// objects. See <see cref="DefaultLoggerFactory"/> for the default
		/// logger creator.
		/// </para>
		/// </remarks>
		protected Logger(string name) 
		{
#if NETCF
			// NETCF: String.Intern causes Native Exception
			m_name = name;
#else
			m_name = string.Intern(name);
#endif
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the parent logger in the hierarchy.
		/// </summary>
		/// <value>
		/// The parent logger in the hierarchy.
		/// </value>
		/// <remarks>
		/// <para>
		/// Part of the Composite pattern that makes the hierarchy.
		/// The hierarchy is parent linked rather than child linked.
		/// </para>
		/// </remarks>
		virtual public Logger Parent
		{
			get { return m_parent; }
			set { m_parent = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating if child loggers inherit their parent's appenders.
		/// </summary>
		/// <value>
		/// <c>true</c> if child loggers inherit their parent's appenders.
		/// </value>
		/// <remarks>
		/// <para>
		/// Additivity is set to <c>true</c> by default, that is children inherit
		/// the appenders of their ancestors by default. If this variable is
		/// set to <c>false</c> then the appenders found in the
		/// ancestors of this logger are not used. However, the children
		/// of this logger will inherit its appenders, unless the children
		/// have their additivity flag set to <c>false</c> too. See
		/// the user manual for more details.
		/// </para>
		/// </remarks>
		virtual public bool Additivity
		{
			get { return m_additive; }
			set { m_additive = value; }
		}

		/// <summary>
		/// Gets the effective level for this logger.
		/// </summary>
		/// <returns>The nearest level in the logger hierarchy.</returns>
		/// <remarks>
		/// <para>
		/// Starting from this logger, searches the logger hierarchy for a
		/// non-null level and returns it. Otherwise, returns the level of the
		/// root logger.
		/// </para>
		/// <para>The Logger class is designed so that this method executes as
		/// quickly as possible.</para>
		/// </remarks>
		virtual public Level EffectiveLevel
		{
			get 
			{
				for(Logger c = this; c != null; c = c.m_parent) 
				{
					Level level = c.m_level;

					// Casting level to Object for performance, otherwise the overloaded operator is called
					if ((object)level != null) 
					{
						return level;
					}
				}
				return null; // If reached will cause an NullPointerException.
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="Hierarchy"/> where this 
		/// <c>Logger</c> instance is attached to.
		/// </summary>
		/// <value>The hierarchy that this logger belongs to.</value>
		/// <remarks>
		/// <para>
		/// This logger must be attached to a single <see cref="Hierarchy"/>.
		/// </para>
		/// </remarks>
		virtual public Hierarchy Hierarchy
		{
			get { return m_hierarchy; }
			set { m_hierarchy = value; }
		}

		/// <summary>
		/// Gets or sets the assigned <see cref="Level"/>, if any, for this Logger.  
		/// </summary>
		/// <value>
		/// The <see cref="Level"/> of this logger.
		/// </value>
		/// <remarks>
		/// <para>
		/// The assigned <see cref="Level"/> can be <c>null</c>.
		/// </para>
		/// </remarks>
		virtual public Level Level
		{
			get { return m_level; }
			set { m_level = value; }
		}

		#endregion Public Instance Properties

		#region Implementation of IAppenderAttachable

		/// <summary>
		/// Add <paramref name="newAppender"/> to the list of appenders of this
		/// Logger instance.
		/// </summary>
		/// <param name="newAppender">An appender to add to this logger</param>
		/// <remarks>
		/// <para>
		/// Add <paramref name="newAppender"/> to the list of appenders of this
		/// Logger instance.
		/// </para>
		/// <para>
		/// If <paramref name="newAppender"/> is already in the list of
		/// appenders, then it won't be added again.
		/// </para>
		/// </remarks>
		virtual public void AddAppender(IAppender newAppender) 
		{
			if (newAppender == null)
			{
				throw new ArgumentNullException("newAppender");
			}

			m_appenderLock.AcquireWriterLock();
			try
			{
				if (m_appenderAttachedImpl == null) 
				{
					m_appenderAttachedImpl = new log4net.Util.AppenderAttachedImpl();
				}
				m_appenderAttachedImpl.AddAppender(newAppender);
			}
			finally
			{
				m_appenderLock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Get the appenders contained in this logger as an 
		/// <see cref="System.Collections.ICollection"/>.
		/// </summary>
		/// <returns>A collection of the appenders in this logger</returns>
		/// <remarks>
		/// <para>
		/// Get the appenders contained in this logger as an 
		/// <see cref="System.Collections.ICollection"/>. If no appenders 
		/// can be found, then a <see cref="EmptyCollection"/> is returned.
		/// </para>
		/// </remarks>
		virtual public AppenderCollection Appenders 
		{
			get
			{
				m_appenderLock.AcquireReaderLock();
				try
				{
					if (m_appenderAttachedImpl == null)
					{
						return AppenderCollection.EmptyCollection;
					}
					else 
					{
						return m_appenderAttachedImpl.Appenders;
					}
				}
				finally
				{
					m_appenderLock.ReleaseReaderLock();
				}
			}
		}

		/// <summary>
		/// Look for the appender named as <c>name</c>
		/// </summary>
		/// <param name="name">The name of the appender to lookup</param>
		/// <returns>The appender with the name specified, or <c>null</c>.</returns>
		/// <remarks>
		/// <para>
		/// Returns the named appender, or null if the appender is not found.
		/// </para>
		/// </remarks>
		virtual public IAppender GetAppender(string name) 
		{
			m_appenderLock.AcquireReaderLock();
			try
			{
				if (m_appenderAttachedImpl == null || name == null)
				{
					return null;
				}

				return m_appenderAttachedImpl.GetAppender(name);
			}
			finally
			{
				m_appenderLock.ReleaseReaderLock();
			}
		}

		/// <summary>
		/// Remove all previously added appenders from this Logger instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Remove all previously added appenders from this Logger instance.
		/// </para>
		/// <para>
		/// This is useful when re-reading configuration information.
		/// </para>
		/// </remarks>
		virtual public void RemoveAllAppenders() 
		{
			m_appenderLock.AcquireWriterLock();
			try
			{
				if (m_appenderAttachedImpl != null) 
				{
					m_appenderAttachedImpl.RemoveAllAppenders();
					m_appenderAttachedImpl = null;
				}
			}
			finally
			{
				m_appenderLock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// Remove the appender passed as parameter form the list of appenders.
		/// </summary>
		/// <param name="appender">The appender to remove</param>
		/// <returns>The appender removed from the list</returns>
		/// <remarks>
		/// <para>
		/// Remove the appender passed as parameter form the list of appenders.
		/// The appender removed is not closed.
		/// If you are discarding the appender you must call
		/// <see cref="IAppender.Close"/> on the appender removed.
		/// </para>
		/// </remarks>
		virtual public IAppender RemoveAppender(IAppender appender) 
		{
			m_appenderLock.AcquireWriterLock();
			try
			{
				if (appender != null && m_appenderAttachedImpl != null) 
				{
					return m_appenderAttachedImpl.RemoveAppender(appender);
				}
			}
			finally
			{
				m_appenderLock.ReleaseWriterLock();
			}
			return null;
		}

		/// <summary>
		/// Remove the appender passed as parameter form the list of appenders.
		/// </summary>
		/// <param name="name">The name of the appender to remove</param>
		/// <returns>The appender removed from the list</returns>
		/// <remarks>
		/// <para>
		/// Remove the named appender passed as parameter form the list of appenders.
		/// The appender removed is not closed.
		/// If you are discarding the appender you must call
		/// <see cref="IAppender.Close"/> on the appender removed.
		/// </para>
		/// </remarks>
		virtual public IAppender RemoveAppender(string name) 
		{
			m_appenderLock.AcquireWriterLock();
			try
			{
				if (name != null && m_appenderAttachedImpl != null)
				{
					return m_appenderAttachedImpl.RemoveAppender(name);
				}
			}
			finally
			{
				m_appenderLock.ReleaseWriterLock();
			}
			return null;
		}
  
		#endregion

		#region Implementation of ILogger

		/// <summary>
		/// Gets the logger name.
		/// </summary>
		/// <value>
		/// The name of the logger.
		/// </value>
		/// <remarks>
		/// <para>
		/// The name of this logger
		/// </para>
		/// </remarks>
		virtual public string Name
		{
			get { return m_name; }
		}

		/// <summary>
		/// This generic form is intended to be used by wrappers.
		/// </summary>
		/// <param name="callerStackBoundaryDeclaringType">The declaring type of the method that is
		/// the stack boundary into the logging system for this call.</param>
		/// <param name="level">The level of the message to be logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Generate a logging event for the specified <paramref name="level"/> using
		/// the <paramref name="message"/> and <paramref name="exception"/>.
		/// </para>
		/// <para>
		/// This method must not throw any exception to the caller.
		/// </para>
		/// </remarks>
		virtual public void Log(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception) 
		{
			try
			{
				if (IsEnabledFor(level))
				{
                    ForcedLog((callerStackBoundaryDeclaringType != null) ? callerStackBoundaryDeclaringType : declaringType, level, message, exception);
				}
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
#if !NET_2_0 && !MONO_2_0
			catch
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging");
			}
#endif
		}

		/// <summary>
		/// This is the most generic printing method that is intended to be used 
		/// by wrappers.
		/// </summary>
		/// <param name="logEvent">The event being logged.</param>
		/// <remarks>
		/// <para>
		/// Logs the specified logging event through this logger.
		/// </para>
		/// <para>
		/// This method must not throw any exception to the caller.
		/// </para>
		/// </remarks>
		virtual public void Log(LoggingEvent logEvent)
		{
			try
			{
				if (logEvent != null)
				{
					if (IsEnabledFor(logEvent.Level))
					{
						ForcedLog(logEvent);
					}
				}
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
#if !NET_2_0 && !MONO_2_0
			catch
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging");
			}
#endif
		}

		/// <summary>
		/// Checks if this logger is enabled for a given <see cref="Level"/> passed as parameter.
		/// </summary>
		/// <param name="level">The level to check.</param>
		/// <returns>
		/// <c>true</c> if this logger is enabled for <c>level</c>, otherwise <c>false</c>.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Test if this logger is going to log events of the specified <paramref name="level"/>.
		/// </para>
		/// <para>
		/// This method must not throw any exception to the caller.
		/// </para>
		/// </remarks>
		virtual public bool IsEnabledFor(Level level)
		{
			try
			{
				if (level != null)
				{
					if (m_hierarchy.IsDisabled(level))
					{
						return false;
					}
					return level >= this.EffectiveLevel;
				}
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
#if !NET_2_0 && !MONO_2_0
			catch
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging");
			}
#endif
			return false;
		}

		/// <summary>
		/// Gets the <see cref="ILoggerRepository"/> where this 
		/// <c>Logger</c> instance is attached to.
		/// </summary>
		/// <value>
		/// The <see cref="ILoggerRepository" /> that this logger belongs to.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets the <see cref="ILoggerRepository"/> where this 
		/// <c>Logger</c> instance is attached to.
		/// </para>
		/// </remarks>
		public ILoggerRepository Repository
		{ 
			get { return m_hierarchy; }
		}

  		#endregion Implementation of ILogger

		/// <summary>
		/// Deliver the <see cref="LoggingEvent"/> to the attached appenders.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Call the appenders in the hierarchy starting at
		/// <c>this</c>. If no appenders could be found, emit a
		/// warning.
		/// </para>
		/// <para>
		/// This method calls all the appenders inherited from the
		/// hierarchy circumventing any evaluation of whether to log or not
		/// to log the particular log request.
		/// </para>
		/// </remarks>
		virtual protected void CallAppenders(LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			int writes = 0;

			for(Logger c=this; c != null; c=c.m_parent) 
			{
				if (c.m_appenderAttachedImpl != null) 
				{
					// Protected against simultaneous call to addAppender, removeAppender,...
					c.m_appenderLock.AcquireReaderLock();
					try
					{
						if (c.m_appenderAttachedImpl != null) 
						{
							writes += c.m_appenderAttachedImpl.AppendLoopOnAppenders(loggingEvent);
						}
					}
					finally
					{
						c.m_appenderLock.ReleaseReaderLock();
					}
				}

				if (!c.m_additive) 
				{
					break;
				}
			}
			
			// No appenders in hierarchy, warn user only once.
			//
			// Note that by including the AppDomain values for the currently running
			// thread, it becomes much easier to see which application the warning
			// is from, which is especially helpful in a multi-AppDomain environment
			// (like IIS with multiple VDIRS).  Without this, it can be difficult
			// or impossible to determine which .config file is missing appender
			// definitions.
			//
			if (!m_hierarchy.EmittedNoAppenderWarning && writes == 0) 
			{
				LogLog.Debug(declaringType, "No appenders could be found for logger [" + Name + "] repository [" + Repository.Name + "]");
				LogLog.Debug(declaringType, "Please initialize the log4net system properly.");
				try
				{
					LogLog.Debug(declaringType, "    Current AppDomain context information: ");
					LogLog.Debug(declaringType, "       BaseDirectory   : " + SystemInfo.ApplicationBaseDirectory);
#if !NETCF
					LogLog.Debug(declaringType, "       FriendlyName    : " + AppDomain.CurrentDomain.FriendlyName);
					LogLog.Debug(declaringType, "       DynamicDirectory: " + AppDomain.CurrentDomain.DynamicDirectory);
#endif
				}
				catch(System.Security.SecurityException)
				{
					// Insufficient permissions to display info from the AppDomain
				}
				m_hierarchy.EmittedNoAppenderWarning = true;
			}
		}

		/// <summary>
		/// Closes all attached appenders implementing the <see cref="IAppenderAttachable"/> interface.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Used to ensure that the appenders are correctly shutdown.
		/// </para>
		/// </remarks>
		virtual public void CloseNestedAppenders() 
		{
			m_appenderLock.AcquireWriterLock();
			try
			{
				if (m_appenderAttachedImpl != null)
				{
					AppenderCollection appenders = m_appenderAttachedImpl.Appenders;
					foreach(IAppender appender in appenders)
					{
						if (appender is IAppenderAttachable)
						{
							appender.Close();
						}
					}
				}
			}
			finally
			{
				m_appenderLock.ReleaseWriterLock();
			}
		}

		/// <summary>
		/// This is the most generic printing method. This generic form is intended to be used by wrappers
		/// </summary>
		/// <param name="level">The level of the message to be logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Generate a logging event for the specified <paramref name="level"/> using
		/// the <paramref name="message"/>.
		/// </para>
		/// </remarks>
		virtual public void Log(Level level, object message, Exception exception) 
		{
			if (IsEnabledFor(level))
			{
                ForcedLog(declaringType, level, message, exception);
			}
		}

		/// <summary>
		/// Creates a new logging event and logs the event without further checks.
		/// </summary>
		/// <param name="callerStackBoundaryDeclaringType">The declaring type of the method that is
		/// the stack boundary into the logging system for this call.</param>
		/// <param name="level">The level of the message to be logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Generates a logging event and delivers it to the attached
		/// appenders.
		/// </para>
		/// </remarks>
		virtual protected void ForcedLog(Type callerStackBoundaryDeclaringType, Level level, object message, Exception exception) 
		{
			CallAppenders(new LoggingEvent(callerStackBoundaryDeclaringType, this.Hierarchy, this.Name, level, message, exception));
		}

		/// <summary>
		/// Creates a new logging event and logs the event without further checks.
		/// </summary>
		/// <param name="logEvent">The event being logged.</param>
		/// <remarks>
		/// <para>
		/// Delivers the logging event to the attached appenders.
		/// </para>
		/// </remarks>
		virtual protected void ForcedLog(LoggingEvent logEvent) 
		{
			// The logging event may not have been created by this logger
			// the Repository may not be correctly set on the event. This
			// is required for the appenders to correctly lookup renderers etc...
			logEvent.EnsureRepository(this.Hierarchy);

			CallAppenders(logEvent);
		}

		#region Private Static Fields

        /// <summary>
        /// The fully qualified type of the Logger class.
        /// </summary>
        private readonly static Type declaringType = typeof(Logger);

		#endregion Private Static Fields

		#region Private Instance Fields

		/// <summary>
		/// The name of this logger.
		/// </summary>
		private readonly string m_name;  

		/// <summary>
		/// The assigned level of this logger. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <c>level</c> variable need not be 
		/// assigned a value in which case it is inherited 
		/// form the hierarchy.
		/// </para>
		/// </remarks>
		private Level m_level;

		/// <summary>
		/// The parent of this logger.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The parent of this logger. 
		/// All loggers have at least one ancestor which is the root logger.
		/// </para>
		/// </remarks>
		private Logger m_parent;

		/// <summary>
		/// Loggers need to know what Hierarchy they are in.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Loggers need to know what Hierarchy they are in.
		/// The hierarchy that this logger is a member of is stored
		/// here.
		/// </para>
		/// </remarks>
		private Hierarchy m_hierarchy;

		/// <summary>
		/// Helper implementation of the <see cref="IAppenderAttachable"/> interface
		/// </summary>
		private log4net.Util.AppenderAttachedImpl m_appenderAttachedImpl;

		/// <summary>
		/// Flag indicating if child loggers inherit their parents appenders
		/// </summary>
		/// <remarks>
		/// <para>
		/// Additivity is set to true by default, that is children inherit
		/// the appenders of their ancestors by default. If this variable is
		/// set to <c>false</c> then the appenders found in the
		/// ancestors of this logger are not used. However, the children
		/// of this logger will inherit its appenders, unless the children
		/// have their additivity flag set to <c>false</c> too. See
		/// the user manual for more details.
		/// </para>
		/// </remarks>
		private bool m_additive = true;

		/// <summary>
		/// Lock to protect AppenderAttachedImpl variable m_appenderAttachedImpl
		/// </summary>
		private readonly ReaderWriterLock m_appenderLock = new ReaderWriterLock();
  
		#endregion
	}
}
