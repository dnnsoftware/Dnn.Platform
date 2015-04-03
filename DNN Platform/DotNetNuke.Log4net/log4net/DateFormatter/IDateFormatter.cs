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
	/// Render a <see cref="DateTime"/> as a string.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Interface to abstract the rendering of a <see cref="DateTime"/>
	/// instance into a string.
	/// </para>
	/// <para>
	/// The <see cref="FormatDate"/> method is used to render the
	/// date to a text writer.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IDateFormatter
	{
		/// <summary>
		/// Formats the specified date as a string.
		/// </summary>
		/// <param name="dateToFormat">The date to format.</param>
		/// <param name="writer">The writer to write to.</param>
		/// <remarks>
		/// <para>
		/// Format the <see cref="DateTime"/> as a string and write it
		/// to the <see cref="TextWriter"/> provided.
		/// </para>
		/// </remarks>
		void FormatDate(DateTime dateToFormat, TextWriter writer);
	}
}
