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

namespace log4net.DateFormatter
{
	/// <summary>
	/// Formats the <see cref="DateTime"/> using the <see cref="M:DateTime.ToString(string, IFormatProvider)"/> method.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Formats the <see cref="DateTime"/> using the <see cref="DateTime"/> <see cref="M:DateTime.ToString(string, IFormatProvider)"/> method.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class SimpleDateFormatter : IDateFormatter
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="format">The format string.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="SimpleDateFormatter" /> class 
		/// with the specified format string.
		/// </para>
		/// <para>
		/// The format string must be compatible with the options
		/// that can be supplied to <see cref="M:DateTime.ToString(string, IFormatProvider)"/>.
		/// </para>
		/// </remarks>
		public SimpleDateFormatter(string format)
		{
			m_formatString = format;
		}

		#endregion Public Instance Constructors

		#region Implementation of IDateFormatter

		/// <summary>
		/// Formats the date using <see cref="M:DateTime.ToString(string, IFormatProvider)"/>.
		/// </summary>
		/// <param name="dateToFormat">The date to convert to a string.</param>
		/// <param name="writer">The writer to write to.</param>
		/// <remarks>
		/// <para>
		/// Uses the date format string supplied to the constructor to call
		/// the <see cref="M:DateTime.ToString(string, IFormatProvider)"/> method to format the date.
		/// </para>
		/// </remarks>
		virtual public void FormatDate(DateTime dateToFormat, TextWriter writer)
		{
			writer.Write(dateToFormat.ToString(m_formatString, System.Globalization.DateTimeFormatInfo.InvariantInfo));
		}

		#endregion

		#region Private Instance Fields

		/// <summary>
		/// The format string used to format the <see cref="DateTime" />.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The format string must be compatible with the options
		/// that can be supplied to <see cref="M:DateTime.ToString(string, IFormatProvider)"/>.
		/// </para>
		/// </remarks>
		private readonly string m_formatString;

		#endregion Private Instance Fields
	}
}
