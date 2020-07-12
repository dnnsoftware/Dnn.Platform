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

    using log4net.Core;
    using log4net.Util;

    /// <summary>
    /// The <see cref="RootLogger" /> sits at the root of the logger hierarchy tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="RootLogger" /> is a regular <see cref="Logger" /> except
    /// that it provides several guarantees.
    /// </para>
    /// <para>
    /// First, it cannot be assigned a <c>null</c>
    /// level. Second, since the root logger cannot have a parent, the
    /// <see cref="EffectiveLevel"/> property always returns the value of the
    /// level field without walking the hierarchy.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public class RootLogger : Logger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RootLogger"/> class.
        /// Construct a <see cref="RootLogger"/>.
        /// </summary>
        /// <param name="level">The level to assign to the root logger.</param>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="RootLogger" /> class with
        /// the specified logging level.
        /// </para>
        /// <para>
        /// The root logger names itself as "root". However, the root
        /// logger cannot be retrieved by name.
        /// </para>
        /// </remarks>
        public RootLogger(Level level)
            : base("root")
        {
            this.Level = level;
        }

        /// <summary>
        /// Gets the assigned level value without walking the logger hierarchy.
        /// </summary>
        /// <value>The assigned level value without walking the logger hierarchy.</value>
        /// <remarks>
        /// <para>
        /// Because the root logger cannot have a parent and its level
        /// must not be <c>null</c> this property just returns the
        /// value of <see cref="Logger.Level"/>.
        /// </para>
        /// </remarks>
        public override Level EffectiveLevel
        {
            get
            {
                return base.Level;
            }
        }

        /// <summary>
        /// Gets or sets the assigned <see cref="Level"/> for the root logger.
        /// </summary>
        /// <value>
        /// The <see cref="Level"/> of the root logger.
        /// </value>
        /// <remarks>
        /// <para>
        /// Setting the level of the root logger to a <c>null</c> reference
        /// may have catastrophic results. We prevent this here.
        /// </para>
        /// </remarks>
        public override Level Level
        {
            get { return base.Level; }

            set
            {
                if (value == null)
                {
                    LogLog.Error(declaringType, "You have tried to set a null level to root.", new LogException());
                }
                else
                {
                    base.Level = value;
                }
            }
        }

        /// <summary>
        /// The fully qualified type of the RootLogger class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(RootLogger);
    }
}
