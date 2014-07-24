using log4net.Core;

namespace log4net.Filter
{
	public sealed class DenyAllFilter : FilterSkeleton
	{
		public DenyAllFilter()
		{
		}

		public override FilterDecision Decide(LoggingEvent loggingEvent)
		{
			return FilterDecision.Deny;
		}
	}
}