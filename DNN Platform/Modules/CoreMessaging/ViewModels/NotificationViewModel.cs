// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.ViewModels
{
    using System.Collections.Generic;

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
