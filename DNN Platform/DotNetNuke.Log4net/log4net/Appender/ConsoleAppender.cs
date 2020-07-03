﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Appender
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
    using System.Globalization;

    using log4net.Core;
    using log4net.Layout;
    using log4net.Util;

    /// <summary>
    /// Appends logging events to the console.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ConsoleAppender appends log events to the standard output stream
    /// or the error output stream using a layout specified by the
    /// user.
    /// </para>
    /// <para>
    /// By default, all output is written to the console's standard output stream.
    /// The <see cref="Target"/> property can be set to direct the output to the
    /// error stream.
    /// </para>
    /// <para>
    /// NOTE: This appender writes each message to the <c>System.Console.Out</c> or
    /// <c>System.Console.Error</c> that is set at the time the event is appended.
    /// Therefore it is possible to programmatically redirect the output of this appender
    /// (for example NUnit does this to capture program output). While this is the desired
    /// behavior of this appender it may have security implications in your application.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public class ConsoleAppender : AppenderSkeleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleAppender" /> class.
        /// </summary>
        /// <remarks>
        /// The instance of the <see cref="ConsoleAppender" /> class is set up to write
        /// to the standard output stream.
        /// </remarks>
        public ConsoleAppender()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleAppender" /> class
        /// with the specified layout.
        /// </summary>
        /// <param name="layout">the layout to use for this appender.</param>
        /// <remarks>
        /// The instance of the <see cref="ConsoleAppender" /> class is set up to write
        /// to the standard output stream.
        /// </remarks>
        [Obsolete("Instead use the default constructor and set the Layout property. Scheduled removal in v10.0.0.")]
        public ConsoleAppender(ILayout layout)
            : this(layout, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleAppender" /> class
        /// with the specified layout.
        /// </summary>
        /// <param name="layout">the layout to use for this appender.</param>
        /// <param name="writeToErrorStream">flag set to <c>true</c> to write to the console error stream.</param>
        /// <remarks>
        /// When <paramref name="writeToErrorStream" /> is set to <c>true</c>, output is written to
        /// the standard error output stream.  Otherwise, output is written to the standard
        /// output stream.
        /// </remarks>
        [Obsolete("Instead use the default constructor and set the Layout & Target properties. Scheduled removal in v10.0.0.")]
        public ConsoleAppender(ILayout layout, bool writeToErrorStream)
        {
            this.Layout = layout;
            this.m_writeToErrorStream = writeToErrorStream;
        }

        /// <summary>
        /// Gets or sets target is the value of the console output stream.
        /// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
        /// </summary>
        /// <value>
        /// Target is the value of the console output stream.
        /// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// Target is the value of the console output stream.
        /// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
        /// </para>
        /// </remarks>
        public virtual string Target
        {
            get { return this.m_writeToErrorStream ? ConsoleError : ConsoleOut; }

            set
            {
                string v = value.Trim();

                if (SystemInfo.EqualsIgnoringCase(ConsoleError, v))
                {
                    this.m_writeToErrorStream = true;
                }
                else
                {
                    this.m_writeToErrorStream = false;
                }
            }
        }

        /// <summary>
        /// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)"/> method.
        /// </summary>
        /// <param name="loggingEvent">The event to log.</param>
        /// <remarks>
        /// <para>
        /// Writes the event to the console.
        /// </para>
        /// <para>
        /// The format of the output will depend on the appender's layout.
        /// </para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
#if NETCF_1_0
			// Write to the output stream
			Console.Write(RenderLoggingEvent(loggingEvent));
#else
            if (this.m_writeToErrorStream)
            {
                // Write to the error stream
                Console.Error.Write(this.RenderLoggingEvent(loggingEvent));
            }
            else
            {
                // Write to the output stream
                Console.Write(this.RenderLoggingEvent(loggingEvent));
            }
#endif
        }

        /// <summary>
        /// Gets a value indicating whether this appender requires a <see cref="Layout"/> to be set.
        /// </summary>
        /// <value><c>true</c>.</value>
        /// <remarks>
        /// <para>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </para>
        /// </remarks>
        protected override bool RequiresLayout
        {
            get { return true; }
        }

        /// <summary>
        /// The <see cref="ConsoleAppender.Target"/> to use when writing to the Console
        /// standard output stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ConsoleAppender.Target"/> to use when writing to the Console
        /// standard output stream.
        /// </para>
        /// </remarks>
        public const string ConsoleOut = "Console.Out";

        /// <summary>
        /// The <see cref="ConsoleAppender.Target"/> to use when writing to the Console
        /// standard error output stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ConsoleAppender.Target"/> to use when writing to the Console
        /// standard error output stream.
        /// </para>
        /// </remarks>
        public const string ConsoleError = "Console.Error";
        private bool m_writeToErrorStream = false;
    }
}
