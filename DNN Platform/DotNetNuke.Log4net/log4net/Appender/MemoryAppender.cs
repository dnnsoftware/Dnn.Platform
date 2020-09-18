// Licensed to the .NET Foundation under one or more agreements.
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
    using System.Collections;

    using log4net.Core;

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
    /// <author>Julian Biddle.</author>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public class MemoryAppender : AppenderSkeleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryAppender" /> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default constructor.
        /// </para>
        /// </remarks>
        public MemoryAppender()
            : base()
        {
            this.m_eventsList = new ArrayList();
        }

        /// <summary>
        /// Gets the events that have been logged.
        /// </summary>
        /// <returns>The events that have been logged.</returns>
        /// <remarks>
        /// <para>
        /// Gets the events that have been logged.
        /// </para>
        /// </remarks>
        public virtual LoggingEvent[] GetEvents()
        {
            lock (this.m_eventsList.SyncRoot)
            {
                return (LoggingEvent[])this.m_eventsList.ToArray(typeof(LoggingEvent));
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
        [Obsolete("Use Fix property. Scheduled removal in v10.0.0.")]
        public virtual bool OnlyFixPartialEventData
        {
            get { return this.Fix == FixFlags.Partial; }

            set
            {
                if (value)
                {
                    this.Fix = FixFlags.Partial;
                }
                else
                {
                    this.Fix = FixFlags.All;
                }
            }
        }

        /// <summary>
        /// Gets or sets the fields that will be fixed in the event.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The logging event needs to have certain thread specific values
        /// captured before it can be buffered. See <see cref="LoggingEvent.Fix"/>
        /// for details.
        /// </para>
        /// </remarks>
        public virtual FixFlags Fix
        {
            get { return this.m_fixFlags; }
            set { this.m_fixFlags = value; }
        }

        /// <summary>
        /// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)"/> method.
        /// </summary>
        /// <param name="loggingEvent">the event to log.</param>
        /// <remarks>
        /// <para>Stores the <paramref name="loggingEvent"/> in the events list.</para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
            // Because we are caching the LoggingEvent beyond the
            // lifetime of the Append() method we must fix any
            // volatile data in the event.
            loggingEvent.Fix = this.Fix;

            lock (this.m_eventsList.SyncRoot)
            {
                this.m_eventsList.Add(loggingEvent);
            }
        }

        /// <summary>
        /// Clear the list of events.
        /// </summary>
        /// <remarks>
        /// Clear the list of events.
        /// </remarks>
        public virtual void Clear()
        {
            lock (this.m_eventsList.SyncRoot)
            {
                this.m_eventsList.Clear();
            }
        }

        /// <summary>
        /// Gets the events that have been logged and clears the list of events.
        /// </summary>
        /// <returns>The events that have been logged.</returns>
        /// <remarks>
        /// <para>
        /// Gets the events that have been logged and clears the list of events.
        /// </para>
        /// </remarks>
        public virtual LoggingEvent[] PopAllEvents()
        {
            lock (this.m_eventsList.SyncRoot)
            {
                LoggingEvent[] tmp = (LoggingEvent[])this.m_eventsList.ToArray(typeof(LoggingEvent));
                this.m_eventsList.Clear();
                return tmp;
            }
        }

        /// <summary>
        /// The list of events that have been appended.
        /// </summary>
        protected ArrayList m_eventsList;

        /// <summary>
        /// Value indicating which fields in the event should be fixed.
        /// </summary>
        /// <remarks>
        /// By default all fields are fixed.
        /// </remarks>
        protected FixFlags m_fixFlags = FixFlags.All;
    }
}
