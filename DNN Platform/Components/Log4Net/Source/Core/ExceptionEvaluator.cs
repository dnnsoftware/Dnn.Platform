using System;

namespace log4net.Core
{
	public class ExceptionEvaluator : ITriggeringEventEvaluator
	{
		private Type m_type;

		private bool m_triggerOnSubclass;

		public Type ExceptionType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = value;
			}
		}

		public bool TriggerOnSubclass
		{
			get
			{
				return this.m_triggerOnSubclass;
			}
			set
			{
				this.m_triggerOnSubclass = value;
			}
		}

		public ExceptionEvaluator()
		{
		}

		public ExceptionEvaluator(Type exType, bool triggerOnSubClass)
		{
			if (exType == null)
			{
				throw new ArgumentNullException("exType");
			}
			this.m_type = exType;
			this.m_triggerOnSubclass = triggerOnSubClass;
		}

		public bool IsTriggeringEvent(LoggingEvent loggingEvent)
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}
			if (!this.m_triggerOnSubclass || loggingEvent.ExceptionObject == null)
			{
				if (this.m_triggerOnSubclass || loggingEvent.ExceptionObject == null)
				{
					return false;
				}
				return loggingEvent.ExceptionObject.GetType() == this.m_type;
			}
			Type type = loggingEvent.ExceptionObject.GetType();
			if (type == this.m_type)
			{
				return true;
			}
			return type.IsSubclassOf(this.m_type);
		}
	}
}