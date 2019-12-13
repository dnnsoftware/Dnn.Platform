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
    /// Sent Status of a Message - Is this a Sent Message or a Received Message
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
        Any = -1
    }
}
