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

using log4net.Core;

namespace log4net.Util
{
	/// <summary>
	/// <see cref="TextWriter"/> that does not leak exceptions
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="QuietTextWriter"/> does not throw exceptions when things go wrong. 
	/// Instead, it delegates error handling to its <see cref="IErrorHandler"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class QuietTextWriter : TextWriterAdapter
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="writer">the writer to actually write to</param>
		/// <param name="errorHandler">the error handler to report error to</param>
		/// <remarks>
		/// <para>
		/// Create a new QuietTextWriter using a writer and error handler
		/// </para>
		/// </remarks>
		public QuietTextWriter(TextWriter writer, IErrorHandler errorHandler) : base(writer)
		{
			if (errorHandler == null)
			{
				throw new ArgumentNullException("errorHandler");
			}
			ErrorHandler = errorHandler;
		}

		#endregion Public Instance Constructors

		#region Public Instance Properties

		/// <summary>
		/// Gets or sets the error handler that all errors are passed to.
		/// </summary>
		/// <value>
		/// The error handler that all errors are passed to.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets or sets the error handler that all errors are passed to.
		/// </para>
		/// </remarks>
		public IErrorHandler ErrorHandler
		{
			get { return m_errorHandler; }
			set
			{
				if (value == null)
				{
					// This is a programming error on the part of the enclosing appender.
					throw new ArgumentNullException("value");
				}
				m_errorHandler = value;
			}
		}	

		/// <summary>
		/// Gets a value indicating whether this writer is closed.
		/// </summary>
		/// <value>
		/// <c>true</c> if this writer is closed, otherwise <c>false</c>.
		/// </value>
		/// <remarks>
		/// <para>
		/// Gets a value indicating whether this writer is closed.
		/// </para>
		/// </remarks>
		public bool Closed
		{
			get { return m_closed; }
		}

		#endregion Public Instance Properties

		#region Override Implementation of TextWriter

		/// <summary>
		/// Writes a character to the underlying writer
		/// </summary>
		/// <param name="value">the char to write</param>
		/// <remarks>
		/// <para>
		/// Writes a character to the underlying writer
		/// </para>
		/// </remarks>
		public override void Write(char value) 
		{
			try 
			{
				base.Write(value);
			} 
			catch(Exception e) 
			{
				m_errorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
			}
		}
    
		/// <summary>
		/// Writes a buffer to the underlying writer
		/// </summary>
		/// <param name="buffer">the buffer to write</param>
		/// <param name="index">the start index to write from</param>
		/// <param name="count">the number of characters to write</param>
		/// <remarks>
		/// <para>
		/// Writes a buffer to the underlying writer
		/// </para>
		/// </remarks>
		public override void Write(char[] buffer, int index, int count) 
		{
			try 
			{
				base.Write(buffer, index, count);
			} 
			catch(Exception e) 
			{
				m_errorHandler.Error("Failed to write buffer.", e, ErrorCode.WriteFailure);
			}
		}
    
		/// <summary>
		/// Writes a string to the output.
		/// </summary>
		/// <param name="value">The string data to write to the output.</param>
		/// <remarks>
		/// <para>
		/// Writes a string to the output.
		/// </para>
		/// </remarks>
		override public void Write(string value) 
		{
			try 
			{
				base.Write(value);
			} 
			catch(Exception e) 
			{
				m_errorHandler.Error("Failed to write [" + value + "].", e, ErrorCode.WriteFailure);
			}
		}

		/// <summary>
		/// Closes the underlying output writer.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Closes the underlying output writer.
		/// </para>
		/// </remarks>
		override public void Close()
		{
			m_closed = true;
			base.Close();
		}

		#endregion Public Instance Methods

		#region Private Instance Fields

		/// <summary>
		/// The error handler instance to pass all errors to
		/// </summary>
		private IErrorHandler m_errorHandler;

		/// <summary>
		/// Flag to indicate if this writer is closed
		/// </summary>
		private bool m_closed = false;

		#endregion Private Instance Fields
	}
}
