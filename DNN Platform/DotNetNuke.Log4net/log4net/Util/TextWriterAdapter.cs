// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util
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
    using System.Globalization;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Adapter that extends <see cref="TextWriter"/> and forwards all
    /// messages to an instance of <see cref="TextWriter"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Adapter that extends <see cref="TextWriter"/> and forwards all
    /// messages to an instance of <see cref="TextWriter"/>.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public abstract class TextWriterAdapter : TextWriter
    {
        /// <summary>
        /// The writer to forward messages to.
        /// </summary>
        private TextWriter m_writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextWriterAdapter"/> class.
        /// Create an instance of <see cref="TextWriterAdapter"/> that forwards all
        /// messages to a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> to forward to.</param>
        /// <remarks>
        /// <para>
        /// Create an instance of <see cref="TextWriterAdapter"/> that forwards all
        /// messages to a <see cref="TextWriter"/>.
        /// </para>
        /// </remarks>
        protected TextWriterAdapter(TextWriter writer)
            : base(CultureInfo.InvariantCulture)
        {
            this.m_writer = writer;
        }

        /// <summary>
        /// Gets or sets the underlying <see cref="TextWriter" />.
        /// </summary>
        /// <value>
        /// The underlying <see cref="TextWriter" />.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets the underlying <see cref="TextWriter" />.
        /// </para>
        /// </remarks>
        protected TextWriter Writer
        {
            get { return this.m_writer; }
            set { this.m_writer = value; }
        }

        /// <summary>
        /// Gets the Encoding in which the output is written.
        /// </summary>
        /// <value>
        /// The <see cref="Encoding"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// The Encoding in which the output is written.
        /// </para>
        /// </remarks>
        public override Encoding Encoding
        {
            get { return this.m_writer.Encoding; }
        }

        /// <summary>
        /// Gets an object that controls formatting.
        /// </summary>
        /// <value>
        /// The format provider.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets an object that controls formatting.
        /// </para>
        /// </remarks>
        public override IFormatProvider FormatProvider
        {
            get { return this.m_writer.FormatProvider; }
        }

        /// <summary>
        /// Gets or sets the line terminator string used by the TextWriter.
        /// </summary>
        /// <value>
        /// The line terminator to use.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets the line terminator string used by the TextWriter.
        /// </para>
        /// </remarks>
        public override string NewLine
        {
            get { return this.m_writer.NewLine; }
            set { this.m_writer.NewLine = value; }
        }

        /// <summary>
        /// Closes the writer and releases any system resources associated with the writer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// </para>
        /// </remarks>
#if NETSTANDARD1_3
		virtual public void Close()
		{
			m_writer.Dispose();
		}
#else
        public override void Close()
        {
            this.m_writer.Close();
        }
#endif

        /// <summary>
        /// Dispose this writer.
        /// </summary>
        /// <param name="disposing">flag indicating if we are being disposed.</param>
        /// <remarks>
        /// <para>
        /// Dispose this writer.
        /// </para>
        /// </remarks>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ((IDisposable)this.m_writer).Dispose();
            }
        }

        /// <summary>
        /// Flushes any buffered output.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Clears all buffers for the writer and causes any buffered data to be written
        /// to the underlying device.
        /// </para>
        /// </remarks>
        public override void Flush()
        {
            this.m_writer.Flush();
        }

        /// <summary>
        /// Writes a character to the wrapped TextWriter.
        /// </summary>
        /// <param name="value">the value to write to the TextWriter.</param>
        /// <remarks>
        /// <para>
        /// Writes a character to the wrapped TextWriter.
        /// </para>
        /// </remarks>
        public override void Write(char value)
        {
            this.m_writer.Write(value);
        }

        /// <summary>
        /// Writes a character buffer to the wrapped TextWriter.
        /// </summary>
        /// <param name="buffer">the data buffer.</param>
        /// <param name="index">the start index.</param>
        /// <param name="count">the number of characters to write.</param>
        /// <remarks>
        /// <para>
        /// Writes a character buffer to the wrapped TextWriter.
        /// </para>
        /// </remarks>
        public override void Write(char[] buffer, int index, int count)
        {
            this.m_writer.Write(buffer, index, count);
        }

        /// <summary>
        /// Writes a string to the wrapped TextWriter.
        /// </summary>
        /// <param name="value">the value to write to the TextWriter.</param>
        /// <remarks>
        /// <para>
        /// Writes a string to the wrapped TextWriter.
        /// </para>
        /// </remarks>
        public override void Write(string value)
        {
            this.m_writer.Write(value);
        }
    }
}
