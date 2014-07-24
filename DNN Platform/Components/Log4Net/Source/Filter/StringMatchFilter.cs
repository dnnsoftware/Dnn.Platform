using log4net.Core;
using System;
using System.Text.RegularExpressions;

namespace log4net.Filter
{
	public class StringMatchFilter : FilterSkeleton
	{
		protected bool m_acceptOnMatch = true;

		protected string m_stringToMatch;

		protected string m_stringRegexToMatch;

		protected Regex m_regexToMatch;

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

		public string RegexToMatch
		{
			get
			{
				return this.m_stringRegexToMatch;
			}
			set
			{
				this.m_stringRegexToMatch = value;
			}
		}

		public string StringToMatch
		{
			get
			{
				return this.m_stringToMatch;
			}
			set
			{
				this.m_stringToMatch = value;
			}
		}

		public StringMatchFilter()
		{
		}

		public override void ActivateOptions()
		{
			if (this.m_stringRegexToMatch != null)
			{
				this.m_regexToMatch = new Regex(this.m_stringRegexToMatch, RegexOptions.Compiled);
			}
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			string renderedMessage = loggingEvent.RenderedMessage;
			if (renderedMessage == null || this.m_stringToMatch == null && this.m_regexToMatch == null)
			{
				return FilterDecision.Neutral;
			}
			if (this.m_regexToMatch != null)
			{
				if (!this.m_regexToMatch.Match(renderedMessage).Success)
				{
					return FilterDecision.Neutral;
				}
				if (this.m_acceptOnMatch)
				{
					return FilterDecision.Accept;
				}
				return FilterDecision.Deny;
			}
			if (this.m_stringToMatch == null)
			{
				return FilterDecision.Neutral;
			}
			if (renderedMessage.IndexOf(this.m_stringToMatch) == -1)
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