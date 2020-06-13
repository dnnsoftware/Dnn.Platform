// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Repository.Hierarchy
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

    /// <summary>
    /// Used internally to accelerate hash table searches.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Internal class used to improve performance of
    /// string keyed hashtables.
    /// </para>
    /// <para>
    /// The hashcode of the string is cached for reuse.
    /// The string is stored as an interned value.
    /// When comparing two <see cref="LoggerKey"/> objects for equality
    /// the reference equality of the interned strings is compared.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    internal sealed class LoggerKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerKey"/> class.
        /// Construct key with string name.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="LoggerKey" /> class
        /// with the specified name.
        /// </para>
        /// <para>
        /// Stores the hashcode of the string and interns
        /// the string key to optimize comparisons.
        /// </para>
        /// <note>
        /// The Compact Framework 1.0 the <see cref="string.Intern"/>
        /// method does not work. On the Compact Framework
        /// the string keys are not interned nor are they
        /// compared by reference.
        /// </note>
        /// </remarks>
        /// <param name="name">The name of the logger.</param>
        internal LoggerKey(string name)
        {
#if NETCF || NETSTANDARD1_3
			// NETCF: String.Intern causes Native Exception
			m_name = name;
#else
            this.m_name = string.Intern(name);
#endif
            this.m_hashCache = name.GetHashCode();
        }

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        /// <returns>A hash code for the current instance.</returns>
        /// <remarks>
        /// <para>
        /// Returns the cached hashcode.
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return this.m_hashCache;
        }

        /// <summary>
        /// Determines whether two <see cref="LoggerKey" /> instances
        /// are equal.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with the current <see cref="LoggerKey" />.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object" /> is equal to the current <see cref="LoggerKey" />; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Compares the references of the interned strings.
        /// </para>
        /// </remarks>
        public override bool Equals(object obj)
        {
            // Compare reference type of this against argument
            if (((object)this) == obj)
            {
                return true;
            }

            LoggerKey objKey = obj as LoggerKey;
            if (objKey != null)
            {
#if NETCF || NETSTANDARD1_3
				return ( m_name == objKey.m_name );
#else
                // Compare reference types rather than string's overloaded ==
                return ((object)this.m_name) == ((object)objKey.m_name);
#endif
            }

            return false;
        }

        private readonly string m_name;
        private readonly int m_hashCache;
    }
}
