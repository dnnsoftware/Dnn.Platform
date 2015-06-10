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

using log4net;
using log4net.Core;
using log4net.Util.TypeConverters;

namespace log4net.Layout
{
	/// <summary>
	/// Type converter for the <see cref="IRawLayout"/> interface
	/// </summary>
	/// <remarks>
	/// <para>
	/// Used to convert objects to the <see cref="IRawLayout"/> interface.
	/// Supports converting from the <see cref="ILayout"/> interface to
	/// the <see cref="IRawLayout"/> interface using the <see cref="Layout2RawLayoutAdapter"/>.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public class RawLayoutConverter : IConvertFrom
	{
		#region Override Implementation of IRawLayout

		/// <summary>
		/// Can the sourceType be converted to an <see cref="IRawLayout"/>
		/// </summary>
		/// <param name="sourceType">the source to be to be converted</param>
		/// <returns><c>true</c> if the source type can be converted to <see cref="IRawLayout"/></returns>
		/// <remarks>
		/// <para>
		/// Test if the <paramref name="sourceType"/> can be converted to a
		/// <see cref="IRawLayout"/>. Only <see cref="ILayout"/> is supported
		/// as the <paramref name="sourceType"/>.
		/// </para>
		/// </remarks>
		public bool CanConvertFrom(Type sourceType) 
		{
			// Accept an ILayout object
			return (typeof(ILayout).IsAssignableFrom(sourceType));
		}

		/// <summary>
		/// Convert the value to a <see cref="IRawLayout"/> object
		/// </summary>
		/// <param name="source">the value to convert</param>
		/// <returns>the <see cref="IRawLayout"/> object</returns>
		/// <remarks>
		/// <para>
		/// Convert the <paramref name="source"/> object to a 
		/// <see cref="IRawLayout"/> object. If the <paramref name="source"/> object
		/// is a <see cref="ILayout"/> then the <see cref="Layout2RawLayoutAdapter"/>
		/// is used to adapt between the two interfaces, otherwise an
		/// exception is thrown.
		/// </para>
		/// </remarks>
		public object ConvertFrom(object source) 
		{
			ILayout layout = source as ILayout;
			if (layout != null) 
			{
				return new Layout2RawLayoutAdapter(layout);
			}
			throw ConversionNotSupportedException.Create(typeof(IRawLayout), source);
		}

		#endregion
	}
}
