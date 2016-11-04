using System.Collections.Generic;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    [JsonObject]
    public class PackageManifestDto : PackageInfoDto
    {
        [JsonProperty("archiveName")]
        public string ArchiveName { get; set; }

        [JsonProperty("manifestName")]
        public string ManifestName { get; set; }

        [JsonProperty("basePath")]
        public string BasePath { get; set; }

        [JsonProperty("manifests")]
        public IDictionary<string, string> Manifests  { get; set; } = new Dictionary<string, string>();

        [JsonProperty("assemblies")]
        public IList<string> Assemblies { get; set; } = new List<string>();

        [JsonProperty("files")]
        public IList<string> Files { get; set; } = new List<string>();

        public PackageManifestDto()
        {
            
        }

        public PackageManifestDto(int portalId, PackageInfo package) : base(portalId, package)
        {
            
        }
    }
}