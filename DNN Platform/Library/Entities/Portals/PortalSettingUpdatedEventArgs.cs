using System;

namespace DotNetNuke.Entities.Portals
{
    public class PortalSettingUpdatedEventArgs : EventArgs
    {
        public int PortalId { get; set; }

        public string SettingName { get; set; }

        public string SettingValue { get; set; }

    }
}
