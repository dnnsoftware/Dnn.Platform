// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Social.Messaging
{
    [DataContract]
    public enum Frequency
    {
        [EnumMember] Never = -1,
        [EnumMember] Instant = 0,
        [EnumMember] Daily = 1,
        [EnumMember] Hourly = 2,
        [EnumMember] Weekly = 3,
        [EnumMember] Monthly = 4
    }
}
