// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Users
{
    using System;

    public enum RelationshipDirection
    {
        /// <summary>
        /// One way relationship, e.g. Follower, where user 1 is following user 2, but user 2 is not following user 1.
        /// </summary>
        OneWay = 1,

        /// <summary>
        /// Two way relationship, e.g. Friend, where user 1 and user 2 are both friends and mutually following each other.
        /// </summary>
        TwoWay = 2,
    }
}
