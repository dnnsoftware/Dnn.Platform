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
using log4net.Core;

namespace log4net.Util.PatternStringConverters
{
	/// <summary>
	/// Writes a newline to the output
	/// </summary>
	/// <remarks>
	/// <para>
	/// Writes the system dependent line terminator to the output.
	/// This behavior can be overridden by setting the <see cref="PatternConverter.Option"/>:
	/// </para>
	/// <list type="definition">
	///   <listheader>
	///     <term>Option Value</term>
	///     <description>Output</description>
	///   </listheader>
	///   <item>
	///     <term>DOS</term>
	///     <description>DOS or Windows line terminator <c>"\r\n"</c></description>
	///   </item>
	///   <item>
	///     <term>UNIX</term>
	///     <description>UNIX line terminator <c>"\n"</c></description>
	///   </item>
	/// </list>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	internal sealed class NewLinePatternConverter : LiteralPatternConverter, IOptionHandler
	{
		#region Implementation of IOptionHandler

		/// <summary>
		/// Initialize the converter
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is part of the <see cref="IOptionHandler"/> delayed object
		/// activation scheme. The <see cref="ActivateOptions"/> method must 
		/// be called on this object after the configuration properties have
		/// been set. Until <see cref="ActivateOptions"/> is called this
		/// object is in an undefined state and must not be used. 
		/// </para>
		/// <para>
		/// If any of the configuration properties are modified then 
		/// <see cref="ActivateOptions"/> must be called again.
		/// </para>
		/// </remarks>
		public void ActivateOptions()
		{
			if (string.Compare(Option, "DOS", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
			{
				Option = "\r\n";
			}
			else if (string.Compare(Option, "UNIX", true, System.Globalization.CultureInfo.InvariantCulture) == 0)
			{
				Option = "\n";
			}
			else
			{
				Option = SystemInfo.NewLine;
			}
		}

		#endregion
	}
}
