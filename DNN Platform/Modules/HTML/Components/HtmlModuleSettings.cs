using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Settings;
using System.Web.Caching;
using System;

namespace DotNetNuke.Modules.Html.Components
{
    /// <summary>
    /// An example implementation of the <see cref="ModuleSettingAttribute"/>
    /// </summary>
    /// <remarks>
    /// HtmlModuleSettings provides a strongly typed list of properties used by 
    /// the HTML module.  Settings will automatically be serialized and deserialized
    /// for storage in the underlying settings table.
    /// </remarks>
    [Serializable]
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

    /// <summary>
    /// The <see cref="SettingsRepository{T}"/> used for storing and retrieving <see cref="HtmlModuleSettings"/>
    /// </summary>
    public class HtmlModuleSettingsRepository : SettingsRepository<HtmlModuleSettings>
    {
    }
}