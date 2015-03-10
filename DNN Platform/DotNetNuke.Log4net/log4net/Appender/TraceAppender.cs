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

#define TRACE

using log4net.Layout;
using log4net.Core;

namespace log4net.Appender
{
	/// <summary>
	/// Appends log events to the <see cref="System.Diagnostics.Trace"/> system.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The application configuration file can be used to control what listeners 
	/// are actually used. See the MSDN documentation for the 
	/// <see cref="System.Diagnostics.Trace"/> class for details on configuring the
	/// trace system.
	/// </para>
	/// <para>
	/// Events are written using the <c>System.Diagnostics.Trace.Write(string,string)</c>
	/// method. The event's logger name is the default value for the category parameter 
    /// of the Write method. 
	/// </para>
	/// <para>
	/// <b>Compact Framework</b><br />
	/// The Compact Framework does not support the <see cref="System.Diagnostics.Trace"/>
	/// class for any operation except <c>Assert</c>. When using the Compact Framework this
	/// appender will write to the <see cref="System.Diagnostics.Debug"/> system rather than
	/// the Trace system. This appender will therefore behave like the <see cref="DebugAppender"/>.
	/// </para>
	/// </remarks>
	/// <author>Douglas de la Torre</author>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
    /// <author>Ron Grabowski</author>
	public class TraceAppender : AppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceAppender" />.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor.
		/// </para>
		/// </remarks>
		public TraceAppender()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TraceAppender" /> 
		/// with a specified layout.
		/// </summary>
		/// <param name="layout">The layout to use with this appender.</param>
		/// <remarks>
		/// <para>
		/// Obsolete constructor.
		/// </para>
		/// </remarks>
		[System.Obsolete("Instead use the default constructor and set the Layout property")]
		public TraceAppender(ILayout layout)
		{
			Layout = layout;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

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
			get { return m_immediateFlush; }
			set { m_immediateFlush = value; }
		}

        /// <summary>
        /// The category parameter sent to the Trace method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Defaults to %logger which will use the logger name of the current 
        /// <see cref="LoggingEvent"/> as the category parameter.
        /// </para>
        /// <para>
        /// </para> 
        /// </remarks>
	    public PatternLayout Category
	    {
	        get { return m_category; }
	        set { m_category = value; }
	    }

	    #endregion Public Instance Properties

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Writes the logging event to the <see cref="System.Diagnostics.Trace"/> system.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Writes the logging event to the <see cref="System.Diagnostics.Trace"/> system.
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			//
			// Write the string to the Trace system
			//
#if NETCF
			System.Diagnostics.Debug.Write(RenderLoggingEvent(loggingEvent), m_category.Format(loggingEvent));
#else
            System.Diagnostics.Trace.Write(RenderLoggingEvent(loggingEvent), m_category.Format(loggingEvent));
#endif
	 
			//
			// Flush the Trace system if needed
			//
			if (m_immediateFlush) 
			{
#if NETCF
				System.Diagnostics.Debug.Flush();
#else
				System.Diagnostics.Trace.Flush();
#endif
			} 
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
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		#endregion Override implementation of AppenderSkeleton

		#region Private Instance Fields

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

        /// <summary>
        /// Defaults to %logger
        /// </summary>
        private PatternLayout m_category = new PatternLayout("%logger");

		#endregion Private Instance Fields
	}
}
