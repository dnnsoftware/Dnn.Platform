// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;
using System.Xml.Serialization;
using DotNetNuke.Entities.Modules;

#endregion

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
        CustomList = 3        
    }
}
