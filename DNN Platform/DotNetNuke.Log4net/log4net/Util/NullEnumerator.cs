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
    /// An always empty <see cref="IEnumerator"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A singleton implementation of the <see cref="IEnumerator"/> over a collection
    /// that is empty and not modifiable.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public sealed class NullEnumerator : IEnumerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullEnumerator" /> class.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uses a private access modifier to enforce the singleton pattern.
        /// </para>
        /// </remarks>
        private NullEnumerator()
        {
        }

        /// <summary>
        /// Gets get the singleton instance of the <see cref="NullEnumerator" />.
        /// </summary>
        /// <returns>The singleton instance of the <see cref="NullEnumerator" />.</returns>
        /// <remarks>
        /// <para>
        /// Gets the singleton instance of the <see cref="NullEnumerator" />.
        /// </para>
        /// </remarks>
        public static NullEnumerator Instance
        {
            get { return s_instance; }
        }

        /// <summary>
        /// Gets the current object from the enumerator.
        /// </summary>
        /// <remarks>
        /// Throws an <see cref="InvalidOperationException" /> because the
        /// <see cref="NullDictionaryEnumerator" /> never has a current value.
        /// </remarks>
        /// <remarks>
        /// <para>
        /// As the enumerator is over an empty collection its <see cref="Current"/>
        /// value cannot be moved over a valid position, therefore <see cref="Current"/>
        /// will throw an <see cref="InvalidOperationException"/>.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">The collection is empty and <see cref="Current"/>
        /// cannot be positioned over a valid location.</exception>
        public object Current
        {
            get { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Test if the enumerator can advance, if so advance.
        /// </summary>
        /// <returns><c>false</c> as the <see cref="NullEnumerator" /> cannot advance.</returns>
        /// <remarks>
        /// <para>
        /// As the enumerator is over an empty collection its <see cref="Current"/>
        /// value cannot be moved over a valid position, therefore <see cref="MoveNext"/>
        /// will always return <c>false</c>.
        /// </para>
        /// </remarks>
        public bool MoveNext()
        {
            return false;
        }

        /// <summary>
        /// Resets the enumerator back to the start.
        /// </summary>
        /// <remarks>
        /// <para>
        /// As the enumerator is over an empty collection <see cref="Reset"/> does nothing.
        /// </para>
        /// </remarks>
        public void Reset()
        {
        }

        /// <summary>
        /// The singleton instance of the <see cref="NullEnumerator" />.
        /// </summary>
        private static readonly NullEnumerator s_instance = new NullEnumerator();
    }
}
