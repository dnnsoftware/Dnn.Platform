using System;

namespace DotNetNuke.Entities.Portals
{
    public class PortalCreatedEventArgs : EventArgs
    {
        public int PortalId { get; set; }
    }
}
