using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DotNetNuke.Web.Models
{
    [DataContract]
    public class ModuleDetail
    {
        [DataMember]
        public string ModuleVersion { get; set; }

        [DataMember]
        public string ModuleName { get; set; }

        [DataMember]
        public IList<ModuleInstance> ModuleInstances { get; set; }

        public ModuleDetail()
        {
            ModuleInstances = new List<ModuleInstance>();
        }
    }
}