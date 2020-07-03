﻿// Licensed to the .NET Foundation under one or more agreements.
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
    /// Implementation of Stacks collection for the <see cref="log4net.ThreadContext"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Implementation of Stacks collection for the <see cref="log4net.ThreadContext"/>.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    public sealed class ThreadContextStacks
    {
        private readonly ContextPropertiesBase m_properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadContextStacks"/> class.
        /// Internal constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="ThreadContextStacks" /> class.
        /// </para>
        /// </remarks>
        internal ThreadContextStacks(ContextPropertiesBase properties)
        {
            this.m_properties = properties;
        }

        /// <summary>
        /// Gets the named thread context stack.
        /// </summary>
        /// <value>
        /// The named stack.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets the named thread context stack.
        /// </para>
        /// </remarks>
        public ThreadContextStack this[string key]
        {
            get
            {
                ThreadContextStack stack = null;

                object propertyValue = this.m_properties[key];
                if (propertyValue == null)
                {
                    // Stack does not exist, create
                    stack = new ThreadContextStack();
                    this.m_properties[key] = stack;
                }
                else
                {
                    // Look for existing stack
                    stack = propertyValue as ThreadContextStack;
                    if (stack == null)
                    {
                        // Property is not set to a stack!
                        string propertyValueString = SystemInfo.NullText;

                        try
                        {
                            propertyValueString = propertyValue.ToString();
                        }
                        catch
                        {
                        }

                        LogLog.Error(declaringType, "ThreadContextStacks: Request for stack named [" + key + "] failed because a property with the same name exists which is a [" + propertyValue.GetType().Name + "] with value [" + propertyValueString + "]");

                        stack = new ThreadContextStack();
                    }
                }

                return stack;
            }
        }

        /// <summary>
        /// The fully qualified type of the ThreadContextStacks class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(ThreadContextStacks);
    }
}
