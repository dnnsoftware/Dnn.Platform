// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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

#define DEBUG

using log4net.Layout;
using log4net.Core;

namespace log4net.Appender;

/// <summary>
/// Appends log events to the <see cref="System.Diagnostics.Debug"/> system.
/// </summary>
/// <remarks>
/// <para>
/// The application configuration file can be used to control what listeners 
/// are actually used. See the MSDN documentation for the 
/// <see cref="System.Diagnostics.Debug"/> class for details on configuring the
/// debug system.
/// </para>
/// <para>
/// Events are written using the <see cref="M:System.Diagnostics.Debug.Write(string,string)"/>
/// method. The event's logger name is passed as the value for the category name to the Write method.
/// </para>
/// </remarks>
/// <author>Nicko Cadell</author>
public class DebugAppender : AppenderSkeleton
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DebugAppender" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Default constructor.
    /// </para>
    /// </remarks>
    public DebugAppender()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugAppender" /> 
    /// with a specified layout.
    /// </summary>
    /// <param name="layout">The layout to use with this appender.</param>
    /// <remarks>
    /// <para>
    /// Obsolete constructor.
    /// </para>
    /// </remarks>
    [System.Obsolete("Instead use the default constructor and set the Layout property")]
    public DebugAppender(ILayout layout)
    {
        this.Layout = layout;
    }

#if !NETSTANDARD1_3 // System.Diagnostics.Debug has no Flush() in netstandard1.3
    /// <summary>
    /// Gets or sets a value that indicates whether the appender will 
    /// flush at the end of each write.
    /// </summary>
    /// <remarks>
    /// <para>The default behavior is to flush at the end of each 
    /// write. If the option is set to<c>false</c>, then the underlying 
    /// stream can defer writing to physical medium to a later time. 
    /// </para>
    /// <para>
    /// Avoiding the flush operation at the end of each append results 
    /// in a performance gain of 10 to 20 percent. However, there is safety
    /// trade-off involved in skipping flushing. Indeed, when flushing is
    /// skipped, then it is likely that the last few log events will not
    /// be recorded on disk when the application exits. This is a high
    /// price to pay even for a 20% performance gain.
    /// </para>
    /// </remarks>
    public bool ImmediateFlush
    {
        get { return this.m_immediateFlush; }
        set { this.m_immediateFlush = value; }
    }
#endif // !NETSTANDARD1_3

    /// <summary>
    /// Formats the category parameter sent to the Debug method.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Defaults to a <see cref="Layout.PatternLayout"/> with %logger as the pattern which will use the logger name of the current 
    /// <see cref="LoggingEvent"/> as the category parameter.
    /// </para>
    /// <para>
    /// </para> 
    /// </remarks>
    public PatternLayout Category
    {
        get { return this.m_category; }
        set { this.m_category = value; }
    }

#if !NETSTANDARD1_3
    /// <summary>
    /// Flushes any buffered log data.
    /// </summary>
    /// <param name="millisecondsTimeout">The maximum time to wait for logging events to be flushed.</param>
    /// <returns><c>True</c> if all logging events were flushed successfully, else <c>false</c>.</returns>
    public override bool Flush(int millisecondsTimeout)
    {
        // Nothing to do if ImmediateFlush is true
        if (this.m_immediateFlush)
        {
            return true;
        }

        // System.Diagnostics.Debug is thread-safe, so no need for lock(this).
        System.Diagnostics.Debug.Flush();

        return true;
    }
#endif

#if NETSTANDARD1_3
        /// <summary>
        /// Writes the logging event to the <see cref="System.Diagnostics.Debug"/> system.
        /// </summary>
        /// <param name="loggingEvent">The event to log.</param>
        /// <remarks>
        /// <para>
        /// Writes the logging event to the <see cref="System.Diagnostics.Debug"/> system.
        /// </para>
        /// </remarks>
#else
    /// <summary>
    /// Writes the logging event to the <see cref="System.Diagnostics.Debug"/> system.
    /// </summary>
    /// <param name="loggingEvent">The event to log.</param>
    /// <remarks>
    /// <para>
    /// Writes the logging event to the <see cref="System.Diagnostics.Debug"/> system.
    /// If <see cref="ImmediateFlush"/> is <c>true</c> then the <see cref="System.Diagnostics.Debug.Flush"/>
    /// is called.
    /// </para>
    /// </remarks> 
#endif
    protected override void Append(LoggingEvent loggingEvent)
    {
        //
        // Write the string to the Debug system
        //
        if(this.m_category == null)
        {
            System.Diagnostics.Debug.Write(this.RenderLoggingEvent(loggingEvent));
        }
        else
        {
            string category = this.m_category.Format(loggingEvent);
            if (string.IsNullOrEmpty(category))
            {
                System.Diagnostics.Debug.Write(this.RenderLoggingEvent(loggingEvent));
            }
            else
            {
                System.Diagnostics.Debug.Write(this.RenderLoggingEvent(loggingEvent), category);
            }
        }
#if !NETSTANDARD1_3
        //
        // Flush the Debug system if needed
        //
        if (this.m_immediateFlush) 
        {
            System.Diagnostics.Debug.Flush();
        } 
#endif
    }

    /// <summary>
    /// This appender requires a <see cref="Layout"/> to be set.
    /// </summary>
    /// <value><c>true</c></value>
    /// <remarks>
    /// <para>
    /// This appender requires a <see cref="Layout"/> to be set.
    /// </para>
    /// </remarks>
    protected override bool RequiresLayout
    {
        get { return true; }
    }

#if !NETSTANDARD1_3		
    /// <summary>
    /// Immediate flush means that the underlying writer or output stream
    /// will be flushed at the end of each append operation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Immediate flush is slower but ensures that each append request is 
    /// actually written. If <see cref="ImmediateFlush"/> is set to
    /// <c>false</c>, then there is a good chance that the last few
    /// logs events are not actually written to persistent media if and
    /// when the application crashes.
    /// </para>
    /// <para>
    /// The default value is <c>true</c>.</para>
    /// </remarks>
    private bool m_immediateFlush = true;
#endif

    /// <summary>
    /// Defaults to a <see cref="Layout.PatternLayout"/> with %logger as the pattern.
    /// </summary>
    private PatternLayout m_category = new PatternLayout("%logger");
}