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

#if NET_3_5 || NET_4_0 || MONO_3_5 || MONO_4_0 || NETSTANDARD1_3

using System;

using log4net.Core;

namespace log4net.Util
{
	/// <summary>
	/// The static class ILogExtensions contains a set of widely used
	/// methods that ease the interaction with the ILog interface implementations.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class contains methods for logging at different levels and checks the
	/// properties for determining if those logging levels are enabled in the current
	/// configuration.
	/// </para>
	/// </remarks>
	/// <example>Simple example of logging messages
	/// <code lang="C#">
	/// using log4net.Util;
	/// 
	/// ILog log = LogManager.GetLogger("application-log");
	/// 
	/// log.InfoExt("Application Start");
	/// log.DebugExt("This is a debug message");
	/// </code>
	/// </example>
	public static class ILogExtensions
	{
		#region Private Static Fields

		/// <summary>
		/// The fully qualified type of the Logger class.
		/// </summary>
		private readonly static Type declaringType = typeof(ILogExtensions);

		#endregion //Private Static Fields

		#region debug extensions

		#region debug extensions that uses log message lambda expression

		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by reading the value <seealso cref="ILog.IsDebugEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation.  If this logger is <c>INFO</c> enabled, then it converts 
		/// the message object (retrieved by invocation of the provided callback) to a 
		/// string by invoking the appropriate <see cref="log4net.ObjectRenderer.IObjectRenderer"/>.
		/// It then proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="DebugExt(ILog,Func{object},Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugExt(this ILog logger, Func<object> callback)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.Debug(callback());
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="DebugExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugExt(this ILog logger, Func<object> callback, Exception exception)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.Debug(callback(), exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region debug extension that use the formatter

		/// <overloads>Log a message object with the <see cref="Level.Debug"/> level.</overloads> //TODO
		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by reading the value <seealso cref="ILog.IsDebugEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation. If this logger is <c>INFO</c> enabled, then it converts 
		/// the message object (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="DebugExt(ILog,object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugExt(this ILog logger, object message)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.Debug(message);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="DebugExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugExt(this ILog logger, object message, Exception exception)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.Debug(message, exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region debug extension that use string format

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="DebugExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugFormatExt(this ILog logger, string format, object arg0)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.DebugFormat(format, arg0);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="DebugExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugFormatExt(this ILog logger, string format, params object[] args)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.DebugFormat(format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="DebugExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugFormatExt(this ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.DebugFormat(provider, format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="DebugExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugFormatExt(this ILog logger, string format, object arg0, object arg1)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.DebugFormat(format, arg0, arg1);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="DebugExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Debug(object)"/>
		/// <seealso cref="ILog.IsDebugEnabled"/>
		public static void DebugFormatExt(this ILog logger, string format, object arg0, object arg1, object arg2)
		{
			try
			{
				if (!logger.IsDebugEnabled)
					return;

				logger.DebugFormat(format, arg0, arg1, arg2);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#endregion

		#region info extensions

		#region info extensions that uses log message lambda expression

		/// <summary>
		/// Log a message object with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by reading the value <seealso cref="ILog.IsInfoEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation.  If this logger is <c>INFO</c> enabled, then it converts 
		/// the message object (retrieved by invocation of the provided callback) to a 
		/// string by invoking the appropriate <see cref="log4net.ObjectRenderer.IObjectRenderer"/>.
		/// It then proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="InfoExt(ILog,Func{object},Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoExt(this ILog logger, Func<object> callback)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.Info(callback());
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Info"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="InfoExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoExt(this ILog logger, Func<object> callback, Exception exception)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.Info(callback(), exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region info extension that use the formatter

		/// <overloads>Log a message object with the <see cref="Level.Info"/> level.</overloads> //TODO
		/// <summary>
		/// Log a message object with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>INFO</c>
		/// enabled by reading the value <seealso cref="ILog.IsInfoEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation. If this logger is <c>INFO</c> enabled, then it converts 
		/// the message object (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="InfoExt(ILog,object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoExt(this ILog logger, object message)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.Info(message);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Info"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="InfoExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoExt(this ILog logger, object message, Exception exception)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.Info(message, exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region info extension that use string format

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="InfoExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoFormatExt(this ILog logger, string format, object arg0)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.InfoFormat(format, arg0);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="InfoExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoFormatExt(this ILog logger, string format, params object[] args)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.InfoFormat(format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="InfoExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoFormatExt(this ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.InfoFormat(provider, format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="InfoExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoFormatExt(this ILog logger, string format, object arg0, object arg1)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.InfoFormat(format, arg0, arg1);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Info"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="InfoExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Info(object)"/>
		/// <seealso cref="ILog.IsInfoEnabled"/>
		public static void InfoFormatExt(this ILog logger, string format, object arg0, object arg1, object arg2)
		{
			try
			{
				if (!logger.IsInfoEnabled)
					return;

				logger.InfoFormat(format, arg0, arg1, arg2);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#endregion

		#region warn extensions

		#region warn extensions that uses log message lambda expression

		/// <summary>
		/// Log a message object with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>WARN</c>
		/// enabled by reading the value <seealso cref="ILog.IsWarnEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation.  If this logger is <c>WARN</c> enabled, then it converts 
		/// the message object (retrieved by invocation of the provided callback) to a 
		/// string by invoking the appropriate <see cref="log4net.ObjectRenderer.IObjectRenderer"/>.
		/// It then proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="WarnExt(ILog,Func{object},Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnExt(this ILog logger, Func<object> callback)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.Warn(callback());
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Warn"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="WarnExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnExt(this ILog logger, Func<object> callback, Exception exception)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.Warn(callback(), exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region warn extension that use the formatter

		/// <overloads>Log a message object with the <see cref="Level.Warn"/> level.</overloads> //TODO
		/// <summary>
		/// Log a message object with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>WARN</c>
		/// enabled by reading the value <seealso cref="ILog.IsWarnEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation. If this logger is <c>WARN</c> enabled, then it converts 
		/// the message object (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="WarnExt(ILog,object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnExt(this ILog logger, object message)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.Warn(message);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Warn"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="WarnExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnExt(this ILog logger, object message, Exception exception)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.Warn(message, exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region warn extension that use string format

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="WarnExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnFormatExt(this ILog logger, string format, object arg0)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.WarnFormat(format, arg0);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="WarnExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnFormatExt(this ILog logger, string format, params object[] args)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.WarnFormat(format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="WarnExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnFormatExt(this ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.WarnFormat(provider, format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="WarnExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnFormatExt(this ILog logger, string format, object arg0, object arg1)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.WarnFormat(format, arg0, arg1);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Warn"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="WarnExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Warn(object)"/>
		/// <seealso cref="ILog.IsWarnEnabled"/>
		public static void WarnFormatExt(this ILog logger, string format, object arg0, object arg1, object arg2)
		{
			try
			{
				if (!logger.IsWarnEnabled)
					return;

				logger.WarnFormat(format, arg0, arg1, arg2);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#endregion

		#region error extensions

		#region error extensions that uses log message lambda expression

		/// <summary>
		/// Log a message object with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>ERROR</c>
		/// enabled by reading the value <seealso cref="ILog.IsErrorEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation.  If this logger is <c>ERROR</c> enabled, then it converts 
		/// the message object (retrieved by invocation of the provided callback) to a 
		/// string by invoking the appropriate <see cref="log4net.ObjectRenderer.IObjectRenderer"/>.
		/// It then proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="ErrorExt(ILog,Func{object},Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorExt(this ILog logger, Func<object> callback)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.Error(callback());
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Error"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="ErrorExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorExt(this ILog logger, Func<object> callback, Exception exception)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.Error(callback(), exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region error extension that use the formatter

		/// <overloads>Log a message object with the <see cref="Level.Error"/> level.</overloads> //TODO
		/// <summary>
		/// Log a message object with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>ERROR</c>
		/// enabled by reading the value <seealso cref="ILog.IsErrorEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation. If this logger is <c>ERROR</c> enabled, then it converts 
		/// the message object (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="ErrorExt(ILog,object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorExt(this ILog logger, object message)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.Error(message);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Error"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="ErrorExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorExt(this ILog logger, object message, Exception exception)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.Error(message, exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region error extension that use string format

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="ErrorExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorFormatExt(this ILog logger, string format, object arg0)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.ErrorFormat(format, arg0);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="ErrorExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorFormatExt(this ILog logger, string format, params object[] args)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.ErrorFormat(format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="ErrorExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorFormatExt(this ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.ErrorFormat(provider, format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="ErrorExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorFormatExt(this ILog logger, string format, object arg0, object arg1)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.ErrorFormat(format, arg0, arg1);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Error"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="ErrorExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Error(object)"/>
		/// <seealso cref="ILog.IsErrorEnabled"/>
		public static void ErrorFormatExt(this ILog logger, string format, object arg0, object arg1, object arg2)
		{
			try
			{
				if (!logger.IsErrorEnabled)
					return;

				logger.ErrorFormat(format, arg0, arg1, arg2);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#endregion

		#region fatal extensions

		#region fatal extensions that uses log message lambda expression

		/// <summary>
		/// Log a message object with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>FATAL</c>
		/// enabled by reading the value <seealso cref="ILog.IsFatalEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation.  If this logger is <c>FATAL</c> enabled, then it converts 
		/// the message object (retrieved by invocation of the provided callback) to a 
		/// string by invoking the appropriate <see cref="log4net.ObjectRenderer.IObjectRenderer"/>.
		/// It then proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="FatalExt(ILog,Func{object},Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalExt(this ILog logger, Func<object> callback)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.Fatal(callback());
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Fatal"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="callback">The lambda expression that gets the object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="FatalExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalExt(this ILog logger, Func<object> callback, Exception exception)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.Fatal(callback(), exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region fatal extension that use the formatter

		/// <overloads>Log a message object with the <see cref="Level.Fatal"/> level.</overloads> //TODO
		/// <summary>
		/// Log a message object with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <remarks>
		/// <para>
		/// This method first checks if this logger is <c>FATAL</c>
		/// enabled by reading the value <seealso cref="ILog.IsFatalEnabled"/> property.
		/// This check happens always and does not depend on the <seealso cref="ILog"/>
		/// implementation. If this logger is <c>FATAL</c> enabled, then it converts 
		/// the message object (passed as parameter) to a string by invoking the appropriate
		/// <see cref="log4net.ObjectRenderer.IObjectRenderer"/>. It then 
		/// proceeds to call all the registered appenders in this logger 
		/// and also higher in the hierarchy depending on the value of 
		/// the additivity flag.
		/// </para>
		/// <para><b>WARNING</b> Note that passing an <see cref="Exception"/> 
		/// to this method will print the name of the <see cref="Exception"/> 
		/// but no stack trace. To print a stack trace use the 
		/// <see cref="FatalExt(ILog,object,Exception)"/> form instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalExt(this ILog logger, object message)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.Fatal(message);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Log a message object with the <see cref="Level.Fatal"/> level including
		/// the stack trace of the <see cref="Exception"/> passed
		/// as a parameter.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="message">The message object to log.</param>
		/// <param name="exception">The exception to log, including its stack trace.</param>
		/// <remarks>
		/// <para>
		/// See the <see cref="FatalExt(ILog, object)"/> form for more detailed information.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalExt(this ILog logger, object message, Exception exception)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.Fatal(message, exception);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#region fatal extension that use string format

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="FatalExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalFormatExt(this ILog logger, string format, object arg0)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.FatalFormat(format, arg0);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="FatalExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalFormatExt(this ILog logger, string format, params object[] args)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.FatalFormat(format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="args">An Object array containing zero or more objects to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="FatalExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalFormatExt(this ILog logger, IFormatProvider provider, string format, params object[] args)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.FatalFormat(provider, format, args);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="FatalExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalFormatExt(this ILog logger, string format, object arg0, object arg1)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.FatalFormat(format, arg0, arg1);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		/// <summary>
		/// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
		/// </summary>
		/// <param name="logger">The logger on which the message is logged.</param>
		/// <param name="format">A String containing zero or more format items</param>
		/// <param name="arg0">An Object to format</param>
		/// <param name="arg1">An Object to format</param>
		/// <param name="arg2">An Object to format</param>
		/// <remarks>
		/// <para>
		/// The message is formatted using the <c>String.Format</c> method. See
		/// <see cref="String.Format(string, object[])"/> for details of the syntax of the format string and the behavior
		/// of the formatting.
		/// </para>
		/// <para>
		/// This method does not take an <see cref="Exception"/> object to include in the
		/// log event. To pass an <see cref="Exception"/> use one of the <see cref="FatalExt(ILog,object,Exception)"/>
		/// methods instead.
		/// </para>
		/// </remarks>
		/// <seealso cref="ILog.Fatal(object)"/>
		/// <seealso cref="ILog.IsFatalEnabled"/>
		public static void FatalFormatExt(this ILog logger, string format, object arg0, object arg1, object arg2)
		{
			try
			{
				if (!logger.IsFatalEnabled)
					return;

				logger.FatalFormat(format, arg0, arg1, arg2);
			}
			catch (Exception ex)
			{
				log4net.Util.LogLog.Error(declaringType, "Exception while logging", ex);
			}
		}

		#endregion

		#endregion
	}
}
#endif
