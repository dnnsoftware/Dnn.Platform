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

namespace log4net.Config
{
    /// <summary>
    /// Assembly level attribute that specifies a repository to alias to this assembly's repository.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An assembly's logger repository is defined by its <see cref="RepositoryAttribute"/>,
    /// however this can be overridden by an assembly loaded before the target assembly.
    /// </para>
    /// <para>
    /// An assembly can alias another assembly's repository to its repository by
    /// specifying this attribute with the name of the target repository.
    /// </para>
    /// <para>
    /// This attribute can only be specified on the assembly and may be used
    /// as many times as necessary to alias all the required repositories.
    /// </para>
    /// </remarks>
    /// <author>Nicko Cadell.</author>
    /// <author>Gert Driesen.</author>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    [Serializable]
    public /*sealed*/ class AliasRepositoryAttribute : Attribute
    {
        // Class is not sealed because AliasDomainAttribute extends it while it is obsoleted
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="AliasRepositoryAttribute" /> class with
        /// the specified repository to alias to this assembly's repository.
        /// </summary>
        /// <param name="name">The repository to alias to this assembly's repository.</param>
        /// <remarks>
        /// <para>
        /// Initializes a new instance of the <see cref="AliasRepositoryAttribute" /> class with
        /// the specified repository to alias to this assembly's repository.
        /// </para>
        /// </remarks>
        public AliasRepositoryAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Gets or sets the repository to alias to this assembly's repository.
        /// </summary>
        /// <value>
        /// The repository to alias to this assemby's repository.
        /// </value>
        /// <remarks>
        /// <para>
        /// The name of the repository to alias to this assembly's repository.
        /// </para>
        /// </remarks>
        public string Name
        {
            get { return this.m_name; }
            set { this.m_name = value; }
        }

        private string m_name = null;
    }
}

#endif // !NETCF
