// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net
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

    using log4net.Util;

    /// <summary>
    /// The log4net Thread Context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>ThreadContext</c> provides a location for thread specific debugging
    /// information to be stored.
    /// The <c>ThreadContext</c> properties override any <see cref="GlobalContext"/>
    /// properties with the same name.
    /// </para>
    /// <para>
    /// The thread context has a properties map and a stack.
    /// The properties and stack can
    /// be included in the output of log messages. The <see cref="log4net.Layout.PatternLayout"/>
    /// supports selecting and outputting these properties.
    /// </para>
    /// <para>
    /// The Thread Context provides a diagnostic context for the current thread.
    /// This is an instrument for distinguishing interleaved log
    /// output from different sources. Log output is typically interleaved
    /// when a server handles multiple clients near-simultaneously.
    /// </para>
    /// <para>
    /// The Thread Context is managed on a per thread basis.
    /// </para>
    /// </remarks>
    /// <example>Example of using the thread context properties to store a username.
    /// <code lang="C#">
    /// ThreadContext.Properties["user"] = userName;
    ///     log.Info("This log message has a ThreadContext Property called 'user'");
    /// </code>
    /// </example>
    /// <example>Example of how to push a message into the context stack.
    /// <code lang="C#">
    ///     using(ThreadContext.Stacks["NDC"].Push("my context message"))
    ///     {
    ///         log.Info("This log message has a ThreadContext Stack message that includes 'my context message'");
    ///
    ///     } // at the end of the using block the message is automatically popped
    /// </code>
    /// </example>
    /// <threadsafety static="true" instance="true" />
    /// <author>Nicko Cadell.</author>
    public sealed class ThreadContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadContext"/> class.
        /// Private Constructor.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uses a private access modifier to prevent instantiation of this class.
        /// </para>
        /// </remarks>
        private ThreadContext()
        {
        }

        /// <summary>
        /// Gets the thread properties map.
        /// </summary>
        /// <value>
        /// The thread properties map.
        /// </value>
        /// <remarks>
        /// <para>
        /// The <c>ThreadContext</c> properties override any <see cref="GlobalContext"/>
        /// properties with the same name.
        /// </para>
        /// </remarks>
        public static ThreadContextProperties Properties
        {
            get { return s_properties; }
        }

        /// <summary>
        /// Gets the thread stacks.
        /// </summary>
        /// <value>
        /// stack map.
        /// </value>
        /// <remarks>
        /// <para>
        /// The thread local stacks.
        /// </para>
        /// </remarks>
        public static ThreadContextStacks Stacks
        {
            get { return s_stacks; }
        }

        /// <summary>
        /// The thread context properties instance.
        /// </summary>
        private static readonly ThreadContextProperties s_properties = new ThreadContextProperties();

        /// <summary>
        /// The thread context stacks instance.
        /// </summary>
        private static readonly ThreadContextStacks s_stacks = new ThreadContextStacks(s_properties);
    }
}
