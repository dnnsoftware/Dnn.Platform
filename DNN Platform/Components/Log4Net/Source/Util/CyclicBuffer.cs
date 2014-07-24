using log4net.Core;
using System;

namespace log4net.Util
{
	public class CyclicBuffer
	{
		private LoggingEvent[] m_events;

		private int m_first;

		private int m_last;

		private int m_numElems;

		private int m_maxSize;

		public LoggingEvent this[int i]
		{
			get
			{
				LoggingEvent mEvents;
				lock (this)
				{
					if (i < 0 || i >= this.m_numElems)
					{
						mEvents = null;
					}
					else
					{
						mEvents = this.m_events[(this.m_first + i) % this.m_maxSize];
					}
				}
				return mEvents;
			}
		}

		public int Length
		{
			get
			{
				int mNumElems;
				lock (this)
				{
					mNumElems = this.m_numElems;
				}
				return mNumElems;
			}
		}

		public int MaxSize
		{
			get
			{
				int mMaxSize;
				lock (this)
				{
					mMaxSize = this.m_maxSize;
				}
				return mMaxSize;
			}
		}

		public CyclicBuffer(int maxSize)
		{
			if (maxSize < 1)
			{
				throw SystemInfo.CreateArgumentOutOfRangeException("maxSize", maxSize, string.Concat("Parameter: maxSize, Value: [", maxSize, "] out of range. Non zero positive integer required"));
			}
			this.m_maxSize = maxSize;
			this.m_events = new LoggingEvent[maxSize];
			this.m_first = 0;
			this.m_last = 0;
			this.m_numElems = 0;
		}

		public LoggingEvent Append(LoggingEvent loggingEvent)
		{
			LoggingEvent loggingEvent1;
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			lock (this)
			{
				LoggingEvent mEvents = this.m_events[this.m_last];
				this.m_events[this.m_last] = loggingEvent;
				CyclicBuffer cyclicBuffer = this;
				int mLast = cyclicBuffer.m_last + 1;
				int num = mLast;
				cyclicBuffer.m_last = mLast;
				if (num == this.m_maxSize)
				{
					this.m_last = 0;
				}
				if (this.m_numElems >= this.m_maxSize)
				{
					CyclicBuffer cyclicBuffer1 = this;
					int mFirst = cyclicBuffer1.m_first + 1;
					int num1 = mFirst;
					cyclicBuffer1.m_first = mFirst;
					if (num1 == this.m_maxSize)
					{
						this.m_first = 0;
					}
				}
				else
				{
					CyclicBuffer mNumElems = this;
					mNumElems.m_numElems = mNumElems.m_numElems + 1;
				}
				if (this.m_numElems >= this.m_maxSize)
				{
					loggingEvent1 = mEvents;
				}
				else
				{
					loggingEvent1 = null;
				}
			}
			return loggingEvent1;
		}

		public void Clear()
		{
			lock (this)
			{
				Array.Clear(this.m_events, 0, (int)this.m_events.Length);
				this.m_first = 0;
				this.m_last = 0;
				this.m_numElems = 0;
			}
		}

		public LoggingEvent[] PopAll()
		{
			LoggingEvent[] loggingEventArray;
			lock (this)
			{
				LoggingEvent[] loggingEventArray1 = new LoggingEvent[this.m_numElems];
				if (this.m_numElems > 0)
				{
					if (this.m_first >= this.m_last)
					{
						Array.Copy(this.m_events, this.m_first, loggingEventArray1, 0, this.m_maxSize - this.m_first);
						Array.Copy(this.m_events, 0, loggingEventArray1, this.m_maxSize - this.m_first, this.m_last);
					}
					else
					{
						Array.Copy(this.m_events, this.m_first, loggingEventArray1, 0, this.m_numElems);
					}
				}
				this.Clear();
				loggingEventArray = loggingEventArray1;
			}
			return loggingEventArray;
		}

		public LoggingEvent PopOldest()
		{
			LoggingEvent loggingEvent;
			lock (this)
			{
				LoggingEvent mEvents = null;
				if (this.m_numElems > 0)
				{
					CyclicBuffer mNumElems = this;
					mNumElems.m_numElems = mNumElems.m_numElems - 1;
					mEvents = this.m_events[this.m_first];
					this.m_events[this.m_first] = null;
					CyclicBuffer cyclicBuffer = this;
					int mFirst = cyclicBuffer.m_first + 1;
					int num = mFirst;
					cyclicBuffer.m_first = mFirst;
					if (num == this.m_maxSize)
					{
						this.m_first = 0;
					}
				}
				loggingEvent = mEvents;
			}
			return loggingEvent;
		}
	}
}