// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Runtime.Serialization;

namespace DotNetNuke.Modules.CoreMessaging.ViewModels
{
    public class InboxSubscriptionViewModel
    {
        [DataMember(Name = "notifyFreq")]
        public int NotifyFreq { get; set; }

        [DataMember(Name = "msgFreq")]
        public int MsgFreq { get; set; }
    }
}
