// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary></summary>
    /// <remarks></remarks>
    public class ModuleMessage : SkinObjectBase
    {
        protected Panel dnnSkinMessage;
        protected Label lblHeading;
        protected Label lblMessage;
        protected Control scrollScript;

        public enum ModuleMessageType
        {
            GreenSuccess,
            YellowWarning,
            RedError,
            BlueInfo,
        }

        /// <summary>
        /// Gets a value indicating whether check this message is shown as page message or module message.
        /// </summary>
        public bool IsModuleMessage
        {
            get
            {
                return this.Parent.ID == "MessagePlaceHolder";
            }
        }

        public string Text { get; set; }

        public string Heading { get; set; }

        public ModuleMessageType IconType { get; set; }

        public string IconImage { get; set; }

        /// <summary>
        /// The Page_Load server event handler on this page is used
        /// to populate the role information for the page.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                var strMessage = string.Empty;

                // check to see if a url
                // was passed in for an icon
                if (!string.IsNullOrEmpty(this.IconImage))
                {
                    strMessage += this.Text;
                    this.dnnSkinMessage.CssClass = "dnnFormMessage dnnFormWarning";
                }
                else
                {
                    switch (this.IconType)
                    {
                        case ModuleMessageType.GreenSuccess:
                            strMessage += this.Text;
                            this.dnnSkinMessage.CssClass = "dnnFormMessage dnnFormSuccess";
                            break;
                        case ModuleMessageType.YellowWarning:
                            strMessage += this.Text;
                            this.dnnSkinMessage.CssClass = "dnnFormMessage dnnFormWarning";
                            break;
                        case ModuleMessageType.BlueInfo:
                            strMessage += this.Text;
                            this.dnnSkinMessage.CssClass = "dnnFormMessage dnnFormInfo";
                            break;
                        case ModuleMessageType.RedError:
                            strMessage += this.Text;
                            this.dnnSkinMessage.CssClass = "dnnFormMessage dnnFormValidationSummary";
                            break;
                    }
                }

                this.lblMessage.Text = strMessage;

                if (!string.IsNullOrEmpty(this.Heading))
                {
                    this.lblHeading.Visible = true;
                    this.lblHeading.Text = this.Heading;
                }
            }
            catch (Exception exc) // Control failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc, false);
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // set the scroll js only shown for module message and in postback mode.
            this.scrollScript.Visible = this.IsPostBack && this.IsModuleMessage;
        }
    }
}
