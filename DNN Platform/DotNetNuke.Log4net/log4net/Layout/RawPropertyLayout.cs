﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Layout
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

    using log4net.Core;
    using log4net.Util;

    /// <summary>
    /// Extract the value of a property from the <see cref="LoggingEvent"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Extract the value of a property from the <see cref="LoggingEvent"/>.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public class RawPropertyLayout : IRawLayout
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawPropertyLayout"/> class.
        /// Constructs a RawPropertyLayout.
        /// </summary>
        public RawPropertyLayout()
        {
        }

        private string m_key;

        /// <summary>
        /// Gets or sets the name of the value to lookup in the LoggingEvent Properties collection.
        /// </summary>
        /// <value>
        /// Value to lookup in the LoggingEvent Properties collection.
        /// </value>
        /// <remarks>
        /// <para>
        /// String name of the property to lookup in the <see cref="LoggingEvent"/>.
        /// </para>
        /// </remarks>
        public string Key
        {
            get { return this.m_key; }
            set { this.m_key = value; }
        }

        /// <summary>
        /// Lookup the property for <see cref="Key"/>.
        /// </summary>
        /// <param name="loggingEvent">The event to format.</param>
        /// <returns>returns property value.</returns>
        /// <remarks>
        /// <para>
        /// Looks up and returns the object value of the property
        /// named <see cref="Key"/>. If there is no property defined
        /// with than name then <c>null</c> will be returned.
        /// </para>
        /// </remarks>
        public virtual object Format(LoggingEvent loggingEvent)
        {
            return loggingEvent.LookupProperty(this.m_key);
        }
    }
}
