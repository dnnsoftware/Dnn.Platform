// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

namespace log4net.Util
{
    /// <summary>
    /// A class to hold the key and data for a property set in the config file.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A class to hold the key and data for a property set in the config file.
    /// </para>
    /// </remarks>
    public class PropertyEntry
    {
        private string m_key = null;
        private object m_value = null;

        /// <summary>
        /// Gets or sets property Key.
        /// </summary>
        /// <value>
        /// Property Key.
        /// </value>
        /// <remarks>
        /// <para>
        /// Property Key.
        /// </para>
        /// </remarks>
        public string Key
        {
            get { return this.m_key; }
            set { this.m_key = value; }
        }

        /// <summary>
        /// Gets or sets property Value.
        /// </summary>
        /// <value>
        /// Property Value.
        /// </value>
        /// <remarks>
        /// <para>
        /// Property Value.
        /// </para>
        /// </remarks>
        public object Value
        {
            get { return this.m_value; }
            set { this.m_value = value; }
        }

        /// <summary>
        /// Override <c>Object.ToString</c> to return sensible debug info.
        /// </summary>
        /// <returns>string info about this object.</returns>
        public override string ToString()
        {
            return "PropertyEntry(Key=" + this.m_key + ", Value=" + this.m_value + ")";
        }
    }
}
