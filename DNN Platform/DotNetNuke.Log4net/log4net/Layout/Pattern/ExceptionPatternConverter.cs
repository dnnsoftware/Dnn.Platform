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

using System.Diagnostics;
using System.IO;
using log4net.Core;

namespace log4net.Layout.Pattern
{
	/// <summary>
	/// Write the exception text to the output
	/// </summary>
	/// <remarks>
	/// <para>
	/// If an exception object is stored in the logging event
	/// it will be rendered into the pattern output with a
	/// trailing newline.
	/// </para>
	/// <para>
	/// If there is no exception then nothing will be output
	/// and no trailing newline will be appended.
	/// It is typical to put a newline before the exception
	/// and to have the exception as the last data in the pattern.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	internal sealed class ExceptionPatternConverter : PatternLayoutConverter 
	{
		/// <summary>
		/// Default constructor
		/// </summary>
		public ExceptionPatternConverter()
		{
			// This converter handles the exception
			IgnoresException = false;
		}

		/// <summary>
		/// Write the exception text to the output
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="loggingEvent">the event being logged</param>
		/// <remarks>
		/// <para>
		/// If an exception object is stored in the logging event
		/// it will be rendered into the pattern output with a
		/// trailing newline.
		/// </para>
		/// <para>
		/// If there is no exception or the exception property specified
		/// by the Option value does not exist then nothing will be output
		/// and no trailing newline will be appended.
		/// It is typical to put a newline before the exception
		/// and to have the exception as the last data in the pattern.
		/// </para>
		/// <para>
		/// Recognized values for the Option parameter are:
		/// </para>
		/// <list type="bullet">
		///		<item>
		///			<description>Message</description>
		///		</item>
		///		<item>
		///			<description>Source</description>
		///		</item>
		///		<item>
		///			<description>StackTrace</description>
		///		</item>
		///		<item>
		///			<description>TargetSite</description>
		///		</item>
		///		<item>
		///			<description>HelpLink</description>
		///		</item>		
		/// </list>
		/// </remarks>
		override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			if (loggingEvent.ExceptionObject != null && Option != null && Option.Length > 0)
			{
				switch (Option.ToLower())
				{
					case "message":
						WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.Message);
						break;
#if !NETCF						
					case "source":
						WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.Source);
						break;
					case "stacktrace":
						WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.StackTrace);
						break;
					case "targetsite":
						WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.TargetSite);
						break;
					case "helplink":
						WriteObject(writer, loggingEvent.Repository, loggingEvent.ExceptionObject.HelpLink);
						break;
#endif						
					default:
						// do not output SystemInfo.NotAvailableText
						break;
				}
			}
			else
			{
				string exceptionString = loggingEvent.GetExceptionString();
				if (exceptionString != null && exceptionString.Length > 0) 
				{
					writer.WriteLine(exceptionString);
				}
				else
				{
					// do not output SystemInfo.NotAvailableText
				}
			}
		}
	}
}
