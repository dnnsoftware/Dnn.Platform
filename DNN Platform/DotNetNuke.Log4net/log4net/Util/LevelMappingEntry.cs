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

    using log4net.Core;

    /// <summary>
    /// An entry in the <see cref="LevelMapping"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is an abstract base class for types that are stored in the
    /// <see cref="LevelMapping"/> object.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public abstract class LevelMappingEntry : IOptionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LevelMappingEntry"/> class.
        /// Default protected constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Default protected constructor.
        /// </para>
        /// </remarks>
        protected LevelMappingEntry()
        {
        }

        /// <summary>
        /// Gets or sets the level that is the key for this mapping.
        /// </summary>
        /// <value>
        /// The <see cref="Level"/> that is the key for this mapping.
        /// </value>
        /// <remarks>
        /// <para>
        /// Get or set the <see cref="Level"/> that is the key for this
        /// mapping subclass.
        /// </para>
        /// </remarks>
        public Level Level
        {
            get { return this.m_level; }
            set { this.m_level = value; }
        }

        /// <summary>
        /// Initialize any options defined on this entry.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Should be overridden by any classes that need to initialise based on their options.
        /// </para>
        /// </remarks>
        public virtual void ActivateOptions()
        {
            // default implementation is to do nothing
        }

        private Level m_level;
    }
}
