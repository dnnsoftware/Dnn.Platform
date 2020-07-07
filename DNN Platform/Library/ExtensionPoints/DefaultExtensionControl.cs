// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.ExtensionPoints
{
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.UI.Modules;

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
                var s = (string)this.ViewState["Module"];
                return s ?? string.Empty;
            }

            set
            {
                this.ViewState["Module"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string Group
        {
            get
            {
                var s = (string)this.ViewState["Group"];
                return s ?? string.Empty;
            }

            set
            {
                this.ViewState["Group"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string Name
        {
            get
            {
                var s = (string)this.ViewState["Name"];
                return s ?? string.Empty;
            }

            set
            {
                this.ViewState["Name"] = value;
            }
        }

        public ModuleInstanceContext ModuleContext { get; set; }
    }
}
