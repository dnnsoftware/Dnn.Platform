// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Messaging.Data
{
    public enum MessageStatusType
    {
        /// <summary>Drafted message.</summary>
        Draft = 0,

        /// <summary>Unread message.</summary>
        Unread = 1,

        /// <summary>Read message.</summary>
        Read = 2,

        /// <summary>Deleted message.</summary>
        Deleted = 3,
    }
}
