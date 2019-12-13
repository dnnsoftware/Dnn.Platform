using System.Collections.Generic;

namespace DotNetNuke.Modules.CoreMessaging.ViewModels
{
    public class NotificationsViewModel
    {
        public int TotalNotifications { get; set; }
        public IList<NotificationViewModel> Notifications { get; set; }
    }
}
