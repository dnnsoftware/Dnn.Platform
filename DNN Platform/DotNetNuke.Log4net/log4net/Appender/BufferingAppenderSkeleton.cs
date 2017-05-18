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

using log4net.Util;
using log4net.Core;

namespace log4net.Appender
{
	/// <summary>
	/// Abstract base class implementation of <see cref="IAppender"/> that 
	/// buffers events in a fixed size buffer.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This base class should be used by appenders that need to buffer a 
	/// number of events before logging them. For example the <see cref="AdoNetAppender"/> 
	/// buffers events and then submits the entire contents of the buffer to 
	/// the underlying database in one go.
	/// </para>
	/// <para>
	/// Subclasses should override the <see cref="M:SendBuffer(LoggingEvent[])"/>
	/// method to deliver the buffered events.
	/// </para>
	/// <para>The BufferingAppenderSkeleton maintains a fixed size cyclic 
	/// buffer of events. The size of the buffer is set using 
	/// the <see cref="BufferSize"/> property.
	/// </para>
	/// <para>A <see cref="ITriggeringEventEvaluator"/> is used to inspect 
	/// each event as it arrives in the appender. If the <see cref="Evaluator"/> 
	/// triggers, then the current buffer is sent immediately 
	/// (see <see cref="M:SendBuffer(LoggingEvent[])"/>). Otherwise the event 
	/// is stored in the buffer. For example, an evaluator can be used to 
	/// deliver the events immediately when an ERROR event arrives.
	/// </para>
	/// <para>
	/// The buffering appender can be configured in a <see cref="Lossy"/> mode. 
	/// By default the appender is NOT lossy. When the buffer is full all 
	/// the buffered events are sent with <see cref="M:SendBuffer(LoggingEvent[])"/>.
	/// If the <see cref="Lossy"/> property is set to <c>true</c> then the 
	/// buffer will not be sent when it is full, and new events arriving 
	/// in the appender will overwrite the oldest event in the buffer. 
	/// In lossy mode the buffer will only be sent when the <see cref="Evaluator"/>
	/// triggers. This can be useful behavior when you need to know about 
	/// ERROR events but not about events with a lower level, configure an 
	/// evaluator that will trigger when an ERROR event arrives, the whole 
	/// buffer will be sent which gives a history of events leading up to
	/// the ERROR event.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
    public abstract class BufferingAppenderSkeleton : AppenderSkeleton
	{
		#region Protected Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="BufferingAppenderSkeleton" /> class.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Protected default constructor to allow subclassing.
		/// </para>
		/// </remarks>
		protected BufferingAppenderSkeleton() : this(true)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BufferingAppenderSkeleton" /> class.
		/// </summary>
		/// <param name="eventMustBeFixed">the events passed through this appender must be
		/// fixed by the time that they arrive in the derived class' <c>SendBuffer</c> method.</param>
		/// <remarks>
		/// <para>
		/// Protected constructor to allow subclassing.
		/// </para>
		/// <para>
		/// The <paramref name="eventMustBeFixed"/> should be set if the subclass
		/// expects the events delivered to be fixed even if the 
		/// <see cref="BufferSize"/> is set to zero, i.e. when no buffering occurs.
		/// </para>
		/// </remarks>
		protected BufferingAppenderSkeleton(bool eventMustBeFixed) : base()
		{
			m_eventMustBeFixed = eventMustBeFixed;
		}

		#endregion Protected Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets a value that indicates whether the appender is lossy.
		/// </summary>
		/// <value>
		/// <c>true</c> if the appender is lossy, otherwise <c>false</c>. The default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// This appender uses a buffer to store logging events before 
		/// delivering them. A triggering event causes the whole buffer
		/// to be send to the remote sink. If the buffer overruns before
		/// a triggering event then logging events could be lost. Set
		/// <see cref="Lossy"/> to <c>false</c> to prevent logging events 
		/// from being lost.
		/// </para>
		/// <para>If <see cref="Lossy"/> is set to <c>true</c> then an
		/// <see cref="Evaluator"/> must be specified.</para>
		/// </remarks>
		public bool Lossy
		{
			get { return m_lossy; }
			set { m_lossy = value; }
		}

		/// <summary>
		/// Gets or sets the size of the cyclic buffer used to hold the 
		/// logging events.
		/// </summary>
		/// <value>
		/// The size of the cyclic buffer used to hold the logging events.
		/// </value>
		/// <remarks>
		/// <para>
		/// The <see cref="BufferSize"/> option takes a positive integer
		/// representing the maximum number of logging events to collect in 
		/// a cyclic buffer. When the <see cref="BufferSize"/> is reached,
		/// oldest events are deleted as new events are added to the
		/// buffer. By default the size of the cyclic buffer is 512 events.
		/// </para>
		/// <para>
		/// If the <see cref="BufferSize"/> is set to a value less than
		/// or equal to 1 then no buffering will occur. The logging event
		/// will be delivered synchronously (depending on the <see cref="Lossy"/>
		/// and <see cref="Evaluator"/> properties). Otherwise the event will
		/// be buffered.
		/// </para>
		/// </remarks>
		public int BufferSize
		{
			get { return m_bufferSize; }
			set { m_bufferSize = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="ITriggeringEventEvaluator"/> that causes the 
		/// buffer to be sent immediately.
		/// </summary>
		/// <value>
		/// The <see cref="ITriggeringEventEvaluator"/> that causes the buffer to be
		/// sent immediately.
		/// </value>
		/// <remarks>
		/// <para>
		/// The evaluator will be called for each event that is appended to this 
		/// appender. If the evaluator triggers then the current buffer will 
		/// immediately be sent (see <see cref="M:SendBuffer(LoggingEvent[])"/>).
		/// </para>
		/// <para>If <see cref="Lossy"/> is set to <c>true</c> then an
		/// <see cref="Evaluator"/> must be specified.</para>
		/// </remarks>
		public ITriggeringEventEvaluator Evaluator
		{
			get { return m_evaluator; }
			set	{ m_evaluator = value; }
		}

		/// <summary>
		/// Gets or sets the value of the <see cref="ITriggeringEventEvaluator"/> to use.
		/// </summary>
		/// <value>
		/// The value of the <see cref="ITriggeringEventEvaluator"/> to use.
		/// </value>
		/// <remarks>
		/// <para>
		/// The evaluator will be called for each event that is discarded from this 
		/// appender. If the evaluator triggers then the current buffer will immediately 
		/// be sent (see <see cref="M:SendBuffer(LoggingEvent[])"/>).
		/// </para>
		/// </remarks>
		public ITriggeringEventEvaluator LossyEvaluator
		{
			get { return m_lossyEvaluator; }
			set	{ m_lossyEvaluator = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating if only part of the logging event data
		/// should be fixed.
		/// </summary>
		/// <value>
		/// <c>true</c> if the appender should only fix part of the logging event 
		/// data, otherwise <c>false</c>. The default is <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Setting this property to <c>true</c> will cause only part of the
		/// event data to be fixed and serialized. This will improve performance.
		/// </para>
		/// <para>
		/// See <see cref="M:LoggingEvent.FixVolatileData(FixFlags)"/> for more information.
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
		/// Gets or sets a the fields that will be fixed in the event
		/// </summary>
		/// <value>
		/// The event fields that will be fixed before the event is buffered
		/// </value>
		/// <remarks>
		/// <para>
		/// The logging event needs to have certain thread specific values 
		/// captured before it can be buffered. See <see cref="LoggingEvent.Fix"/>
		/// for details.
		/// </para>
		/// </remarks>
		/// <seealso cref="LoggingEvent.Fix"/>
		virtual public FixFlags Fix
		{
			get { return m_fixFlags; }
			set { m_fixFlags = value; }
		}

		#endregion Public Instance Properties

		#region Public Methods

        /// <summary>
        /// Flushes any buffered log data.
        /// </summary>
        /// <param name="millisecondsTimeout">The maximum time to wait for logging events to be flushed.</param>
        /// <returns><c>True</c> if all logging events were flushed successfully, else <c>false</c>.</returns>
        public override bool Flush(int millisecondsTimeout)
        {
            Flush();
            return true;
        }

		/// <summary>
		/// Flush the currently buffered events
		/// </summary>
		/// <remarks>
		/// <para>
		/// Flushes any events that have been buffered.
		/// </para>
		/// <para>
		/// If the appender is buffering in <see cref="Lossy"/> mode then the contents
		/// of the buffer will NOT be flushed to the appender.
		/// </para>
		/// </remarks>
		public virtual void Flush()
		{
			Flush(false);
		}

		/// <summary>
		/// Flush the currently buffered events
		/// </summary>
		/// <param name="flushLossyBuffer">set to <c>true</c> to flush the buffer of lossy events</param>
		/// <remarks>
		/// <para>
		/// Flushes events that have been buffered. If <paramref name="flushLossyBuffer" /> is
		/// <c>false</c> then events will only be flushed if this buffer is non-lossy mode.
		/// </para>
		/// <para>
		/// If the appender is buffering in <see cref="Lossy"/> mode then the contents
		/// of the buffer will only be flushed if <paramref name="flushLossyBuffer" /> is <c>true</c>.
		/// In this case the contents of the buffer will be tested against the 
		/// <see cref="LossyEvaluator"/> and if triggering will be output. All other buffered
		/// events will be discarded.
		/// </para>
		/// <para>
		/// If <paramref name="flushLossyBuffer" /> is <c>true</c> then the buffer will always
		/// be emptied by calling this method.
		/// </para>
		/// </remarks>
		public virtual void Flush(bool flushLossyBuffer)
		{
			// This method will be called outside of the AppenderSkeleton DoAppend() method
			// therefore it needs to be protected by its own lock. This will block any
			// Appends while the buffer is flushed.
			lock(this)
			{
				if (m_cb != null && m_cb.Length > 0)
				{
					if (m_lossy)
					{
						// If we are allowed to eagerly flush from the lossy buffer
						if (flushLossyBuffer)
						{
							if (m_lossyEvaluator != null)
							{
								// Test the contents of the buffer against the lossy evaluator
								LoggingEvent[] bufferedEvents = m_cb.PopAll();
								ArrayList filteredEvents = new ArrayList(bufferedEvents.Length);

								foreach(LoggingEvent loggingEvent in bufferedEvents)
								{
									if (m_lossyEvaluator.IsTriggeringEvent(loggingEvent))
									{
										filteredEvents.Add(loggingEvent);
									}
								}

								// Send the events that meet the lossy evaluator criteria
								if (filteredEvents.Count > 0)
								{
									SendBuffer((LoggingEvent[])filteredEvents.ToArray(typeof(LoggingEvent)));
								}
							}
							else
							{
								// No lossy evaluator, all buffered events are discarded
								m_cb.Clear();
							}
						}
					}
					else
					{
						// Not lossy, send whole buffer
						SendFromBuffer(null, m_cb);
					}
				}
			}
		}

		#endregion Public Methods

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
		override public void ActivateOptions() 
		{
			base.ActivateOptions();

			// If the appender is in Lossy mode then we will
			// only send the buffer when the Evaluator triggers
			// therefore check we have an evaluator.
			if (m_lossy && m_evaluator == null)
			{
				ErrorHandler.Error("Appender ["+Name+"] is Lossy but has no Evaluator. The buffer will never be sent!"); 
			}

			if (m_bufferSize > 1)
			{
				m_cb = new CyclicBuffer(m_bufferSize);
			}
			else
			{
				m_cb = null;
			}
		}

		#endregion Implementation of IOptionHandler

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Close this appender instance.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Close this appender instance. If this appender is marked
		/// as not <see cref="Lossy"/> then the remaining events in 
		/// the buffer must be sent when the appender is closed.
		/// </para>
		/// </remarks>
		override protected void OnClose() 
		{
			// Flush the buffer on close
			Flush(true);
		}

		/// <summary>
		/// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)"/> method. 
		/// </summary>
		/// <param name="loggingEvent">the event to log</param>
		/// <remarks>
		/// <para>
		/// Stores the <paramref name="loggingEvent"/> in the cyclic buffer.
		/// </para>
		/// <para>
		/// The buffer will be sent (i.e. passed to the <see cref="SendBuffer"/> 
		/// method) if one of the following conditions is met:
		/// </para>
		/// <list type="bullet">
		///		<item>
		///			<description>The cyclic buffer is full and this appender is
		///			marked as not lossy (see <see cref="Lossy"/>)</description>
		///		</item>
		///		<item>
		///			<description>An <see cref="Evaluator"/> is set and
		///			it is triggered for the <paramref name="loggingEvent"/>
		///			specified.</description>
		///		</item>
		/// </list>
		/// <para>
		/// Before the event is stored in the buffer it is fixed
		/// (see <see cref="M:LoggingEvent.FixVolatileData(FixFlags)"/>) to ensure that
		/// any data referenced by the event will be valid when the buffer
		/// is processed.
		/// </para>
		/// </remarks>
		override protected void Append(LoggingEvent loggingEvent) 
		{
			// If the buffer size is set to 1 or less then the buffer will be
			// sent immediately because there is not enough space in the buffer
			// to buffer up more than 1 event. Therefore as a special case
			// we don't use the buffer at all.
			if (m_cb == null || m_bufferSize <= 1)
			{
				// Only send the event if we are in non lossy mode or the event is a triggering event
				if ((!m_lossy) || 
					(m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent)) || 
					(m_lossyEvaluator != null && m_lossyEvaluator.IsTriggeringEvent(loggingEvent)))
				{
					if (m_eventMustBeFixed)
					{
						// Derive class expects fixed events
						loggingEvent.Fix = this.Fix;
					}

					// Not buffering events, send immediately
					SendBuffer(new LoggingEvent[] { loggingEvent } );
				}
			}
			else
			{
				// Because we are caching the LoggingEvent beyond the
				// lifetime of the Append() method we must fix any
				// volatile data in the event.
				loggingEvent.Fix = this.Fix;

				// Add to the buffer, returns the event discarded from the buffer if there is no space remaining after the append
				LoggingEvent discardedLoggingEvent = m_cb.Append(loggingEvent);

				if (discardedLoggingEvent != null)
				{
					// Buffer is full and has had to discard an event
					if (!m_lossy)
					{
						// Not lossy, must send all events
						SendFromBuffer(discardedLoggingEvent, m_cb);
					}
					else
					{
						// Check if the discarded event should not be logged
						if (m_lossyEvaluator == null || !m_lossyEvaluator.IsTriggeringEvent(discardedLoggingEvent))
						{
							// Clear the discarded event as we should not forward it
							discardedLoggingEvent = null;
						}

						// Check if the event should trigger the whole buffer to be sent
						if (m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent))
						{
							SendFromBuffer(discardedLoggingEvent, m_cb);
						}
						else if (discardedLoggingEvent != null)
						{
							// Just send the discarded event
							SendBuffer(new LoggingEvent[] { discardedLoggingEvent } );
						}
					}
				}
				else
				{
					// Buffer is not yet full

					// Check if the event should trigger the whole buffer to be sent
					if (m_evaluator != null && m_evaluator.IsTriggeringEvent(loggingEvent))
					{
						SendFromBuffer(null, m_cb);
					}
				}
			}
		}

		#endregion Override implementation of AppenderSkeleton

		#region Protected Instance Methods

		/// <summary>
		/// Sends the contents of the buffer.
		/// </summary>
		/// <param name="firstLoggingEvent">The first logging event.</param>
		/// <param name="buffer">The buffer containing the events that need to be send.</param>
		/// <remarks>
		/// <para>
		/// The subclass must override <see cref="M:SendBuffer(LoggingEvent[])"/>.
		/// </para>
		/// </remarks>
		virtual protected void SendFromBuffer(LoggingEvent firstLoggingEvent, CyclicBuffer buffer)
		{
			LoggingEvent[] bufferEvents = buffer.PopAll();

			if (firstLoggingEvent == null)
			{
				SendBuffer(bufferEvents);
			}
			else if (bufferEvents.Length == 0)
			{
				SendBuffer(new LoggingEvent[] { firstLoggingEvent } );
			}
			else
			{
				// Create new array with the firstLoggingEvent at the head
				LoggingEvent[] events = new LoggingEvent[bufferEvents.Length + 1];
				Array.Copy(bufferEvents, 0, events, 1, bufferEvents.Length);
				events[0] = firstLoggingEvent;

				SendBuffer(events);
			}
		}

		#endregion Protected Instance Methods

		/// <summary>
		/// Sends the events.
		/// </summary>
		/// <param name="events">The events that need to be send.</param>
		/// <remarks>
		/// <para>
		/// The subclass must override this method to process the buffered events.
		/// </para>
		/// </remarks>
		abstract protected void SendBuffer(LoggingEvent[] events);

		#region Private Static Fields

		/// <summary>
		/// The default buffer size.
		/// </summary>
		/// <remarks>
		/// The default size of the cyclic buffer used to store events.
		/// This is set to 512 by default.
		/// </remarks>
		private const int DEFAULT_BUFFER_SIZE = 512;

		#endregion Private Static Fields

		#region Private Instance Fields

		/// <summary>
		/// The size of the cyclic buffer used to hold the logging events.
		/// </summary>
		/// <remarks>
		/// Set to <see cref="DEFAULT_BUFFER_SIZE"/> by default.
		/// </remarks>
		private int m_bufferSize = DEFAULT_BUFFER_SIZE;

		/// <summary>
		/// The cyclic buffer used to store the logging events.
		/// </summary>
		private CyclicBuffer m_cb;

		/// <summary>
		/// The triggering event evaluator that causes the buffer to be sent immediately.
		/// </summary>
		/// <remarks>
		/// The object that is used to determine if an event causes the entire
		/// buffer to be sent immediately. This field can be <c>null</c>, which 
		/// indicates that event triggering is not to be done. The evaluator
		/// can be set using the <see cref="Evaluator"/> property. If this appender
		/// has the <see cref="m_lossy"/> (<see cref="Lossy"/> property) set to 
		/// <c>true</c> then an <see cref="Evaluator"/> must be set.
		/// </remarks>
		private ITriggeringEventEvaluator m_evaluator;

		/// <summary>
		/// Indicates if the appender should overwrite events in the cyclic buffer 
		/// when it becomes full, or if the buffer should be flushed when the 
		/// buffer is full.
		/// </summary>
		/// <remarks>
		/// If this field is set to <c>true</c> then an <see cref="Evaluator"/> must 
		/// be set.
		/// </remarks>
		private bool m_lossy = false;

		/// <summary>
		/// The triggering event evaluator filters discarded events.
		/// </summary>
		/// <remarks>
		/// The object that is used to determine if an event that is discarded should
		/// really be discarded or if it should be sent to the appenders. 
		/// This field can be <c>null</c>, which indicates that all discarded events will
		/// be discarded. 
		/// </remarks>
		private ITriggeringEventEvaluator m_lossyEvaluator;

		/// <summary>
		/// Value indicating which fields in the event should be fixed
		/// </summary>
		/// <remarks>
		/// By default all fields are fixed
		/// </remarks>
		private FixFlags m_fixFlags = FixFlags.All;

		/// <summary>
		/// The events delivered to the subclass must be fixed.
		/// </summary>
		private readonly bool m_eventMustBeFixed;

		#endregion Private Instance Fields
	}
}
