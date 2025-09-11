using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetNuke.Web.Client.Cdf
{
    internal class BasicFile
    {
        public BasicFile(ClientDependencyType type)
        {
            DependencyType = type;
            HtmlAttributes = new Dictionary<string, string>();
            Priority = 100;
            Group = 100;
            Name = "";
            Version = "";
            ForceVersion = false;
        }

        public string FilePath { get; set; }
        public ClientDependencyType DependencyType { get; private set; }
        public int Priority { get; set; }
        public int Group { get; set; }
        public string PathNameAlias { get; set; }
        public string ForceProvider { get; set; }
        public IDictionary<string, string> HtmlAttributes { get; private set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public bool ForceVersion { get; set; }
    }
}
