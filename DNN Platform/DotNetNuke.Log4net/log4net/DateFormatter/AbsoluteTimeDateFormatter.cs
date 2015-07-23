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

namespace log4net.DateFormatter
{
	/// <summary>
	/// Formats a <see cref="DateTime"/> as <c>"HH:mm:ss,fff"</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Formats a <see cref="DateTime"/> in the format <c>"HH:mm:ss,fff"</c> for example, <c>"15:49:37,459"</c>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class AbsoluteTimeDateFormatter : IDateFormatter
	{
		#region Protected Instance Methods

		/// <summary>
		/// Renders the date into a string. Format is <c>"HH:mm:ss"</c>.
		/// </summary>
		/// <param name="dateToFormat">The date to render into a string.</param>
		/// <param name="buffer">The string builder to write to.</param>
		/// <remarks>
		/// <para>
		/// Subclasses should override this method to render the date
		/// into a string using a precision up to the second. This method
		/// will be called at most once per second and the result will be
		/// reused if it is needed again during the same second.
		/// </para>
		/// </remarks>
		virtual protected void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
		{
			int hour = dateToFormat.Hour;
			if (hour < 10) 
			{
				buffer.Append('0');
			}
			buffer.Append(hour);
			buffer.Append(':');

			int mins = dateToFormat.Minute;
			if (mins < 10) 
			{
				buffer.Append('0');
			}
			buffer.Append(mins);
			buffer.Append(':');
	
			int secs = dateToFormat.Second;
			if (secs < 10) 
			{
				buffer.Append('0');
			}
			buffer.Append(secs);
		}

		#endregion Protected Instance Methods

		#region Implementation of IDateFormatter

		/// <summary>
		/// Renders the date into a string. Format is "HH:mm:ss,fff".
		/// </summary>
		/// <param name="dateToFormat">The date to render into a string.</param>
		/// <param name="writer">The writer to write to.</param>
		/// <remarks>
		/// <para>
		/// Uses the <see cref="FormatDateWithoutMillis"/> method to generate the
		/// time string up to the seconds and then appends the current
		/// milliseconds. The results from <see cref="FormatDateWithoutMillis"/> are
		/// cached and <see cref="FormatDateWithoutMillis"/> is called at most once
		/// per second.
		/// </para>
		/// <para>
		/// Sub classes should override <see cref="FormatDateWithoutMillis"/>
		/// rather than <see cref="FormatDate"/>.
		/// </para>
		/// </remarks>
		virtual public void FormatDate(DateTime dateToFormat, TextWriter writer)
		{
			// Calculate the current time precise only to the second
			long currentTimeToTheSecond = (dateToFormat.Ticks - (dateToFormat.Ticks % TimeSpan.TicksPerSecond));

			// Compare this time with the stored last time
			// If we are in the same second then append
			// the previously calculated time string
			if (s_lastTimeToTheSecond != currentTimeToTheSecond)
			{
				// lock so that only one thread can use the buffer and
				// update the s_lastTimeToTheSecond and s_lastTimeString

				// PERF: Try removing this lock and using a new StringBuilder each time
				lock(s_lastTimeBuf)
				{
					if (s_lastTimeToTheSecond != currentTimeToTheSecond)
					{
						// We are in a new second.
						s_lastTimeBuf.Length = 0;

						// Calculate the new string for this second
						FormatDateWithoutMillis(dateToFormat, s_lastTimeBuf);

						// Render the string buffer to a string
						string currentDateWithoutMillis = s_lastTimeBuf.ToString();

#if NET_1_1
						// Ensure that the above string is written into the variable NOW on all threads.
						// This is only required on multiprocessor machines with weak memeory models
						System.Threading.Thread.MemoryBarrier();
#endif
						// Store the time as a string (we only have to do this once per second)
						s_lastTimeString = currentDateWithoutMillis;
						s_lastTimeToTheSecond = currentTimeToTheSecond;
					}
				}
			}
			writer.Write(s_lastTimeString);
	
			// Append the current millisecond info
			writer.Write(',');
			int millis = dateToFormat.Millisecond;
			if (millis < 100) 
			{
				writer.Write('0');
			}
			if (millis < 10) 
			{
				writer.Write('0');
			}
			writer.Write(millis);
		}

		#endregion Implementation of IDateFormatter

		#region Public Static Fields

		/// <summary>
		/// String constant used to specify AbsoluteTimeDateFormat in layouts. Current value is <b>ABSOLUTE</b>.
		/// </summary>
		public const string AbsoluteTimeDateFormat = "ABSOLUTE";

		/// <summary>
		/// String constant used to specify DateTimeDateFormat in layouts.  Current value is <b>DATE</b>.
		/// </summary>
		public const string DateAndTimeDateFormat = "DATE";

		/// <summary>
		/// String constant used to specify ISO8601DateFormat in layouts. Current value is <b>ISO8601</b>.
		/// </summary>
		public const string Iso8601TimeDateFormat = "ISO8601";

		#endregion Public Static Fields

		#region Private Static Fields

		/// <summary>
		/// Last stored time with precision up to the second.
		/// </summary>
		private static long s_lastTimeToTheSecond = 0;

		/// <summary>
		/// Last stored time with precision up to the second, formatted
		/// as a string.
		/// </summary>
		private static StringBuilder s_lastTimeBuf = new StringBuilder();

		/// <summary>
		/// Last stored time with precision up to the second, formatted
		/// as a string.
		/// </summary>
		private static string s_lastTimeString;

		#endregion Private Static Fields
	}
}
