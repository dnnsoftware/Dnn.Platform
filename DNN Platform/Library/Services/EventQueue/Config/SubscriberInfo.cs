#region Usings

using System;

using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Services.EventQueue.Config
{
    [Serializable]
    public class SubscriberInfo
    {
        public SubscriberInfo()
        {
            ID = Guid.NewGuid().ToString();
            Name = "";
            Description = "";
            Address = "";
            var oPortalSecurity = PortalSecurity.Instance;
            PrivateKey = oPortalSecurity.CreateKey(16);
        }

        public SubscriberInfo(string subscriberName) : this()
        {
            Name = subscriberName;
        }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string PrivateKey { get; set; }
    }
}
