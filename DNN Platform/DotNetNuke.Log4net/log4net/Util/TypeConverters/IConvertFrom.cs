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
	/// This interface supports conversion from arbitrary types
	/// to a single target type. See <see cref="TypeConverterAttribute"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public interface IConvertFrom
	{
		/// <summary>
		/// Can the source type be converted to the type supported by this object
		/// </summary>
		/// <param name="sourceType">the type to convert</param>
		/// <returns>true if the conversion is possible</returns>
		/// <remarks>
		/// <para>
		/// Test if the <paramref name="sourceType"/> can be converted to the
		/// type supported by this converter.
		/// </para>
		/// </remarks>
		bool CanConvertFrom(Type sourceType);

		/// <summary>
		/// Convert the source object to the type supported by this object
		/// </summary>
		/// <param name="source">the object to convert</param>
		/// <returns>the converted object</returns>
		/// <remarks>
		/// <para>
		/// Converts the <paramref name="source"/> to the type supported
		/// by this converter.
		/// </para>
		/// </remarks>
		object ConvertFrom(object source);
	}
}
