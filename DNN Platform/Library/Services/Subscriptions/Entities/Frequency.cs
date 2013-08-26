#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Runtime.Serialization;

namespace DotNetNuke.Services.Subscriptions.Entities
{
    [DataContract]
    [Flags]
    public enum Frequency
    {
        [EnumMember] Never = -1,
        [EnumMember] Instant = 0,
        [EnumMember] Daily = 1,
        [EnumMember] Hourly  = 2,
        [EnumMember] Weekly  = 3,
        //[EnumMember] Monthly = 4
    }
}