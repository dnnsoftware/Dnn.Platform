// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

// MONO 1.0 Beta mcs does not like #if !A && !B && !C syntax

// .NET Compact Framework 1.0 has no support for Win32 Console API's
#if !NETCF
// .Mono 1.0 has no support for Win32 Console API's
#if !MONO
// SSCLI 1.0 has no support for Win32 Console API's
#if !SSCLI
// We don't want framework or platform specific code in the CLI version of log4net
#if !CLI_1_0

using System;
using System.Globalization;
using System.Runtime.InteropServices;

using log4net.Layout;
using log4net.Util;

namespace log4net.Appender
{
    /// <summary>
    /// Appends logging events to the console.
    /// </summary>
    /// <remarks>
    /// <para>
    /// ColoredConsoleAppender appends log events to the standard output stream
    /// or the error output stream using a layout specified by the
    /// user. It also allows the color of a specific type of message to be set.
    /// </para>
    /// <para>
    /// By default, all output is written to the console's standard output stream.
    /// The <see cref="Target"/> property can be set to direct the output to the
    /// error stream.
    /// </para>
    /// <para>
    /// NOTE: This appender writes directly to the application's attached console
    /// not to the <c>System.Console.Out</c> or <c>System.Console.Error</c> <c>TextWriter</c>.
    /// The <c>System.Console.Out</c> and <c>System.Console.Error</c> streams can be
    /// programmatically redirected (for example NUnit does this to capture program output).
    /// This appender will ignore these redirections because it needs to use Win32
    /// API calls to colorize the output. To respect these redirections the <see cref="ConsoleAppender"/>
    /// must be used.
    /// </para>
    /// <para>
    /// When configuring the colored console appender, mapping should be
    /// specified to map a logging level to a color. For example:
    /// </para>
    /// <code lang="XML" escaped="true">
    /// <mapping>
    ///     <level value="ERROR" />
    ///     <foreColor value="White" />
    ///     <backColor value="Red, HighIntensity" />
    /// </mapping>
    /// <mapping>
    ///     <level value="DEBUG" />
    ///     <backColor value="Green" />
    /// </mapping>
    /// </code>
    /// <para>
    /// The Level is the standard log4net logging level and ForeColor and BackColor can be any
    /// combination of the following values:
    /// <list type="bullet">
    /// <item><term>Blue</term><description></description></item>
    /// <item><term>Green</term><description></description></item>
    /// <item><term>Red</term><description></description></item>
    /// <item><term>White</term><description></description></item>
    /// <item><term>Yellow</term><description></description></item>
    /// <item><term>Purple</term><description></description></item>
    /// <item><term>Cyan</term><description></description></item>
    /// <item><term>HighIntensity</term><description></description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <author>Rick Hobbs.</author>
    /// <author>Nicko Cadell.</author>
    public class ColoredConsoleAppender : AppenderSkeleton
    {
        /// <summary>
        /// The enum of possible color values for use with the color mapping method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The following flags can be combined together to
        /// form the colors.
        /// </para>
        /// </remarks>
        /// <seealso cref="ColoredConsoleAppender" />
        [Flags]
        public enum Colors : int
        {
            /// <summary>
            /// color is blue
            /// </summary>
            Blue = 0x0001,

            /// <summary>
            /// color is green
            /// </summary>
            Green = 0x0002,

            /// <summary>
            /// color is red
            /// </summary>
            Red = 0x0004,

            /// <summary>
            /// color is white
            /// </summary>
            White = Blue | Green | Red,

            /// <summary>
            /// color is yellow
            /// </summary>
            Yellow = Red | Green,

            /// <summary>
            /// color is purple
            /// </summary>
            Purple = Red | Blue,

            /// <summary>
            /// color is cyan
            /// </summary>
            Cyan = Green | Blue,

            /// <summary>
            /// color is intensified
            /// </summary>
            HighIntensity = 0x0008,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleAppender" /> class.
        /// </summary>
        /// <remarks>
        /// The instance of the <see cref="ColoredConsoleAppender" /> class is set up to write
        /// to the standard output stream.
        /// </remarks>
        public ColoredConsoleAppender()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleAppender" /> class
        /// with the specified layout.
        /// </summary>
        /// <param name="layout">the layout to use for this appender.</param>
        /// <remarks>
        /// The instance of the <see cref="ColoredConsoleAppender" /> class is set up to write
        /// to the standard output stream.
        /// </remarks>
        [Obsolete("Instead use the default constructor and set the Layout property. Scheduled removal in v10.0.0.")]
        public ColoredConsoleAppender(ILayout layout)
            : this(layout, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColoredConsoleAppender" /> class
        /// with the specified layout.
        /// </summary>
        /// <param name="layout">the layout to use for this appender.</param>
        /// <param name="writeToErrorStream">flag set to <c>true</c> to write to the console error stream.</param>
        /// <remarks>
        /// When <paramref name="writeToErrorStream" /> is set to <c>true</c>, output is written to
        /// the standard error output stream.  Otherwise, output is written to the standard
        /// output stream.
        /// </remarks>
        [Obsolete("Instead use the default constructor and set the Layout & Target properties. Scheduled removal in v10.0.0.")]
        public ColoredConsoleAppender(ILayout layout, bool writeToErrorStream)
        {
            this.Layout = layout;
            this.m_writeToErrorStream = writeToErrorStream;
        }

        /// <summary>
        /// Gets or sets target is the value of the console output stream.
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
        public virtual string Target
        {
            get { return this.m_writeToErrorStream ? ConsoleError : ConsoleOut; }

            set
            {
                string v = value.Trim();

                if (string.Compare(ConsoleError, v, true, CultureInfo.InvariantCulture) == 0)
                {
                    this.m_writeToErrorStream = true;
                }
                else
                {
                    this.m_writeToErrorStream = false;
                }
            }
        }

        /// <summary>
        /// Add a mapping of level to color - done by the config file.
        /// </summary>
        /// <param name="mapping">The mapping to add.</param>
        /// <remarks>
        /// <para>
        /// Add a <see cref="LevelColors"/> mapping to this appender.
        /// Each mapping defines the foreground and background colors
        /// for a level.
        /// </para>
        /// </remarks>
        public void AddMapping(LevelColors mapping)
        {
            this.m_levelMapping.Add(mapping);
        }

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
#if NET_4_0 || MONO_4_0
        [System.Security.SecuritySafeCritical]
#endif
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
        protected override void Append(log4net.Core.LoggingEvent loggingEvent)
        {
            if (this.m_consoleOutputWriter != null)
            {
                IntPtr consoleHandle = IntPtr.Zero;
                if (this.m_writeToErrorStream)
                {
                    // Write to the error stream
                    consoleHandle = GetStdHandle(STD_ERROR_HANDLE);
                }
                else
                {
                    // Write to the output stream
                    consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);
                }

                // Default to white on black
                ushort colorInfo = (ushort)Colors.White;

                // see if there is a specified lookup
                LevelColors levelColors = this.m_levelMapping.Lookup(loggingEvent.Level) as LevelColors;
                if (levelColors != null)
                {
                    colorInfo = levelColors.CombinedColor;
                }

                // Render the event to a string
                string strLoggingMessage = this.RenderLoggingEvent(loggingEvent);

                // get the current console color - to restore later
                CONSOLE_SCREEN_BUFFER_INFO bufferInfo;
                GetConsoleScreenBufferInfo(consoleHandle, out bufferInfo);

                // set the console colors
                SetConsoleTextAttribute(consoleHandle, colorInfo);

                // Using WriteConsoleW seems to be unreliable.
                // If a large buffer is written, say 15,000 chars
                // Followed by a larger buffer, say 20,000 chars
                // then WriteConsoleW will fail, last error 8
                // 'Not enough storage is available to process this command.'
                //
                // Although the documentation states that the buffer must
                // be less that 64KB (i.e. 32,000 WCHARs) the longest string
                // that I can write out a the first call to WriteConsoleW
                // is only 30,704 chars.
                //
                // Unlike the WriteFile API the WriteConsoleW method does not
                // seem to be able to partially write out from the input buffer.
                // It does have a lpNumberOfCharsWritten parameter, but this is
                // either the length of the input buffer if any output was written,
                // or 0 when an error occurs.
                //
                // All results above were observed on Windows XP SP1 running
                // .NET runtime 1.1 SP1.
                //
                // Old call to WriteConsoleW:
                //
                // WriteConsoleW(
                //     consoleHandle,
                //     strLoggingMessage,
                //     (UInt32)strLoggingMessage.Length,
                //     out (UInt32)ignoreWrittenCount,
                //     IntPtr.Zero);
                //
                // Instead of calling WriteConsoleW we use WriteFile which
                // handles large buffers correctly. Because WriteFile does not
                // handle the codepage conversion as WriteConsoleW does we
                // need to use a System.IO.StreamWriter with the appropriate
                // Encoding. The WriteFile calls are wrapped up in the
                // System.IO.__ConsoleStream internal class obtained through
                // the System.Console.OpenStandardOutput method.
                //
                // See the ActivateOptions method below for the code that
                // retrieves and wraps the stream.

                // The windows console uses ScrollConsoleScreenBuffer internally to
                // scroll the console buffer when the display buffer of the console
                // has been used up. ScrollConsoleScreenBuffer fills the area uncovered
                // by moving the current content with the background color
                // currently specified on the console. This means that it fills the
                // whole line in front of the cursor position with the current
                // background color.
                // This causes an issue when writing out text with a non default
                // background color. For example; We write a message with a Blue
                // background color and the scrollable area of the console is full.
                // When we write the newline at the end of the message the console
                // needs to scroll the buffer to make space available for the new line.
                // The ScrollConsoleScreenBuffer internals will fill the newly created
                // space with the current background color: Blue.
                // We then change the console color back to default (White text on a
                // Black background). We write some text to the console, the text is
                // written correctly in White with a Black background, however the
                // remainder of the line still has a Blue background.
                //
                // This causes a disjointed appearance to the output where the background
                // colors change.
                //
                // This can be remedied by restoring the console colors before causing
                // the buffer to scroll, i.e. before writing the last newline. This does
                // assume that the rendered message will end with a newline.
                //
                // Therefore we identify a trailing newline in the message and don't
                // write this to the output, then we restore the console color and write
                // a newline. Note that we must AutoFlush before we restore the console
                // color otherwise we will have no effect.
                //
                // There will still be a slight artefact for the last line of the message
                // will have the background extended to the end of the line, however this
                // is unlikely to cause any user issues.
                //
                // Note that none of the above is visible while the console buffer is scrollable
                // within the console window viewport, the effects only arise when the actual
                // buffer is full and needs to be scrolled.
                char[] messageCharArray = strLoggingMessage.ToCharArray();
                int arrayLength = messageCharArray.Length;
                bool appendNewline = false;

                // Trim off last newline, if it exists
                if (arrayLength > 1 && messageCharArray[arrayLength - 2] == '\r' && messageCharArray[arrayLength - 1] == '\n')
                {
                    arrayLength -= 2;
                    appendNewline = true;
                }

                // Write to the output stream
                this.m_consoleOutputWriter.Write(messageCharArray, 0, arrayLength);

                // Restore the console back to its previous color scheme
                SetConsoleTextAttribute(consoleHandle, bufferInfo.wAttributes);

                if (appendNewline)
                {
                    // Write the newline, after changing the color scheme
                    this.m_consoleOutputWriter.Write(s_windowsNewline, 0, 2);
                }
            }
        }

        private static readonly char[] s_windowsNewline = { '\r', '\n' };

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
        /// Initialize the options for this appender.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initialize the level to color mappings set on this appender.
        /// </para>
        /// </remarks>
#if NET_4_0 || MONO_4_0
        [System.Security.SecuritySafeCritical]
#endif
        [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand, UnmanagedCode = true)]
        public override void ActivateOptions()
        {
            base.ActivateOptions();
            this.m_levelMapping.ActivateOptions();

            System.IO.Stream consoleOutputStream = null;

            // Use the Console methods to open a Stream over the console std handle
            if (this.m_writeToErrorStream)
            {
                // Write to the error stream
                consoleOutputStream = Console.OpenStandardError();
            }
            else
            {
                // Write to the output stream
                consoleOutputStream = Console.OpenStandardOutput();
            }

            // Lookup the codepage encoding for the console
            System.Text.Encoding consoleEncoding = System.Text.Encoding.GetEncoding(GetConsoleOutputCP());

            // Create a writer around the console stream
            this.m_consoleOutputWriter = new System.IO.StreamWriter(consoleOutputStream, consoleEncoding, 0x100);

            this.m_consoleOutputWriter.AutoFlush = true;

            // SuppressFinalize on m_consoleOutputWriter because all it will do is flush
            // and close the file handle. Because we have set AutoFlush the additional flush
            // is not required. The console file handle should not be closed, so we don't call
            // Dispose, Close or the finalizer.
            GC.SuppressFinalize(this.m_consoleOutputWriter);
        }

        /// <summary>
        /// The <see cref="ColoredConsoleAppender.Target"/> to use when writing to the Console
        /// standard output stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ColoredConsoleAppender.Target"/> to use when writing to the Console
        /// standard output stream.
        /// </para>
        /// </remarks>
        public const string ConsoleOut = "Console.Out";

        /// <summary>
        /// The <see cref="ColoredConsoleAppender.Target"/> to use when writing to the Console
        /// standard error output stream.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="ColoredConsoleAppender.Target"/> to use when writing to the Console
        /// standard error output stream.
        /// </para>
        /// </remarks>
        public const string ConsoleError = "Console.Error";

        /// <summary>
        /// Flag to write output to the error stream rather than the standard output stream.
        /// </summary>
        private bool m_writeToErrorStream = false;

        /// <summary>
        /// Mapping from level object to color value.
        /// </summary>
        private LevelMapping m_levelMapping = new LevelMapping();

        /// <summary>
        /// The console output stream writer to write to.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This writer is not thread safe.
        /// </para>
        /// </remarks>
        private System.IO.StreamWriter m_consoleOutputWriter = null;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetConsoleOutputCP();

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetConsoleTextAttribute(
            IntPtr consoleHandle,
            ushort attributes);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetConsoleScreenBufferInfo(
            IntPtr consoleHandle,
            out CONSOLE_SCREEN_BUFFER_INFO bufferInfo);

        // [DllImport("Kernel32.dll", SetLastError=true, CharSet=CharSet.Unicode)]
        // private static extern bool WriteConsoleW(
        // IntPtr hConsoleHandle,
        // [MarshalAs(UnmanagedType.LPWStr)] string strBuffer,
        // UInt32 bufferLen,
        // out UInt32 written,
        // IntPtr reserved);

        // private const UInt32 STD_INPUT_HANDLE = unchecked((UInt32)(-10));
        private const uint STD_OUTPUT_HANDLE = unchecked((uint)(-11));
        private const uint STD_ERROR_HANDLE = unchecked((uint)(-12));

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetStdHandle(
            uint type);

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public ushort x;
            public ushort y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SMALL_RECT
        {
            public ushort Left;
            public ushort Top;
            public ushort Right;
            public ushort Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CONSOLE_SCREEN_BUFFER_INFO
        {
            public COORD dwSize;
            public COORD dwCursorPosition;
            public ushort wAttributes;
            public SMALL_RECT srWindow;
            public COORD dwMaximumWindowSize;
        }

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
            private Colors m_foreColor;
            private Colors m_backColor;
            private ushort m_combinedColor = 0;

            /// <summary>
            /// Gets or sets the mapped foreground color for the specified level.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Required property.
            /// The mapped foreground color for the specified level.
            /// </para>
            /// </remarks>
            public Colors ForeColor
            {
                get { return this.m_foreColor; }
                set { this.m_foreColor = value; }
            }

            /// <summary>
            /// Gets or sets the mapped background color for the specified level.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Required property.
            /// The mapped background color for the specified level.
            /// </para>
            /// </remarks>
            public Colors BackColor
            {
                get { return this.m_backColor; }
                set { this.m_backColor = value; }
            }

            /// <summary>
            /// Initialize the options for the object.
            /// </summary>
            /// <remarks>
            /// <para>
            /// Combine the <see cref="ForeColor"/> and <see cref="BackColor"/> together.
            /// </para>
            /// </remarks>
            public override void ActivateOptions()
            {
                base.ActivateOptions();
                this.m_combinedColor = (ushort)((int)this.m_foreColor + (((int)this.m_backColor) << 4));
            }

            /// <summary>
            /// Gets the combined <see cref="ForeColor"/> and <see cref="BackColor"/> suitable for
            /// setting the console color.
            /// </summary>
            internal ushort CombinedColor
            {
                get { return this.m_combinedColor; }
            }
        }
    }
}

#endif // !CLI_1_0
#endif // !SSCLI
#endif // !MONO
#endif // !NETCF
