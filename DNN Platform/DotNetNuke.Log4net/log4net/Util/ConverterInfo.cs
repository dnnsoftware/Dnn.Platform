// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util
{
    /*
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 *
*/

    using System;

    /// <summary>
    /// Wrapper class used to map converter names to converter types.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Pattern converter info class used during configuration by custom
    /// PatternString and PatternLayer converters.
    /// </para>
    /// </remarks>
    public sealed class ConverterInfo
    {
        private string m_name;
        private Type m_type;
        private readonly PropertiesDictionary properties = new PropertiesDictionary();

        /// <summary>
        /// Initializes a new instance of the <see cref="ConverterInfo"/> class.
        /// default constructor.
        /// </summary>
        public ConverterInfo()
        {
        }

        /// <summary>
        /// Gets or sets the name of the conversion pattern.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The name of the pattern in the format string.
        /// </para>
        /// </remarks>
        public string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        /// <summary>
        /// Gets or sets the type of the converter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The value specified must extend the
        /// <see cref="PatternConverter"/> type.
        /// </para>
        /// </remarks>
        public Type Type
        {
            get { return this.m_type; }
            set { this.m_type = value; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="entry"></param>
        public void AddProperty(PropertyEntry entry)
        {
            this.properties[entry.Key] = entry.Value;
        }

        /// <summary>
        ///
        /// </summary>
        public PropertiesDictionary Properties
        {
            get { return this.properties; }
        }
    }
}
