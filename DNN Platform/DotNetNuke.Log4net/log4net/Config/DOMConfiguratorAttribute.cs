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
using System.IO;
using System.Reflection;

using log4net.Repository;
using log4net.Repository.Hierarchy;
using log4net.Util;

namespace log4net.Config
{
    /// <summary>
    /// Assembly level attribute to configure the <see cref="XmlConfigurator"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>AliasDomainAttribute is obsolete. Use AliasRepositoryAttribute instead of AliasDomainAttribute.</b>
    /// </para>
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
    /// <author>Gert Driesen.</author>
    [AttributeUsage(AttributeTargets.Assembly)]
    [Serializable]
    [Obsolete("Use XmlConfiguratorAttribute instead of DOMConfiguratorAttribute. Scheduled removal in v10.0.0.")]
    public sealed class DOMConfiguratorAttribute : XmlConfiguratorAttribute
    {
    }
}

#endif // !NETCF
