// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Appender
{
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
    using System;
    using System.IO;
    using System.Text;

    using log4net.Core;
    using log4net.Layout;
    using log4net.Util;

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
    /// <author>Niall Daley.</author>
    /// <author>Nicko Cadell.</author>
    public class SmtpPickupDirAppender : BufferingAppenderSkeleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpPickupDirAppender"/> class.
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default constructor.
        /// </para>
        /// </remarks>
        public SmtpPickupDirAppender()
        {
            this.m_fileExtension = string.Empty; // Default to empty string, not null
        }

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
            get { return this.m_to; }
            set { this.m_to = value; }
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
            get { return this.m_from; }
            set { this.m_from = value; }
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
            get { return this.m_subject; }
            set { this.m_subject = value; }
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
            get { return this.m_pickupDir; }
            set { this.m_pickupDir = value; }
        }

        /// <summary>
        /// Gets or sets the file extension for the generated files.
        /// </summary>
        /// <value>
        /// The file extension for the generated files.
        /// </value>
        /// <remarks>
        /// <para>
        /// The file extension for the generated files.
        /// </para>
        /// </remarks>
        public string FileExtension
        {
            get { return this.m_fileExtension; }

            set
            {
                this.m_fileExtension = value;
                if (this.m_fileExtension == null)
                {
                    this.m_fileExtension = string.Empty;
                }

                // Make sure any non empty extension starts with a dot
#if NET_2_0 || MONO_2_0
                if (!string.IsNullOrEmpty(this.m_fileExtension) && !this.m_fileExtension.StartsWith("."))
#else
				if (m_fileExtension != null && m_fileExtension.Length > 0 && !m_fileExtension.StartsWith("."))
#endif
                {
                    this.m_fileExtension = "." + this.m_fileExtension;
                }
            }
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
            get { return this.m_securityContext; }
            set { this.m_securityContext = value; }
        }

        /// <summary>
        /// Sends the contents of the cyclic buffer as an e-mail message.
        /// </summary>
        /// <param name="events">The logging events to send.</param>
        /// <remarks>
        /// <para>
        /// Sends the contents of the cyclic buffer as an e-mail message.
        /// </para>
        /// </remarks>
        protected override void SendBuffer(LoggingEvent[] events)
        {
            // Note: this code already owns the monitor for this
            // appender. This frees us from needing to synchronize again.
            try
            {
                string filePath = null;
                StreamWriter writer = null;

                // Impersonate to open the file
                using (this.SecurityContext.Impersonate(this))
                {
                    filePath = Path.Combine(this.m_pickupDir, SystemInfo.NewGuid().ToString("N") + this.m_fileExtension);
                    writer = File.CreateText(filePath);
                }

                if (writer == null)
                {
                    this.ErrorHandler.Error("Failed to create output file for writing [" + filePath + "]", null, ErrorCode.FileOpenFailure);
                }
                else
                {
                    using (writer)
                    {
                        writer.WriteLine("To: " + this.m_to);
                        writer.WriteLine("From: " + this.m_from);
                        writer.WriteLine("Subject: " + this.m_subject);
                        writer.WriteLine("Date: " + DateTime.UtcNow.ToString("r"));
                        writer.WriteLine(string.Empty);

                        string t = this.Layout.Header;
                        if (t != null)
                        {
                            writer.Write(t);
                        }

                        for (int i = 0; i < events.Length; i++)
                        {
                            // Render the event and append the text to the buffer
                            this.RenderLoggingEvent(writer, events[i]);
                        }

                        t = this.Layout.Footer;
                        if (t != null)
                        {
                            writer.Write(t);
                        }

                        writer.WriteLine(string.Empty);
                        writer.WriteLine(".");
                    }
                }
            }
            catch (Exception e)
            {
                this.ErrorHandler.Error("Error occurred while sending e-mail notification.", e);
            }
        }

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
        public override void ActivateOptions()
        {
            base.ActivateOptions();

            if (this.m_securityContext == null)
            {
                this.m_securityContext = SecurityContextProvider.DefaultProvider.CreateSecurityContext(this);
            }

            using (this.SecurityContext.Impersonate(this))
            {
                this.m_pickupDir = ConvertToFullPath(this.m_pickupDir.Trim());
            }
        }

        /// <summary>
        /// Gets a value indicating whether this appender requires a <see cref="Layout"/> to be set.
        /// </summary>
        /// <value><c>true</c>.</value>
        /// <remarks>
        /// <para>
        /// This appender requires a <see cref="Layout"/> to be set.
        /// </para>
        /// </remarks>
        protected override bool RequiresLayout
        {
            get { return true; }
        }

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

        private string m_to;
        private string m_from;
        private string m_subject;
        private string m_pickupDir;
        private string m_fileExtension;

        /// <summary>
        /// The security context to use for privileged calls.
        /// </summary>
        private SecurityContext m_securityContext;
    }
}
