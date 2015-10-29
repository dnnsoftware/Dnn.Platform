using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Settings;
using System.Web.Caching;

namespace DotNetNuke.Modules.Html.Components
{
    public class HtmlModuleSettings
    {
        [ModuleSetting(Prefix = "HtmlText_", Default = false)]
        public bool ReplaceTokens { get; set; }

        [ModuleSetting(Prefix = "HtmlText_", Default = true)]
        public bool UseDecorate { get; set; }

        [ModuleSetting(Prefix = "HtmlText_", Default = 100)]
        public int SearchDescLength { get; set; }
    }
}