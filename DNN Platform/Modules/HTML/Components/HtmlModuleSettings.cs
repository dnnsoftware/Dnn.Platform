using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Settings;
using System.Web.Caching;

namespace DotNetNuke.Modules.Html.Components
{
    public class HtmlModuleSettings
    {
        [ModuleSetting(Prefix = "HtmlText_")]
        public bool ReplaceTokens { get; set; } = false;

        [ModuleSetting(Prefix = "HtmlText_")]
        public bool UseDecorate { get; set; } = true;

        [ModuleSetting(Prefix = "HtmlText_")]
        public int SearchDescLength { get; set; } = 100;

        [ModuleSetting()]
        public int WorkFlowID { get; set; } = -1;

    }

    public class HtmlModuleSettingsRepository : SettingsRepository<HtmlModuleSettings>
    {
    }
}