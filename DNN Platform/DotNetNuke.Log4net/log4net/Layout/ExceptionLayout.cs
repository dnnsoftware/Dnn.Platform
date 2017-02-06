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
using System.Text;

using log4net.Util;
using log4net.Core;

namespace log4net.Layout
{
	/// <summary>
	/// A Layout that renders only the Exception text from the logging event
	/// </summary>
	/// <remarks>
	/// <para>
	/// A Layout that renders only the Exception text from the logging event.
	/// </para>
	/// <para>
	/// This Layout should only be used with appenders that utilize multiple
	/// layouts (e.g. <see cref="log4net.Appender.AdoNetAppender"/>).
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class ExceptionLayout : LayoutSkeleton
	{
		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Constructs a ExceptionLayout
		/// </para>
		/// </remarks>
		public ExceptionLayout()
		{
			this.IgnoresException = false;
		}

		#endregion
  
		#region Implementation of IOptionHandler

		/// <summary>
		/// Activate component options
		/// </summary>
		/// <remarks>
		/// <para>
		/// Part of the <see cref="IOptionHandler"/> component activation
		/// framework.
		/// </para>
		/// <para>
		/// This method does nothing as options become effective immediately.
		/// </para>
		/// </remarks>
		override public void ActivateOptions() 
		{
			// nothing to do.
		}

		#endregion

		#region Override implementation of LayoutSkeleton

		/// <summary>
		/// Gets the exception text from the logging event
		/// </summary>
		/// <param name="writer">The TextWriter to write the formatted event to</param>
		/// <param name="loggingEvent">the event being logged</param>
		/// <remarks>
		/// <para>
		/// Write the exception string to the <see cref="TextWriter"/>.
		/// The exception string is retrieved from <see cref="M:LoggingEvent.GetExceptionString()"/>.
		/// </para>
		/// </remarks>
		override public void Format(TextWriter writer, LoggingEvent loggingEvent) 
		{
			if (loggingEvent == null)
			{
				throw new ArgumentNullException("loggingEvent");
			}

			writer.Write(loggingEvent.GetExceptionString());
		}

		#endregion
	}
}
