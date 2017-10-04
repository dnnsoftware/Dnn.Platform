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
using System.IO;

using log4net;
using log4net.Core;

namespace log4net.Layout
{
	/// <summary>
	/// Interface implemented by layout objects
	/// </summary>
	/// <remarks>
	/// <para>
	/// An <see cref="ILayout"/> object is used to format a <see cref="LoggingEvent"/>
	/// as text. The <see cref="M:Format(TextWriter,LoggingEvent)"/> method is called by an
	/// appender to transform the <see cref="LoggingEvent"/> into a string.
	/// </para>
	/// <para>
	/// The layout can also supply <see cref="Header"/> and <see cref="Footer"/>
	/// text that is appender before any events and after all the events respectively.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface ILayout
	{
		/// <summary>
		/// Implement this method to create your own layout format.
		/// </summary>
		/// <param name="writer">The TextWriter to write the formatted event to</param>
		/// <param name="loggingEvent">The event to format</param>
		/// <remarks>
		/// <para>
		/// This method is called by an appender to format
		/// the <paramref name="loggingEvent"/> as text and output to a writer.
		/// </para>
		/// <para>
		/// If the caller does not have a <see cref="TextWriter"/> and prefers the
		/// event to be formatted as a <see cref="String"/> then the following
		/// code can be used to format the event into a <see cref="StringWriter"/>.
		/// </para>
		/// <code lang="C#">
		/// StringWriter writer = new StringWriter();
		/// Layout.Format(writer, loggingEvent);
		/// string formattedEvent = writer.ToString();
		/// </code>
		/// </remarks>
		void Format(TextWriter writer, LoggingEvent loggingEvent);

		/// <summary>
		/// The content type output by this layout. 
		/// </summary>
		/// <value>The content type</value>
		/// <remarks>
		/// <para>
		/// The content type output by this layout.
		/// </para>
		/// <para>
		/// This is a MIME type e.g. <c>"text/plain"</c>.
		/// </para>
		/// </remarks>
		string ContentType { get; }

		/// <summary>
		/// The header for the layout format.
		/// </summary>
		/// <value>the layout header</value>
		/// <remarks>
		/// <para>
		/// The Header text will be appended before any logging events
		/// are formatted and appended.
		/// </para>
		/// </remarks>
		string Header { get; }

		/// <summary>
		/// The footer for the layout format.
		/// </summary>
		/// <value>the layout footer</value>
		/// <remarks>
		/// <para>
		/// The Footer text will be appended after all the logging events
		/// have been formatted and appended.
		/// </para>
		/// </remarks>
		string Footer { get; }

		/// <summary>
		/// Flag indicating if this layout handle exceptions
		/// </summary>
		/// <value><c>false</c> if this layout handles exceptions</value>
		/// <remarks>
		/// <para>
		/// If this layout handles the exception object contained within
		/// <see cref="LoggingEvent"/>, then the layout should return
		/// <c>false</c>. Otherwise, if the layout ignores the exception
		/// object, then the layout should return <c>true</c>.
		/// </para>
		/// </remarks>
		bool IgnoresException { get; }
	}
}
