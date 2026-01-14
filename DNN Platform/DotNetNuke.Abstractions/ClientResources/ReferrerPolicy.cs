// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Abstractions.ClientResources
{
    /// <summary>Defines the referrer policy to use when including a script or stylesheet.</summary>
    public enum ReferrerPolicy
    {
        /// <summary>No referrer policy is specified.</summary>
        None,

        /// <summary>Referrer will not be sent.</summary>
        NoReferrer,

        /// <summary>Referrer will not be sent when navigating from HTTPS to HTTP, but will be sent to the same protocol level (e.g. HTTPS→HTTPS) or to a more secure protocol (e.g. HTTP→HTTPS).</summary>
        NoReferrerWhenDowngrade,

        /// <summary>Referrer will be sent with same-origin requests, but cross-origin requests will contain only the origin of the document as the referrer.</summary>
        Origin,

        /// <summary>Referrer will be sent with same-origin requests, but cross-origin requests will contain only the origin of the document as the referrer when the protocol security level stays the same (e.g. HTTPS→HTTPS), but will contain no referrer information when navigating from HTTPS to HTTP.</summary>
        OriginWhenCrossOrigin,

        /// <summary>Referrer will be sent with same-origin requests, but cross-origin requests will contain no referrer information.</summary>
        SameOrigin,

        /// <summary>Referrer will be sent with same-origin requests, but cross-origin requests will contain only the origin of the document as the referrer when the protocol security level stays the same (e.g. HTTPS→HTTPS), but will contain no referrer information when navigating from HTTPS to HTTP or to a less secure protocol.</summary>
        StrictOrigin,

        /// <summary>Referrer will be sent with same-origin requests, but cross-origin requests will contain no referrer information when navigating from HTTPS to HTTP or to a less secure protocol, and will contain only the origin of the document as the referrer when the protocol security level stays the same (e.g. HTTPS→HTTPS).</summary>
        StrictOriginWhenCrossOrigin,

        /// <summary>Referrer will be sent with all requests, including cross-origin requests.</summary>
        UnsafeUrl,
    }
}
