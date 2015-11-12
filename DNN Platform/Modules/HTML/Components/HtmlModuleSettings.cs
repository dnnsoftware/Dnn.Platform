using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Settings;
using System.Web.Caching;

namespace DotNetNuke.Modules.Html.Components
{
    public class HtmlModuleSettings
    {
        [ModuleSetting(Prefix = "HtmlText_", DefaultValue = false)]
        public bool ReplaceTokens { get; set; }

        [ModuleSetting(Prefix = "HtmlText_", DefaultValue = true)]
        public bool UseDecorate { get; set; }

        [ModuleSetting(Prefix = "HtmlText_", DefaultValue = 100)]
        public int SearchDescLength { get; set; }

        [ModuleSetting(DefaultValue = -1)]
        public int WorkFlowID { get; set; }

        [ModuleSetting(Prefix = "Content_", DefaultValue = -1)]
        public int LockedBy { get; set; }
    }

    public class HtmlModuleSettingsRepository : SettingsRepository<HtmlModuleSettings>
    {
    }
}