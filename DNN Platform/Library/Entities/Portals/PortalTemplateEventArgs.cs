using System;

namespace DotNetNuke.Entities.Portals
{
    public class PortalTemplateEventArgs : EventArgs
    {
        public int PortalId { get; set; }

        public string TemplatePath { get; set; }
    }
}
