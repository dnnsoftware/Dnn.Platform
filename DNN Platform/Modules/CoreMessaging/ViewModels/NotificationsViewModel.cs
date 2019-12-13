// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Collections.Generic;

namespace DotNetNuke.Modules.CoreMessaging.ViewModels
{
    public class NotificationsViewModel
    {
        public int TotalNotifications { get; set; }
        public IList<NotificationViewModel> Notifications { get; set; }
    }
}
