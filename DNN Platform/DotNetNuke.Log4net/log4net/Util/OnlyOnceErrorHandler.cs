﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util
{
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
    using System;

    using log4net.Core;

    /// <summary>
    /// Implements log4net's default error handling policy which consists
    /// of emitting a message for the first error in an appender and
    /// ignoring all subsequent errors.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The error message is processed using the LogLog sub-system by default.
    /// </para>
    /// <para>
    /// This policy aims at protecting an otherwise working application
    /// from being flooded with error messages when logging fails.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    /// <author>Ron Grabowski.</author>
    public class OnlyOnceErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyOnceErrorHandler"/> class.
        /// Default Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="OnlyOnceErrorHandler" /> class.
        /// </para>
        /// </remarks>
        public OnlyOnceErrorHandler()
        {
            this.m_prefix = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OnlyOnceErrorHandler"/> class.
        /// Constructor.
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
            this.m_prefix = prefix;
        }

        /// <summary>
        /// Reset the error handler back to its initial disabled state.
        /// </summary>
        public void Reset()
        {
            this.m_enabledDateUtc = DateTime.MinValue;
            this.m_errorCode = ErrorCode.GenericFailure;
            this.m_exception = null;
            this.m_message = null;
            this.m_firstTime = true;
        }

        /// <summary>
        /// Log an Error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="errorCode">The internal error code.</param>
        /// <remarks>
        /// <para>
        /// Invokes <see cref="FirstError"/> if and only if this is the first error or the first error after <see cref="Reset"/> has been called.
        /// </para>
        /// </remarks>
        public void Error(string message, Exception e, ErrorCode errorCode)
        {
            if (this.m_firstTime)
            {
                this.FirstError(message, e, errorCode);
            }
        }

        /// <summary>
        /// Log the very first error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception.</param>
        /// <param name="errorCode">The internal error code.</param>
        /// <remarks>
        /// <para>
        /// Sends the error information to <see cref="LogLog"/>'s Error method.
        /// </para>
        /// </remarks>
        public virtual void FirstError(string message, Exception e, ErrorCode errorCode)
        {
            this.m_enabledDateUtc = DateTime.UtcNow;
            this.m_errorCode = errorCode;
            this.m_exception = e;
            this.m_message = message;
            this.m_firstTime = false;

            if (LogLog.InternalDebugging && !LogLog.QuietMode)
            {
                LogLog.Error(declaringType, "[" + this.m_prefix + "] ErrorCode: " + errorCode.ToString() + ". " + message, e);
            }
        }

        /// <summary>
        /// Log an Error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="e">The exception.</param>
        /// <remarks>
        /// <para>
        /// Invokes <see cref="FirstError"/> if and only if this is the first error or the first error after <see cref="Reset"/> has been called.
        /// </para>
        /// </remarks>
        public void Error(string message, Exception e)
        {
            this.Error(message, e, ErrorCode.GenericFailure);
        }

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <remarks>
        /// <para>
        /// Invokes <see cref="FirstError"/> if and only if this is the first error or the first error after <see cref="Reset"/> has been called.
        /// </para>
        /// </remarks>
        public void Error(string message)
        {
            this.Error(message, null, ErrorCode.GenericFailure);
        }

        /// <summary>
        /// Gets a value indicating whether is error logging enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Is error logging enabled. Logging is only enabled for the
        /// first error delivered to the <see cref="OnlyOnceErrorHandler"/>.
        /// </para>
        /// </remarks>
        public bool IsEnabled
        {
            get { return this.m_firstTime; }
        }

        /// <summary>
        /// Gets the date the first error that trigged this error handler occurred, or <see cref="DateTime.MinValue"/> if it has not been triggered.
        /// </summary>
        public DateTime EnabledDate
        {
            get
            {
                if (this.m_enabledDateUtc == DateTime.MinValue)
                {
                    return DateTime.MinValue;
                }

                return this.m_enabledDateUtc.ToLocalTime();
            }
        }

        /// <summary>
        /// Gets the UTC date the first error that trigged this error handler occured, or <see cref="DateTime.MinValue"/> if it has not been triggered.
        /// </summary>
        public DateTime EnabledDateUtc
        {
            get { return this.m_enabledDateUtc; }
        }

        /// <summary>
        /// Gets the message from the first error that trigged this error handler.
        /// </summary>
        public string ErrorMessage
        {
            get { return this.m_message; }
        }

        /// <summary>
        /// Gets the exception from the first error that trigged this error handler.
        /// </summary>
        /// <remarks>
        /// May be <see langword="null" />.
        /// </remarks>
        public Exception Exception
        {
            get { return this.m_exception; }
        }

        /// <summary>
        /// Gets the error code from the first error that trigged this error handler.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="log4net.Core.ErrorCode.GenericFailure"/>.
        /// </remarks>
        public ErrorCode ErrorCode
        {
            get { return this.m_errorCode; }
        }

        /// <summary>
        /// The UTC date the error was recorded.
        /// </summary>
        private DateTime m_enabledDateUtc;

        /// <summary>
        /// Flag to indicate if it is the first error.
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
        /// String to prefix each message with.
        /// </summary>
        private readonly string m_prefix;

        /// <summary>
        /// The fully qualified type of the OnlyOnceErrorHandler class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(OnlyOnceErrorHandler);
    }
}
