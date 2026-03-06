// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework.JavaScriptLibraries;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;

    using Microsoft.Extensions.DependencyInjection;

    using Globals = DotNetNuke.Common.Globals;

    /// <summary>A form label control.</summary>
    public class DnnFormLabel : Panel
    {
        private readonly IApplicationStatusInfo appStatus;
        private readonly IEventLogger eventLogger;

        /// <summary>Initializes a new instance of the <see cref="DnnFormLabel"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IApplicationStatusInfo. Scheduled removal in v12.0.0.")]
        public DnnFormLabel()
            : this(null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DnnFormLabel"/> class.</summary>
        /// <param name="appStatus">The application status.</param>
        /// <param name="eventLogger">The event logger.</param>
        public DnnFormLabel(IApplicationStatusInfo appStatus, IEventLogger eventLogger)
        {
            this.appStatus = appStatus ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationStatusInfo>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        }

        /// <summary>Gets or sets the ID of the associated control.</summary>
        public string AssociatedControlID { get; set; }

        /// <summary>Gets or sets the path to the local resource file.</summary>
        public string LocalResourceFile { get; set; }

        /// <summary>Gets or sets the resource key of the label.</summary>
        public string ResourceKey { get; set; }

        /// <summary>Gets or sets the resource key of the tooltip.</summary>
        public string ToolTipKey { get; set; }

        /// <summary>Gets or sets a value indicating whether the field is required.</summary>
        public bool RequiredField { get; set; }

        /// <inheritdoc />
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

            ////var outerPanel = new Panel();
            ////outerPanel.CssClass = "dnnLabel";
            ////Controls.Add(outerPanel);
            var outerLabel = new System.Web.UI.HtmlControls.HtmlGenericControl { TagName = "label", };
            this.Controls.Add(outerLabel);

            var label = new Label { ID = "Label", Text = this.LocalizeString(this.ResourceKey), };
            if (this.RequiredField)
            {
                label.CssClass += " dnnFormRequired";
            }

            outerLabel.Controls.Add(label);

            var link = new LinkButton { ID = "Link", CssClass = "dnnFormHelp", TabIndex = -1, };
            link.Attributes.Add("aria-label", "Help");
            this.Controls.Add(link);

            if (!string.IsNullOrEmpty(toolTipText))
            {
                ////CssClass += "dnnLabel";
                var tooltipPanel = new Panel { CssClass = "dnnTooltip", };
                this.Controls.Add(tooltipPanel);

                var panel = new Panel { ID = "Help", CssClass = "dnnFormHelpContent dnnClear", };
                tooltipPanel.Controls.Add(panel);

                var helpLabel = new Label { ID = "Text", CssClass = "dnnHelpText", Text = this.LocalizeString(this.ToolTipKey), };
                panel.Controls.Add(helpLabel);

                var pinLink = new HyperLink { CssClass = "pinHelp", };
                pinLink.Attributes.Add("href", "#");
                pinLink.Attributes.Add("aria-label", "Pin");
                panel.Controls.Add(pinLink);

                JavaScript.RegisterClientReference(this.Page, ClientAPI.ClientNamespaceReferences.dnn);
                JavaScript.RequestRegistration(this.appStatus, this.eventLogger, PortalSettings.Current, CommonJs.DnnPlugins);

                ////ClientResourceManager.RegisterScript(this.Page, "~/Resources/Shared/Scripts/initTooltips.js");
            }
        }

        /// <summary>Gets the localized string corresponding to the <paramref name="key"/>.</summary>
        /// <param name="key">The resource key to find.</param>
        /// <returns>The localized text.</returns>
        protected string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }
    }
}
