// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNN.Connectors.GoogleTagManager
{
    /// <summary>The kinds of IDs which can be configured in the Google Tag Manager connector.</summary>
    internal enum GoogleTagIdType
    {
        /// <summary>The ID is empty or does not match a known format.</summary>
        Invalid = 0,

        /// <summary>A Google Tag Manager container ID (<c>GTM-XXXXXX</c>), loaded through <c>gtm.js</c>.</summary>
        Container = 1,

        /// <summary>A Google tag ID (<c>G-</c>, <c>AW-</c>, <c>DC-</c> or <c>GT-</c>), loaded through <c>gtag.js</c>.</summary>
        GoogleTag = 2,
    }
}
