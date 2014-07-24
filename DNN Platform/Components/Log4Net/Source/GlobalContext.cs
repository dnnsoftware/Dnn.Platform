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

using log4net.Util;

namespace log4net
{
    /// <summary>
    /// The log4net Global Context.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>GlobalContext</c> provides a location for global debugging 
    /// information to be stored.
    /// </para>
    /// <para>
    /// The global context has a properties map and these properties can 
    /// be included in the output of log messages. The <see cref="log4net.Layout.PatternLayout"/>
    /// supports selecting and outputing these properties.
    /// </para>
    /// <para>
    /// By default the <c>log4net:HostName</c> property is set to the name of 
    /// the current machine.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// GlobalContext.Properties["hostname"] = Environment.MachineName;
    /// </code>
    /// </example>
    /// <threadsafety static="true" instance="true" />
    /// <author>Nicko Cadell</author>
    public sealed class GlobalContext
    {
        /// <summary>
        /// Private Constructor.
        /// </summary>
        private readonly static GlobalContextProperties s_properties;

        /// <summary>
        /// The global properties map.
        /// </summary>
        /// <value>The global properties map.</value>
        /// <remarks>The global properties map.</remarks>
        public static GlobalContextProperties Properties
        {
            get
            {
                return GlobalContext.s_properties;
            }
        }

        static GlobalContext()
        {
            GlobalContext.s_properties = new GlobalContextProperties();
            GlobalContext.Properties["log4net:HostName"] = SystemInfo.HostName;
        }

        private GlobalContext()
        {
        }
    }
}