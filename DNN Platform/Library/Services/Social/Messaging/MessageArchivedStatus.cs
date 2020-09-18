// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging
{
    using System;
    using System.Data;
    using System.Xml.Serialization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Modules;

    /// <summary>
    /// Archived Status of a Message.
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
        Any = -1,
    }
}
