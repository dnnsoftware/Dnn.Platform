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

namespace log4net.DateFormatter
{
	/// <summary>
	/// Formats the <see cref="DateTime"/> as <c>"yyyy-MM-dd HH:mm:ss,fff"</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Formats the <see cref="DateTime"/> specified as a string: <c>"yyyy-MM-dd HH:mm:ss,fff"</c>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class Iso8601DateFormatter : AbsoluteTimeDateFormatter
	{
		#region Public Instance Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="Iso8601DateFormatter" /> class.
		/// </para>
		/// </remarks>
		public Iso8601DateFormatter()
		{
		}

		#endregion Public Instance Constructors

		#region Override implementation of AbsoluteTimeDateFormatter

		/// <summary>
		/// Formats the date without the milliseconds part
		/// </summary>
		/// <param name="dateToFormat">The date to format.</param>
		/// <param name="buffer">The string builder to write to.</param>
		/// <remarks>
		/// <para>
		/// Formats the date specified as a string: <c>"yyyy-MM-dd HH:mm:ss"</c>.
		/// </para>
		/// <para>
		/// The base class will append the <c>",fff"</c> milliseconds section.
		/// This method will only be called at most once per second.
		/// </para>
		/// </remarks>
		override protected void FormatDateWithoutMillis(DateTime dateToFormat, StringBuilder buffer)
		{
			buffer.Append(dateToFormat.Year);

			buffer.Append('-');
			int month = dateToFormat.Month;
			if (month < 10)
			{
				buffer.Append('0');
			}
			buffer.Append(month);
			buffer.Append('-');

			int day = dateToFormat.Day;
			if (day < 10) 
			{
				buffer.Append('0');
			}
			buffer.Append(day);
			buffer.Append(' ');

			// Append the 'HH:mm:ss'
			base.FormatDateWithoutMillis(dateToFormat, buffer);
		}

		#endregion Override implementation of AbsoluteTimeDateFormatter
	}
}
