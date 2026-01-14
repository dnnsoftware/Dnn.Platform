// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    /// <summary>The length of time an API token is valid.</summary>
    public enum ApiTokenTimespan
    {
        /// <summary>30 days.</summary>
        Days30,

        /// <summary>60 days.</summary>
        Days60,

        /// <summary>90 days.</summary>
        Days90,

        /// <summary>180 days.</summary>
        Days180,

        /// <summary>One year.</summary>
        Years1,

        /// <summary>Two years.</summary>
        Years2,
    }
}
