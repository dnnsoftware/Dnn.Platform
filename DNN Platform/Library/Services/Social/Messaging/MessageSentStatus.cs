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
    /// Sent Status of a Message - Is this a Sent Message or a Received Message.
    /// </summary>
    public enum MessageSentStatus
    {
        /// <summary>
        /// This Message was Received
        /// </summary>
        Received = 1,

        /// <summary>
        /// This Message was Sent
        /// </summary>
        Sent = 0,

        /// <summary>
        /// Any Message Status - Both Sent and Received
        /// </summary>
        Any = -1,
    }
}
