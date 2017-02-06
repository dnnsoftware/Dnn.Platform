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

// Compatibility:
//	http://msdn.microsoft.com/en-us/library/system.console.foregroundcolor.aspx
// Disable for unsupported targets
#if !NETCF 
#if !SSCLI
#if !CLI_1_0
#if !MONO_1_0
#if !NET_1_0
#if !NET_1_1

// The original ColoredConsoleAppender was written before the .NET framework
// (and Mono) had built-in support for console colors so it was written using
// Win32 API calls. The AnsiColorTerminalAppender, while it works, isn't
// understood by the Windows command prompt.
// This is a replacement for both that uses the new (.NET 2) Console colors
// and works on both platforms.

// On Mono/Linux (at least), setting the background color to 'Black' is
// not the same as the default background color, as it is after
// Console.Reset(). The difference becomes apparent while running in a
// terminal application that supports background transparency; the
// default color is treated as transparent while 'Black' isn't.
// For this reason, we always reset the colors and only set those
// explicitly specified in the configuration (Console.BackgroundColor
// isn't set if ommited).

using System;
using log4net.Util;

namespace log4net.Appender
{
	/// <summary>
	/// Appends colorful logging events to the console, using the .NET 2
	/// built-in capabilities.
	/// </summary>
	/// <remarks>
	/// <para>
	/// ManagedColoredConsoleAppender appends log events to the standard output stream
	/// or the error output stream using a layout specified by the 
	/// user. It also allows the color of a specific type of message to be set.
	/// </para>
	/// <para>
	/// By default, all output is written to the console's standard output stream.
	/// The <see cref="Target"/> property can be set to direct the output to the
	/// error stream.
	/// </para>
	/// <para>
	/// When configuring the colored console appender, mappings should be
	/// specified to map logging levels to colors. For example:
	/// </para>
	/// <code lang="XML" escaped="true">
	///	<mapping>
	///		<level value="ERROR" />
	///		<foreColor value="DarkRed" />
	///		<backColor value="White" />
	///	</mapping>
	///	<mapping>
	///		<level value="WARN" />
	///		<foreColor value="Yellow" />
	///	</mapping>
	///	<mapping>
	///		<level value="INFO" />
	///		<foreColor value="White" />
	///	</mapping>
	///	<mapping>
	///		<level value="DEBUG" />
	///		<foreColor value="Blue" />
	///	</mapping>
	/// </code>
	/// <para>
	/// The Level is the standard log4net logging level while
	/// ForeColor and BackColor are the values of <see cref="System.ConsoleColor"/>
	/// enumeration.
	/// </para>
	/// <para>
	/// Based on the ColoredConsoleAppender
	/// </para>
	/// </remarks>
	/// <author>Rick Hobbs</author>
	/// <author>Nicko Cadell</author>
	/// <author>Pavlos Touboulidis</author>
	public class ManagedColoredConsoleAppender: AppenderSkeleton
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ManagedColoredConsoleAppender" /> class.
		/// </summary>
		/// <remarks>
		/// The instance of the <see cref="ManagedColoredConsoleAppender" /> class is set up to write 
		/// to the standard output stream.
		/// </remarks>
		public ManagedColoredConsoleAppender() 
		{
		}
		
		#region Public Instance Properties
		/// <summary>
		/// Target is the value of the console output stream.
		/// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
		/// </summary>
		/// <value>
		/// Target is the value of the console output stream.
		/// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Target is the value of the console output stream.
		/// This is either <c>"Console.Out"</c> or <c>"Console.Error"</c>.
		/// </para>
		/// </remarks>
		virtual public string Target
		{
			get { return m_writeToErrorStream ? ConsoleError : ConsoleOut; }
			set
			{
				string v = value.Trim();

				if (SystemInfo.EqualsIgnoringCase(ConsoleError, v))
				{
					m_writeToErrorStream = true;
				} 
				else 
				{
					m_writeToErrorStream = false;
				}
			}
		}

		/// <summary>
		/// Add a mapping of level to color - done by the config file
		/// </summary>
		/// <param name="mapping">The mapping to add</param>
		/// <remarks>
		/// <para>
		/// Add a <see cref="LevelColors"/> mapping to this appender.
		/// Each mapping defines the foreground and background colors
		/// for a level.
		/// </para>
		/// </remarks>
		public void AddMapping(LevelColors mapping)
		{
			m_levelMapping.Add(mapping);
		}
		#endregion // Public Instance Properties

		#region Override implementation of AppenderSkeleton
		/// <summary>
		/// This method is called by the <see cref="M:AppenderSkeleton.DoAppend(log4net.Core.LoggingEvent)"/> method.
		/// </summary>
		/// <param name="loggingEvent">The event to log.</param>
		/// <remarks>
		/// <para>
		/// Writes the event to the console.
		/// </para>
		/// <para>
		/// The format of the output will depend on the appender's layout.
		/// </para>
		/// </remarks>
		override protected void Append(log4net.Core.LoggingEvent loggingEvent) 
		{
			System.IO.TextWriter writer;
			
			if (m_writeToErrorStream)
				writer = Console.Error;
			else
				writer = Console.Out;
			
			// Reset color
			Console.ResetColor();
			
			// see if there is a specified lookup
			LevelColors levelColors = m_levelMapping.Lookup(loggingEvent.Level) as LevelColors;
			if (levelColors != null)
			{
				// if the backColor has been explicitly set
				if (levelColors.HasBackColor)
					Console.BackgroundColor = levelColors.BackColor;
				// if the foreColor has been explicitly set
				if (levelColors.HasForeColor)
					Console.ForegroundColor = levelColors.ForeColor;
			}
			
			// Render the event to a string
			string strLoggingMessage = RenderLoggingEvent(loggingEvent);
			// and write it
			writer.Write(strLoggingMessage);

			// Reset color again
			Console.ResetColor();
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

		/// <summary>
		/// Initialize the options for this appender
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initialize the level to color mappings set on this appender.
		/// </para>
		/// </remarks>
		public override void ActivateOptions()
		{
			base.ActivateOptions();
			m_levelMapping.ActivateOptions();
		}
		#endregion // Override implementation of AppenderSkeleton

		#region Public Static Fields
		/// <summary>
		/// The <see cref="ManagedColoredConsoleAppender.Target"/> to use when writing to the Console 
		/// standard output stream.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <see cref="ManagedColoredConsoleAppender.Target"/> to use when writing to the Console 
		/// standard output stream.
		/// </para>
		/// </remarks>
		public const string ConsoleOut = "Console.Out";

		/// <summary>
		/// The <see cref="ManagedColoredConsoleAppender.Target"/> to use when writing to the Console 
		/// standard error output stream.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The <see cref="ManagedColoredConsoleAppender.Target"/> to use when writing to the Console 
		/// standard error output stream.
		/// </para>
		/// </remarks>
		public const string ConsoleError = "Console.Error";
		#endregion // Public Static Fields

		#region Private Instances Fields
		/// <summary>
		/// Flag to write output to the error stream rather than the standard output stream
		/// </summary>
		private bool m_writeToErrorStream = false;

		/// <summary>
		/// Mapping from level object to color value
		/// </summary>
		private LevelMapping m_levelMapping = new LevelMapping();
		#endregion // Private Instances Fields

		#region LevelColors LevelMapping Entry
		/// <summary>
		/// A class to act as a mapping between the level that a logging call is made at and
		/// the color it should be displayed as.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Defines the mapping between a level and the color it should be displayed in.
		/// </para>
		/// </remarks>
		public class LevelColors : LevelMappingEntry
		{
			/// <summary>
			/// The mapped foreground color for the specified level
			/// </summary>
			/// <remarks>
			/// <para>
			/// Required property.
			/// The mapped foreground color for the specified level.
			/// </para>
			/// </remarks>
			public ConsoleColor ForeColor
			{
				get { return (this.foreColor); }
				// Keep a flag that the color has been set
				// and is no longer the default.
				set { this.foreColor = value; this.hasForeColor = true; }
			}
			private ConsoleColor foreColor;
			private bool hasForeColor;
            internal bool HasForeColor {
                get {
                    return hasForeColor;
                }
            }

			/// <summary>
			/// The mapped background color for the specified level
			/// </summary>
			/// <remarks>
			/// <para>
			/// Required property.
			/// The mapped background color for the specified level.
			/// </para>
			/// </remarks>
			public ConsoleColor BackColor
			{
				get { return (this.backColor); }
				// Keep a flag that the color has been set
				// and is no longer the default.
				set { this.backColor = value; this.hasBackColor = true; }
			}
			private ConsoleColor backColor;
            private bool hasBackColor;
            internal bool HasBackColor {
                get {
                    return hasBackColor;
                }
            }
		}
		#endregion // LevelColors LevelMapping Entry
	}
}

#endif
#endif
#endif // !MONO_1_0
#endif // !CLI_1_0
#endif // !SSCLI
#endif // !NETCF
