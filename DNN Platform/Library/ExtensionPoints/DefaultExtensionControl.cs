using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:DefaultExtensionControl runat=server></{0}:DefaultExtensionControl>")]
    public class DefaultExtensionControl : WebControl
    {
        [Bindable(true)]
        [DefaultValue("")]
        public string Module
        {
            get
            {
                var s = (string)ViewState["Module"];
                return s ?? string.Empty;
            }
            set
            {
                ViewState["Module"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string Group
        {
            get
            {
                var s = (string)ViewState["Group"];
                return s ?? string.Empty;
            }
            set
            {
                ViewState["Group"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string Name
        {
            get
            {
                var s = (string)ViewState["Name"];
                return s ?? string.Empty;
            }
            set
            {
                ViewState["Name"] = value;
            }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
