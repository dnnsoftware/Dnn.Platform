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
