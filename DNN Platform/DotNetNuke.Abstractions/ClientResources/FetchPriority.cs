// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources
{
    /// <summary>Defines the fetch priority for a resource.</summary>
    public enum FetchPriority
    {
        /// <summary>Default fetch priority.</summary>
        Auto = 0,

        /// <summary>High fetch priority.</summary>
        High = 1,

        /// <summary>Low fetch priority.</summary>
        Low = 2,
    }
}
