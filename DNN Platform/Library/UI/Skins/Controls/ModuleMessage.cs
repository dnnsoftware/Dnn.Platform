// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Exceptions;

    /// <summary>A skin object to display a message from a module.</summary>
    public class ModuleMessage : SkinObjectBase
    {
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Panel dnnSkinMessage;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblHeading;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Label lblMessage;
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Breaking change")]
        protected Control scrollScript;

        public enum ModuleMessageType
        {
            /// <summary>A green success message.</summary>
            GreenSuccess = 0,

            /// <summary>A yellow warning message.</summary>
            YellowWarning = 1,

            /// <summary>A red error message.</summary>
            RedError = 2,

            /// <summary>A blue info message.</summary>
            BlueInfo = 3,
        }

        /// <summary>Gets a value indicating whether check this message is shown as page message or module message.</summary>
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

        /// <summary>The Page_Load server event handler on this page is used to populate the role information for the page.</summary>
        /// <param name="e">The event arguments.</param>
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
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc, false);
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            // set the scroll js only shown for module message and in postback mode.
            this.scrollScript.Visible = this.IsPostBack && this.IsModuleMessage;
        }
    }
}
