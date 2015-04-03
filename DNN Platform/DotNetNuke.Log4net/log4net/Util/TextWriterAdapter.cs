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
using System.Globalization;

namespace log4net.Util
{
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
	/// <author>Nicko Cadell</author>
	public abstract class TextWriterAdapter : TextWriter
	{
		#region Private Member Variables

		/// <summary>
		/// The writer to forward messages to
		/// </summary>
		private TextWriter m_writer;

		#endregion

		#region Constructors

		/// <summary>
		/// Create an instance of <see cref="TextWriterAdapter"/> that forwards all
		/// messages to a <see cref="TextWriter"/>.
		/// </summary>
		/// <param name="writer">The <see cref="TextWriter"/> to forward to</param>
		/// <remarks>
		/// <para>
		/// Create an instance of <see cref="TextWriterAdapter"/> that forwards all
		/// messages to a <see cref="TextWriter"/>.
		/// </para>
		/// </remarks>
		protected TextWriterAdapter(TextWriter writer) :  base(CultureInfo.InvariantCulture)
		{
			m_writer = writer;
		}

		#endregion

		#region Protected Instance Properties

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
			get { return m_writer; }
			set { m_writer = value; }
		}

		#endregion Protected Instance Properties

		#region Public Properties
    
		/// <summary>
		/// The Encoding in which the output is written
		/// </summary>
		/// <value>
		/// The <see cref="Encoding"/>
		/// </value>
		/// <remarks>
		/// <para>
		/// The Encoding in which the output is written
		/// </para>
		/// </remarks>
		override public Encoding Encoding 
		{
			get { return m_writer.Encoding; }
		}

		/// <summary>
		/// Gets an object that controls formatting
		/// </summary>
		/// <value>
		/// The format provider
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets an object that controls formatting
		/// </para>
		/// </remarks>
		override public IFormatProvider FormatProvider 
		{
			get { return m_writer.FormatProvider; }
		}

		/// <summary>
		/// Gets or sets the line terminator string used by the TextWriter
		/// </summary>
		/// <value>
		/// The line terminator to use
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets or sets the line terminator string used by the TextWriter
		/// </para>
		/// </remarks>
		override public String NewLine 
		{
			get { return m_writer.NewLine; }
			set { m_writer.NewLine = value; }
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Closes the writer and releases any system resources associated with the writer
		/// </summary>
		/// <remarks>
		/// <para>
		/// </para>
		/// </remarks>
		override public void Close() 
		{
			m_writer.Close();
		}

		/// <summary>
		/// Dispose this writer
		/// </summary>
		/// <param name="disposing">flag indicating if we are being disposed</param>
		/// <remarks>
		/// <para>
		/// Dispose this writer
		/// </para>
		/// </remarks>
		override protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)m_writer).Dispose();
			}
		}

		/// <summary>
		/// Flushes any buffered output
		/// </summary>
		/// <remarks>
		/// <para>
		/// Clears all buffers for the writer and causes any buffered data to be written 
		/// to the underlying device
		/// </para>
		/// </remarks>
		override public void Flush() 
		{
			m_writer.Flush();
		}

		/// <summary>
		/// Writes a character to the wrapped TextWriter
		/// </summary>
		/// <param name="value">the value to write to the TextWriter</param>
		/// <remarks>
		/// <para>
		/// Writes a character to the wrapped TextWriter
		/// </para>
		/// </remarks>
		override public void Write(char value) 
		{
			m_writer.Write(value);
		}
    
		/// <summary>
		/// Writes a character buffer to the wrapped TextWriter
		/// </summary>
		/// <param name="buffer">the data buffer</param>
		/// <param name="index">the start index</param>
		/// <param name="count">the number of characters to write</param>
		/// <remarks>
		/// <para>
		/// Writes a character buffer to the wrapped TextWriter
		/// </para>
		/// </remarks>
		override public void Write(char[] buffer, int index, int count) 
		{
			m_writer.Write(buffer, index, count);
		}
    
		/// <summary>
		/// Writes a string to the wrapped TextWriter
		/// </summary>
		/// <param name="value">the value to write to the TextWriter</param>
		/// <remarks>
		/// <para>
		/// Writes a string to the wrapped TextWriter
		/// </para>
		/// </remarks>
		override public void Write(String value) 
		{
			m_writer.Write(value);
		}

		#endregion
	}
}
