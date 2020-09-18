// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public enum Frequency
    {
        [EnumMember]
        Never = -1,
        [EnumMember]
        Instant = 0,
        [EnumMember]
        Daily = 1,
        [EnumMember]
        Hourly = 2,
        [EnumMember]
        Weekly = 3,
        [EnumMember]
        Monthly = 4,
    }
}
