// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Utilities;

    /// -----------------------------------------------------------------------------
    /// <summary>
    /// SectionHeadControl is a user control that provides all the server code to allow a
    /// section to be collapsed/expanded, using user provided images for the button.
    /// </summary>
    /// <remarks>
    /// To use this control the user must provide somewhere in the asp page the
    /// implementation of the javascript required to expand/collapse the display.
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class SectionHeadControl : UserControl
    {
        protected ImageButton imgIcon;
        protected Label lblTitle;
        protected Panel pnlRule;
        private bool _includeRule;
        private bool _isExpanded = true;
        private string _javaScript = "__dnn_SectionMaxMin";
        private string _maxImageUrl = "images/plus.gif";
        private string _minImageUrl = "images/minus.gif";

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets cssClass determines the Css Class used for the Title Text.
        /// </summary>
        /// <value>A string representing the name of the css class.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string CssClass
        {
            get
            {
                return this.lblTitle.CssClass;
            }

            set
            {
                this.lblTitle.CssClass = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether includeRule determines whether there is a horizontal rule displayed under the
        /// header text.
        /// </summary>
        /// <value>A string representing true or false.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool IncludeRule
        {
            get
            {
                return this._includeRule;
            }

            set
            {
                this._includeRule = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets a value indicating whether isExpanded determines whether the section is expanded or collapsed.
        /// </summary>
        /// <value>Boolean value that determines whether the panel is expanded (true)
        /// or collapsed (false).  The default is true.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public bool IsExpanded
        {
            get
            {
                return DNNClientAPI.MinMaxContentVisibile(this.imgIcon, -1, !this._isExpanded, DNNClientAPI.MinMaxPersistanceType.Page);
            }

            set
            {
                this._isExpanded = value;
                DNNClientAPI.MinMaxContentVisibile(this.imgIcon, -1, !this._isExpanded, DNNClientAPI.MinMaxPersistanceType.Page, value);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets javaScript is the name of the javascript function implementation.
        /// </summary>
        /// <value>A string representing the name of the javascript function implementation.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string JavaScript
        {
            get
            {
                return this._javaScript;
            }

            set
            {
                this._javaScript = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the MaxImageUrl is the url of the image displayed when the contained panel is
        /// collapsed.
        /// </summary>
        /// <value>A string representing the url of the Max Image.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string MaxImageUrl
        {
            get
            {
                return this._maxImageUrl;
            }

            set
            {
                this._maxImageUrl = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the MinImageUrl is the url of the image displayed when the contained panel is
        /// expanded.
        /// </summary>
        /// <value>A string representing the url of the Min Image.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string MinImageUrl
        {
            get
            {
                return this._minImageUrl;
            }

            set
            {
                this._minImageUrl = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the ResourceKey is the key used to identify the Localization Resource for the
        /// title text.
        /// </summary>
        /// <value>A string representing the ResourceKey.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string ResourceKey { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Section is the Id of the DHTML object  that contains the xection content
        /// title text.
        /// </summary>
        /// <value>A string representing the Section.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string Section { get; set; }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the Text is the name or title of the section.
        /// </summary>
        /// <value>A string representing the Title Text.</value>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        public string Text
        {
            get
            {
                return this.lblTitle.Text;
            }

            set
            {
                this.lblTitle.Text = value;
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Assign resource key to label for localization.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // set the resourcekey attribute to the label
                if (!string.IsNullOrEmpty(this.ResourceKey))
                {
                    this.lblTitle.Attributes["resourcekey"] = this.ResourceKey;
                }
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Renders the SectionHeadControl.
        /// </summary>
        /// <param name="e"></param>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                var ctl = (HtmlControl)this.Parent.FindControl(this.Section);
                if (ctl != null)
                {
                    this.lblTitle.Attributes.Add("onclick", this.imgIcon.ClientID + ".click();");
                    this.lblTitle.Attributes.Add("style", "cursor: pointer");
                    DNNClientAPI.EnableMinMax(this.imgIcon, ctl, !this.IsExpanded, this.Page.ResolveUrl(this.MinImageUrl), this.Page.ResolveUrl(this.MaxImageUrl), DNNClientAPI.MinMaxPersistanceType.Page);
                }

                // optionlly show hr
                this.pnlRule.Visible = this._includeRule;
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void imgIcon_Click(object sender, ImageClickEventArgs e)
        {
            var ctl = (HtmlControl)this.Parent.FindControl(this.Section);
            if (ctl != null)
            {
                this.IsExpanded = !this.IsExpanded;
            }
        }
    }
}
