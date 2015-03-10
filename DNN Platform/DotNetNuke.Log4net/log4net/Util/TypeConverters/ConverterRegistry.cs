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
using System.Globalization;
using System.Reflection;
using System.Collections;

namespace log4net.Util.TypeConverters
{
	/// <summary>
	/// Register of type converters for specific types.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Maintains a registry of type converters used to convert between
	/// types.
	/// </para>
	/// <para>
	/// Use the <see cref="AddConverter(Type, object)"/> and 
	/// <see cref="AddConverter(Type, Type)"/> methods to register new converters.
	/// The <see cref="GetConvertTo"/> and <see cref="GetConvertFrom"/> methods
	/// lookup appropriate converters to use.
	/// </para>
	/// </remarks>
	/// <seealso cref="IConvertFrom"/>
	/// <seealso cref="IConvertTo"/>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
	public sealed class ConverterRegistry
	{
		#region Private Constructors

		/// <summary>
		/// Private constructor
		/// </summary>
		/// <remarks>
		/// Initializes a new instance of the <see cref="ConverterRegistry" /> class.
		/// </remarks>
		private ConverterRegistry() 
		{
		}

		#endregion Private Constructors

		#region Static Constructor

		/// <summary>
		/// Static constructor.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This constructor defines the intrinsic type converters.
		/// </para>
		/// </remarks>
		static ConverterRegistry()
		{
			// Add predefined converters here
			AddConverter(typeof(bool), typeof(BooleanConverter));
			AddConverter(typeof(System.Text.Encoding), typeof(EncodingConverter));
			AddConverter(typeof(System.Type), typeof(TypeConverter));
			AddConverter(typeof(log4net.Layout.PatternLayout), typeof(PatternLayoutConverter));
			AddConverter(typeof(log4net.Util.PatternString), typeof(PatternStringConverter));
			AddConverter(typeof(System.Net.IPAddress), typeof(IPAddressConverter));
		}

		#endregion Static Constructor

		#region Public Static Methods

		/// <summary>
		/// Adds a converter for a specific type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <param name="converter">The type converter to use to convert to the destination type.</param>
		/// <remarks>
		/// <para>
		/// Adds a converter instance for a specific type.
		/// </para>
		/// </remarks>
		public static void AddConverter(Type destinationType, object converter)
		{
			if (destinationType != null && converter != null)
			{
				lock(s_type2converter)
				{
					s_type2converter[destinationType] = converter;
				}
			}
		}

		/// <summary>
		/// Adds a converter for a specific type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <param name="converterType">The type of the type converter to use to convert to the destination type.</param>
		/// <remarks>
		/// <para>
		/// Adds a converter <see cref="Type"/> for a specific type.
		/// </para>
		/// </remarks>
		public static void AddConverter(Type destinationType, Type converterType)
		{
			AddConverter(destinationType, CreateConverterInstance(converterType));
		}

		/// <summary>
		/// Gets the type converter to use to convert values to the destination type.
		/// </summary>
		/// <param name="sourceType">The type being converted from.</param>
		/// <param name="destinationType">The type being converted to.</param>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Gets the type converter to use to convert values to the destination type.
		/// </para>
		/// </remarks>
		public static IConvertTo GetConvertTo(Type sourceType, Type destinationType)
		{
			// TODO: Support inheriting type converters.
			// i.e. getting a type converter for a base of sourceType

			// TODO: Is destinationType required? We don't use it for anything.

			lock(s_type2converter)
			{
				// Lookup in the static registry
				IConvertTo converter = s_type2converter[sourceType] as IConvertTo;

				if (converter == null)
				{
					// Lookup using attributes
					converter = GetConverterFromAttribute(sourceType) as IConvertTo;

					if (converter != null)
					{
						// Store in registry
						s_type2converter[sourceType] = converter;
					}
				}

				return converter;
			}
		}

		/// <summary>
		/// Gets the type converter to use to convert values to the destination type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.
		/// </returns>
		/// <remarks>
		/// <para>
		/// Gets the type converter to use to convert values to the destination type.
		/// </para>
		/// </remarks>
		public static IConvertFrom GetConvertFrom(Type destinationType)
		{
			// TODO: Support inheriting type converters.
			// i.e. getting a type converter for a base of destinationType

			lock(s_type2converter)
			{
				// Lookup in the static registry
				IConvertFrom converter = s_type2converter[destinationType] as IConvertFrom;

				if (converter == null)
				{
					// Lookup using attributes
					converter = GetConverterFromAttribute(destinationType) as IConvertFrom;

					if (converter != null)
					{
						// Store in registry
						s_type2converter[destinationType] = converter;
					}
				}

				return converter;
			}
		}
		
		/// <summary>
		/// Lookups the type converter to use as specified by the attributes on the 
		/// destination type.
		/// </summary>
		/// <param name="destinationType">The type being converted to.</param>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.
		/// </returns>
		private static object GetConverterFromAttribute(Type destinationType)
		{
			// Look for an attribute on the destination type
			object[] attributes = destinationType.GetCustomAttributes(typeof(TypeConverterAttribute), true);
			if (attributes != null && attributes.Length > 0)
			{
				TypeConverterAttribute tcAttr = attributes[0] as TypeConverterAttribute;
				if (tcAttr != null)
				{
					Type converterType = SystemInfo.GetTypeFromString(destinationType, tcAttr.ConverterTypeName, false, true);
					return CreateConverterInstance(converterType);
				}
			}

			// Not found converter using attributes
			return null;
		}

		/// <summary>
		/// Creates the instance of the type converter.
		/// </summary>
		/// <param name="converterType">The type of the type converter.</param>
		/// <returns>
		/// The type converter instance to use for type conversions or <c>null</c> 
		/// if no type converter is found.
		/// </returns>
		/// <remarks>
		/// <para>
		/// The type specified for the type converter must implement 
		/// the <see cref="IConvertFrom"/> or <see cref="IConvertTo"/> interfaces 
		/// and must have a public default (no argument) constructor.
		/// </para>
		/// </remarks>
		private static object CreateConverterInstance(Type converterType)
		{
			if (converterType == null)
			{
				throw new ArgumentNullException("converterType", "CreateConverterInstance cannot create instance, converterType is null");
			}

			// Check type is a converter
			if (typeof(IConvertFrom).IsAssignableFrom(converterType) || typeof(IConvertTo).IsAssignableFrom(converterType))
			{
				try
				{
					// Create the type converter
					return Activator.CreateInstance(converterType);
				}
				catch(Exception ex)
				{
					LogLog.Error(declaringType, "Cannot CreateConverterInstance of type ["+converterType.FullName+"], Exception in call to Activator.CreateInstance", ex);
				}
			}
			else
			{
				LogLog.Error(declaringType, "Cannot CreateConverterInstance of type ["+converterType.FullName+"], type does not implement IConvertFrom or IConvertTo");
			}
			return null;
		}

		#endregion Public Static Methods

		#region Private Static Fields

	    /// <summary>
	    /// The fully qualified type of the ConverterRegistry class.
	    /// </summary>
	    /// <remarks>
	    /// Used by the internal logger to record the Type of the
	    /// log message.
	    /// </remarks>
	    private readonly static Type declaringType = typeof(ConverterRegistry);

		/// <summary>
		/// Mapping from <see cref="Type" /> to type converter.
		/// </summary>
		private static Hashtable s_type2converter = new Hashtable();

		#endregion
	}
}
