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
	/// A fixed size rolling buffer of logging events.
	/// </summary>
	/// <remarks>
	/// <para>
	/// An array backed fixed size leaky bucket.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class CyclicBuffer
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="maxSize">The maximum number of logging events in the buffer.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="CyclicBuffer" /> class with 
		/// the specified maximum number of buffered logging events.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">The <paramref name="maxSize"/> argument is not a positive integer.</exception>
		public CyclicBuffer(int maxSize) 
		{
			if (maxSize < 1) 
			{
				throw SystemInfo.CreateArgumentOutOfRangeException("maxSize", (object)maxSize, "Parameter: maxSize, Value: [" + maxSize + "] out of range. Non zero positive integer required");
			}

			m_maxSize = maxSize;
			m_events = new LoggingEvent[maxSize];
			m_first = 0;
			m_last = 0;
			m_numElems = 0;
		}

		#endregion Public Instance Constructors

		#region Public Instance Methods
	
		/// <summary>
		/// Appends a <paramref name="loggingEvent"/> to the buffer.
		/// </summary>
		/// <param name="loggingEvent">The event to append to the buffer.</param>
		/// <returns>The event discarded from the buffer, if the buffer is full, otherwise <c>null</c>.</returns>
		/// <remarks>
		/// <para>
		/// Append an event to the buffer. If the buffer still contains free space then
		/// <c>null</c> is returned. If the buffer is full then an event will be dropped
		/// to make space for the new event, the event dropped is returned.
		/// </para>
		/// </remarks>
		public LoggingEvent Append(LoggingEvent loggingEvent)
		{	
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			lock(this)
			{
				// save the discarded event
				LoggingEvent discardedLoggingEvent = m_events[m_last];

				// overwrite the last event position
				m_events[m_last] = loggingEvent;	
				if (++m_last == m_maxSize)
				{
					m_last = 0;
				}

				if (m_numElems < m_maxSize)
				{
					m_numElems++;
				}
				else if (++m_first == m_maxSize)
				{
					m_first = 0;
				}

				if (m_numElems < m_maxSize)
				{
					// Space remaining
					return null;
				}
				else
				{
					// Buffer is full and discarding an event
					return discardedLoggingEvent;
				}
			}
		}

		/// <summary>
		/// Get and remove the oldest event in the buffer.
		/// </summary>
		/// <returns>The oldest logging event in the buffer</returns>
		/// <remarks>
		/// <para>
		/// Gets the oldest (first) logging event in the buffer and removes it 
		/// from the buffer.
		/// </para>
		/// </remarks>
		public LoggingEvent PopOldest() 
		{
			lock(this)
			{
				LoggingEvent ret = null;
				if (m_numElems > 0) 
				{
					m_numElems--;
					ret = m_events[m_first];
					m_events[m_first] = null;
					if (++m_first == m_maxSize)
					{
						m_first = 0;
					}
				} 
				return ret;
			}
		}

		/// <summary>
		/// Pops all the logging events from the buffer into an array.
		/// </summary>
		/// <returns>An array of all the logging events in the buffer.</returns>
		/// <remarks>
		/// <para>
		/// Get all the events in the buffer and clear the buffer.
		/// </para>
		/// </remarks>
		public LoggingEvent[] PopAll()
		{
			lock(this)
			{
				LoggingEvent[] ret = new LoggingEvent[m_numElems];

				if (m_numElems > 0)
				{
					if (m_first < m_last)
					{
						Array.Copy(m_events, m_first, ret, 0, m_numElems);
					}
					else
					{
						Array.Copy(m_events, m_first, ret, 0, m_maxSize - m_first);
						Array.Copy(m_events, 0, ret, m_maxSize - m_first, m_last);
					}
				}

				Clear();

				return ret;
			}
		}

		/// <summary>
		/// Clear the buffer
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clear the buffer of all events. The events in the buffer are lost.
		/// </para>
		/// </remarks>
		public void Clear()
		{
			lock(this)
			{
				// Set all the elements to null
				Array.Clear(m_events, 0, m_events.Length);

				m_first = 0;
				m_last = 0;
				m_numElems = 0;
			}
		}

#if RESIZABLE_CYCLIC_BUFFER
		/// <summary>
		/// Resizes the cyclic buffer to <paramref name="newSize"/>.
		/// </summary>
		/// <param name="newSize">The new size of the buffer.</param>
		/// <remarks>
		/// <para>
		/// Resize the cyclic buffer. Events in the buffer are copied into
		/// the newly sized buffer. If the buffer is shrunk and there are
		/// more events currently in the buffer than the new size of the
		/// buffer then the newest events will be dropped from the buffer.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">The <paramref name="newSize"/> argument is not a positive integer.</exception>
		public void Resize(int newSize) 
		{
			lock(this)
			{
				if (newSize < 0) 
				{
					throw log4net.Util.SystemInfo.CreateArgumentOutOfRangeException("newSize", (object)newSize, "Parameter: newSize, Value: [" + newSize + "] out of range. Non zero positive integer required");
				}
				if (newSize == m_numElems)
				{
					return; // nothing to do
				}
	
				LoggingEvent[] temp = new  LoggingEvent[newSize];

				int loopLen = (newSize < m_numElems) ? newSize : m_numElems;
	
				for(int i = 0; i < loopLen; i++) 
				{
					temp[i] = m_events[m_first];
					m_events[m_first] = null;

					if (++m_first == m_numElems) 
					{
						m_first = 0;
					}
				}

				m_events = temp;
				m_first = 0;
				m_numElems = loopLen;
				m_maxSize = newSize;

				if (loopLen == newSize) 
				{
					m_last = 0;
				} 
				else 
				{
					m_last = loopLen;
				}
			}
		}
#endif

		#endregion Public Instance Methods

		#region Public Instance Properties

		/// <summary>
		/// Gets the <paramref name="i"/>th oldest event currently in the buffer.
		/// </summary>
		/// <value>The <paramref name="i"/>th oldest event currently in the buffer.</value>
		/// <remarks>
		/// <para>
		/// If <paramref name="i"/> is outside the range 0 to the number of events
		/// currently in the buffer, then <c>null</c> is returned.
		/// </para>
		/// </remarks>
		public LoggingEvent this[int i] 
		{
			get
			{
				lock(this)
				{
					if (i < 0 || i >= m_numElems)
					{
						return null;
					}

					return m_events[(m_first + i) % m_maxSize];
				}
			}
		}

		/// <summary>
		/// Gets the maximum size of the buffer.
		/// </summary>
		/// <value>The maximum size of the buffer.</value>
		/// <remarks>
		/// <para>
		/// Gets the maximum size of the buffer
		/// </para>
		/// </remarks>
		public int MaxSize 
		{
			get 
			{ 
				lock(this)
				{
					return m_maxSize; 
				}
			}
#if RESIZABLE_CYCLIC_BUFFER
			set 
			{ 
				/// Setting the MaxSize will cause the buffer to resize.
				Resize(value); 
			}
#endif
		}

		/// <summary>
		/// Gets the number of logging events in the buffer.
		/// </summary>
		/// <value>The number of logging events in the buffer.</value>
		/// <remarks>
		/// <para>
		/// This number is guaranteed to be in the range 0 to <see cref="MaxSize"/>
		/// (inclusive).
		/// </para>
		/// </remarks>
		public int Length
		{
			get 
			{ 
				lock(this) 
				{ 
					return m_numElems; 
				}
			}									
		}

		#endregion Public Instance Properties

		#region Private Instance Fields

		private LoggingEvent[] m_events;
		private int m_first; 
		private int m_last; 
		private int m_numElems;
		private int m_maxSize;

		#endregion Private Instance Fields
	}
}
