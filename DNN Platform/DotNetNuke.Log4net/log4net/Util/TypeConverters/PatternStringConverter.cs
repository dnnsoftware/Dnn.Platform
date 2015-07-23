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

using log4net.Util;

namespace log4net.Util.TypeConverters
{
	/// <summary>
	/// Convert between string and <see cref="PatternString"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// Supports conversion from string to <see cref="PatternString"/> type, 
	/// and from a <see cref="PatternString"/> type to a string.
	/// </para>
	/// <para>
	/// The string is used as the <see cref="PatternString.ConversionPattern"/> 
	/// of the <see cref="PatternString"/>.
	/// </para>
	/// </remarks>
	/// <seealso cref="ConverterRegistry"/>
	/// <seealso cref="IConvertFrom"/>
	/// <seealso cref="IConvertTo"/>
	/// <author>Nicko Cadell</author>
	internal class PatternStringConverter : IConvertTo, IConvertFrom
	{
		#region Implementation of IConvertTo

		/// <summary>
		/// Can the target type be converted to the type supported by this object
		/// </summary>
		/// <param name="targetType">A <see cref="Type"/> that represents the type you want to convert to</param>
		/// <returns>true if the conversion is possible</returns>
		/// <remarks>
		/// <para>
		/// Returns <c>true</c> if the <paramref name="targetType"/> is
		/// assignable from a <see cref="String"/> type.
		/// </para>
		/// </remarks>
		public bool CanConvertTo(Type targetType)
		{
			return (typeof(string).IsAssignableFrom(targetType));
		}

		/// <summary>
		/// Converts the given value object to the specified type, using the arguments
		/// </summary>
		/// <param name="source">the object to convert</param>
		/// <param name="targetType">The Type to convert the value parameter to</param>
		/// <returns>the converted object</returns>
		/// <remarks>
		/// <para>
		/// Uses the <see cref="PatternString.Format()"/> method to convert the
		/// <see cref="PatternString"/> argument to a <see cref="String"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ConversionNotSupportedException">
		/// The <paramref name="source"/> object cannot be converted to the
		/// <paramref name="targetType"/>. To check for this condition use the 
		/// <see cref="CanConvertTo"/> method.
		/// </exception>
		public object ConvertTo(object source, Type targetType)
		{
			PatternString patternString = source as PatternString;
			if (patternString != null && CanConvertTo(targetType))
			{
				return patternString.Format();
			}
			throw ConversionNotSupportedException.Create(targetType, source);
		}

		#endregion

		#region Implementation of IConvertFrom

		/// <summary>
		/// Can the source type be converted to the type supported by this object
		/// </summary>
		/// <param name="sourceType">the type to convert</param>
		/// <returns>true if the conversion is possible</returns>
		/// <remarks>
		/// <para>
		/// Returns <c>true</c> if the <paramref name="sourceType"/> is
		/// the <see cref="String"/> type.
		/// </para>
		/// </remarks>
		public bool CanConvertFrom(System.Type sourceType)
		{
			return (sourceType == typeof(string));
		}

		/// <summary>
		/// Overrides the ConvertFrom method of IConvertFrom.
		/// </summary>
		/// <param name="source">the object to convert to a PatternString</param>
		/// <returns>the PatternString</returns>
		/// <remarks>
		/// <para>
		/// Creates and returns a new <see cref="PatternString"/> using
		/// the <paramref name="source"/> <see cref="String"/> as the
		/// <see cref="PatternString.ConversionPattern"/>.
		/// </para>
		/// </remarks>
		/// <exception cref="ConversionNotSupportedException">
		/// The <paramref name="source"/> object cannot be converted to the
		/// target type. To check for this condition use the <see cref="CanConvertFrom"/>
		/// method.
		/// </exception>
		public object ConvertFrom(object source) 
		{
			string str = source as string;
			if (str != null)
			{
				return new PatternString(str);
			}
			throw ConversionNotSupportedException.Create(typeof(PatternString), source);
		}

		#endregion
	}
}
