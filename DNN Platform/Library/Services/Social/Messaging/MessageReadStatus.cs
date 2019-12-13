// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
    /// Read Status of a Message
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
        Any = -1
    }
}
