

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Users
{
    /// <summary>
    /// DefaultRelationshipType defined in system
    /// </summary>
    public enum DefaultRelationshipTypes
    {
        /// <summary>
        /// Friends Relationship Type
        /// </summary>
        Friends = 1,

        /// <summary>
        /// Followers Relationship Type
        /// </summary>
        Followers = 2,

        /// <summary>
        /// A user-owned custom-list, e.g. my best friends
        /// </summary>
        CustomList = 3,
    }
}
