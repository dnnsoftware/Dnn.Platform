﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util.TypeConverters
{
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
    using System;
    using System.Text;

    /// <summary>
    /// Supports conversion from string to <see cref="Encoding"/> type.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports conversion from string to <see cref="Encoding"/> type.
    /// </para>
    /// </remarks>
    /// <seealso cref="ConverterRegistry"/>
    /// <seealso cref="IConvertFrom"/>
    /// <seealso cref="IConvertTo"/>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    internal class EncodingConverter : IConvertFrom
    {
        /// <summary>
        /// Can the source type be converted to the type supported by this object.
        /// </summary>
        /// <param name="sourceType">the type to convert.</param>
        /// <returns>true if the conversion is possible.</returns>
        /// <remarks>
        /// <para>
        /// Returns <c>true</c> if the <paramref name="sourceType"/> is
        /// the <see cref="string"/> type.
        /// </para>
        /// </remarks>
        public bool CanConvertFrom(Type sourceType)
        {
            return sourceType == typeof(string);
        }

        /// <summary>
        /// Overrides the ConvertFrom method of IConvertFrom.
        /// </summary>
        /// <param name="source">the object to convert to an encoding.</param>
        /// <returns>the encoding.</returns>
        /// <remarks>
        /// <para>
        /// Uses the <see cref="M:Encoding.GetEncoding(string)"/> method to
        /// convert the <see cref="string"/> argument to an <see cref="Encoding"/>.
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
                return Encoding.GetEncoding(str);
            }

            throw ConversionNotSupportedException.Create(typeof(Encoding), source);
        }
    }
}
