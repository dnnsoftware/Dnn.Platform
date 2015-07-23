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
using System.Text;
using System.IO;

using log4net.Layout;
using log4net.Core;
using log4net.Util;

namespace log4net.Appender
{
	/// <summary>
	/// Send an email when a specific logging event occurs, typically on errors 
	/// or fatal errors. Rather than sending via smtp it writes a file into the
	/// directory specified by <see cref="PickupDir"/>. This allows services such
	/// as the IIS SMTP agent to manage sending the messages.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The configuration for this appender is identical to that of the <c>SMTPAppender</c>,
	/// except that instead of specifying the <c>SMTPAppender.SMTPHost</c> you specify
	/// <see cref="PickupDir"/>.
	/// </para>
	/// <para>
	/// The number of logging events delivered in this e-mail depend on
	/// the value of <see cref="BufferingAppenderSkeleton.BufferSize"/> option. The
	/// <see cref="SmtpPickupDirAppender"/> keeps only the last
	/// <see cref="BufferingAppenderSkeleton.BufferSize"/> logging events in its 
	/// cyclic buffer. This keeps memory requirements at a reasonable level while 
	/// still delivering useful application context.
	/// </para>
	/// </remarks>
	/// <author>Niall Daley</author>
	/// <author>Nicko Cadell</author>
	public class SmtpPickupDirAppender : BufferingAppenderSkeleton
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
		public SmtpPickupDirAppender()
		{
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets a semicolon-delimited list of recipient e-mail addresses.
		/// </summary>
		/// <value>
		/// A semicolon-delimited list of e-mail addresses.
		/// </value>
		/// <remarks>
		/// <para>
		/// A semicolon-delimited list of e-mail addresses.
		/// </para>
		/// </remarks>
		public string To 
		{
			get { return m_to; }
			set { m_to = value; }
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
		/// Gets or sets the path to write the messages to.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Gets or sets the path to write the messages to. This should be the same
		/// as that used by the agent sending the messages.
		/// </para>
		/// </remarks>
		public string PickupDir
		{
			get { return m_pickupDir; }
			set { m_pickupDir = value; }
		}

		/// <summary>
		/// Gets or sets the <see cref="SecurityContext"/> used to write to the pickup directory.
		/// </summary>
		/// <value>
		/// The <see cref="SecurityContext"/> used to write to the pickup directory.
		/// </value>
		/// <remarks>
		/// <para>
		/// Unless a <see cref="SecurityContext"/> specified here for this appender
		/// the <see cref="SecurityContextProvider.DefaultProvider"/> is queried for the
		/// security context to use. The default behavior is to use the security context
		/// of the current thread.
		/// </para>
		/// </remarks>
		public SecurityContext SecurityContext 
		{
			get { return m_securityContext; }
			set { m_securityContext = value; }
		}

		#endregion Public Instance Properties

		#region Override implementation of BufferingAppenderSkeleton

		/// <summary>
		/// Sends the contents of the cyclic buffer as an e-mail message.
		/// </summary>
		/// <param name="events">The logging events to send.</param>
		/// <remarks>
		/// <para>
		/// Sends the contents of the cyclic buffer as an e-mail message.
		/// </para>
		/// </remarks>
		override protected void SendBuffer(LoggingEvent[] events) 
		{
			// Note: this code already owns the monitor for this
			// appender. This frees us from needing to synchronize again.
			try 
			{
				string filePath = null;
				StreamWriter writer = null;

				// Impersonate to open the file
				using(SecurityContext.Impersonate(this))
				{
					filePath = Path.Combine(m_pickupDir, SystemInfo.NewGuid().ToString("N"));
					writer = File.CreateText(filePath);
				}

				if (writer == null)
				{
					ErrorHandler.Error("Failed to create output file for writing ["+filePath+"]", null, ErrorCode.FileOpenFailure);
				}
				else
				{
					using(writer)
					{
						writer.WriteLine("To: " + m_to);
						writer.WriteLine("From: " + m_from);
						writer.WriteLine("Subject: " + m_subject);
						writer.WriteLine("");

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

						writer.WriteLine("");
						writer.WriteLine(".");
					}
				}
			} 
			catch(Exception e) 
			{
				ErrorHandler.Error("Error occurred while sending e-mail notification.", e);
			}
		}

		#endregion Override implementation of BufferingAppenderSkeleton

		#region Override implementation of AppenderSkeleton

		/// <summary>
		/// Activate the options on this appender. 
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is part of the <see cref="IOptionHandler"/> delayed object
		/// activation scheme. The <see cref="ActivateOptions"/> method must 
		/// be called on this object after the configuration properties have
		/// been set. Until <see cref="ActivateOptions"/> is called this
		/// object is in an undefined state and must not be used. 
		/// </para>
		/// <para>
		/// If any of the configuration properties are modified then 
		/// <see cref="ActivateOptions"/> must be called again.
		/// </para>
		/// </remarks>
		override public void ActivateOptions() 
		{	
			base.ActivateOptions();

			if (m_securityContext == null)
			{
				m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
			}

			using(SecurityContext.Impersonate(this))
			{
				m_pickupDir = ConvertToFullPath(m_pickupDir.Trim());
			}
		}

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

		#endregion Override implementation of AppenderSkeleton

		#region Protected Static Methods

		/// <summary>
		/// Convert a path into a fully qualified path.
		/// </summary>
		/// <param name="path">The path to convert.</param>
		/// <returns>The fully qualified path.</returns>
		/// <remarks>
		/// <para>
		/// Converts the path specified to a fully
		/// qualified path. If the path is relative it is
		/// taken as relative from the application base 
		/// directory.
		/// </para>
		/// </remarks>
		protected static string ConvertToFullPath(string path)
		{
			return SystemInfo.ConvertToFullPath(path);
		}

		#endregion Protected Static Methods

		#region Private Instance Fields

		private string m_to;
		private string m_from;
		private string m_subject;
		private string m_pickupDir;

		/// <summary>
		/// The security context to use for privileged calls
		/// </summary>
		private SecurityContext m_securityContext;

		#endregion Private Instance Fields
	}
}
