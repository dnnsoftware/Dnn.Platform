using log4net.Core;

using System;

namespace log4net.Filter
{
	public class PropertyFilter : StringMatchFilter
	{
		private string m_key;

		public string Key
		{
			get
			{
				return this.m_key;
			}
			set
			{
				this.m_key = value;
			}
		}

		public PropertyFilter()
		{
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (this.m_key == null)
			{
				return FilterDecision.Neutral;
			}
			object obj = loggingEvent.LookupProperty(this.m_key);
			string str = loggingEvent.Repository.RendererMap.FindAndRender(obj);
			if (str == null || this.m_stringToMatch == null && this.m_regexToMatch == null)
			{
				return FilterDecision.Neutral;
			}
			if (this.m_regexToMatch != null)
			{
				if (!this.m_regexToMatch.Match(str).Success)
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
			if (str.IndexOf(this.m_stringToMatch) == -1)
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