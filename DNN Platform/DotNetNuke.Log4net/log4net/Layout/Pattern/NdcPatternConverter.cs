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
	/// Converter to include event NDC
	/// </summary>
	/// <remarks>
	/// <para>
	/// Outputs the value of the event property named <c>NDC</c>.
	/// </para>
	/// <para>
	/// The <see cref="PropertyPatternConverter"/> should be used instead.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	internal sealed class NdcPatternConverter : PatternLayoutConverter 
	{
		/// <summary>
		/// Write the event NDC to the output
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="loggingEvent">the event being logged</param>
		/// <remarks>
		/// <para>
		/// As the thread context stacks are now stored in named event properties
		/// this converter simply looks up the value of the <c>NDC</c> property.
		/// </para>
		/// <para>
		/// The <see cref="PropertyPatternConverter"/> should be used instead.
		/// </para>
		/// </remarks>
		override protected void Convert(TextWriter writer, LoggingEvent loggingEvent)
		{
			// Write the value for the specified key
			WriteObject(writer, loggingEvent.Repository, loggingEvent.LookupProperty("NDC"));
		}
	}
}
