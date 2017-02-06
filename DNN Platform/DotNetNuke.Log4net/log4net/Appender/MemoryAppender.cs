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

using log4net.Core;

namespace log4net.Appender
{
	/// <summary>
	/// Stores logging events in an array.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The memory appender stores all the logging events
	/// that are appended in an in-memory array.
	/// </para>
	/// <para>
	/// Use the <see cref="M:PopAllEvents()"/> method to get
	/// and clear the current list of events that have been appended.
	/// </para>
	/// <para>
	/// Use the <see cref="M:GetEvents()"/> method to get the current
	/// list of events that have been appended.  Note there is a
	/// race-condition when calling <see cref="M:GetEvents()"/> and
	/// <see cref="M:Clear()"/> in pairs, you better use <see
	/// mref="M:PopAllEvents()"/> in that case.
	/// </para>
	/// <para>
	/// Use the <see cref="M:Clear()"/> method to clear the
	/// current list of events.  Note there is a
	/// race-condition when calling <see cref="M:GetEvents()"/> and
	/// <see cref="M:Clear()"/> in pairs, you better use <see
	/// mref="M:PopAllEvents()"/> in that case.
	/// </para>
	/// </remarks>
	/// <author>Julian Biddle</author>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class MemoryAppender : AppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="MemoryAppender" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor.
		/// </para>
		/// </remarks>
		public MemoryAppender() : base()
		{
			m_eventsList = new ArrayList();
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets the events that have been logged.
		/// </summary>
		/// <returns>The events that have been logged</returns>
		/// <remarks>
		/// <para>
		/// Gets the events that have been logged.
		/// </para>
		/// </remarks>
		virtual public LoggingEvent[] GetEvents()
		{
            lock (m_eventsList.SyncRoot)
            {
                return (LoggingEvent[]) m_eventsList.ToArray(typeof(LoggingEvent));
            }
		}

		/// <summary>
		/// Gets or sets a value indicating whether only part of the logging event 
		/// data should be fixed.
		/// </summary>
		/// <value>
		/// <c>true</c> if the appender should only fix part of the logging event 
		/// data, otherwise <c>false</c>. The default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Setting this property to <c>true</c> will cause only part of the event 
		/// data to be fixed and stored in the appender, hereby improving performance. 
		/// </para>
		/// <para>
		/// See <see cref="M:LoggingEvent.FixVolatileData(bool)"/> for more information.
		/// </para>
		/// </remarks>
		[Obsolete("Use Fix property")]
		virtual public bool OnlyFixPartialEventData
		{
			get { return (Fix == FixFlags.Partial); }
			set 
			{ 
				if (value)
				{
					Fix = FixFlags.Partial;
				}
				else
				{
					Fix = FixFlags.All;
				}
			}
		}

		/// <summary>
		/// Gets or sets the fields that will be fixed in the event
		/// </summary>
		/// <remarks>
		/// <para>
		/// The logging event needs to have certain thread specific values 
		/// captured before it can be buffered. See <see cref="LoggingEvent.Fix"/>
		/// for details.
		/// </para>
		/// </remarks>
		virtual public FixFlags Fix
		{
			get { return m_fixFlags; }
			set { m_fixFlags = value; }
		}

		#endregion Public Instance Properties

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)"/> method. 
		/// </summary>
		/// <param name="loggingEvent">the event to log</param>
		/// <remarks>
		/// <para>Stores the <paramref name="loggingEvent"/> in the events list.</para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			// Because we are caching the LoggingEvent beyond the
			// lifetime of the Append() method we must fix any
			// volatile data in the event.
			loggingEvent.Fix = this.Fix;

            lock (m_eventsList.SyncRoot)
            {
                m_eventsList.Add(loggingEvent);
            }
		} 

		#endregion Override implementation of AppenderSkeleton

		#region Public Instance Methods

		/// <summary>
		/// Clear the list of events
		/// </summary>
		/// <remarks>
		/// Clear the list of events
		/// </remarks>
		virtual public void Clear()
		{
            lock (m_eventsList.SyncRoot)
            {
                m_eventsList.Clear();
            }
		}

        /// <summary>
        /// Gets the events that have been logged and clears the list of events.
        /// </summary>
        /// <returns>The events that have been logged</returns>
        /// <remarks>
        /// <para>
        /// Gets the events that have been logged and clears the list of events.
        /// </para>
        /// </remarks>
        virtual public LoggingEvent[] PopAllEvents()
        {
            lock (m_eventsList.SyncRoot)
            {
                LoggingEvent[] tmp = (LoggingEvent[]) m_eventsList.ToArray(typeof (LoggingEvent));
                m_eventsList.Clear();
                return tmp;
            }
        }

		#endregion Public Instance Methods

		#region Protected Instance Fields

		/// <summary>
		/// The list of events that have been appended.
		/// </summary>
		protected ArrayList m_eventsList;

		/// <summary>
		/// Value indicating which fields in the event should be fixed
		/// </summary>
		/// <remarks>
		/// By default all fields are fixed
		/// </remarks>
		protected FixFlags m_fixFlags = FixFlags.All;

		#endregion Protected Instance Fields
	}
}
