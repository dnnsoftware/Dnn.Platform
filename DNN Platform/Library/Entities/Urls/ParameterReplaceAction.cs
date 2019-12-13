using System;

namespace DotNetNuke.Entities.Urls
{
    [Serializable]
    public class ParameterReplaceAction
    {
        public bool ChangeToSiteRoot { get; set; }
        public string LookFor { get; set; }
        public string Name { get; set; }
        public int PortalId { get; set; }
        public string ReplaceWith { get; set; }
        public int TabId { get; set; }
    }
}
