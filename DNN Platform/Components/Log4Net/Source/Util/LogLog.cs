using System;
using System.Collections;
using System.Diagnostics;

namespace log4net.Util
{
	public sealed class LogLog
	{
		private const string PREFIX = "log4net: ";

		private const string ERR_PREFIX = "log4net:ERROR ";

		private const string WARN_PREFIX = "log4net:WARN ";

		private readonly Type source;

		private readonly DateTime timeStamp;

		private readonly string prefix;

		private readonly string message;

		private readonly System.Exception exception;

		private static bool s_debugEnabled;

		private static bool s_quietMode;

		private static bool s_emitInternalMessages;

		public static bool EmitInternalMessages
		{
			get
			{
				return LogLog.s_emitInternalMessages;
			}
			set
			{
				LogLog.s_emitInternalMessages = value;
			}
		}

		public System.Exception Exception
		{
			get
			{
				return this.exception;
			}
		}

		public static bool InternalDebugging
		{
			get
			{
				return LogLog.s_debugEnabled;
			}
			set
			{
				LogLog.s_debugEnabled = value;
			}
		}

		public static bool IsDebugEnabled
		{
			get
			{
				if (!LogLog.s_debugEnabled)
				{
					return false;
				}
				return !LogLog.s_quietMode;
			}
		}

		public static bool IsErrorEnabled
		{
			get
			{
				return !LogLog.s_quietMode;
			}
		}

		public static bool IsWarnEnabled
		{
			get
			{
				return !LogLog.s_quietMode;
			}
		}

		public string Message
		{
			get
			{
				return this.message;
			}
		}

		public string Prefix
		{
			get
			{
				return this.prefix;
			}
		}

		public static bool QuietMode
		{
			get
			{
				return LogLog.s_quietMode;
			}
			set
			{
				LogLog.s_quietMode = value;
			}
		}

		public Type Source
		{
			get
			{
				return this.source;
			}
		}

		public DateTime TimeStamp
		{
			get
			{
				return this.timeStamp;
			}
		}

		static LogLog()
		{
			LogLog.s_debugEnabled = false;
			LogLog.s_quietMode = false;
			LogLog.s_emitInternalMessages = true;
			try
			{
				LogLog.InternalDebugging = OptionConverter.ToBoolean(SystemInfo.GetAppSetting("log4net.Internal.Debug"), false);
				LogLog.QuietMode = OptionConverter.ToBoolean(SystemInfo.GetAppSetting("log4net.Internal.Quiet"), false);
				LogLog.EmitInternalMessages = OptionConverter.ToBoolean(SystemInfo.GetAppSetting("log4net.Internal.Emit"), true);
			}
			catch (System.Exception exception)
			{
				LogLog.Error(typeof(LogLog), "Exception while reading ConfigurationSettings. Check your .config file is well formed XML.", exception);
			}
		}

		public LogLog(Type source, string prefix, string message, System.Exception exception)
		{
			this.timeStamp = DateTime.Now;
			this.source = source;
			this.prefix = prefix;
			this.message = message;
			this.exception = exception;
		}

		public static void Debug(Type source, string message)
		{
			if (LogLog.IsDebugEnabled)
			{
				if (LogLog.EmitInternalMessages)
				{
					LogLog.EmitOutLine(string.Concat("log4net: ", message));
				}
				LogLog.OnLogReceived(source, "log4net: ", message, null);
			}
		}

		public static void Debug(Type source, string message, System.Exception exception)
		{
			if (LogLog.IsDebugEnabled)
			{
				if (LogLog.EmitInternalMessages)
				{
					LogLog.EmitOutLine(string.Concat("log4net: ", message));
					if (exception != null)
					{
						LogLog.EmitOutLine(exception.ToString());
					}
				}
				LogLog.OnLogReceived(source, "log4net: ", message, exception);
			}
		}

		private static void EmitErrorLine(string message)
		{
			try
			{
				Console.Error.WriteLine(message);
				Trace.WriteLine(message);
			}
			catch
			{
			}
		}

		private static void EmitOutLine(string message)
		{
			try
			{
				Console.Out.WriteLine(message);
				Trace.WriteLine(message);
			}
			catch
			{
			}
		}

		public static void Error(Type source, string message)
		{
			if (LogLog.IsErrorEnabled)
			{
				if (LogLog.EmitInternalMessages)
				{
					LogLog.EmitErrorLine(string.Concat("log4net:ERROR ", message));
				}
				LogLog.OnLogReceived(source, "log4net:ERROR ", message, null);
			}
		}

		public static void Error(Type source, string message, System.Exception exception)
		{
			if (LogLog.IsErrorEnabled)
			{
				if (LogLog.EmitInternalMessages)
				{
					LogLog.EmitErrorLine(string.Concat("log4net:ERROR ", message));
					if (exception != null)
					{
						LogLog.EmitErrorLine(exception.ToString());
					}
				}
				LogLog.OnLogReceived(source, "log4net:ERROR ", message, exception);
			}
		}

		public static void OnLogReceived(Type source, string prefix, string message, System.Exception exception)
		{
			if (LogLog.LogReceived != null)
			{
				LogLog.LogReceived(null, new LogReceivedEventArgs(new LogLog(source, prefix, message, exception)));
			}
		}

		public override string ToString()
		{
			return string.Concat(this.Prefix, this.Source.Name, ": ", this.Message);
		}

		public static void Warn(Type source, string message)
		{
			if (LogLog.IsWarnEnabled)
			{
				if (LogLog.EmitInternalMessages)
				{
					LogLog.EmitErrorLine(string.Concat("log4net:WARN ", message));
				}
				LogLog.OnLogReceived(source, "log4net:WARN ", message, null);
			}
		}

		public static void Warn(Type source, string message, System.Exception exception)
		{
			if (LogLog.IsWarnEnabled)
			{
				if (LogLog.EmitInternalMessages)
				{
					LogLog.EmitErrorLine(string.Concat("log4net:WARN ", message));
					if (exception != null)
					{
						LogLog.EmitErrorLine(exception.ToString());
					}
				}
				LogLog.OnLogReceived(source, "log4net:WARN ", message, exception);
			}
		}

		public static event LogReceivedEventHandler LogReceived;

		public class LogReceivedAdapter : IDisposable
		{
			private readonly IList items;

			private readonly LogReceivedEventHandler handler;

			public IList Items
			{
				get
				{
					return this.items;
				}
			}

			public LogReceivedAdapter(IList items)
			{
				this.items = items;
				this.handler = new LogReceivedEventHandler(this.LogLog_LogReceived);
				LogLog.LogReceived += this.handler;
			}

			public void Dispose()
			{
				LogLog.LogReceived -= this.handler;
			}

			private void LogLog_LogReceived(object source, LogReceivedEventArgs e)
			{
				this.items.Add(e.LogLog);
			}
		}
	}
}