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
