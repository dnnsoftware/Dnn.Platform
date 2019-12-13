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
