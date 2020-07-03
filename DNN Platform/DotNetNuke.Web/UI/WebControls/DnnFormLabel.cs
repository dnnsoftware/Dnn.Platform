// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Framework;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.UserControls;
    using DotNetNuke.UI.Utilities;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    public class DnnFormLabel : Panel
    {
        public string AssociatedControlID { get; set; }

        public string LocalResourceFile { get; set; }

        public string ResourceKey { get; set; }

        public string ToolTipKey { get; set; }

        public bool RequiredField { get; set; }

        protected override void CreateChildControls()
        {
            string toolTipText = this.LocalizeString(this.ToolTipKey);
            if (string.IsNullOrEmpty(this.CssClass))
            {
                this.CssClass = "dnnLabel";
            }
            else if (!this.CssClass.Contains("dnnLabel"))
            {
                this.CssClass += " dnnLabel";
            }

            // var outerPanel = new Panel();
            // outerPanel.CssClass = "dnnLabel";
            // Controls.Add(outerPanel);
            var outerLabel = new System.Web.UI.HtmlControls.HtmlGenericControl { TagName = "label" };
            this.Controls.Add(outerLabel);

            var label = new Label { ID = "Label", Text = this.LocalizeString(this.ResourceKey) };
            if (this.RequiredField)
            {
                label.CssClass += " dnnFormRequired";
            }

            outerLabel.Controls.Add(label);

            var link = new LinkButton { ID = "Link", CssClass = "dnnFormHelp", TabIndex = -1 };
            link.Attributes.Add("aria-label", "Help");
            this.Controls.Add(link);

            if (!string.IsNullOrEmpty(toolTipText))
            {
                // CssClass += "dnnLabel";
                var tooltipPanel = new Panel() { CssClass = "dnnTooltip" };
                this.Controls.Add(tooltipPanel);

                var panel = new Panel { ID = "Help", CssClass = "dnnFormHelpContent dnnClear" };
                tooltipPanel.Controls.Add(panel);

                var helpLabel = new Label { ID = "Text", CssClass = "dnnHelpText", Text = this.LocalizeString(this.ToolTipKey) };
                panel.Controls.Add(helpLabel);

                var pinLink = new HyperLink { CssClass = "pinHelp" };
                pinLink.Attributes.Add("href", "#");
                pinLink.Attributes.Add("aria-label", "Pin");
                panel.Controls.Add(pinLink);

                JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
                JavaScript.RequestRegistration(CommonJs.DnnPlugins);

                // ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
            }
        }

        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }
    }
}
