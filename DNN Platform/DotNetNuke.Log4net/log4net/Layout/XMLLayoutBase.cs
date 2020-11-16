// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Layout
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
    using System.Xml;

    using log4net.Core;
    using log4net.Util;

    /// <summary>
    /// Layout that formats the log events as XML elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an abstract class that must be subclassed by an implementation
    /// to conform to a specific schema.
    /// </para>
    /// <para>
    /// Deriving classes must implement the <see cref="FormatXml"/> method.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public abstract class XmlLayoutBase : LayoutSkeleton
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayoutBase"/> class.
        /// Protected constructor to support subclasses.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="XmlLayoutBase" /> class
        /// with no location info.
        /// </para>
        /// </remarks>
        protected XmlLayoutBase()
            : this(false)
        {
            this.IgnoresException = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlLayoutBase"/> class.
        /// Protected constructor to support subclasses.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <paramref name="locationInfo" /> parameter determines whether
        /// location information will be output by the layout. If
        /// <paramref name="locationInfo" /> is set to <c>true</c>, then the
        /// file name and line number of the statement at the origin of the log
        /// statement will be output.
        /// </para>
        /// <para>
        /// If you are embedding this layout within an SMTPAppender
        /// then make sure to set the <b>LocationInfo</b> option of that
        /// appender as well.
        /// </para>
        /// </remarks>
        protected XmlLayoutBase(bool locationInfo)
        {
            this.IgnoresException = false;
            this.m_locationInfo = locationInfo;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include location information in
        /// the XML events.
        /// </summary>
        /// <value>
        /// <c>true</c> if location information should be included in the XML
        /// events; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// If <see cref="LocationInfo" /> is set to <c>true</c>, then the file
        /// name and line number of the statement at the origin of the log
        /// statement will be output.
        /// </para>
        /// <para>
        /// If you are embedding this layout within an <c>SMTPAppender</c>
        /// then make sure to set the <b>LocationInfo</b> option of that
        /// appender as well.
        /// </para>
        /// </remarks>
        public bool LocationInfo
        {
            get { return this.m_locationInfo; }
            set { this.m_locationInfo = value; }
        }

        /// <summary>
        /// Gets or sets the string to replace characters that can not be expressed in XML with.
        /// <remarks>
        /// <para>
        /// Not all characters may be expressed in XML. This property contains the
        /// string to replace those that can not with. This defaults to a ?. Set it
        /// to the empty string to simply remove offending characters. For more
        /// details on the allowed character ranges see http://www.w3.org/TR/REC-xml/#charsets
        /// Character replacement will occur in  the log message, the property names
        /// and the property values.
        /// </para>
        /// </remarks>
        /// </summary>
        public string InvalidCharReplacement
        {
            get { return this.m_invalidCharReplacement; }
            set { this.m_invalidCharReplacement = value; }
        }

        /// <summary>
        /// Initialize layout options.
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
            // nothing to do
        }

        /// <summary>
        /// Gets the content type output by this layout.
        /// </summary>
        /// <value>
        /// As this is the XML layout, the value is always <c>"text/xml"</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// As this is the XML layout, the value is always <c>"text/xml"</c>.
        /// </para>
        /// </remarks>
        public override string ContentType
        {
            get { return "text/xml"; }
        }

        /// <summary>
        /// Produces a formatted string.
        /// </summary>
        /// <param name="loggingEvent">The event being logged.</param>
        /// <param name="writer">The TextWriter to write the formatted event to.</param>
        /// <remarks>
        /// <para>
        /// Format the <see cref="LoggingEvent"/> and write it to the <see cref="TextWriter"/>.
        /// </para>
        /// <para>
        /// This method creates an <see cref="XmlTextWriter"/> that writes to the
        /// <paramref name="writer"/>. The <see cref="XmlTextWriter"/> is passed
        /// to the <see cref="FormatXml"/> method. Subclasses should override the
        /// <see cref="FormatXml"/> method rather than this method.
        /// </para>
        /// </remarks>
        public override void Format(TextWriter writer, LoggingEvent loggingEvent)
        {
            if (loggingEvent == null)
            {
                throw new ArgumentNullException("loggingEvent");
            }
#if NETSTANDARD1_3
			var settings = new XmlWriterSettings
			{
				Indent = false,
				OmitXmlDeclaration = true
			};
			var xmlWriter = XmlWriter.Create(new ProtectCloseTextWriter(writer), settings);
#else
            XmlTextWriter xmlWriter = new XmlTextWriter(new ProtectCloseTextWriter(writer));
            xmlWriter.Formatting = Formatting.None;
            xmlWriter.Namespaces = false;
#endif

            // Write the event to the writer
            this.FormatXml(xmlWriter, loggingEvent);

            xmlWriter.WriteWhitespace(SystemInfo.NewLine);

            // Close on xmlWriter will ensure xml is flushed
            // the protected writer will ignore the actual close
            xmlWriter.Close();
        }

        /// <summary>
        /// Does the actual writing of the XML.
        /// </summary>
        /// <param name="writer">The writer to use to output the event to.</param>
        /// <param name="loggingEvent">The event to write.</param>
        /// <remarks>
        /// <para>
        /// Subclasses should override this method to format
        /// the <see cref="LoggingEvent"/> as XML.
        /// </para>
        /// </remarks>
        protected abstract void FormatXml(XmlWriter writer, LoggingEvent loggingEvent);

        /// <summary>
        /// Flag to indicate if location information should be included in
        /// the XML events.
        /// </summary>
        private bool m_locationInfo = false;

        /// <summary>
        /// The string to replace invalid chars with.
        /// </summary>
        private string m_invalidCharReplacement = "?";
    }
}
