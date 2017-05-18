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

// .NET Compact Framework 1.0 has no support for ASP.NET
// SSCLI 1.0 has no support for ASP.NET
#if !NETCF && !SSCLI && !CLIENT_PROFILE

using System.Web;

using log4net.Layout;
using log4net.Core;

namespace log4net.Appender 
{
	/// <summary>
	/// <para>
	/// Appends log events to the ASP.NET <see cref="TraceContext"/> system.
	/// </para>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Diagnostic information and tracing messages that you specify are appended to the output 
	/// of the page that is sent to the requesting browser. Optionally, you can view this information
	/// from a separate trace viewer (Trace.axd) that displays trace information for every page in a 
	/// given application.
	/// </para>
	/// <para>
	/// Trace statements are processed and displayed only when tracing is enabled. You can control 
	/// whether tracing is displayed to a page, to the trace viewer, or both.
	/// </para>
	/// <para>
	/// The logging event is passed to the <see cref="M:TraceContext.Write(string)"/> or 
	/// <see cref="M:TraceContext.Warn(string)"/> method depending on the level of the logging event.
    /// The event's logger name is the default value for the category parameter of the Write/Warn method. 
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	/// <author>Ron Grabowski</author>
	public class AspNetTraceAppender : AppenderSkeleton 
	{
		#region Public Instances Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="AspNetTraceAppender" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor.
		/// </para>
		/// </remarks>
		public AspNetTraceAppender() 
		{
		}

		#endregion // Public Instances Constructors

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Write the logging event to the ASP.NET trace
		/// </summary>
		/// <param name="loggingEvent">the event to log</param>
		/// <remarks>
		/// <para>
		/// Write the logging event to the ASP.NET trace
		/// <c>HttpContext.Current.Trace</c> 
		/// (<see cref="TraceContext"/>).
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			// check if log4net is running in the context of an ASP.NET application
			if (HttpContext.Current != null) 
			{
				// check if tracing is enabled for the current context
				if (HttpContext.Current.Trace.IsEnabled) 
				{
					if (loggingEvent.Level >= Level.Warn) 
					{
                        HttpContext.Current.Trace.Warn(m_category.Format(loggingEvent), RenderLoggingEvent(loggingEvent));
					}
					else 
					{
                        HttpContext.Current.Trace.Write(m_category.Format(loggingEvent), RenderLoggingEvent(loggingEvent));
					}
				}
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

		#endregion // Override implementation of AppenderSkeleton

        #region Public Instance Properties

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

	    #endregion

	    #region Private Instance Fields

	    /// <summary>
	    /// Defaults to %logger
	    /// </summary>
	    private PatternLayout m_category = new PatternLayout("%logger");

	    #endregion
	}
}

#endif // !NETCF && !SSCLI
