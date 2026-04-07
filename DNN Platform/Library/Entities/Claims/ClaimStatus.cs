// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Claims
{
    /// <summary>Represents the status of a claim.</summary>
    public enum ClaimStatus
    {
        /// <summary>A newly created claim.</summary>
        New = 0,

        /// <summary>A closed claim.</summary>
        Closed = 1,

        /// <summary>A previously closed claim that has been reopened.</summary>
        Reopened = 2,
    }
}
