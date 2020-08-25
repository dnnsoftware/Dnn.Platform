// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.EventQueue.Config
{
    using System;

    using DotNetNuke.Security;

    [Serializable]
    public class SubscriberInfo
    {
        public SubscriberInfo()
        {
            this.ID = Guid.NewGuid().ToString();
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.Address = string.Empty;
            var oPortalSecurity = PortalSecurity.Instance;
            this.PrivateKey = oPortalSecurity.CreateKey(16);
        }

        public SubscriberInfo(string subscriberName)
            : this()
        {
            this.Name = subscriberName;
        }

        public string ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string PrivateKey { get; set; }
    }
}
