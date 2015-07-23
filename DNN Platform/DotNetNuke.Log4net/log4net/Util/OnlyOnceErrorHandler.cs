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

using log4net.Core;

namespace log4net.Util
{
	/// <summary>
	/// Implements log4net's default error handling policy which consists 
	/// of emitting a message for the first error in an appender and 
	/// ignoring all subsequent errors.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The error message is processed using the LogLog sub-system.
	/// </para>
	/// <para>
	/// This policy aims at protecting an otherwise working application
	/// from being flooded with error messages when logging fails.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Ron Grabowski</author>
	public class OnlyOnceErrorHandler : IErrorHandler
	{
		#region Public Instance Constructors

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="OnlyOnceErrorHandler" /> class.
		/// </para>
		/// </remarks>
		public OnlyOnceErrorHandler()
		{
			m_prefix = "";
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="prefix">The prefix to use for each message.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="OnlyOnceErrorHandler" /> class
		/// with the specified prefix.
		/// </para>
		/// </remarks>
		public OnlyOnceErrorHandler(string prefix)
		{
			m_prefix = prefix;
		}

		#endregion Public Instance Constructors

		#region Public Instance Methods

		/// <summary>
		/// Reset the error handler back to its initial disabled state.
		/// </summary>
		public void Reset()
		{
			m_enabledDate = DateTime.MinValue;
			m_errorCode = ErrorCode.GenericFailure;
			m_exception = null;
			m_message = null;
			m_firstTime = true;
		}

		#region Implementation of IErrorHandler

		/// <summary>
		/// Log an Error
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="e">The exception.</param>
		/// <param name="errorCode">The internal error code.</param>
		/// <remarks>
		/// <para>
		/// Sends the error information to <see cref="LogLog"/>'s Error method.
		/// </para>
		/// </remarks>
		public void Error(string message, Exception e, ErrorCode errorCode) 
		{
			if (m_firstTime)
			{
				m_enabledDate = DateTime.Now;
				m_errorCode = errorCode;
				m_exception = e;
				m_message = message;
				m_firstTime = false;

				if (LogLog.InternalDebugging && !LogLog.QuietMode)
				{
					LogLog.Error(declaringType, "[" + m_prefix + "] ErrorCode: " + errorCode.ToString() + ". " + message, e);
				}
			}
		}

		/// <summary>
		/// Log an Error
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="e">The exception.</param>
		/// <remarks>
		/// <para>
		/// Prints the message and the stack trace of the exception on the standard
		/// error output stream.
		/// </para>
		/// </remarks>
		public void Error(string message, Exception e) 
		{
			Error(message, e, ErrorCode.GenericFailure);
		}

		/// <summary>
		/// Log an error
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <remarks>
		/// <para>
		/// Print a the error message passed as parameter on the standard
		/// error output stream.
		/// </para>
		/// </remarks>
		public void Error(string message) 
		{
			Error(message, null, ErrorCode.GenericFailure);
		}

		#endregion Implementation of IErrorHandler

		#endregion

		#region Public Instance Properties

		/// <summary>
		/// Is error logging enabled
		/// </summary>
		/// <remarks>
		/// <para>
		/// Is error logging enabled. Logging is only enabled for the
		/// first error delivered to the <see cref="OnlyOnceErrorHandler"/>.
		/// </para>
		/// </remarks>
		public bool IsEnabled
		{
			get { return m_firstTime; }
		}

		/// <summary>
		/// The date the first error that trigged this error handler occured.
		/// </summary>
		public DateTime EnabledDate
		{
			get { return m_enabledDate; }
		}

		/// <summary>
		/// The message from the first error that trigged this error handler.
		/// </summary>
		public string ErrorMessage
		{
			get { return m_message; }
		}

		/// <summary>
		/// The exception from the first error that trigged this error handler.
		/// </summary>
		/// <remarks>
		/// May be <see langword="null" />.
		/// </remarks>
		public Exception Exception
		{
			get { return m_exception; }
		}

		/// <summary>
		/// The error code from the first error that trigged this error handler.
		/// </summary>
		/// <remarks>
		/// Defaults to <see cref="log4net.Core.ErrorCode.GenericFailure"/>
		/// </remarks>
		public ErrorCode ErrorCode
		{
			get { return m_errorCode; }
		}

		#endregion

		#region Private Instance Fields

		/// <summary>
		/// The date the error was recorded.
		/// </summary>
		private DateTime m_enabledDate;

		/// <summary>
		/// Flag to indicate if it is the first error
		/// </summary>
		private bool m_firstTime = true;

		/// <summary>
		/// The message recorded during the first error.
		/// </summary>
		private string m_message = null;

		/// <summary>
		/// The exception recorded during the first error.
		/// </summary>
		private Exception m_exception = null;

		/// <summary>
		/// The error code recorded during the first error.
		/// </summary>
		private ErrorCode m_errorCode = ErrorCode.GenericFailure;

		/// <summary>
		/// String to prefix each message with
		/// </summary>
		private readonly string m_prefix;

		#endregion Private Instance Fields

		#region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the OnlyOnceErrorHandler class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
		private readonly static Type declaringType = typeof(OnlyOnceErrorHandler);

		#endregion
	}
}
