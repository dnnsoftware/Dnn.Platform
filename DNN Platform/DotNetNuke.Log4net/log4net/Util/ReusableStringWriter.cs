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

namespace log4net.Util
{
	/// <summary>
	/// A <see cref="StringWriter"/> that can be <see cref="Reset"/> and reused
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="StringWriter"/> that can be <see cref="Reset"/> and reused.
	/// This uses a single buffer for string operations.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public class ReusableStringWriter : StringWriter
	{
		#region Constructor

		/// <summary>
		/// Create an instance of <see cref="ReusableStringWriter"/>
		/// </summary>
		/// <param name="formatProvider">the format provider to use</param>
		/// <remarks>
		/// <para>
		/// Create an instance of <see cref="ReusableStringWriter"/>
		/// </para>
		/// </remarks>
		public ReusableStringWriter(IFormatProvider formatProvider) : base(formatProvider) 
		{
		}

		#endregion

		/// <summary>
		/// Override Dispose to prevent closing of writer
		/// </summary>
		/// <param name="disposing">flag</param>
		/// <remarks>
		/// <para>
		/// Override Dispose to prevent closing of writer
		/// </para>
		/// </remarks>
		protected override void Dispose(bool disposing)
		{
			// Do not close the writer
		}

		/// <summary>
		/// Reset this string writer so that it can be reused.
		/// </summary>
		/// <param name="maxCapacity">the maximum buffer capacity before it is trimmed</param>
		/// <param name="defaultSize">the default size to make the buffer</param>
		/// <remarks>
		/// <para>
		/// Reset this string writer so that it can be reused.
		/// The internal buffers are cleared and reset.
		/// </para>
		/// </remarks>
		public void Reset(int maxCapacity, int defaultSize)
		{
			// Reset working string buffer
			StringBuilder sb = this.GetStringBuilder();

			sb.Length = 0;
			
			// Check if over max size
			if (sb.Capacity > maxCapacity) 
			{
				sb.Capacity = defaultSize;
			} 
		}
	}
}
