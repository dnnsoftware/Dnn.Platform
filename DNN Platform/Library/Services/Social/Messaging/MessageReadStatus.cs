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
    /// Read Status of a Message.
    /// </summary>
    public enum MessageReadStatus
    {
        /// <summary>
        /// Read Message Status
        /// </summary>
        Read = 1,

        /// <summary>
        /// UnRead Message Status
        /// </summary>
        UnRead = 0,

        /// <summary>
        /// Any Message Status - Both Read and UnRead
        /// </summary>
        Any = -1,
    }
}
