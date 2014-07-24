using log4net.Core;
using log4net.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace log4net.Layout.Pattern
{
	internal class StackTracePatternConverter : PatternLayoutConverter, IOptionHandler
	{
		private int m_stackFrameLevel = 1;

		private readonly static Type declaringType;

		static StackTracePatternConverter()
		{
			StackTracePatternConverter.declaringType = typeof(StackTracePatternConverter);
		}

		public StackTracePatternConverter()
		{
		}

		public void ActivateOptions()
		{
			int num;
			if (this.Option == null)
			{
				return;
			}
			string str = this.Option.Trim();
			if (str.Length != 0)
			{
				if (SystemInfo.TryParse(str, out num))
				{
					if (num > 0)
					{
						this.m_stackFrameLevel = num;
						return;
					}
					LogLog.Error(StackTracePatternConverter.declaringType, string.Concat("StackTracePatternConverter: StackeFrameLevel option (", str, ") isn't a positive integer."));
					return;
				}
				LogLog.Error(StackTracePatternConverter.declaringType, string.Concat("StackTracePatternConverter: StackFrameLevel option \"", str, "\" not a decimal integer."));
			}
		}

		protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			StackFrame[] stackFrames = loggingEvent.LocationInformation.StackFrames;
			if (stackFrames == null || (int)stackFrames.Length <= 0)
			{
				LogLog.Error(StackTracePatternConverter.declaringType, "loggingEvent.LocationInformation.StackFrames was null or empty.");
				return;
			}
			int mStackFrameLevel = this.m_stackFrameLevel - 1;
			while (mStackFrameLevel >= 0)
			{
				if (mStackFrameLevel <= (int)stackFrames.Length)
				{
					StackFrame stackFrame = stackFrames[mStackFrameLevel];
					writer.Write("{0}.{1}", stackFrame.GetMethod().DeclaringType.Name, this.GetMethodInformation(stackFrame.GetMethod()));
					if (mStackFrameLevel > 0)
					{
						writer.Write(" > ");
					}
					mStackFrameLevel--;
				}
				else
				{
					mStackFrameLevel--;
				}
			}
		}

		internal virtual string GetMethodInformation(MethodBase method)
		{
			return method.Name;
		}
	}
}