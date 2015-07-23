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

using log4net.Util;

namespace log4net.Util.PatternStringConverters
{
	/// <summary>
	/// Pattern converter for literal string instances in the pattern
	/// </summary>
	/// <remarks>
	/// <para>
	/// Writes the literal string value specified in the 
	/// <see cref="log4net.Util.PatternConverter.Option"/> property to 
	/// the output.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	internal class LiteralPatternConverter : PatternConverter 
	{
		/// <summary>
		/// Set the next converter in the chain
		/// </summary>
		/// <param name="pc">The next pattern converter in the chain</param>
		/// <returns>The next pattern converter</returns>
		/// <remarks>
		/// <para>
		/// Special case the building of the pattern converter chain
		/// for <see cref="LiteralPatternConverter"/> instances. Two adjacent
		/// literals in the pattern can be represented by a single combined
		/// pattern converter. This implementation detects when a 
		/// <see cref="LiteralPatternConverter"/> is added to the chain
		/// after this converter and combines its value with this converter's
		/// literal value.
		/// </para>
		/// </remarks>
		public override PatternConverter SetNext(PatternConverter pc)
		{
			LiteralPatternConverter literalPc = pc as LiteralPatternConverter;
			if (literalPc != null)
			{
				// Combine the two adjacent literals together
				Option = Option + literalPc.Option;

				// We are the next converter now
				return this;
			}

			return base.SetNext(pc);
		}

		/// <summary>
		/// Write the literal to the output
		/// </summary>
		/// <param name="writer">the writer to write to</param>
		/// <param name="state">null, not set</param>
		/// <remarks>
		/// <para>
		/// Override the formatting behavior to ignore the FormattingInfo
		/// because we have a literal instead.
		/// </para>
		/// <para>
		/// Writes the value of <see cref="log4net.Util.PatternConverter.Option"/>
		/// to the output <paramref name="writer"/>.
		/// </para>
		/// </remarks>
		override public void Format(TextWriter writer, object state) 
		{
			writer.Write(Option);
		}

		/// <summary>
		/// Convert this pattern into the rendered message
		/// </summary>
		/// <param name="writer"><see cref="TextWriter" /> that will receive the formatted result.</param>
		/// <param name="state">null, not set</param>
		/// <remarks>
		/// <para>
		/// This method is not used.
		/// </para>
		/// </remarks>
		override protected void Convert(TextWriter writer, object state) 
		{
			throw new InvalidOperationException("Should never get here because of the overridden Format method");
		}
	}
}
