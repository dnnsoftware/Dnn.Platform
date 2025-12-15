// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;

    /// <summary>Base class for Dnn specific WebControls.</summary>
    public abstract class WebControlBase : WebControl
    {
        private string styleSheetUrl = string.Empty;
        private string theme = string.Empty;

        /// <summary>Gets a url to the web control resources folder.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public string ResourcesFolderUrl => Globals.ResolveUrl("~/Resources/");

        /// <summary>Gets a value indicating whether the control is currently in a page visible only to hosts.</summary>
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public bool IsHostMenu => Globals.IsHostTab(TabController.CurrentPage.TabID);

        /// <summary>Gets the portal settings for the portal where the WebControl is displayed.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use PortalController.Instance.GetCurrentSettings() instead, if you need access to the ActiveTab, use TabController.CurrentPage. Scheduled for removal in v11.0.0.")]
#pragma warning disable 612, 618 // GetCurrentPortalSettings is obsolete
        public PortalSettings PortalSettings => PortalController.Instance.GetCurrentPortalSettings();
#pragma warning restore 612, 618

        /// <summary>Gets the Html content for this WebControl rendering.</summary>
        public abstract string HtmlOutput { get; }

        /// <summary>Gets a value indicating whether this WebControl is currently displayed in the admin menu.</summary>
        [Obsolete("Deprecated in DotNetNuke 7.0.0. There is no longer the concept of an Admin Page.  All pages are controlled by Permissions. Scheduled for removal in v11.0.0.", true)]
        public bool IsAdminMenu => false;

        /// <summary>Gets or sets a string representing the theme to use for this WebControl.</summary>
        public string Theme
        {
            get => this.theme;
            set => this.theme = value;
        }

        /// <summary>Gets or sets the url to the stylesheet to use for this WebControl.</summary>
        public string StyleSheetUrl
        {
            get
            {
                if (this.styleSheetUrl.StartsWith("~"))
                {
                    return Globals.ResolveUrl(this.styleSheetUrl);
                }
                else
                {
                    return this.styleSheetUrl;
                }
            }

            set
            {
                this.styleSheetUrl = value;
            }
        }

        /// <summary>Renders the html for this WebControl to the output.</summary>
        /// <param name="output">The output to write to.</param>
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(this.HtmlOutput);
        }
    }
}
