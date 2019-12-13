#region Usings

using System;
using System.Data;
using System.Xml.Serialization;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Modules;

#endregion

namespace DotNetNuke.Services.Social.Messaging
{
    /// <summary>
    /// Archived Status of a Message
    /// </summary>
    public enum MessageArchivedStatus
    {
        /// <summary>
        /// Archived Message Status
        /// </summary>
        Archived = 1,

        /// <summary>
        /// UnArchived Message Status
        /// </summary>
        UnArchived = 0,

        /// <summary>
        /// Any Message Status - Both Archived and UnArchived
        /// </summary>
        Any = -1
    }
}
