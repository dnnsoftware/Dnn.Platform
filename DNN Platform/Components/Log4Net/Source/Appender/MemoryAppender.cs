using log4net.Core;
using System;
using System.Collections;

namespace log4net.Appender
{
	public class MemoryAppender : AppenderSkeleton
	{
		protected ArrayList m_eventsList;

		protected FixFlags m_fixFlags = FixFlags.All;

		public virtual FixFlags Fix
		{
			get
			{
				return this.m_fixFlags;
			}
			set
			{
				this.m_fixFlags = value;
			}
		}

		[Obsolete("Use Fix property")]
		public virtual bool OnlyFixPartialEventData
		{
			get
			{
				return this.Fix == FixFlags.Partial;
			}
			set
			{
				if (value)
				{
					this.Fix = FixFlags.Partial;
					return;
				}
				this.Fix = FixFlags.All;
			}
		}

		public MemoryAppender()
		{
			this.m_eventsList = new ArrayList();
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			loggingEvent.Fix = this.Fix;
			this.m_eventsList.Add(loggingEvent);
		}

		public virtual void Clear()
		{
			this.m_eventsList.Clear();
		}

		public virtual LoggingEvent[] GetEvents()
		{
			return (LoggingEvent[])this.m_eventsList.ToArray(typeof(LoggingEvent));
		}
	}
}