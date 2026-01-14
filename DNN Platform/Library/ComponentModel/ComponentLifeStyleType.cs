// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ComponentModel
{
    public enum ComponentLifeStyleType
    {
        /// <summary>A single instance is created for the lifetime of the application.</summary>
        Singleton = 0,

        /// <summary>A new instance is created each time it is requested.</summary>
        Transient = 1,
    }
}
