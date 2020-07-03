﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace log4net.Plugin
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
    using log4net.Repository;

    /// <summary>
    /// Base implementation of <see cref="IPlugin"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Default abstract implementation of the <see cref="IPlugin"/>
    /// interface. This base class can be used by implementors
    /// of the <see cref="IPlugin"/> interface.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    public abstract class PluginSkeleton : IPlugin
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginSkeleton"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="name">the name of the plugin.</param>
        /// <remarks>
        /// Initializes a new Plugin with the specified name.
        /// </remarks>
        protected PluginSkeleton(string name)
        {
            this.m_name = name;
        }

        /// <summary>
        /// Gets or sets the name of the plugin.
        /// </summary>
        /// <value>
        /// The name of the plugin.
        /// </value>
        /// <remarks>
        /// <para>
        /// Plugins are stored in the <see cref="PluginMap"/>
        /// keyed by name. Each plugin instance attached to a
        /// repository must be a unique name.
        /// </para>
        /// <para>
        /// The name of the plugin must not change one the
        /// plugin has been attached to a repository.
        /// </para>
        /// </remarks>
        public virtual string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        /// <summary>
        /// Attaches this plugin to a <see cref="ILoggerRepository"/>.
        /// </summary>
        /// <param name="repository">The <see cref="ILoggerRepository"/> that this plugin should be attached to.</param>
        /// <remarks>
        /// <para>
        /// A plugin may only be attached to a single repository.
        /// </para>
        /// <para>
        /// This method is called when the plugin is attached to the repository.
        /// </para>
        /// </remarks>
        public virtual void Attach(ILoggerRepository repository)
        {
            this.m_repository = repository;
        }

        /// <summary>
        /// Is called when the plugin is to shutdown.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method is called to notify the plugin that
        /// it should stop operating and should detach from
        /// the repository.
        /// </para>
        /// </remarks>
        public virtual void Shutdown()
        {
        }

        /// <summary>
        /// Gets or sets the repository for this plugin.
        /// </summary>
        /// <value>
        /// The <see cref="ILoggerRepository" /> that this plugin is attached to.
        /// </value>
        /// <remarks>
        /// <para>
        /// Gets or sets the <see cref="ILoggerRepository" /> that this plugin is
        /// attached to.
        /// </para>
        /// </remarks>
        protected virtual ILoggerRepository LoggerRepository
        {
            get { return this.m_repository; }
            set { this.m_repository = value; }
        }

        /// <summary>
        /// The name of this plugin.
        /// </summary>
        private string m_name;

        /// <summary>
        /// The repository this plugin is attached to.
        /// </summary>
        private ILoggerRepository m_repository;
    }
}
