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
#if !(NETCF || NETSTANDARD1_3)
using System.Runtime.Serialization;
#endif

namespace log4net.Util.TypeConverters
{
	/// <summary>
	/// Exception base type for conversion errors.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This type extends <see cref="ApplicationException"/>. It
	/// does not add any new functionality but does differentiate the
	/// type of exception being thrown.
	/// </para>
	/// </remarks>
	/// <author>Nicko Cadell</author>
	/// <author>Gert Driesen</author>
#if !NETCF
	[Serializable]
#endif
#if NETSTANDARD1_3
	public class ConversionNotSupportedException : Exception
#else
	public class ConversionNotSupportedException : ApplicationException 
#endif
	{
		#region Public Instance Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </para>
		/// </remarks>
		public ConversionNotSupportedException()
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">A message to include with the exception.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class
		/// with the specified message.
		/// </para>
		/// </remarks>
		public ConversionNotSupportedException(String message) : base(message) 
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message">A message to include with the exception.</param>
		/// <param name="innerException">A nested exception to include.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class
		/// with the specified message and inner exception.
		/// </para>
		/// </remarks>
		public ConversionNotSupportedException(String message, Exception innerException) : base(message, innerException) 
		{
		}

		#endregion Public Instance Constructors

		#region Protected Instance Constructors

#if !(NETCF || NETSTANDARD1_3)
		/// <summary>
		/// Serialization constructor
		/// </summary>
		/// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
		/// <remarks>
		/// <para>
		/// Initializes a new instance of the <see cref="ConversionNotSupportedException" /> class 
		/// with serialized data.
		/// </para>
		/// </remarks>
		protected ConversionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) 
		{
		}
#endif

		#endregion Protected Instance Constructors

		#region  Public Static Methods

		/// <summary>
		/// Creates a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </summary>
		/// <param name="destinationType">The conversion destination type.</param>
		/// <param name="sourceValue">The value to convert.</param>
		/// <returns>An instance of the <see cref="ConversionNotSupportedException" />.</returns>
		/// <remarks>
		/// <para>
		/// Creates a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </para>
		/// </remarks>
		public static ConversionNotSupportedException Create(Type destinationType, object sourceValue)
		{
			return Create(destinationType, sourceValue, null);
		}

		/// <summary>
		/// Creates a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </summary>
		/// <param name="destinationType">The conversion destination type.</param>
		/// <param name="sourceValue">The value to convert.</param>
		/// <param name="innerException">A nested exception to include.</param>
		/// <returns>An instance of the <see cref="ConversionNotSupportedException" />.</returns>
		/// <remarks>
		/// <para>
		/// Creates a new instance of the <see cref="ConversionNotSupportedException" /> class.
		/// </para>
		/// </remarks>
		public static ConversionNotSupportedException Create(Type destinationType, object sourceValue, Exception innerException)
		{
			if (sourceValue == null)
			{
				return new ConversionNotSupportedException("Cannot convert value [null] to type ["+destinationType+"]", innerException);
			}
			else
			{
				return new ConversionNotSupportedException("Cannot convert from type ["+sourceValue.GetType()+"] value ["+sourceValue+"] to type ["+destinationType+"]", innerException);
			}
		}

		#endregion  Public Static Methods
	}
}
