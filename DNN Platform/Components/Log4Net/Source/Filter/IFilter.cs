using log4net.Core;

namespace log4net.Filter
{
	public interface IFilter : IOptionHandler
	{
		IFilter Next
		{
			get;
			set;
		}

		FilterDecision Decide(LoggingEvent loggingEvent);
	}
}