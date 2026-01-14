// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum Frequency
    {
        /// <summary>Never.</summary>
        [EnumMember]
        Never = -1,

        /// <summary>Immediate.</summary>
        [EnumMember]
        Instant = 0,

        /// <summary>Daily.</summary>
        [EnumMember]
        Daily = 1,

        /// <summary>Hourly.</summary>
        [EnumMember]
        Hourly = 2,

        /// <summary>Weekly.</summary>
        [EnumMember]
        Weekly = 3,

        /// <summary>Monthly.</summary>
        [EnumMember]
        Monthly = 4,
    }
}
