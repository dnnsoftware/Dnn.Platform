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

namespace log4net.Util.TypeConverters
{
	/// <summary>
	/// Interface supported by type converters
	/// </summary>
	/// <remarks>
	/// <para>
	/// This interface supports conversion from a single type to arbitrary types.
	/// See <see cref="TypeConverterAttribute"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	public interface IConvertTo
	{
		/// <summary>
		/// Returns whether this converter can convert the object to the specified type
		/// </summary>
		/// <param name="targetType">A Type that represents the type you want to convert to</param>
		/// <returns>true if the conversion is possible</returns>
		/// <remarks>
		/// <para>
		/// Test if the type supported by this converter can be converted to the
		/// <paramref name="targetType"/>.
		/// </para>
		/// </remarks>
		bool CanConvertTo(Type targetType);

		/// <summary>
		/// Converts the given value object to the specified type, using the arguments
		/// </summary>
		/// <param name="source">the object to convert</param>
		/// <param name="targetType">The Type to convert the value parameter to</param>
		/// <returns>the converted object</returns>
		/// <remarks>
		/// <para>
		/// Converts the <paramref name="source"/> (which must be of the type supported
		/// by this converter) to the <paramref name="targetType"/> specified..
		/// </para>
		/// </remarks>
		object ConvertTo(object source, Type targetType);
	}
}
