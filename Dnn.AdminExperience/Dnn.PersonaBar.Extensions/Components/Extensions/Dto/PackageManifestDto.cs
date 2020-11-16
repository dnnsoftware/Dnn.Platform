// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Extensions.Components.Dto
{
    using System.Collections.Generic;

    using DotNetNuke.Services.Installer.Packages;
    using Newtonsoft.Json;

    [JsonObject]
    public class PackageManifestDto : PackageInfoDto
    {
        public PackageManifestDto()
        {
        }

        public PackageManifestDto(int portalId, PackageInfo package) : base(portalId, package)
        {
        }

        [JsonProperty("archiveName")]
        public string ArchiveName { get; set; }

        [JsonProperty("manifestName")]
        public string ManifestName { get; set; }

        [JsonProperty("basePath")]
        public string BasePath { get; set; }

        [JsonProperty("manifests")]
        public IDictionary<string, string> Manifests { get; set; } = new Dictionary<string, string>();

        [JsonProperty("assemblies")]
        public IList<string> Assemblies { get; set; } = new List<string>();

        [JsonProperty("files")]
        public IList<string> Files { get; set; } = new List<string>();
    }
}
