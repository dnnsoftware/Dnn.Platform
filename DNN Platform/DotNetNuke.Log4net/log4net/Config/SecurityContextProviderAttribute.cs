// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

// .NET Compact Framework 1.0 has no support for reading assembly attributes
#if !NETCF

using System;
using System.Reflection;

using log4net.Core;
using log4net.Repository;
using log4net.Util;

namespace log4net.Config
{
    /// <summary>
    /// Assembly level attribute to configure the <see cref="SecurityContextProvider"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This attribute may only be used at the assembly scope and can only
    /// be used once per assembly.
    /// </para>
    /// <para>
    /// Use this attribute to configure the <see cref="XmlConfigurator"/>
    /// without calling one of the <see cref="M:XmlConfigurator.Configure()"/>
    /// methods.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    [AttributeUsage(AttributeTargets.Assembly)]
    [Serializable]
    public sealed class SecurityContextProviderAttribute : ConfiguratorAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityContextProviderAttribute"/> class.
        /// Construct provider attribute with type specified.
        /// </summary>
        /// <param name="providerType">the type of the provider to use.</param>
        /// <remarks>
        /// <para>
        /// The provider specified must subclass the <see cref="SecurityContextProvider"/>
        /// class.
        /// </para>
        /// </remarks>
        public SecurityContextProviderAttribute(Type providerType)
            : base(100) /* configurator priority 100 to execute before the XmlConfigurator */
        {
            this.m_providerType = providerType;
        }

        /// <summary>
        /// Gets or sets the type of the provider to use.
        /// </summary>
        /// <value>
        /// the type of the provider to use.
        /// </value>
        /// <remarks>
        /// <para>
        /// The provider specified must subclass the <see cref="SecurityContextProvider"/>
        /// class.
        /// </para>
        /// </remarks>
        public Type ProviderType
        {
            get { return this.m_providerType; }
            set { this.m_providerType = value; }
        }

        /// <summary>
        /// Configures the SecurityContextProvider.
        /// </summary>
        /// <param name="sourceAssembly">The assembly that this attribute was defined on.</param>
        /// <param name="targetRepository">The repository to configure.</param>
        /// <remarks>
        /// <para>
        /// Creates a provider instance from the <see cref="ProviderType"/> specified.
        /// Sets this as the default security context provider <see cref="SecurityContextProvider.DefaultProvider"/>.
        /// </para>
        /// </remarks>
        public override void Configure(Assembly sourceAssembly, ILoggerRepository targetRepository)
        {
            if (this.m_providerType == null)
            {
                LogLog.Error(declaringType, "Attribute specified on assembly [" + sourceAssembly.FullName + "] with null ProviderType.");
            }
            else
            {
                LogLog.Debug(declaringType, "Creating provider of type [" + this.m_providerType.FullName + "]");

                SecurityContextProvider provider = Activator.CreateInstance(this.m_providerType) as SecurityContextProvider;

                if (provider == null)
                {
                    LogLog.Error(declaringType, "Failed to create SecurityContextProvider instance of type [" + this.m_providerType.Name + "].");
                }
                else
                {
                    SecurityContextProvider.DefaultProvider = provider;
                }
            }
        }

        private Type m_providerType = null;

        /// <summary>
        /// The fully qualified type of the SecurityContextProviderAttribute class.
        /// </summary>
        /// <remarks>
        /// Used by the internal logger to record the Type of the
        /// log message.
        /// </remarks>
        private static readonly Type declaringType = typeof(SecurityContextProviderAttribute);
    }
}

#endif // !NETCF
