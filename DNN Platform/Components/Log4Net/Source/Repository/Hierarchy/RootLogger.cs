using log4net.Core;
using log4net.Util;
using System;

namespace log4net.Repository.Hierarchy
{
	public class RootLogger : Logger
	{
		private readonly static Type declaringType;

		public override log4net.Core.Level EffectiveLevel
		{
			get
			{
				return base.Level;
			}
		}

		public override log4net.Core.Level Level
		{
			get
			{
				return base.Level;
			}
			set
			{
				if (value != null)
				{
					base.Level = value;
					return;
				}
				LogLog.Error(RootLogger.declaringType, "You have tried to set a null level to root.", new LogException());
			}
		}

		static RootLogger()
		{
			RootLogger.declaringType = typeof(RootLogger);
		}

		public RootLogger(log4net.Core.Level level) : base("root")
		{
			this.Level = level;
		}
	}
}