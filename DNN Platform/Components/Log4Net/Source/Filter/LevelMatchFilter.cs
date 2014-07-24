using log4net.Core;
using System;

namespace log4net.Filter
{
	public class LevelMatchFilter : FilterSkeleton
	{
		private bool m_acceptOnMatch = true;

		private Level m_levelToMatch;

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

		public Level LevelToMatch
		{
			get
			{
				return this.m_levelToMatch;
			}
			set
			{
				this.m_levelToMatch = value;
			}
		}

		public LevelMatchFilter()
		{
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (!(this.m_levelToMatch != null) || !(this.m_levelToMatch == loggingEvent.Level))
			{
				return FilterDecision.Neutral;
			}
			if (!this.m_acceptOnMatch)
			{
				return FilterDecision.Deny;
			}
			return FilterDecision.Accept;
		}
	}
}