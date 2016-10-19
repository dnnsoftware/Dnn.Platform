using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DotNetNuke.Services.Installer.Packages;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [DataContract]
    public class PackageManifestDto : PackageInfoDto
    {
        [DataMember(Name = "archiveName")]
        public string ArchiveName { get; set; }

        [DataMember(Name = "manifestName")]
        public string ManifestName { get; set; }

        [DataMember(Name = "basePath")]
        public string BasePath { get; set; }

        [DataMember(Name = "manifests")]
        public IDictionary<string, string> Manifests  { get; set; } = new Dictionary<string, string>();

        [DataMember(Name = "assemblies")]
        public IList<string> Assemblies { get; set; } = new List<string>();

        [DataMember(Name = "files")]
        public IList<string> Files { get; set; } = new List<string>();

        public PackageManifestDto()
        {
            
        }

        public PackageManifestDto(int portalId, PackageInfo package) : base(portalId, package)
        {
            
        }
    }
}