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
using System.Globalization;

using log4net.Repository;
using log4net.Util;

namespace log4net.Core
{
	/// <summary>
	/// Implementation of <see cref="ILog"/> wrapper interface.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This implementation of the <see cref="ILog"/> interface
	/// forwards to the <see cref="ILogger"/> held by the base class.
	/// </para>
	/// <para>
	/// This logger has methods to allow the caller to log at the following
	/// levels:
	/// </para>
	/// <list type="definition">
	///   <item>
	///     <term>DEBUG</term>
	///     <description>
	///     The <see cref="M:Debug(object)"/> and <see cref="M:DebugFormat(string, object[])"/> methods log messages
	///     at the <c>DEBUG</c> level. That is the level with that name defined in the
	///     repositories <see cref="ILoggerRepository.LevelMap"/>. The default value
	///     for this level is <see cref="Level.Debug"/>. The <see cref="IsDebugEnabled"/>
	///     property tests if this level is enabled for logging.
	///     </description>
	///   </item>
	///   <item>
	///     <term>INFO</term>
	///     <description>
	///     The <see cref="M:Info(object)"/> and <see cref="M:InfoFormat(string, object[])"/> methods log messages
	///     at the <c>INFO</c> level. That is the level with that name defined in the
	///     repositories <see cref="ILoggerRepository.LevelMap"/>. The default value
	///     for this level is <see cref="Level.Info"/>. The <see cref="IsInfoEnabled"/>
	///     property tests if this level is enabled for logging.
	///     </description>
	///   </item>
	///   <item>
	///     <term>WARN</term>
	///     <description>
	///     The <see cref="M:Warn(object)"/> and <see cref="M:WarnFormat(string, object[])"/> methods log messages
	///     at the <c>WARN</c> level. That is the level with that name defined in the
	///     repositories <see cref="ILoggerRepository.LevelMap"/>. The default value
	///     for this level is <see cref="Level.Warn"/>. The <see cref="IsWarnEnabled"/>
	///     property tests if this level is enabled for logging.
	///     </description>
	///   </item>
	///   <item>
	///     <term>ERROR</term>
	///     <description>
	///     The <see cref="M:Error(object)"/> and <see cref="M:ErrorFormat(string, object[])"/> methods log messages
	///     at the <c>ERROR</c> level. That is the level with that name defined in the
	///     repositories <see cref="ILoggerRepository.LevelMap"/>. The default value
	///     for this level is <see cref="Level.Error"/>. The <see cref="IsErrorEnabled"/>
	///     property tests if this level is enabled for logging.
	///     </description>
	///   </item>
	///   <item>
	///     <term>FATAL</term>
	///     <description>
	///     The <see cref="M:Fatal(object)"/> and <see cref="M:FatalFormat(string, object[])"/> methods log messages
	///     at the <c>FATAL</c> level. That is the level with that name defined in the
	///     repositories <see cref="ILoggerRepository.LevelMap"/>. The default value
	///     for this level is <see cref="Level.Fatal"/>. The <see cref="IsFatalEnabled"/>
	///     property tests if this level is enabled for logging.
	///     </description>
	///   </item>
	/// </list>
	/// <para>
	/// The values for these levels and their semantic meanings can be changed by 
	/// configuring the <see cref="ILoggerRepository.LevelMap"/> for the repository.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class LogImpl : LoggerWrapperImpl, ILog
	{
		#region Public Instance Constructors

		/// <summary>
		/// Construct a new wrapper for the specified logger.
		/// </summary>
		/// <param name="logger">The logger to wrap.</param>
		/// <remarks>
		/// <para>
		/// Construct a new wrapper for the specified logger.
		/// </para>
		/// </remarks>
		public LogImpl(ILogger logger) : base(logger)
		{
			// Listen for changes to the repository
			logger.Repository.ConfigurationChanged += new LoggerRepositoryConfigurationChangedEventHandler(LoggerRepositoryConfigurationChanged);

			// load the current levels
			ReloadLevels(logger.Repository);
		}

		#endregion Public Instance Constructors

		/// <summary>
		/// Virtual method called when the configuration of the repository changes
		/// </summary>
		/// <param name="repository">the repository holding the levels</param>
		/// <remarks>
		/// <para>
		/// Virtual method called when the configuration of the repository changes
		/// </para>
		/// </remarks>
		protected virtual void ReloadLevels(ILoggerRepository repository)
		{
			LevelMap levelMap = repository.LevelMap;

			m_levelDebug = levelMap.LookupWithDefault(Level.Debug);
			m_levelInfo = levelMap.LookupWithDefault(Level.Info);
			m_levelWarn = levelMap.LookupWithDefault(Level.Warn);
			m_levelError = levelMap.LookupWithDefault(Level.Error);
			m_levelFatal = levelMap.LookupWithDefault(Level.Fatal);
		}

		#region Implementation of ILog

		/// <summary>
		/// Logs a message object with the <c>DEBUG</c> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>DEBUG</c>
		/// enabled by comparing the level of this logger with the 
		/// <c>DEBUG</c> level. If this logger is
		/// <c>DEBUG</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para>
		/// <b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="M:Debug(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		virtual public void Debug(object message) 
		{
			Logger.Log(ThisDeclaringType, m_levelDebug, message, null);
		}

		/// <summary>
		/// Logs a message object with the <c>DEBUG</c> level
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Logs a message object with the <c>DEBUG</c> level including
		/// the stack trace of the <see cref="Exception"/> <paramref name="exception"/> passed
		/// as a parameter.
		/// </para>
		/// <para>
		/// See the <see cref="M:Debug(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Debug(object)"/>
		virtual public void Debug(object message, Exception exception) 
		{
			Logger.Log(ThisDeclaringType, m_levelDebug, message, exception);
		}

		/// <summary>
		/// Logs a formatted message string with the <c>DEBUG</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:DebugFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Debug(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void DebugFormat(string format, params object[] args) 
		{
			if (IsDebugEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>DEBUG</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:DebugFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Debug(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void DebugFormat(string format, object arg0) 
		{
			if (IsDebugEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>DEBUG</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:DebugFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Debug(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void DebugFormat(string format, object arg0, object arg1) 
		{
			if (IsDebugEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>DEBUG</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:DebugFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Debug(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void DebugFormat(string format, object arg0, object arg1, object arg2) 
		{
			if (IsDebugEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>DEBUG</c> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Debug(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void DebugFormat(IFormatProvider provider, string format, params object[] args) 
		{
			if (IsDebugEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelDebug, new SystemStringFormat(provider, format, args), null);
			}
		}

		/// <summary>
		/// Logs a message object with the <c>INFO</c> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by comparing the level of this logger with the 
		/// <c>INFO</c> level. If this logger is
		/// <c>INFO</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para>
		/// <b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="M:Info(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		virtual public void Info(object message) 
		{
			Logger.Log(ThisDeclaringType, m_levelInfo, message, null);
		}
  
		/// <summary>
		/// Logs a message object with the <c>INFO</c> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Logs a message object with the <c>INFO</c> level including
		/// the stack trace of the <see cref="Exception"/> <paramref name="exception"/> 
		/// passed as a parameter.
		/// </para>
		/// <para>
		/// See the <see cref="M:Info(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Info(object)"/>
		virtual public void Info(object message, Exception exception) 
		{
			Logger.Log(ThisDeclaringType, m_levelInfo, message, exception);
		}

		/// <summary>
		/// Logs a formatted message string with the <c>INFO</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:InfoFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Info(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void InfoFormat(string format, params object[] args) 
		{
			if (IsInfoEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>INFO</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:InfoFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Info(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void InfoFormat(string format, object arg0) 
		{
			if (IsInfoEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>INFO</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:InfoFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Info(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void InfoFormat(string format, object arg0, object arg1) 
		{
			if (IsInfoEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>INFO</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:InfoFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Info(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void InfoFormat(string format, object arg0, object arg1, object arg2) 
		{
			if (IsInfoEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>INFO</c> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Info(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void InfoFormat(IFormatProvider provider, string format, params object[] args) 
		{
			if (IsInfoEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelInfo, new SystemStringFormat(provider, format, args), null);
			}
		}

		/// <summary>
		/// Logs a message object with the <c>WARN</c> level.
		/// </summary>
		/// <param name="message">the message object to log</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>WARN</c>
		/// enabled by comparing the level of this logger with the 
		/// <c>WARN</c> level. If this logger is
		/// <c>WARN</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger and 
		/// also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para>
		/// <b>WARNING</b> Note that passing an <see cref="Exception"/> to this
		/// method will print the name of the <see cref="Exception"/> but no
		/// stack trace. To print a stack trace use the 
		/// <see cref="M:Warn(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		virtual public void Warn(object message) 
		{
			Logger.Log(ThisDeclaringType, m_levelWarn, message, null);
		}
  
		/// <summary>
		/// Logs a message object with the <c>WARN</c> level
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Logs a message object with the <c>WARN</c> level including
		/// the stack trace of the <see cref="Exception"/> <paramref name="exception"/> 
		/// passed as a parameter.
		/// </para>
		/// <para>
		/// See the <see cref="M:Warn(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Warn(object)"/>
		virtual public void Warn(object message, Exception exception) 
		{
			Logger.Log(ThisDeclaringType, m_levelWarn, message, exception);
		}

		/// <summary>
		/// Logs a formatted message string with the <c>WARN</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:WarnFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Warn(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void WarnFormat(string format, params object[] args) 
		{
			if (IsWarnEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>WARN</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:WarnFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Warn(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void WarnFormat(string format, object arg0) 
		{
			if (IsWarnEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>WARN</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:WarnFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Warn(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void WarnFormat(string format, object arg0, object arg1) 
		{
			if (IsWarnEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>WARN</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:WarnFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Warn(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void WarnFormat(string format, object arg0, object arg1, object arg2) 
		{
			if (IsWarnEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>WARN</c> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Warn(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void WarnFormat(IFormatProvider provider, string format, params object[] args) 
		{
			if (IsWarnEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelWarn, new SystemStringFormat(provider, format, args), null);
			}
		}

		/// <summary>
		/// Logs a message object with the <c>ERROR</c> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>ERROR</c>
		/// enabled by comparing the level of this logger with the 
		/// <c>ERROR</c> level. If this logger is
		/// <c>ERROR</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger and 
		/// also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para>
		/// <b>WARNING</b> Note that passing an <see cref="Exception"/> to this
		/// method will print the name of the <see cref="Exception"/> but no
		/// stack trace. To print a stack trace use the 
		/// <see cref="M:Error(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		virtual public void Error(object message) 
		{
			Logger.Log(ThisDeclaringType, m_levelError, message, null);
		}

		/// <summary>
		/// Logs a message object with the <c>ERROR</c> level
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Logs a message object with the <c>ERROR</c> level including
		/// the stack trace of the <see cref="Exception"/> <paramref name="exception"/> 
		/// passed as a parameter.
		/// </para>
		/// <para>
		/// See the <see cref="M:Error(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Error(object)"/>
		virtual public void Error(object message, Exception exception) 
		{
			Logger.Log(ThisDeclaringType, m_levelError, message, exception);
		}

		/// <summary>
		/// Logs a formatted message string with the <c>ERROR</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:ErrorFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Error(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void ErrorFormat(string format, params object[] args) 
		{
			if (IsErrorEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>ERROR</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:ErrorFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Error(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void ErrorFormat(string format, object arg0) 
		{
			if (IsErrorEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>ERROR</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:ErrorFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Error(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void ErrorFormat(string format, object arg0, object arg1) 
		{
			if (IsErrorEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>ERROR</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:ErrorFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Error(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void ErrorFormat(string format, object arg0, object arg1, object arg2) 
		{
			if (IsErrorEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>ERROR</c> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Error(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void ErrorFormat(IFormatProvider provider, string format, params object[] args) 
		{
			if (IsErrorEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelError, new SystemStringFormat(provider, format, args), null);
			}
		}

		/// <summary>
		/// Logs a message object with the <c>FATAL</c> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>FATAL</c>
		/// enabled by comparing the level of this logger with the 
		/// <c>FATAL</c> level. If this logger is
		/// <c>FATAL</c> enabled, then it converts the message object
		/// (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger and 
		/// also higher in the hierarchy depending on the value of the 
		/// additivity flag.
		/// </para>
		/// <para>
		/// <b>WARNING</b> Note that passing an <see cref="Exception"/> to this
		/// method will print the name of the <see cref="Exception"/> but no
		/// stack trace. To print a stack trace use the 
		/// <see cref="M:Fatal(object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		virtual public void Fatal(object message) 
		{
			Logger.Log(ThisDeclaringType, m_levelFatal, message, null);
		}
  
		/// <summary>
		/// Logs a message object with the <c>FATAL</c> level
		/// </summary>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// Logs a message object with the <c>FATAL</c> level including
		/// the stack trace of the <see cref="Exception"/> <paramref name="exception"/> 
		/// passed as a parameter.
		/// </para>
		/// <para>
		/// See the <see cref="M:Fatal(object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="M:Fatal(object)"/>
		virtual public void Fatal(object message, Exception exception) 
		{
			Logger.Log(ThisDeclaringType, m_levelFatal, message, exception);
		}

		/// <summary>
		/// Logs a formatted message string with the <c>FATAL</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:FatalFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Fatal(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void FatalFormat(string format, params object[] args) 
		{
			if (IsFatalEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, args), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>FATAL</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:FatalFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Fatal(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void FatalFormat(string format, object arg0) 
		{
			if (IsFatalEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>FATAL</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:FatalFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Fatal(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void FatalFormat(string format, object arg0, object arg1) 
		{
			if (IsFatalEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>FATAL</c> level.
		/// </summary>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// The string is formatted using the <see cref="CultureInfo.InvariantCulture"/>
		/// format provider. To specify a localized provider use the
		/// <see cref="M:FatalFormat(IFormatProvider,string,object[])"/> method.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Fatal(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void FatalFormat(string format, object arg0, object arg1, object arg2) 
		{
			if (IsFatalEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(CultureInfo.InvariantCulture, format, new object[] { arg0, arg1, arg2 }), null);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <c>FATAL</c> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <see cref="M:String.Format(IFormatProvider, string, object[])"/> method. See
		/// <c>String.Format</c> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="M:Fatal(object)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		virtual public void FatalFormat(IFormatProvider provider, string format, params object[] args) 
		{
			if (IsFatalEnabled)
			{
				Logger.Log(ThisDeclaringType, m_levelFatal, new SystemStringFormat(provider, format, args), null);
			}
		}

		/// <summary>
		/// Checks if this logger is enabled for the <c>DEBUG</c>
		/// level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <c>DEBUG</c> events,
		/// <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// This function is intended to lessen the computational cost of
		/// disabled log debug statements.
		/// </para>
		/// <para>
		/// For some <c>log</c> Logger object, when you write:
		/// </para>
		/// <code lang="C#">
		/// log.Debug("This is entry number: " + i );
		/// </code>
		/// <para>
		/// You incur the cost constructing the message, concatenation in
		/// this case, regardless of whether the message is logged or not.
		/// </para>
		/// <para>
		/// If you are worried about speed, then you should write:
		/// </para>
		/// <code lang="C#">
		/// if (log.IsDebugEnabled())
		/// { 
		///	 log.Debug("This is entry number: " + i );
		/// }
		/// </code>
		/// <para>
		/// This way you will not incur the cost of parameter
		/// construction if debugging is disabled for <c>log</c>. On
		/// the other hand, if the <c>log</c> is debug enabled, you
		/// will incur the cost of evaluating whether the logger is debug
		/// enabled twice. Once in <c>IsDebugEnabled</c> and once in
		/// the <c>Debug</c>.  This is an insignificant overhead
		/// since evaluating a logger takes about 1% of the time it
		/// takes to actually log.
		/// </para>
		/// </remarks>
		virtual public bool IsDebugEnabled
		{
			get { return Logger.IsEnabledFor(m_levelDebug); }
		}
  
		/// <summary>
		/// Checks if this logger is enabled for the <c>INFO</c> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <c>INFO</c> events,
		/// <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// See <see cref="IsDebugEnabled"/> for more information and examples 
		/// of using this method.
		/// </para>
		/// </remarks>
		/// <seealso cref="LogImpl.IsDebugEnabled"/>
		virtual public bool IsInfoEnabled
		{
			get { return Logger.IsEnabledFor(m_levelInfo); }
		}

		/// <summary>
		/// Checks if this logger is enabled for the <c>WARN</c> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <c>WARN</c> events,
		/// <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// See <see cref="IsDebugEnabled"/> for more information and examples 
		/// of using this method.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		virtual public bool IsWarnEnabled
		{
			get { return Logger.IsEnabledFor(m_levelWarn); }
		}

		/// <summary>
		/// Checks if this logger is enabled for the <c>ERROR</c> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <c>ERROR</c> events,
		/// <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// See <see cref="IsDebugEnabled"/> for more information and examples of using this method.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		virtual public bool IsErrorEnabled
		{
			get { return Logger.IsEnabledFor(m_levelError); }
		}

		/// <summary>
		/// Checks if this logger is enabled for the <c>FATAL</c> level.
		/// </summary>
		/// <value>
		/// <c>true</c> if this logger is enabled for <c>FATAL</c> events,
		/// <c>false</c> otherwise.
		/// </value>
		/// <remarks>
		/// <para>
		/// See <see cref="IsDebugEnabled"/> for more information and examples of using this method.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		virtual public bool IsFatalEnabled
		{
			get { return Logger.IsEnabledFor(m_levelFatal); }
		}

		#endregion Implementation of ILog

		#region Private Methods

		/// <summary>
		/// Event handler for the <see cref="log4net.Repository.ILoggerRepository.ConfigurationChanged"/> event
		/// </summary>
		/// <param name="sender">the repository</param>
		/// <param name="e">Empty</param>
		private void LoggerRepositoryConfigurationChanged(object sender, EventArgs e)
		{
			ILoggerRepository repository = sender as ILoggerRepository;
			if (repository != null)
			{
				ReloadLevels(repository);
			}
		}

		#endregion

		#region Private Static Instance Fields

		/// <summary>
		/// The fully qualified name of this declaring type not the type of any subclass.
		/// </summary>
		private readonly static Type ThisDeclaringType = typeof(LogImpl);

		#endregion Private Static Instance Fields

		#region Private Fields

		private Level m_levelDebug;
		private Level m_levelInfo;
		private Level m_levelWarn;
		private Level m_levelError;
		private Level m_levelFatal;

		#endregion
	}
}
