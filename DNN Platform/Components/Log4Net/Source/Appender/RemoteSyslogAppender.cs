using log4net.Core;
using log4net.Layout;
using log4net.Util;
using System;
using System.Globalization;
using System.IO;
using System.Net;

namespace log4net.Appender
{
	public class RemoteSyslogAppender : UdpAppender
	{
		private const int DefaultSyslogPort = 514;

		private RemoteSyslogAppender.SyslogFacility m_facility = RemoteSyslogAppender.SyslogFacility.User;

		private PatternLayout m_identity;

		private LevelMapping m_levelMapping = new LevelMapping();

		public RemoteSyslogAppender.SyslogFacility Facility
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

		public PatternLayout Identity
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

		public RemoteSyslogAppender()
		{
			base.RemotePort = 514;
			base.RemoteAddress = IPAddress.Parse("127.0.0.1");
			base.Encoding = System.Text.Encoding.ASCII;
		}

		public override void ActivateOptions()
		{
			base.ActivateOptions();
			this.m_levelMapping.ActivateOptions();
		}

		public void AddMapping(RemoteSyslogAppender.LevelSeverity mapping)
		{
			this.m_levelMapping.Add(mapping);
		}

		protected override void Append(LoggingEvent loggingEvent)
		{
			try
			{
				StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
				int num = RemoteSyslogAppender.GeneratePriority(this.m_facility, this.GetSeverity(loggingEvent.Level));
				stringWriter.Write('<');
				stringWriter.Write(num);
				stringWriter.Write('>');
				if (this.m_identity == null)
				{
					stringWriter.Write(loggingEvent.Domain);
				}
				else
				{
					this.m_identity.Format(stringWriter, loggingEvent);
				}
				stringWriter.Write(": ");
				base.RenderLoggingEvent(stringWriter, loggingEvent);
				string str = stringWriter.ToString();
				byte[] bytes = base.Encoding.GetBytes(str.ToCharArray());
				base.Client.Send(bytes, (int)bytes.Length, base.RemoteEndPoint);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IErrorHandler errorHandler = this.ErrorHandler;
				object[] objArray = new object[] { "Unable to send logging event to remote syslog ", base.RemoteAddress.ToString(), " on port ", base.RemotePort, "." };
				errorHandler.Error(string.Concat(objArray), exception, ErrorCode.WriteFailure);
			}
		}

		public static int GeneratePriority(RemoteSyslogAppender.SyslogFacility facility, RemoteSyslogAppender.SyslogSeverity severity)
		{
			if (facility < RemoteSyslogAppender.SyslogFacility.Kernel || facility > RemoteSyslogAppender.SyslogFacility.Local7)
			{
				throw new ArgumentException("SyslogFacility out of range", "facility");
			}
			if (severity < RemoteSyslogAppender.SyslogSeverity.Emergency || severity > RemoteSyslogAppender.SyslogSeverity.Debug)
			{
				throw new ArgumentException("SyslogSeverity out of range", "severity");
			}
			return (int)facility * (int)RemoteSyslogAppender.SyslogFacility.Uucp + (int)severity;
		}

		protected virtual RemoteSyslogAppender.SyslogSeverity GetSeverity(Level level)
		{
			RemoteSyslogAppender.LevelSeverity levelSeverity = this.m_levelMapping.Lookup(level) as RemoteSyslogAppender.LevelSeverity;
			if (levelSeverity != null)
			{
				return levelSeverity.Severity;
			}
			if (level >= Level.Alert)
			{
				return RemoteSyslogAppender.SyslogSeverity.Alert;
			}
			if (level >= Level.Critical)
			{
				return RemoteSyslogAppender.SyslogSeverity.Critical;
			}
			if (level >= Level.Error)
			{
				return RemoteSyslogAppender.SyslogSeverity.Error;
			}
			if (level >= Level.Warn)
			{
				return RemoteSyslogAppender.SyslogSeverity.Warning;
			}
			if (level >= Level.Notice)
			{
				return RemoteSyslogAppender.SyslogSeverity.Notice;
			}
			if (level >= Level.Info)
			{
				return RemoteSyslogAppender.SyslogSeverity.Informational;
			}
			return RemoteSyslogAppender.SyslogSeverity.Debug;
		}

		public class LevelSeverity : LevelMappingEntry
		{
			private RemoteSyslogAppender.SyslogSeverity m_severity;

			public RemoteSyslogAppender.SyslogSeverity Severity
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