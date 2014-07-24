using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System;
using System.Diagnostics;
using System.Threading;

namespace log4net.Appender
{
	public class EventLogAppender : AppenderSkeleton
	{
		private string m_logName;

		private string m_applicationName;

		private string m_machineName;

		private LevelMapping m_levelMapping = new LevelMapping();

		private log4net.Core.SecurityContext m_securityContext;

		private readonly static Type declaringType;

		public string ApplicationName
		{
			get
			{
				return this.m_applicationName;
			}
			set
			{
				this.m_applicationName = value;
			}
		}

		public string LogName
		{
			get
			{
				return this.m_logName;
			}
			set
			{
				this.m_logName = value;
			}
		}

		public string MachineName
		{
			get
			{
				return this.m_machineName;
			}
			set
			{
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public log4net.Core.SecurityContext SecurityContext
		{
			get
			{
				return this.m_securityContext;
			}
			set
			{
				this.m_securityContext = value;
			}
		}

		static EventLogAppender()
		{
			EventLogAppender.declaringType = typeof(EventLogAppender);
		}

		public EventLogAppender()
		{
			this.m_applicationName = Thread.GetDomain().FriendlyName;
			this.m_logName = "Application";
			this.m_machineName = ".";
		}

		[Obsolete("Instead use the default constructor and set the Layout property")]
		public EventLogAppender(ILayout layout) : this()
		{
			this.Layout = layout;
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			if (this.m_securityContext == null)
			{
				this.m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}
			bool flag = false;
			string str = null;
			using (IDisposable disposable = this.SecurityContext.Impersonate(this))
			{
				flag = EventLog.SourceExists(this.m_applicationName);
				if (flag)
				{
					str = EventLog.LogNameFromSourceName(this.m_applicationName, this.m_machineName);
				}
			}
			if (flag && str != this.m_logName)
			{
				Type type = EventLogAppender.declaringType;
				string[] mApplicationName = new string[] { "Changing event source [", this.m_applicationName, "] from log [", str, "] to log [", this.m_logName, "]" };
				LogLog.Debug(type, string.Concat(mApplicationName));
			}
			else if (!flag)
			{
				Type type1 = EventLogAppender.declaringType;
				string[] strArrays = new string[] { "Creating event source Source [", this.m_applicationName, "] in log ", this.m_logName, "]" };
				LogLog.Debug(type1, string.Concat(strArrays));
			}
			string str1 = null;
			using (IDisposable disposable1 = this.SecurityContext.Impersonate(this))
			{
				if (flag && str != this.m_logName)
				{
					EventLog.DeleteEventSource(this.m_applicationName, this.m_machineName);
					EventLogAppender.CreateEventSource(this.m_applicationName, this.m_logName, this.m_machineName);
					str1 = EventLog.LogNameFromSourceName(this.m_applicationName, this.m_machineName);
				}
				else if (!flag)
				{
					EventLogAppender.CreateEventSource(this.m_applicationName, this.m_logName, this.m_machineName);
					str1 = EventLog.LogNameFromSourceName(this.m_applicationName, this.m_machineName);
				}
			}
			this.m_levelMapping.ActivateOptions();
			Type type2 = EventLogAppender.declaringType;
			string[] mApplicationName1 = new string[] { "Source [", this.m_applicationName, "] is registered to log [", str1, "]" };
			LogLog.Debug(type2, string.Concat(mApplicationName1));
		}

		public void AddMapping(EventLogAppender.Level2EventLogEntryType mapping)
		{
			this.m_levelMapping.Add(mapping);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			int num;
			int num1 = 0;
			object obj = loggingEvent.LookupProperty("EventID");
			if (obj != null)
			{
				if (!(obj is int))
				{
					string str = obj as string;
					if (str != null && str.Length > 0)
					{
						if (!SystemInfo.TryParse(str, out num))
						{
							this.ErrorHandler.Error(string.Concat("Unable to parse event ID property [", str, "]."));
						}
						else
						{
							num1 = num;
						}
					}
				}
				else
				{
					num1 = (int)obj;
				}
			}
			try
			{
				string str1 = base.RenderLoggingEvent(loggingEvent);
				if (str1.Length > 32000)
				{
					str1 = str1.Substring(0, 32000);
				}
				EventLogEntryType entryType = this.GetEntryType(loggingEvent.Level);
				using (IDisposable disposable = this.SecurityContext.Impersonate(this))
				{
					EventLog.WriteEntry(this.m_applicationName, str1, entryType, num1);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IErrorHandler errorHandler = this.ErrorHandler;
				string[] mLogName = new string[] { "Unable to write to event log [", this.m_logName, "] using source [", this.m_applicationName, "]" };
				errorHandler.Error(string.Concat(mLogName), exception);
			}
		}

		private static void CreateEventSource(string source, string logName, string machineName)
		{
			EventSourceCreationData eventSourceCreationDatum = new EventSourceCreationData(source, logName)
			{
				MachineName = machineName
			};
			EventLog.CreateEventSource(eventSourceCreationDatum);
		}

		protected virtual EventLogEntryType GetEntryType(Level level)
		{
			EventLogAppender.Level2EventLogEntryType level2EventLogEntryType = this.m_levelMapping.Lookup(level) as EventLogAppender.Level2EventLogEntryType;
			if (level2EventLogEntryType != null)
			{
				return level2EventLogEntryType.EventLogEntryType;
			}
			if (level >= Level.Error)
			{
				return EventLogEntryType.Error;
			}
			if (level == Level.Warn)
			{
				return EventLogEntryType.Warning;
			}
			return EventLogEntryType.Information;
		}

		public class Level2EventLogEntryType : LevelMappingEntry
		{
			private EventLogEntryType m_entryType;

			public EventLogEntryType EventLogEntryType
			{
				get
				{
					return this.m_entryType;
				}
				set
				{
					this.m_entryType = value;
				}
			}

			public Level2EventLogEntryType()
			{
			}
		}
	}
}