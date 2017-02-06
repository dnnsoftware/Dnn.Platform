#region Apache License
//
// Licensed to the Apache Software Foundation (ASF) under one or more 
// contributor license agreements. See the NOTICE file distributed with
// this work for additional information regarding copyright ownership. 
// The ASF licenses this file to you under the Apache License, Version 2.0
// (the "License"); you may not use this file except in compliance with 
// the License. You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

using log4net.Core;
using log4net.Appender;
using log4net.Util;
using log4net.Layout;
using System.Text;

namespace log4net.Appender
{
	/// <summary>
	/// Logs events to a remote syslog daemon.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The BSD syslog protocol is used to remotely log to
	/// a syslog daemon. The syslogd listens for for messages
	/// on UDP port 514.
	/// </para>
	/// <para>
	/// The syslog UDP protocol is not authenticated. Most syslog daemons
	/// do not accept remote log messages because of the security implications.
	/// You may be able to use the LocalSyslogAppender to talk to a local
	/// syslog service.
	/// </para>
	/// <para>
	/// There is an RFC 3164 that claims to document the BSD Syslog Protocol.
	/// This RFC can be seen here: http://www.faqs.org/rfcs/rfc3164.html.
	/// This appender generates what the RFC calls an "Original Device Message",
	/// i.e. does not include the TIMESTAMP or HOSTNAME fields. By observation
	/// this format of message will be accepted by all current syslog daemon
	/// implementations. The daemon will attach the current time and the source
	/// hostname or IP address to any messages received.
	/// </para>
	/// <para>
	/// Syslog messages must have a facility and and a severity. The severity
	/// is derived from the Level of the logging event.
	/// The facility must be chosen from the set of defined syslog 
	/// <see cref="SyslogFacility"/> values. The facilities list is predefined
	/// and cannot be extended.
	/// </para>
	/// <para>
	/// An identifier is specified with each log message. This can be specified
	/// by setting the <see cref="Identity"/> property. The identity (also know 
	/// as the tag) must not contain white space. The default value for the
	/// identity is the application name (from <see cref="LoggingEvent.Domain"/>).
	/// </para>
	/// </remarks>
	/// <author>Rob Lyon</author>
	/// <author>Nicko Cadell</author>
	public class RemoteSyslogAppender : UdpAppender
	{
		/// <summary>
		/// Syslog port 514
		/// </summary>
		private const int DefaultSyslogPort = 514;

		#region Enumerations

		/// <summary>
		/// syslog severities
		/// </summary>
		/// <remarks>
		/// <para>
		/// The syslog severities.
		/// </para>
		/// </remarks>
		public enum SyslogSeverity
		{
			/// <summary>
			/// system is unusable
			/// </summary>
			Emergency = 0,

			/// <summary>
			/// action must be taken immediately
			/// </summary>
			Alert = 1,

			/// <summary>
			/// critical conditions
			/// </summary>
			Critical = 2,

			/// <summary>
			/// error conditions
			/// </summary>
			Error = 3,

			/// <summary>
			/// warning conditions
			/// </summary>
			Warning = 4,

			/// <summary>
			/// normal but significant condition
			/// </summary>
			Notice = 5,

			/// <summary>
			/// informational
			/// </summary>
			Informational = 6,

			/// <summary>
			/// debug-level messages
			/// </summary>
			Debug = 7
		};

		/// <summary>
		/// syslog facilities
		/// </summary>
		/// <remarks>
		/// <para>
		/// The syslog facilities
		/// </para>
		/// </remarks>
		public enum SyslogFacility
		{
			/// <summary>
			/// kernel messages
			/// </summary>
			Kernel = 0,

			/// <summary>
			/// random user-level messages
			/// </summary>
			User = 1,

			/// <summary>
			/// mail system
			/// </summary>
			Mail = 2,

			/// <summary>
			/// system daemons
			/// </summary>
			Daemons = 3,

			/// <summary>
			/// security/authorization messages
			/// </summary>
			Authorization = 4,

			/// <summary>
			/// messages generated internally by syslogd
			/// </summary>
			Syslog = 5,

			/// <summary>
			/// line printer subsystem
			/// </summary>
			Printer = 6,

			/// <summary>
			/// network news subsystem
			/// </summary>
			News = 7,

			/// <summary>
			/// UUCP subsystem
			/// </summary>
			Uucp = 8,

			/// <summary>
			/// clock (cron/at) daemon
			/// </summary>
			Clock = 9,

			/// <summary>
			/// security/authorization  messages (private)
			/// </summary>
			Authorization2 = 10,

			/// <summary>
			/// ftp daemon
			/// </summary>
			Ftp = 11,

			/// <summary>
			/// NTP subsystem
			/// </summary>
			Ntp = 12,

			/// <summary>
			/// log audit
			/// </summary>
			Audit = 13,

			/// <summary>
			/// log alert
			/// </summary>
			Alert = 14,

			/// <summary>
			/// clock daemon
			/// </summary>
			Clock2 = 15,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local0 = 16,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local1 = 17,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local2 = 18,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local3 = 19,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local4 = 20,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local5 = 21,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local6 = 22,

			/// <summary>
			/// reserved for local use
			/// </summary>
			Local7 = 23
		}

		#endregion Enumerations

		#region Public Instance Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteSyslogAppender" /> class.
		/// </summary>
		/// <remarks>
		/// This instance of the <see cref="RemoteSyslogAppender" /> class is set up to write 
		/// to a remote syslog daemon.
		/// </remarks>
		public RemoteSyslogAppender()
		{
			// syslog udp defaults
			this.RemotePort = DefaultSyslogPort;
			this.RemoteAddress = System.Net.IPAddress.Parse("127.0.0.1");
			this.Encoding = System.Text.Encoding.ASCII;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Message identity
		/// </summary>
		/// <remarks>
		/// <para>
		/// An identifier is specified with each log message. This can be specified
		/// by setting the <see cref="Identity"/> property. The identity (also know 
		/// as the tag) must not contain white space. The default value for the
		/// identity is the application name (from <see cref="LoggingEvent.Domain"/>).
		/// </para>
		/// </remarks>
		public PatternLayout Identity
		{
			get { return m_identity; }
			set { m_identity = value; }
		}

		/// <summary>
		/// Syslog facility
		/// </summary>
		/// <remarks>
		/// Set to one of the <see cref="SyslogFacility"/> values. The list of
		/// facilities is predefined and cannot be extended. The default value
		/// is <see cref="SyslogFacility.User"/>.
		/// </remarks>
		public SyslogFacility Facility
		{
			get { return m_facility; }
			set { m_facility = value; }
		}

		#endregion Public Instance Properties

		/// <summary>
		/// Add a mapping of level to severity
		/// </summary>
		/// <param name="mapping">The mapping to add</param>
		/// <remarks>
		/// <para>
		/// Add a <see cref="LevelSeverity"/> mapping to this appender.
		/// </para>
		/// </remarks>
		public void AddMapping(LevelSeverity mapping)
		{
			m_levelMapping.Add(mapping);
		}

		#region AppenderSkeleton Implementation

		/// <summary>
		/// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(LoggingEvent)"/> method.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Writes the event to a remote syslog daemon.
		/// </para>
		/// <para>
		/// The format of the output will depend on the appender's layout.
		/// </para>
		/// </remarks>
		protected override void Append(LoggingEvent loggingEvent)
		{
            try
            {
                // Priority
                int priority = GeneratePriority(m_facility, GetSeverity(loggingEvent.Level));

                // Identity
                string identity;

                if (m_identity != null)
                {
                    identity = m_identity.Format(loggingEvent);
                }
                else
                {
                    identity = loggingEvent.Domain;
                }

                // Message. The message goes after the tag/identity
                string message = RenderLoggingEvent(loggingEvent);

                Byte[] buffer;
                int i = 0;
                char c;

                StringBuilder builder = new StringBuilder();

                while (i < message.Length)
                {
                    // Clear StringBuilder
                    builder.Length = 0;

                    // Write priority
                    builder.Append('<');
                    builder.Append(priority);
                    builder.Append('>');

                    // Write identity
                    builder.Append(identity);
                    builder.Append(": ");

                    for (; i < message.Length; i++)
                    {
                        c = message[i];

                        // Accept only visible ASCII characters and space. See RFC 3164 section 4.1.3
                        if (((int)c >= 32) && ((int)c <= 126))
                        {
                            builder.Append(c);
                        }
                        // If character is newline, break and send the current line
                        else if ((c == '\r') || (c == '\n'))
                        {
                            // Check the next character to handle \r\n or \n\r
                            if ((message.Length > i + 1) && ((message[i + 1] == '\r') || (message[i + 1] == '\n')))
                            {
                                i++;
                            }
                            i++;
                            break;
                        }
                    }

                    // Grab as a byte array
                    buffer = this.Encoding.GetBytes(builder.ToString());

#if NETSTANDARD1_3
                    Client.SendAsync(buffer, buffer.Length, RemoteEndPoint).Wait();
#else
                    this.Client.Send(buffer, buffer.Length, this.RemoteEndPoint);
#endif
                }
            }
            catch (Exception e)
            {
                ErrorHandler.Error(
                    "Unable to send logging event to remote syslog " +
                    this.RemoteAddress.ToString() +
                    " on port " +
                    this.RemotePort + ".",
                    e,
                    ErrorCode.WriteFailure);
            }
		}

		/// <summary>
		/// Initialize the options for this appender
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initialize the level to syslog severity mappings set on this appender.
		/// </para>
		/// </remarks>
		public override void ActivateOptions()
		{
			base.ActivateOptions();
			m_levelMapping.ActivateOptions();
		}

		#endregion AppenderSkeleton Implementation

		#region Protected Members

		/// <summary>
		/// Translates a log4net level to a syslog severity.
		/// </summary>
		/// <param name="level">A log4net level.</param>
		/// <returns>A syslog severity.</returns>
		/// <remarks>
		/// <para>
		/// Translates a log4net level to a syslog severity.
		/// </para>
		/// </remarks>
		virtual protected SyslogSeverity GetSeverity(Level level)
		{
			LevelSeverity levelSeverity = m_levelMapping.Lookup(level) as LevelSeverity;
			if (levelSeverity != null)
			{
				return levelSeverity.Severity;
			}

			//
			// Fallback to sensible default values
			//

			if (level >= Level.Alert)
			{
				return SyslogSeverity.Alert;
			}
			else if (level >= Level.Critical)
			{
				return SyslogSeverity.Critical;
			}
			else if (level >= Level.Error)
			{
				return SyslogSeverity.Error;
			}
			else if (level >= Level.Warn)
			{
				return SyslogSeverity.Warning;
			}
			else if (level >= Level.Notice)
			{
				return SyslogSeverity.Notice;
			}
			else if (level >= Level.Info)
			{
				return SyslogSeverity.Informational;
			}
			// Default setting
			return SyslogSeverity.Debug;
		}

		#endregion Protected Members

		#region Public Static Members

		/// <summary>
		/// Generate a syslog priority.
		/// </summary>
		/// <param name="facility">The syslog facility.</param>
		/// <param name="severity">The syslog severity.</param>
		/// <returns>A syslog priority.</returns>
		/// <remarks>
		/// <para>
		/// Generate a syslog priority.
		/// </para>
		/// </remarks>
		public static int GeneratePriority(SyslogFacility facility, SyslogSeverity severity)
		{
			if (facility < SyslogFacility.Kernel || facility > SyslogFacility.Local7)
			{
				throw new ArgumentException("SyslogFacility out of range", "facility");
			}

			if (severity < SyslogSeverity.Emergency || severity > SyslogSeverity.Debug)
			{
				throw new ArgumentException("SyslogSeverity out of range", "severity");
			}

			unchecked
			{
				return ((int)facility * 8) + (int)severity;
			}
		}

		#endregion Public Static Members

		#region Private Instances Fields

		/// <summary>
		/// The facility. The default facility is <see cref="SyslogFacility.User"/>.
		/// </summary>
		private SyslogFacility m_facility = SyslogFacility.User;

		/// <summary>
		/// The message identity
		/// </summary>
		private PatternLayout m_identity;

		/// <summary>
		/// Mapping from level object to syslog severity
		/// </summary>
		private LevelMapping m_levelMapping = new LevelMapping();

		/// <summary>
		/// Initial buffer size
		/// </summary>
		private const int c_renderBufferSize = 256;

		/// <summary>
		/// Maximum buffer size before it is recycled
		/// </summary>
		private const int c_renderBufferMaxCapacity = 1024;

		#endregion Private Instances Fields

		#region LevelSeverity LevelMapping Entry
		/// <summary>
		/// A class to act as a mapping between the level that a logging call is made at and
		/// the syslog severity that is should be logged at.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A class to act as a mapping between the level that a logging call is made at and
		/// the syslog severity that is should be logged at.
		/// </para>
		/// </remarks>
		public class LevelSeverity : LevelMappingEntry
		{
			private SyslogSeverity m_severity;

			/// <summary>
			/// The mapped syslog severity for the specified level
			/// </summary>
			/// <remarks>
			/// <para>
			/// Required property.
			/// The mapped syslog severity for the specified level
			/// </para>
			/// </remarks>
			public SyslogSeverity Severity
			{
				get { return m_severity; }
				set { m_severity = value; }
			}
		}

		#endregion // LevelSeverity LevelMapping Entry
	}
}
