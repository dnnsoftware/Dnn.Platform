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

using log4net.Core;

namespace log4net.Layout.Pattern
{
	/// <summary>
	/// Converter to output the relative time of the event
	/// </summary>
	/// <remarks>
	/// <para>
	/// Converter to output the time of the event relative to the start of the program.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	internal sealed class RelativeTimePatternConverter : PatternLayoutConverter 
	{
		/// <summary>
		/// Write the relative time to the output
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="loggingEvent">the event being logged</param>
		/// <remarks>
		/// <para>
		/// Writes out the relative time of the event in milliseconds.
		/// That is the number of milliseconds between the event <see cref="LoggingEvent.TimeStamp"/>
		/// and the <see cref="LoggingEvent.StartTime"/>.
		/// </para>
		/// </remarks>
		override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			writer.Write( TimeDifferenceInMillis(LoggingEvent.StartTime, loggingEvent.TimeStamp).ToString(System.Globalization.NumberFormatInfo.InvariantInfo) );
		}

		/// <summary>
		/// Helper method to get the time difference between two DateTime objects
		/// </summary>
		/// <param name="start">start time (in the current local time zone)</param>
		/// <param name="end">end time (in the current local time zone)</param>
		/// <returns>the time difference in milliseconds</returns>
		private static long TimeDifferenceInMillis(DateTime start, DateTime end)
		{
			// We must convert all times to UTC before performing any mathematical
			// operations on them. This allows use to take into account discontinuities
			// caused by daylight savings time transitions.
			return (long)(end.ToUniversalTime() - start.ToUniversalTime()).TotalMilliseconds;
		}
	}
}
