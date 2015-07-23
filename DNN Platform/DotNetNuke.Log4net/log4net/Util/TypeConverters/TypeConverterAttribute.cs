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
	/// Attribute used to associate a type converter
	/// </summary>
	/// <remarks>
	/// <para>
	/// Class and Interface level attribute that specifies a type converter
	/// to use with the associated type.
	/// </para>
	/// <para>
	/// To associate a type converter with a target type apply a
	/// <c>TypeConverterAttribute</c> to the target type. Specify the
	/// type of the type converter on the attribute.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	[AttributeUsage(AttributeTargets.Class|AttributeTargets.Interface|AttributeTargets.Enum)]
	public sealed class TypeConverterAttribute : Attribute
	{
		#region Member Variables

		/// <summary>
		/// The string type name of the type converter
		/// </summary>
		private string m_typeName = null;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Default constructor
		/// </para>
		/// </remarks>
		public TypeConverterAttribute()
		{
		}

		/// <summary>
		/// Create a new type converter attribute for the specified type name
		/// </summary>
		/// <param name="typeName">The string type name of the type converter</param>
		/// <remarks>
		/// <para>
		/// The type specified must implement the <see cref="IConvertFrom"/> 
		/// or the <see cref="IConvertTo"/> interfaces.
		/// </para>
		/// </remarks>
		public TypeConverterAttribute(string typeName)
		{
			m_typeName = typeName;
		}

		/// <summary>
		/// Create a new type converter attribute for the specified type
		/// </summary>
		/// <param name="converterType">The type of the type converter</param>
		/// <remarks>
		/// <para>
		/// The type specified must implement the <see cref="IConvertFrom"/> 
		/// or the <see cref="IConvertTo"/> interfaces.
		/// </para>
		/// </remarks>
		public TypeConverterAttribute(Type converterType)
		{
			m_typeName = log4net.Util.SystemInfo.AssemblyQualifiedName(converterType);
		}

		#endregion

		/// <summary>
		/// The string type name of the type converter 
		/// </summary>
		/// <value>
		/// The string type name of the type converter 
		/// </value>
		/// <remarks>
		/// <para>
		/// The type specified must implement the <see cref="IConvertFrom"/> 
		/// or the <see cref="IConvertTo"/> interfaces.
		/// </para>
		/// </remarks>
		public string ConverterTypeName
		{
			get { return m_typeName; }
			set { m_typeName = value ; }
		}
	}
}
