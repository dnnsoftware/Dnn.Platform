#region Apache License
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
#endregion

using System;

namespace log4net.Appender
{
    /// <summary>
    /// Interface that can be implemented by Appenders that buffer logging data and expose a <see cref="Flush"/> method.
    /// </summary>
    public interface IFlushable
    {
        /// <summary>
        /// Flushes any buffered log data.
        /// </summary>
        /// <remarks>
        /// Appenders that implement the <see cref="Flush"/> method must do so in a thread-safe manner: it can be called concurrently with
        /// the <see cref="log4net.Appender.IAppender.DoAppend"/> method.
        /// <para>
        /// Typically this is done by locking on the Appender instance, e.g.:
        /// <code>
        /// <![CDATA[
        /// public bool Flush(int millisecondsTimeout)
        /// {
        ///     lock(this)
        ///     {
        ///         // Flush buffered logging data
        ///         ...
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </para>
        /// <para>
        /// The <paramref name="millisecondsTimeout"/> parameter is only relevant for appenders that process logging events asynchronously,
        /// such as <see cref="RemotingAppender"/>.
        /// </para>
        /// </remarks>
        /// <param name="millisecondsTimeout">The maximum time to wait for logging events to be flushed.</param>
        /// <returns><c>True</c> if all logging events were flushed successfully, else <c>false</c>.</returns>
        bool Flush(int millisecondsTimeout);
    }
}
