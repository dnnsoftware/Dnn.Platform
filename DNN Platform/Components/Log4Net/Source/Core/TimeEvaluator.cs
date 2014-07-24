using System;

namespace log4net.Core
{
	public class TimeEvaluator : ITriggeringEventEvaluator
	{
		private const int DEFAULT_INTERVAL = 0;

		private int m_interval;

		private DateTime m_lasttime;

		public int Interval
		{
			get
			{
				return this.m_interval;
			}
			set
			{
				this.m_interval = value;
			}
		}

		public TimeEvaluator() : this(0)
		{
		}

		public TimeEvaluator(int interval)
		{
			this.m_interval = interval;
			this.m_lasttime = DateTime.Now;
		}

		public bool IsTriggeringEvent(LoggingEvent loggingEvent)
		{
			bool flag;
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (this.m_interval == 0)
			{
				return false;
			}
			lock (this)
			{
				if (DateTime.Now.Subtract(this.m_lasttime).TotalSeconds <= (double)this.m_interval)
				{
					flag = false;
				}
				else
				{
					this.m_lasttime = DateTime.Now;
					flag = true;
				}
			}
			return flag;
		}
	}
}