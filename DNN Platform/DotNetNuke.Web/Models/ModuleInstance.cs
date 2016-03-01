using System.Runtime.Serialization;

namespace DotNetNuke.Web.Models
{
    [DataContract]
    public class ModuleInstance
    {
        [DataMember]
        public string PageName { get; set; }

        [DataMember]
        public string PagePath { get; set; }

        [DataMember]
        public int TabId { get; set; }

        [DataMember]
        public int ModuleId { get; set; }
    }
}