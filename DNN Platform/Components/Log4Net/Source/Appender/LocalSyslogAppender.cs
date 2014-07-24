using log4net.Core;
using log4net.Util;
using System;
using System.Runtime.InteropServices;

namespace log4net.Appender
{
	public class LocalSyslogAppender : AppenderSkeleton
	{
		private LocalSyslogAppender.SyslogFacility m_facility = LocalSyslogAppender.SyslogFacility.User;

		private string m_identity;

		private IntPtr m_handleToIdentity = IntPtr.Zero;

		private LevelMapping m_levelMapping = new LevelMapping();

		public LocalSyslogAppender.SyslogFacility Facility
		{
			get
			{
				return this.m_facility;
			}
			set
			{
				this.m_facility = value;
			}
		}

		public string Identity
		{
			get
			{
				return this.m_identity;
			}
			set
			{
				this.m_identity = value;
			}
		}

		protected override bool RequiresLayout
		{
			get
			{
				return true;
			}
		}

		public LocalSyslogAppender()
		{
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			this.m_levelMapping.ActivateOptions();
			string mIdentity = this.m_identity ?? SystemInfo.ApplicationFriendlyName;
			this.m_handleToIdentity = Marshal.StringToHGlobalAnsi(mIdentity);
			LocalSyslogAppender.openlog(this.m_handleToIdentity, 1, this.m_facility);
		}

		public void AddMapping(LocalSyslogAppender.LevelSeverity mapping)
		{
			this.m_levelMapping.Add(mapping);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			int num = LocalSyslogAppender.GeneratePriority(this.m_facility, this.GetSeverity(loggingEvent.Level));
			LocalSyslogAppender.syslog(num, "%s", base.RenderLoggingEvent(loggingEvent));
		}

		[DllImport("libc", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern void closelog();

		private static int GeneratePriority(LocalSyslogAppender.SyslogFacility facility, LocalSyslogAppender.SyslogSeverity severity)
		{
			return (int)facility * (int)LocalSyslogAppender.SyslogFacility.Uucp + (int)severity;
		}

		protected virtual LocalSyslogAppender.SyslogSeverity GetSeverity(Level level)
		{
			LocalSyslogAppender.LevelSeverity levelSeverity = this.m_levelMapping.Lookup(level) as LocalSyslogAppender.LevelSeverity;
			if (levelSeverity != null)
			{
				return levelSeverity.Severity;
			}
			if (level >= Level.Alert)
			{
				return LocalSyslogAppender.SyslogSeverity.Alert;
			}
			if (level >= Level.Critical)
			{
				return LocalSyslogAppender.SyslogSeverity.Critical;
			}
			if (level >= Level.Error)
			{
				return LocalSyslogAppender.SyslogSeverity.Error;
			}
			if (level >= Level.Warn)
			{
				return LocalSyslogAppender.SyslogSeverity.Warning;
			}
			if (level >= Level.Notice)
			{
				return LocalSyslogAppender.SyslogSeverity.Notice;
			}
			if (level >= Level.Info)
			{
				return LocalSyslogAppender.SyslogSeverity.Informational;
			}
			return LocalSyslogAppender.SyslogSeverity.Debug;
		}

		protected override void OnClose()
		{
			base.OnClose();
			try
			{
				LocalSyslogAppender.closelog();
			}
			catch (DllNotFoundException dllNotFoundException)
			{
			}
			if (this.m_handleToIdentity != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(this.m_handleToIdentity);
			}
		}

		[DllImport("libc", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern void openlog(IntPtr ident, int option, LocalSyslogAppender.SyslogFacility facility);

		[DllImport("libc", CallingConvention=CallingConvention.Cdecl, CharSet=CharSet.Ansi, ExactSpelling=false)]
		private static extern void syslog(int priority, string format, string message);

		public class LevelSeverity : LevelMappingEntry
		{
			private LocalSyslogAppender.SyslogSeverity m_severity;

			public LocalSyslogAppender.SyslogSeverity Severity
			{
				get
				{
					return this.m_severity;
				}
				set
				{
					this.m_severity = value;
				}
			}

			public LevelSeverity()
			{
			}
		}

		public enum SyslogFacility
		{
			Kernel,
			User,
			Mail,
			Daemons,
			Authorization,
			Syslog,
			Printer,
			News,
			Uucp,
			Clock,
			Authorization2,
			Ftp,
			Ntp,
			Audit,
			Alert,
			Clock2,
			Local0,
			Local1,
			Local2,
			Local3,
			Local4,
			Local5,
			Local6,
			Local7
		}

		public enum SyslogSeverity
		{
			Emergency,
			Alert,
			Critical,
			Error,
			Warning,
			Notice,
			Informational,
			Debug
		}
	}
}