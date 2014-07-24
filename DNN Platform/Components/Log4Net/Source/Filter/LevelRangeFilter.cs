using log4net.Core;
using System;

namespace log4net.Filter
{
	public class LevelRangeFilter : FilterSkeleton
	{
		private bool m_acceptOnMatch = true;

		private Level m_levelMin;

		private Level m_levelMax;

		public bool AcceptOnMatch
		{
			get
			{
				return this.m_acceptOnMatch;
			}
			set
			{
				this.m_acceptOnMatch = value;
			}
		}

		public Level LevelMax
		{
			get
			{
				return this.m_levelMax;
			}
			set
			{
				this.m_levelMax = value;
			}
		}

		public Level LevelMin
		{
			get
			{
				return this.m_levelMin;
			}
			set
			{
				this.m_levelMin = value;
			}
		}

		public LevelRangeFilter()
		{
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (this.m_levelMin != null && loggingEvent.Level < this.m_levelMin)
			{
				return FilterDecision.Deny;
			}
			if (this.m_levelMax != null && loggingEvent.Level > this.m_levelMax)
			{
				return FilterDecision.Deny;
			}
			if (this.m_acceptOnMatch)
			{
				return FilterDecision.Accept;
			}
			return FilterDecision.Neutral;
		}
	}
}