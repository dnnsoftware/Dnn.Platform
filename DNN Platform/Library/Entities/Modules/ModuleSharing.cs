// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    public enum ModuleSharing
    {
        /// <summary>It is unknown whether module sharing is supported.</summary>
        Unknown = 0,

        /// <summary>Module sharing is not supported.</summary>
        Unsupported = 1,

        /// <summary>Module sharing is supported.</summary>
        Supported = 2,
    }
}
