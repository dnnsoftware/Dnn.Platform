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

// .NET Compact Framework 1.0 has no support for System.Web.Mail
// SSCLI 1.0 has no support for System.Web.Mail
#if !NETCF && !SSCLI

using System;
using System.IO;
using System.Text;

#if NET_2_0 || MONO_2_0
using System.Net.Mail;
#else
using System.Web.Mail;
#endif

using log4net.Layout;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender
{
	/// <summary>
	/// Send an e-mail when a specific logging event occurs, typically on errors 
	/// or fatal errors.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The number of logging events delivered in this e-mail depend on
	/// the value of <see cref="BufferingAppenderSkeleton.BufferSize"/> option. The
	/// <see cref="SmtpAppender"/> keeps only the last
	/// <see cref="BufferingAppenderSkeleton.BufferSize"/> logging events in its 
	/// cyclic buffer. This keeps memory requirements at a reasonable level while 
	/// still delivering useful application context.
	/// </para>
	/// <note type="caution">
	/// Authentication and setting the server Port are only available on the MS .NET 1.1 runtime.
	/// For these features to be enabled you need to ensure that you are using a version of
	/// the log4net assembly that is built against the MS .NET 1.1 framework and that you are
	/// running the your application on the MS .NET 1.1 runtime. On all other platforms only sending
	/// unauthenticated messages to a server listening on port 25 (the default) is supported.
	/// </note>
	/// <para>
	/// Authentication is supported by setting the <see cref="Authentication"/> property to
	/// either <see cref="SmtpAuthentication.Basic"/> or <see cref="SmtpAuthentication.Ntlm"/>.
	/// If using <see cref="SmtpAuthentication.Basic"/> authentication then the <see cref="Username"/>
	/// and <see cref="Password"/> properties must also be set.
	/// </para>
	/// <para>
	/// To set the SMTP server port use the <see cref="Port"/> property. The default port is 25.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class SmtpAppender : BufferingAppenderSkeleton
	{
		#region Public Instance Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor
		/// </para>
		/// </remarks>
		public SmtpAppender()
		{	
		}

		#endregion // Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets a comma- or semicolon-delimited list of recipient e-mail addresses (use semicolon on .NET 1.1 and comma for later versions).
		/// </summary>
		/// <value>
        /// <para>
        /// For .NET 1.1 (System.Web.Mail): A semicolon-delimited list of e-mail addresses.
        /// </para>
        /// <para>
        /// For .NET 2.0 (System.Net.Mail): A comma-delimited list of e-mail addresses.
        /// </para>
		/// </value>
		/// <remarks>
        /// <para>
        /// For .NET 1.1 (System.Web.Mail): A semicolon-delimited list of e-mail addresses.
        /// </para>
        /// <para>
        /// For .NET 2.0 (System.Net.Mail): A comma-delimited list of e-mail addresses.
        /// </para>
		/// </remarks>
		public string To
		{
			get { return m_to; }
			set { m_to = MaybeTrimSeparators(value); }
		}

        /// <summary>
        /// Gets or sets a comma- or semicolon-delimited list of recipient e-mail addresses 
        /// that will be carbon copied (use semicolon on .NET 1.1 and comma for later versions).
        /// </summary>
        /// <value>
        /// <para>
        /// For .NET 1.1 (System.Web.Mail): A semicolon-delimited list of e-mail addresses.
        /// </para>
        /// <para>
        /// For .NET 2.0 (System.Net.Mail): A comma-delimited list of e-mail addresses.
        /// </para>
        /// </value>
        /// <remarks>
        /// <para>
        /// For .NET 1.1 (System.Web.Mail): A semicolon-delimited list of e-mail addresses.
        /// </para>
        /// <para>
        /// For .NET 2.0 (System.Net.Mail): A comma-delimited list of e-mail addresses.
        /// </para>
        /// </remarks>
        public string Cc
        {
            get { return m_cc; }
            set { m_cc = MaybeTrimSeparators(value); }
        }

        /// <summary>
        /// Gets or sets a semicolon-delimited list of recipient e-mail addresses
        /// that will be blind carbon copied.
        /// </summary>
        /// <value>
        /// A semicolon-delimited list of e-mail addresses.
        /// </value>
        /// <remarks>
        /// <para>
        /// A semicolon-delimited list of recipient e-mail addresses.
        /// </para>
        /// </remarks>
        public string Bcc
        {
            get { return m_bcc; }
            set { m_bcc = MaybeTrimSeparators(value); }
        }

		/// <summary>
		/// Gets or sets the e-mail address of the sender.
		/// </summary>
		/// <value>
		/// The e-mail address of the sender.
		/// </value>
		/// <remarks>
		/// <para>
		/// The e-mail address of the sender.
		/// </para>
		/// </remarks>
		public string From 
		{
			get { return m_from; }
			set { m_from = value; }
		}

		/// <summary>
		/// Gets or sets the subject line of the e-mail message.
		/// </summary>
		/// <value>
		/// The subject line of the e-mail message.
		/// </value>
		/// <remarks>
		/// <para>
		/// The subject line of the e-mail message.
		/// </para>
		/// </remarks>
		public string Subject 
		{
			get { return m_subject; }
			set { m_subject = value; }
		}
  
		/// <summary>
		/// Gets or sets the name of the SMTP relay mail server to use to send 
		/// the e-mail messages.
		/// </summary>
		/// <value>
		/// The name of the e-mail relay server. If SmtpServer is not set, the 
		/// name of the local SMTP server is used.
		/// </value>
		/// <remarks>
		/// <para>
		/// The name of the e-mail relay server. If SmtpServer is not set, the 
		/// name of the local SMTP server is used.
		/// </para>
		/// </remarks>
		public string SmtpHost
		{
			get { return m_smtpHost; }
			set { m_smtpHost = value; }
		}

		/// <summary>
		/// Obsolete
		/// </summary>
		/// <remarks>
		/// Use the BufferingAppenderSkeleton Fix methods instead 
		/// </remarks>
		/// <remarks>
		/// <para>
		/// Obsolete property.
		/// </para>
		/// </remarks>
		[Obsolete("Use the BufferingAppenderSkeleton Fix methods")]
		public bool LocationInfo
		{
			get { return false; }
			set { ; }
		}

		/// <summary>
		/// The mode to use to authentication with the SMTP server
		/// </summary>
		/// <remarks>
		/// <note type="caution">Authentication is only available on the MS .NET 1.1 runtime.</note>
		/// <para>
		/// Valid Authentication mode values are: <see cref="SmtpAuthentication.None"/>, 
		/// <see cref="SmtpAuthentication.Basic"/>, and <see cref="SmtpAuthentication.Ntlm"/>. 
		/// The default value is <see cref="SmtpAuthentication.None"/>. When using 
		/// <see cref="SmtpAuthentication.Basic"/> you must specify the <see cref="Username"/> 
		/// and <see cref="Password"/> to use to authenticate.
		/// When using <see cref="SmtpAuthentication.Ntlm"/> the Windows credentials for the current
		/// thread, if impersonating, or the process will be used to authenticate. 
		/// </para>
		/// </remarks>
		public SmtpAuthentication Authentication
		{
			get { return m_authentication; }
			set { m_authentication = value; }
		}

		/// <summary>
		/// The username to use to authenticate with the SMTP server
		/// </summary>
		/// <remarks>
		/// <note type="caution">Authentication is only available on the MS .NET 1.1 runtime.</note>
		/// <para>
		/// A <see cref="Username"/> and <see cref="Password"/> must be specified when 
		/// <see cref="Authentication"/> is set to <see cref="SmtpAuthentication.Basic"/>, 
		/// otherwise the username will be ignored. 
		/// </para>
		/// </remarks>
		public string Username
		{
			get { return m_username; }
			set { m_username = value; }
		}

		/// <summary>
		/// The password to use to authenticate with the SMTP server
		/// </summary>
		/// <remarks>
		/// <note type="caution">Authentication is only available on the MS .NET 1.1 runtime.</note>
		/// <para>
		/// A <see cref="Username"/> and <see cref="Password"/> must be specified when 
		/// <see cref="Authentication"/> is set to <see cref="SmtpAuthentication.Basic"/>, 
		/// otherwise the password will be ignored. 
		/// </para>
		/// </remarks>
		public string Password
		{
			get { return m_password; }
			set { m_password = value; }
		}

		/// <summary>
		/// The port on which the SMTP server is listening
		/// </summary>
		/// <remarks>
		/// <note type="caution">Server Port is only available on the MS .NET 1.1 runtime.</note>
		/// <para>
		/// The port on which the SMTP server is listening. The default
		/// port is <c>25</c>. The Port can only be changed when running on
		/// the MS .NET 1.1 runtime.
		/// </para>
		/// </remarks>
		public int Port
		{
			get { return m_port; }
			set { m_port = value; }
		}

		/// <summary>
		/// Gets or sets the priority of the e-mail message
		/// </summary>
		/// <value>
		/// One of the <see cref="MailPriority"/> values.
		/// </value>
		/// <remarks>
		/// <para>
		/// Sets the priority of the e-mails generated by this
		/// appender. The default priority is <see cref="MailPriority.Normal"/>.
		/// </para>
		/// <para>
		/// If you are using this appender to report errors then
		/// you may want to set the priority to <see cref="MailPriority.High"/>.
		/// </para>
		/// </remarks>
		public MailPriority Priority
		{
			get { return m_mailPriority; }
			set { m_mailPriority = value; }
		}

#if NET_2_0 || MONO_2_0
        /// <summary>
        /// Enable or disable use of SSL when sending e-mail message
        /// </summary>
        /// <remarks>
        /// This is available on MS .NET 2.0 runtime and higher
        /// </remarks>
        public bool EnableSsl
        {
            get { return m_enableSsl; }
            set { m_enableSsl = value; }
        }

        /// <summary>
        /// Gets or sets the reply-to e-mail address.
        /// </summary>
        /// <remarks>
        /// This is available on MS .NET 2.0 runtime and higher
        /// </remarks>
        public string ReplyTo
        {
            get { return m_replyTo; }
            set { m_replyTo = value; }
        }
#endif

		/// <summary>
		/// Gets or sets the subject encoding to be used.
		/// </summary>
		/// <remarks>
		/// The default encoding is the operating system's current ANSI codepage.
		/// </remarks>
		public Encoding SubjectEncoding
		{
			get { return m_subjectEncoding; }
			set { m_subjectEncoding = value; }
		}

		/// <summary>
		/// Gets or sets the body encoding to be used.
		/// </summary>
		/// <remarks>
		/// The default encoding is the operating system's current ANSI codepage.
		/// </remarks>
		public Encoding BodyEncoding
		{
			get { return m_bodyEncoding; }
			set { m_bodyEncoding = value; }
		}

		#endregion // Public Instance Properties

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Sends the contents of the cyclic buffer as an e-mail message.
		/// </summary>
		/// <param name="events">The logging events to send.</param>
		override protected void SendBuffer(LoggingEvent[] events) 
		{
			// Note: this code already owns the monitor for this
			// appender. This frees us from needing to synchronize again.
			try 
			{	  
				StringWriter writer = new StringWriter(System.Globalization.CultureInfo.InvariantCulture);

				string t = Layout.Header;
				if (t != null)
				{
					writer.Write(t);
				}

				for(int i = 0; i < events.Length; i++) 
				{
					// Render the event and append the text to the buffer
					RenderLoggingEvent(writer, events[i]);
				}

				t = Layout.Footer;
				if (t != null)
				{
					writer.Write(t);
				}

				SendEmail(writer.ToString());
			} 
			catch(Exception e) 
			{
				ErrorHandler.Error("Error occurred while sending e-mail notification.", e);
			}
		}

		#endregion // Override implementation of BufferingAppenderSkeleton

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </summary>
		/// <value><c>true</c></value>
		/// <remarks>
		/// <para>
		/// This appender requires a <see cref="Layout"/> to be set.
		/// </para>
		/// </remarks>
		override protected bool RequiresLayout
		{
			get { return true; }
		}

		#endregion // Override implementation of AppenderSkeleton

		#region Protected Methods

		/// <summary>
		/// Send the email message
		/// </summary>
		/// <param name="messageBody">the body text to include in the mail</param>
		virtual protected void SendEmail(string messageBody)
		{
#if NET_2_0 || MONO_2_0
			// .NET 2.0 has a new API for SMTP email System.Net.Mail
			// This API supports credentials and multiple hosts correctly.
			// The old API is deprecated.

			// Create and configure the smtp client
			SmtpClient smtpClient = new SmtpClient();
			if (!String.IsNullOrEmpty(m_smtpHost))
			{
				smtpClient.Host = m_smtpHost;
			}
			smtpClient.Port = m_port;
			smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = m_enableSsl;

			if (m_authentication == SmtpAuthentication.Basic)
			{
				// Perform basic authentication
				smtpClient.Credentials = new System.Net.NetworkCredential(m_username, m_password);
			}
			else if (m_authentication == SmtpAuthentication.Ntlm)
			{
				// Perform integrated authentication (NTLM)
				smtpClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
			}

            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.Body = messageBody;
				mailMessage.BodyEncoding = m_bodyEncoding;
                mailMessage.From = new MailAddress(m_from);
                mailMessage.To.Add(m_to);
                if (!String.IsNullOrEmpty(m_cc))
                {
                    mailMessage.CC.Add(m_cc);
                }
                if (!String.IsNullOrEmpty(m_bcc))
                {
                    mailMessage.Bcc.Add(m_bcc);
                }
                if (!String.IsNullOrEmpty(m_replyTo))
                {
                    // .NET 4.0 warning CS0618: 'System.Net.Mail.MailMessage.ReplyTo' is obsolete:
                    // 'ReplyTo is obsoleted for this type.  Please use ReplyToList instead which can accept multiple addresses. http://go.microsoft.com/fwlink/?linkid=14202'
#if !NET_4_0 && !MONO_4_0
                    mailMessage.ReplyTo = new MailAddress(m_replyTo);
#else
                    mailMessage.ReplyToList.Add(new MailAddress(m_replyTo));
#endif
                }
                mailMessage.Subject = m_subject;
				mailMessage.SubjectEncoding = m_subjectEncoding;
                mailMessage.Priority = m_mailPriority;

                // TODO: Consider using SendAsync to send the message without blocking. This would be a change in
                // behaviour compared to .NET 1.x. We would need a SendCompletedCallback to log errors.
                smtpClient.Send(mailMessage);
            }
#else
				// .NET 1.x uses the System.Web.Mail API for sending Mail

				MailMessage mailMessage = new MailMessage();
				mailMessage.Body = messageBody;
				mailMessage.BodyEncoding = m_bodyEncoding;
				mailMessage.From = m_from;
				mailMessage.To = m_to;
                if (m_cc != null && m_cc.Length > 0)
                {
                    mailMessage.Cc = m_cc;
                }
                if (m_bcc != null && m_bcc.Length > 0)
                {
                    mailMessage.Bcc = m_bcc;
                }
				mailMessage.Subject = m_subject;
#if !MONO && !NET_1_0 && !NET_1_1 && !CLI_1_0
				mailMessage.SubjectEncoding = m_subjectEncoding;
#endif
				mailMessage.Priority = m_mailPriority;

#if NET_1_1
				// The Fields property on the MailMessage allows the CDO properties to be set directly.
				// This property is only available on .NET Framework 1.1 and the implementation must understand
				// the CDO properties. For details of the fields available in CDO see:
				//
				// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cdosys/html/_cdosys_configuration_coclass.asp
				// 

				try
				{
					if (m_authentication == SmtpAuthentication.Basic)
					{
						// Perform basic authentication
						mailMessage.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", 1);
						mailMessage.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendusername", m_username);
						mailMessage.Fields.Add("http://schemas.microsoft.com/cdo/configuration/sendpassword", m_password);
					}
					else if (m_authentication == SmtpAuthentication.Ntlm)
					{
						// Perform integrated authentication (NTLM)
						mailMessage.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate", 2);
					}

					// Set the port if not the default value
					if (m_port != 25) 
					{
						mailMessage.Fields.Add("http://schemas.microsoft.com/cdo/configuration/smtpserverport", m_port);
					}
				}
				catch(MissingMethodException missingMethodException)
				{
					// If we were compiled against .NET 1.1 but are running against .NET 1.0 then
					// we will get a MissingMethodException when accessing the MailMessage.Fields property.

					ErrorHandler.Error("SmtpAppender: Authentication and server Port are only supported when running on the MS .NET 1.1 framework", missingMethodException);
				}
#else
				if (m_authentication != SmtpAuthentication.None)
				{
					ErrorHandler.Error("SmtpAppender: Authentication is only supported on the MS .NET 1.1 or MS .NET 2.0 builds of log4net");
				}

				if (m_port != 25)
				{
					ErrorHandler.Error("SmtpAppender: Server Port is only supported on the MS .NET 1.1 or MS .NET 2.0 builds of log4net");
				}
#endif // if NET_1_1

				if (m_smtpHost != null && m_smtpHost.Length > 0)
				{
					SmtpMail.SmtpServer = m_smtpHost;
				}

				SmtpMail.Send(mailMessage);
#endif // if NET_2_0
		}

		#endregion // Protected Methods

		#region Private Instance Fields

		private string m_to;
        private string m_cc;
        private string m_bcc;
		private string m_from;
		private string m_subject;
		private string m_smtpHost;
		private Encoding m_subjectEncoding = Encoding.UTF8;
		private Encoding m_bodyEncoding = Encoding.UTF8;

		// authentication fields
		private SmtpAuthentication m_authentication = SmtpAuthentication.None;
		private string m_username;
		private string m_password;

		// server port, default port 25
		private int m_port = 25;

		private MailPriority m_mailPriority = MailPriority.Normal;

#if NET_2_0 || MONO_2_0
        private bool m_enableSsl = false;
        private string m_replyTo;
#endif

		#endregion // Private Instance Fields

		#region SmtpAuthentication Enum

		/// <summary>
		/// Values for the <see cref="SmtpAppender.Authentication"/> property.
		/// </summary>
		/// <remarks>
		/// <para>
		/// SMTP authentication modes.
		/// </para>
		/// </remarks>
		public enum SmtpAuthentication
		{
			/// <summary>
			/// No authentication
			/// </summary>
			None,

			/// <summary>
			/// Basic authentication.
			/// </summary>
			/// <remarks>
			/// Requires a username and password to be supplied
			/// </remarks>
			Basic,

			/// <summary>
			/// Integrated authentication
			/// </summary>
			/// <remarks>
			/// Uses the Windows credentials from the current thread or process to authenticate.
			/// </remarks>
			Ntlm
		}

		#endregion // SmtpAuthentication Enum

            private static readonly char[] ADDRESS_DELIMITERS = new char[] { ',', ';' };
            
            /// <summary>
            ///   trims leading and trailing commas or semicolons
            /// </summary>
            private static string MaybeTrimSeparators(string s) {
#if NET_2_0 || MONO_2_0
                return string.IsNullOrEmpty(s) ? s : s.Trim(ADDRESS_DELIMITERS);
#else
                return s != null && s.Length > 0 ? s : s.Trim(ADDRESS_DELIMITERS);
#endif
            }
        }
}

#endif // !NETCF && !SSCLI
