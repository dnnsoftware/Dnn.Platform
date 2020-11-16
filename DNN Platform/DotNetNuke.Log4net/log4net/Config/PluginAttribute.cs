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
using System.Globalization;
using System.Reflection;

using log4net.Core;
using log4net.Plugin;
using log4net.Util;

namespace log4net.Config
{
    /// <summary>
    /// Assembly level attribute that specifies a plugin to attach to
    /// the repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Specifies the type of a plugin to create and attach to the
    /// assembly's repository. The plugin type must implement the
    /// <see cref="IPlugin"/> interface.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Serializable]
    public sealed class PluginAttribute : Attribute, IPluginFactory
    {
#if !NETSTANDARD1_3 // Excluded because GetCallingAssembly() is not available in CoreFX (https://github.com/dotnet/corefx/issues/2221).
        /// <summary>
        /// Initializes a new instance of the <see cref="PluginAttribute" /> class
        /// with the specified type.
        /// </summary>
        /// <param name="typeName">The type name of plugin to create.</param>
        /// <remarks>
        /// <para>
        /// Create the attribute with the plugin type specified.
        /// </para>
        /// <para>
        /// Where possible use the constructor that takes a <see cref="System.Type"/>.
        /// </para>
        /// </remarks>
        public PluginAttribute(string typeName)
        {
            this.m_typeName = typeName;
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="PluginAttribute" /> class
        /// with the specified type.
        /// </summary>
        /// <param name="type">The type of plugin to create.</param>
        /// <remarks>
        /// <para>
        /// Create the attribute with the plugin type specified.
        /// </para>
        /// </remarks>
        public PluginAttribute(Type type)
        {
            this.m_type = type;
        }

        /// <summary>
        /// Gets or sets the type for the plugin.
        /// </summary>
        /// <value>
        /// The type for the plugin.
        /// </value>
        /// <remarks>
        /// <para>
        /// The type for the plugin.
        /// </para>
        /// </remarks>
        public Type Type
        {
            get { return this.m_type; }
            set { this.m_type = value; }
        }

        /// <summary>
        /// Gets or sets the type name for the plugin.
        /// </summary>
        /// <value>
        /// The type name for the plugin.
        /// </value>
        /// <remarks>
        /// <para>
        /// The type name for the plugin.
        /// </para>
        /// <para>
        /// Where possible use the <see cref="Type"/> property instead.
        /// </para>
        /// </remarks>
        public string TypeName
        {
            get { return this.m_typeName; }
            set { this.m_typeName = value; }
        }

        /// <summary>
        /// Creates the plugin object defined by this attribute.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Creates the instance of the <see cref="IPlugin"/> object as
        /// specified by this attribute.
        /// </para>
        /// </remarks>
        /// <returns>The plugin object.</returns>
        public IPlugin CreatePlugin()
        {
            Type pluginType = this.m_type;
#if !NETSTANDARD1_3
            if (this.m_type == null)
            {
                // Get the plugin object type from the string type name
                pluginType = SystemInfo.GetTypeFromString(this.m_typeName, true, true);
            }
#endif

            // Check that the type is a plugin
            if (!typeof(IPlugin).IsAssignableFrom(pluginType))
            {
                throw new LogException("Plugin type [" + pluginType.FullName + "] does not implement the log4net.IPlugin interface");
            }

            // Create an instance of the plugin using the default constructor
            IPlugin plugin = (IPlugin)Activator.CreateInstance(pluginType);

            return plugin;
        }

        /// <summary>
        /// Returns a representation of the properties of this object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Overrides base class <see cref="M:Object.ToString()" /> method to
        /// return a representation of the properties of this object.
        /// </para>
        /// </remarks>
        /// <returns>A representation of the properties of this object.</returns>
        public override string ToString()
        {
            if (this.m_type != null)
            {
                return "PluginAttribute[Type=" + this.m_type.FullName + "]";
            }

            return "PluginAttribute[Type=" + this.m_typeName + "]";
        }

        private string m_typeName = null;
        private Type m_type = null;
    }
}

#endif // !NETCF
