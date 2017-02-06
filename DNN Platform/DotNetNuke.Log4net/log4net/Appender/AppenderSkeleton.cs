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
using System.IO;
using System.Collections;

using log4net.Filter;
using log4net.Util;
using log4net.Layout;
using log4net.Core;

namespace log4net.Appender
{
	/// <summary>
	/// Abstract base class implementation of <see cref="IAppender"/>. 
	/// </summary>
	/// <remarks>
	/// <para>
	/// This class provides the code for common functionality, such 
	/// as support for threshold filtering and support for general filters.
	/// </para>
	/// <para>
	/// Appenders can also implement the <see cref="IOptionHandler"/> interface. Therefore
	/// they would require that the <see cref="M:IOptionHandler.ActivateOptions()"/> method
	/// be called after the appenders properties have been configured.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public abstract class AppenderSkeleton : IAppender, IBulkAppender, IOptionHandler, IFlushable
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>Empty default constructor</para>
		/// </remarks>
		protected AppenderSkeleton()
		{
			m_errorHandler = new OnlyOnceErrorHandler(this.GetType().Name);
		}

		#endregion Protected Instance Constructors

		#region Finalizer

		/// <summary>
		/// Finalizes this appender by calling the implementation's 
		/// <see cref="Close"/> method.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If this appender has not been closed then the <c>Finalize</c> method
		/// will call <see cref="Close"/>.
		/// </para>
		/// </remarks>
		~AppenderSkeleton() 
		{
			// An appender might be closed then garbage collected. 
			// There is no point in closing twice.
			if (!m_closed) 
			{
				LogLog.Debug(declaringType, "Finalizing appender named ["+m_name+"].");
				Close();
			}
		}

		#endregion Finalizer

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the threshold <see cref="Level"/> of this appender.
		/// </summary>
		/// <value>
		/// The threshold <see cref="Level"/> of the appender. 
		/// </value>
		/// <remarks>
		/// <para>
		/// All log events with lower level than the threshold level are ignored 
		/// by the appender.
		/// </para>
		/// <para>
		/// In configuration files this option is specified by setting the
		/// value of the <see cref="Threshold"/> option to a level
		/// string, such as "DEBUG", "INFO" and so on.
		/// </para>
		/// </remarks>
		public Level Threshold 
		{
			get { return m_threshold; }
			set { m_threshold = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="IErrorHandler"/> for this appender.
		/// </summary>
		/// <value>The <see cref="IErrorHandler"/> of the appender</value>
		/// <remarks>
		/// <para>
		/// The <see cref="AppenderSkeleton"/> provides a default 
		/// implementation for the <see cref="ErrorHandler"/> property. 
		/// </para>
		/// </remarks>
		virtual public IErrorHandler ErrorHandler 
		{
			get { return this.m_errorHandler; }
			set 
			{
				lock(this) 
				{
					if (value == null) 
					{
						// We do not throw exception here since the cause is probably a
						// bad config file.
						LogLog.Warn(declaringType, "You have tried to set a null error-handler.");
					} 
					else 
					{
						m_errorHandler = value;
					}
				}
			}
		}

		/// <summary>
		/// The filter chain.
		/// </summary>
		/// <value>The head of the filter chain filter chain.</value>
		/// <remarks>
		/// <para>
		/// Returns the head Filter. The Filters are organized in a linked list
		/// and so all Filters on this Appender are available through the result.
		/// </para>
		/// </remarks>
		virtual public IFilter FilterHead
		{
			get { return m_headFilter; }
		}

		/// <summary>
		/// Gets or sets the <see cref="ILayout"/> for this appender.
		/// </summary>
		/// <value>The layout of the appender.</value>
		/// <remarks>
		/// <para>
		/// See <see cref="RequiresLayout"/> for more information.
		/// </para>
		/// </remarks>
		/// <seealso cref="RequiresLayout"/>
		virtual public ILayout Layout 
		{
			get { return m_layout; }
			set { m_layout = value; }
		}

		#endregion

		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialize the appender based on the options set
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is part of the <see cref="IOptionHandler"/> delayed object
		/// activation scheme. The <see cref="ActivateOptions"/> method must 
		/// be called on this object after the configuration properties have
		/// been set. Until <see cref="ActivateOptions"/> is called this
		/// object is in an undefined state and must not be used. 
		/// </para>
		/// <para>
		/// If any of the configuration properties are modified then 
		/// <see cref="ActivateOptions"/> must be called again.
		/// </para>
		/// </remarks>
		virtual public void ActivateOptions() 
		{
		}

		#endregion Implementation of IOptionHandler

		#region Implementation of IAppender

		/// <summary>
		/// Gets or sets the name of this appender.
		/// </summary>
		/// <value>The name of the appender.</value>
		/// <remarks>
		/// <para>
		/// The name uniquely identifies the appender.
		/// </para>
		/// </remarks>
		public string Name 
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// Closes the appender and release resources.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Release any resources allocated within the appender such as file handles, 
		/// network connections, etc.
		/// </para>
		/// <para>
		/// It is a programming error to append to a closed appender.
		/// </para>
		/// <para>
		/// This method cannot be overridden by subclasses. This method 
		/// delegates the closing of the appender to the <see cref="OnClose"/>
		/// method which must be overridden in the subclass.
		/// </para>
		/// </remarks>
		public void Close()
		{
			// This lock prevents the appender being closed while it is still appending
			lock(this)
			{
				if (!m_closed)
				{
					OnClose();
					m_closed = true;
				}
			}
		}

		/// <summary>
		/// Performs threshold checks and invokes filters before 
		/// delegating actual logging to the subclasses specific 
		/// <see cref="M:Append(LoggingEvent)"/> method.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// This method cannot be overridden by derived classes. A
		/// derived class should override the <see cref="M:Append(LoggingEvent)"/> method
		/// which is called by this method.
		/// </para>
		/// <para>
		/// The implementation of this method is as follows:
		/// </para>
		/// <para>
		/// <list type="bullet">
		///		<item>
		///			<description>
		///			Checks that the severity of the <paramref name="loggingEvent"/>
		///			is greater than or equal to the <see cref="Threshold"/> of this
		///			appender.</description>
		///		</item>
		///		<item>
		///			<description>
		///			Checks that the <see cref="IFilter"/> chain accepts the 
		///			<paramref name="loggingEvent"/>.
		///			</description>
		///		</item>
		///		<item>
		///			<description>
		///			Calls <see cref="M:PreAppendCheck()"/> and checks that 
		///			it returns <c>true</c>.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// If all of the above steps succeed then the <paramref name="loggingEvent"/>
		/// will be passed to the abstract <see cref="M:Append(LoggingEvent)"/> method.
		/// </para>
		/// </remarks>
		public void DoAppend(LoggingEvent loggingEvent) 
		{
			// This lock is absolutely critical for correct formatting
			// of the message in a multi-threaded environment.  Without
			// this, the message may be broken up into elements from
			// multiple thread contexts (like get the wrong thread ID).

			lock(this)
			{
				if (m_closed)
				{
					ErrorHandler.Error("Attempted to append to closed appender named ["+m_name+"].");
					return;
				}

				// prevent re-entry
				if (m_recursiveGuard)
				{
					return;
				}

				try
				{
					m_recursiveGuard = true;

					if (FilterEvent(loggingEvent) && PreAppendCheck())
					{
						this.Append(loggingEvent);
					}
				}
				catch(Exception ex)
				{
					ErrorHandler.Error("Failed in DoAppend", ex);
				}
#if !MONO && !NET_2_0 && !NETSTANDARD1_3
				// on .NET 2.0 (and higher) and Mono (all profiles), 
				// exceptions that do not derive from System.Exception will be
				// wrapped in a RuntimeWrappedException by the runtime, and as
				// such will be catched by the catch clause above
				catch
				{
					// Catch handler for non System.Exception types
					ErrorHandler.Error("Failed in DoAppend (unknown exception)");
				}
#endif
				finally
				{
					m_recursiveGuard = false;
				}
			}
		}

		#endregion Implementation of IAppender

		#region Implementation of IBulkAppender

		/// <summary>
		/// Performs threshold checks and invokes filters before 
		/// delegating actual logging to the subclasses specific 
		/// <see cref="M:Append(LoggingEvent[])"/> method.
		/// </summary>
		/// <param name="loggingEvents">The array of events to log.</param>
		/// <remarks>
		/// <para>
		/// This method cannot be overridden by derived classes. A
		/// derived class should override the <see cref="M:Append(LoggingEvent[])"/> method
		/// which is called by this method.
		/// </para>
		/// <para>
		/// The implementation of this method is as follows:
		/// </para>
		/// <para>
		/// <list type="bullet">
		///		<item>
		///			<description>
		///			Checks that the severity of the <paramref name="loggingEvents"/>
		///			is greater than or equal to the <see cref="Threshold"/> of this
		///			appender.</description>
		///		</item>
		///		<item>
		///			<description>
		///			Checks that the <see cref="IFilter"/> chain accepts the 
		///			<paramref name="loggingEvents"/>.
		///			</description>
		///		</item>
		///		<item>
		///			<description>
		///			Calls <see cref="M:PreAppendCheck()"/> and checks that 
		///			it returns <c>true</c>.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// If all of the above steps succeed then the <paramref name="loggingEvents"/>
		/// will be passed to the <see cref="M:Append(LoggingEvent[])"/> method.
		/// </para>
		/// </remarks>
		public void DoAppend(LoggingEvent[] loggingEvents) 
		{
			// This lock is absolutely critical for correct formatting
			// of the message in a multi-threaded environment.  Without
			// this, the message may be broken up into elements from
			// multiple thread contexts (like get the wrong thread ID).

			lock(this)
			{
				if (m_closed)
				{
					ErrorHandler.Error("Attempted to append to closed appender named ["+m_name+"].");
					return;
				}

				// prevent re-entry
				if (m_recursiveGuard)
				{
					return;
				}

				try
				{
					m_recursiveGuard = true;

					ArrayList filteredEvents = new ArrayList(loggingEvents.Length);

					foreach(LoggingEvent loggingEvent in loggingEvents)
					{
						if (FilterEvent(loggingEvent))
						{
							filteredEvents.Add(loggingEvent);
						}
					}

					if (filteredEvents.Count > 0 && PreAppendCheck())
					{
						this.Append((LoggingEvent[])filteredEvents.ToArray(typeof(LoggingEvent)));
					}
				}
				catch(Exception ex)
				{
					ErrorHandler.Error("Failed in Bulk DoAppend", ex);
				}
#if !MONO && !NET_2_0 && !NETSTANDARD1_3
				// on .NET 2.0 (and higher) and Mono (all profiles), 
				// exceptions that do not derive from System.Exception will be
				// wrapped in a RuntimeWrappedException by the runtime, and as
				// such will be catched by the catch clause above
				catch
				{
					// Catch handler for non System.Exception types
					ErrorHandler.Error("Failed in Bulk DoAppend (unknown exception)");
				}
#endif
				finally
				{
					m_recursiveGuard = false;
				}
			}
		}

		#endregion Implementation of IBulkAppender

		/// <summary>
		/// Test if the logging event should we output by this appender
		/// </summary>
		/// <param name="loggingEvent">the event to test</param>
		/// <returns><c>true</c> if the event should be output, <c>false</c> if the event should be ignored</returns>
		/// <remarks>
		/// <para>
		/// This method checks the logging event against the threshold level set
		/// on this appender and also against the filters specified on this
		/// appender.
		/// </para>
		/// <para>
		/// The implementation of this method is as follows:
		/// </para>
		/// <para>
		/// <list type="bullet">
		///		<item>
		///			<description>
		///			Checks that the severity of the <paramref name="loggingEvent"/>
		///			is greater than or equal to the <see cref="Threshold"/> of this
		///			appender.</description>
		///		</item>
		///		<item>
		///			<description>
		///			Checks that the <see cref="IFilter"/> chain accepts the 
		///			<paramref name="loggingEvent"/>.
		///			</description>
		///		</item>
		/// </list>
		/// </para>
		/// </remarks>
		virtual protected bool FilterEvent(LoggingEvent loggingEvent)
		{
			if (!IsAsSevereAsThreshold(loggingEvent.Level)) 
			{
				return false;
			}

			IFilter f = this.FilterHead;

			while(f != null) 
			{
				switch(f.Decide(loggingEvent)) 
				{
					case FilterDecision.Deny: 
						return false;	// Return without appending

					case FilterDecision.Accept:
						f = null;		// Break out of the loop
						break;

					case FilterDecision.Neutral:
						f = f.Next;		// Move to next filter
						break;
				}
			}

			return true;
		}

		#region Public Instance Methods

		/// <summary>
		/// Adds a filter to the end of the filter chain.
		/// </summary>
		/// <param name="filter">the filter to add to this appender</param>
		/// <remarks>
		/// <para>
		/// The Filters are organized in a linked list.
		/// </para>
		/// <para>
		/// Setting this property causes the new filter to be pushed onto the 
		/// back of the filter chain.
		/// </para>
		/// </remarks>
		virtual public void AddFilter(IFilter filter)
		{
			if (filter == null)
			{
				throw new ArgumentNullException("filter param must not be null");
			}

			if (m_headFilter == null) 
			{
				m_headFilter = m_tailFilter = filter;
			} 
			else 
			{
				m_tailFilter.Next = filter;
				m_tailFilter = filter;	
			}
		}

		/// <summary>
		/// Clears the filter list for this appender.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clears the filter list for this appender.
		/// </para>
		/// </remarks>
		virtual public void ClearFilters()
		{
			m_headFilter = m_tailFilter = null;
		}

		#endregion Public Instance Methods

		#region Protected Instance Methods

		/// <summary>
		/// Checks if the message level is below this appender's threshold.
		/// </summary>
		/// <param name="level"><see cref="Level"/> to test against.</param>
		/// <remarks>
		/// <para>
		/// If there is no threshold set, then the return value is always <c>true</c>.
		/// </para>
		/// </remarks>
		/// <returns>
		/// <c>true</c> if the <paramref name="level"/> meets the <see cref="Threshold"/> 
		/// requirements of this appender.
		/// </returns>
		virtual protected bool IsAsSevereAsThreshold(Level level) 
		{
			return ((m_threshold == null) || level >= m_threshold);
		}

		/// <summary>
		/// Is called when the appender is closed. Derived classes should override 
		/// this method if resources need to be released.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Releases any resources allocated within the appender such as file handles, 
		/// network connections, etc.
		/// </para>
		/// <para>
		/// It is a programming error to append to a closed appender.
		/// </para>
		/// </remarks>
		virtual protected void OnClose() 
		{
			// Do nothing by default
		}

		/// <summary>
		/// Subclasses of <see cref="AppenderSkeleton"/> should implement this method 
		/// to perform actual logging.
		/// </summary>
		/// <param name="loggingEvent">The event to append.</param>
		/// <remarks>
		/// <para>
		/// A subclass must implement this method to perform
		/// logging of the <paramref name="loggingEvent"/>.
		/// </para>
		/// <para>This method will be called by <see cref="M:DoAppend(LoggingEvent)"/>
		/// if all the conditions listed for that method are met.
		/// </para>
		/// <para>
		/// To restrict the logging of events in the appender
		/// override the <see cref="M:PreAppendCheck()"/> method.
		/// </para>
		/// </remarks>
		abstract protected void Append(LoggingEvent loggingEvent);

		/// <summary>
		/// Append a bulk array of logging events.
		/// </summary>
		/// <param name="loggingEvents">the array of logging events</param>
		/// <remarks>
		/// <para>
		/// This base class implementation calls the <see cref="M:Append(LoggingEvent)"/>
		/// method for each element in the bulk array.
		/// </para>
		/// <para>
		/// A sub class that can better process a bulk array of events should
		/// override this method in addition to <see cref="M:Append(LoggingEvent)"/>.
		/// </para>
		/// </remarks>
		virtual protected void Append(LoggingEvent[] loggingEvents)
		{
			foreach(LoggingEvent loggingEvent in loggingEvents)
			{
				Append(loggingEvent);
			}
		}

		/// <summary>
		/// Called before <see cref="M:Append(LoggingEvent)"/> as a precondition.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This method is called by <see cref="M:DoAppend(LoggingEvent)"/>
		/// before the call to the abstract <see cref="M:Append(LoggingEvent)"/> method.
		/// </para>
		/// <para>
		/// This method can be overridden in a subclass to extend the checks 
		/// made before the event is passed to the <see cref="M:Append(LoggingEvent)"/> method.
		/// </para>
		/// <para>
		/// A subclass should ensure that they delegate this call to
		/// this base class if it is overridden.
		/// </para>
		/// </remarks>
		/// <returns><c>true</c> if the call to <see cref="M:Append(LoggingEvent)"/> should proceed.</returns>
		virtual protected bool PreAppendCheck()
		{
			if ((m_layout == null) && RequiresLayout)
			{
				ErrorHandler.Error("AppenderSkeleton: No layout set for the appender named ["+m_name+"].");
				return false;
			}

			return true;
		}

		/// <summary>
		/// Renders the <see cref="LoggingEvent"/> to a string.
		/// </summary>
		/// <param name="loggingEvent">The event to render.</param>
		/// <returns>The event rendered as a string.</returns>
		/// <remarks>
		/// <para>
		/// Helper method to render a <see cref="LoggingEvent"/> to 
		/// a string. This appender must have a <see cref="Layout"/>
		/// set to render the <paramref name="loggingEvent"/> to 
		/// a string.
		/// </para>
		/// <para>If there is exception data in the logging event and 
		/// the layout does not process the exception, this method 
		/// will append the exception text to the rendered string.
		/// </para>
		/// <para>
		/// Where possible use the alternative version of this method
		/// <see cref="M:RenderLoggingEvent(TextWriter,LoggingEvent)"/>.
		/// That method streams the rendering onto an existing Writer
		/// which can give better performance if the caller already has
		/// a <see cref="TextWriter"/> open and ready for writing.
		/// </para>
		/// </remarks>
		protected string RenderLoggingEvent(LoggingEvent loggingEvent)
		{
			// Create the render writer on first use
			if (m_renderWriter == null)
			{
				m_renderWriter = new ReusableStringWriter(System.Globalization.CultureInfo.InvariantCulture);
			}

            lock (m_renderWriter)
            {
                // Reset the writer so we can reuse it
                m_renderWriter.Reset(c_renderBufferMaxCapacity, c_renderBufferSize);

                RenderLoggingEvent(m_renderWriter, loggingEvent);
                return m_renderWriter.ToString();
            }
		}

		/// <summary>
		/// Renders the <see cref="LoggingEvent"/> to a string.
		/// </summary>
		/// <param name="loggingEvent">The event to render.</param>
		/// <param name="writer">The TextWriter to write the formatted event to</param>
		/// <remarks>
		/// <para>
		/// Helper method to render a <see cref="LoggingEvent"/> to 
		/// a string. This appender must have a <see cref="Layout"/>
		/// set to render the <paramref name="loggingEvent"/> to 
		/// a string.
		/// </para>
		/// <para>If there is exception data in the logging event and 
		/// the layout does not process the exception, this method 
		/// will append the exception text to the rendered string.
		/// </para>
		/// <para>
		/// Use this method in preference to <see cref="M:RenderLoggingEvent(LoggingEvent)"/>
		/// where possible. If, however, the caller needs to render the event
		/// to a string then <see cref="M:RenderLoggingEvent(LoggingEvent)"/> does
		/// provide an efficient mechanism for doing so.
		/// </para>
		/// </remarks>
		protected void RenderLoggingEvent(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (m_layout == null) 
			{
				throw new InvalidOperationException("A layout must be set");
			}

			if (m_layout.IgnoresException) 
			{
				string exceptionStr = loggingEvent.GetExceptionString();
				if (exceptionStr != null && exceptionStr.Length > 0) 
				{
					// render the event and the exception
					m_layout.Format(writer, loggingEvent);
					writer.WriteLine(exceptionStr);
				}
				else 
				{
					// there is no exception to render
					m_layout.Format(writer, loggingEvent);
				}
			}
			else 
			{
				// The layout will render the exception
				m_layout.Format(writer, loggingEvent);
			}
		}

		/// <summary>
		/// Tests if this appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <remarks>
		/// <para>
		/// In the rather exceptional case, where the appender 
		/// implementation admits a layout but can also work without it, 
		/// then the appender should return <c>true</c>.
		/// </para>
		/// <para>
		/// This default implementation always returns <c>false</c>.
		/// </para>
		/// </remarks>
		/// <returns>
		/// <c>true</c> if the appender requires a layout object, otherwise <c>false</c>.
		/// </returns>
		virtual protected bool RequiresLayout
		{
			get { return false; }
		}

		#endregion

		/// <summary>
        	/// Flushes any buffered log data.
        	/// </summary>
		/// <remarks>
		/// This implementation doesn't flush anything and always returns true
		/// </remarks>
        	/// <returns><c>True</c> if all logging events were flushed successfully, else <c>false</c>.</returns>
        	public virtual bool Flush(int millisecondsTimeout)
        	{
		    return true;
        	}

		#region Private Instance Fields

		/// <summary>
		/// The layout of this appender.
		/// </summary>
		/// <remarks>
		/// See <see cref="Layout"/> for more information.
		/// </remarks>
		private ILayout m_layout;

		/// <summary>
		/// The name of this appender.
		/// </summary>
		/// <remarks>
		/// See <see cref="Name"/> for more information.
		/// </remarks>
		private string m_name;

		/// <summary>
		/// The level threshold of this appender.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There is no level threshold filtering by default.
		/// </para>
		/// <para>
		/// See <see cref="Threshold"/> for more information.
		/// </para>
		/// </remarks>
		private Level m_threshold;

		/// <summary>
		/// It is assumed and enforced that errorHandler is never null.
		/// </summary>
		/// <remarks>
		/// <para>
		/// It is assumed and enforced that errorHandler is never null.
		/// </para>
		/// <para>
		/// See <see cref="ErrorHandler"/> for more information.
		/// </para>
		/// </remarks>
		private IErrorHandler m_errorHandler;

		/// <summary>
		/// The first filter in the filter chain.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Set to <c>null</c> initially.
		/// </para>
		/// <para>
		/// See <see cref="IFilter"/> for more information.
		/// </para>
		/// </remarks>
		private IFilter m_headFilter;

		/// <summary>
		/// The last filter in the filter chain.
		/// </summary>
		/// <remarks>
		/// See <see cref="IFilter"/> for more information.
		/// </remarks>
		private IFilter m_tailFilter;

		/// <summary>
		/// Flag indicating if this appender is closed.
		/// </summary>
		/// <remarks>
		/// See <see cref="Close"/> for more information.
		/// </remarks>
		private bool m_closed = false;

		/// <summary>
		/// The guard prevents an appender from repeatedly calling its own DoAppend method
		/// </summary>
		private bool m_recursiveGuard = false;

		/// <summary>
		/// StringWriter used to render events
		/// </summary>
		private ReusableStringWriter m_renderWriter = null;

		#endregion Private Instance Fields

		#region Constants

		/// <summary>
		/// Initial buffer size
		/// </summary>
		private const int c_renderBufferSize = 256;

		/// <summary>
		/// Maximum buffer size before it is recycled
		/// </summary>
		private const int c_renderBufferMaxCapacity = 1024;

		#endregion

	    #region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the AppenderSkeleton class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(AppenderSkeleton);

	    #endregion Private Static Fields
	}
}
