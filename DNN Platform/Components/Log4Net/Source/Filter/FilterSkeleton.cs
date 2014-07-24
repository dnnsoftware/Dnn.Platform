using log4net.Core;

namespace log4net.Filter
{
	public abstract class FilterSkeleton : IFilter, IOptionHandler
	{
		private IFilter m_next;

		public IFilter Next
		{
			get
			{
				return this.m_next;
			}
			set
			{
				this.m_next = value;
			}
		}

		protected FilterSkeleton()
		{
		}

		public virtual void ActivateOptions()
		{
		}

		public abstract FilterDecision Decide(LoggingEvent loggingEvent);
	}
}