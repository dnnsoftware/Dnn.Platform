namespace DotNetNuke.Web.Client.ResourceManager.Models
{
    using System.Collections.Generic;
    using DotNetNuke.Abstractions.ClientResources;

    public class FileInclude
    {
        public string FilePath { get; set; }
        public string PathNameAlias { get; set; }
        public int Priority { get; set; }
        public string Provider { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool ForceVersion { get; set; }
        public CrossOrigin CrossOrigin { get; set; } = CrossOrigin.None;
        public FetchPriority FetchPriority { get; set; } = FetchPriority.Auto;
        public ReferrerPolicy ReferrerPolicy { get; set; } = ReferrerPolicy.None;
        public bool Preload { get; set; } = false;
        public bool Blocking { get; set; } = false;

        /// <summary>
        /// Contains inline metadata — a base64-encoded cryptographic hash of the resource (file) you're telling the browser to fetch. 
        /// The browser can use this to verify that the fetched resource has been delivered without unexpected manipulation.
        /// </summary>
        public string Integrity { get; set; } = "";

        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
    }
}
