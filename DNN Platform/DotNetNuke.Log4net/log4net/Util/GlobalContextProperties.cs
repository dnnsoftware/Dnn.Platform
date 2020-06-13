// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Util
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
    using System.Collections;

    /// <summary>
    /// Implementation of Properties collection for the <see cref="log4net.GlobalContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements a properties collection that is thread safe and supports both
    /// storing properties and capturing a read only copy of the current propertied.
    /// </para>
    /// <para>
    /// This class is optimized to the scenario where the properties are read frequently
    /// and are modified infrequently.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public sealed class GlobalContextProperties : ContextPropertiesBase
    {
        /// <summary>
        /// The read only copy of the properties.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This variable is declared <c>volatile</c> to prevent the compiler and JIT from
        /// reordering reads and writes of this thread performed on different threads.
        /// </para>
        /// </remarks>
#if NETCF
		private ReadOnlyPropertiesDictionary m_readOnlyProperties = new ReadOnlyPropertiesDictionary();
#else
        private volatile ReadOnlyPropertiesDictionary m_readOnlyProperties = new ReadOnlyPropertiesDictionary();
#endif

        /// <summary>
        /// Lock object used to synchronize updates within this instance.
        /// </summary>
        private readonly object m_syncRoot = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalContextProperties"/> class.
        /// Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="GlobalContextProperties" /> class.
        /// </para>
        /// </remarks>
        internal GlobalContextProperties()
        {
        }

        /// <summary>
        /// Gets or sets the value of a property.
        /// </summary>
        /// <value>
        /// The value for the property with the specified key.
        /// </value>
        /// <remarks>
        /// <para>
        /// Reading the value for a key is faster than setting the value.
        /// When the value is written a new read only copy of
        /// the properties is created.
        /// </para>
        /// </remarks>
        public override object this[string key]
        {
            get
            {
                return this.m_readOnlyProperties[key];
            }

            set
            {
                lock (this.m_syncRoot)
                {
                    PropertiesDictionary mutableProps = new PropertiesDictionary(this.m_readOnlyProperties);

                    mutableProps[key] = value;

                    this.m_readOnlyProperties = new ReadOnlyPropertiesDictionary(mutableProps);
                }
            }
        }

        /// <summary>
        /// Remove a property from the global context.
        /// </summary>
        /// <param name="key">the key for the entry to remove.</param>
        /// <remarks>
        /// <para>
        /// Removing an entry from the global context properties is relatively expensive compared
        /// with reading a value.
        /// </para>
        /// </remarks>
        public void Remove(string key)
        {
            lock (this.m_syncRoot)
            {
                if (this.m_readOnlyProperties.Contains(key))
                {
                    PropertiesDictionary mutableProps = new PropertiesDictionary(this.m_readOnlyProperties);

                    mutableProps.Remove(key);

                    this.m_readOnlyProperties = new ReadOnlyPropertiesDictionary(mutableProps);
                }
            }
        }

        /// <summary>
        /// Clear the global context properties.
        /// </summary>
        public void Clear()
        {
            lock (this.m_syncRoot)
            {
                this.m_readOnlyProperties = new ReadOnlyPropertiesDictionary();
            }
        }

        /// <summary>
        /// Get a readonly immutable copy of the properties.
        /// </summary>
        /// <returns>the current global context properties.</returns>
        /// <remarks>
        /// <para>
        /// This implementation is fast because the GlobalContextProperties class
        /// stores a readonly copy of the properties.
        /// </para>
        /// </remarks>
        internal ReadOnlyPropertiesDictionary GetReadOnlyProperties()
        {
            return this.m_readOnlyProperties;
        }
    }
}
