// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Providers.AspNetClientCapabilityProvider.Properties
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>A list of constants to use with the purchase solution.</summary>
    public static class RetailerConstants
    {
        /// <summary>The url to send purchasers to.</summary>
        public const string RetailerUrl = "http://store.dotnetnuke.com/";

        /// <summary>The name of the retailer.</summary>
        public const string RetailerName = "DotNetNuke Store";

#pragma warning disable SA1310 // Field should not contain an underscore
        /// <summary>The url to send purchasers to.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. Use RetailerUrl instead. Scheduled removal in v11.0.0.")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public const string RETAILER_URL = RetailerUrl;

        /// <summary>The name of the retailer.</summary>
        [Obsolete("Deprecated in DotNetNuke 9.8.1. Use RetailerName instead. Scheduled removal in v11.0.0.")]
        [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
        public const string RETAILER_NAME = RetailerName;
#pragma warning restore SA1310 // Field should not contain an underscore
    }
}
