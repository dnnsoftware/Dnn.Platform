// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Modules.CoreMessaging.ViewModels
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string Subject { get; set; }
        public string From { get; set; }
        public string Body { get; set; }
        public string SenderAvatar { get; set; }
        public string SenderProfileUrl { get; set; }
        public string SenderDisplayName { get; set; }
        public string DisplayDate { get; set; }
        public IList<NotificationActionViewModel> Actions { get; set; }
    }
}
