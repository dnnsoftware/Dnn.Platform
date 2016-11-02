using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;

namespace Dnn.PersonaBar.Extensions.Components.Dto.Editors
{
    [JsonObject]
    public class AuthSystemPackageDetailDto : PackageInfoDto
    {
        [JsonProperty("authenticationType")]
        public string AuthenticationType { get; set; }

        [JsonProperty("settingUrl")]
        public string SettingUrl { get; set; }

        [JsonProperty("loginControlSource")]
        public string LoginControlSource { get; set; }

        [JsonProperty("logoffControlSource")]
        public string LogoffControlSource { get; set; }

        [JsonProperty("settingsControlSource")]
        public string SettingsControlSource { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        // special extension roperties
        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonProperty("appSecret")]
        public string AppSecret { get; set; }

        [JsonProperty("appEnabled")]
        public bool AppEnabled { get; set; }

        public AuthSystemPackageDetailDto()
        {
        }

        public AuthSystemPackageDetailDto(int portalId, PackageInfo package) : base(portalId, package)
        {

        }
    }
}