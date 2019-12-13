// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
