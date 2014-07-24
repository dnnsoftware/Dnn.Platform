using log4net.Core;
using System;

namespace log4net.Filter
{
	public class LoggerMatchFilter : FilterSkeleton
	{
		private bool m_acceptOnMatch = true;

		private string m_loggerToMatch;

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

		public string LoggerToMatch
		{
			get
			{
				return this.m_loggerToMatch;
			}
			set
			{
				this.m_loggerToMatch = value;
			}
		}

		public LoggerMatchFilter()
		{
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (this.m_loggerToMatch == null || this.m_loggerToMatch.Length == 0 || !loggingEvent.LoggerName.StartsWith(this.m_loggerToMatch))
			{
				return FilterDecision.Neutral;
			}
			if (this.m_acceptOnMatch)
			{
				return FilterDecision.Accept;
			}
			return FilterDecision.Deny;
		}
	}
}