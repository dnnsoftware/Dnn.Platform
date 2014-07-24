using log4net.Core;

using System.Runtime.InteropServices;

namespace log4net.Appender
{
	public class OutputDebugStringAppender : AppenderSkeleton
	{
		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public OutputDebugStringAppender()
		{
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			OutputDebugStringAppender.OutputDebugString(base.RenderLoggingEvent(loggingEvent));
		}

		[DllImport("Kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		protected static extern void OutputDebugString(string message);
	}
}