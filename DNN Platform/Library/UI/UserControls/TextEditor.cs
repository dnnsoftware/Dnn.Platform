// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.UI.UserControls
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.HtmlControls;
    using System.Web.UI.WebControls;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Modules.HTMLEditorProvider;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Personalization;

    /// <summary>TextEditor is a user control that provides a wrapper for the HtmlEditor providers.</summary>
    [ValidationPropertyAttribute("Text")]
    public class TextEditor : UserControl
    {
        // ReSharper disable InconsistentNaming
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected Panel PanelTextEditor;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RadioButtonList OptRender;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected RadioButtonList OptView;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected PlaceHolder PlcEditor;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl DivBasicTextBox;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl DivBasicRender;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected HtmlGenericControl DivRichTextBox;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected Panel PanelView;
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1306:FieldNamesMustBeginWithLowerCaseLetter", Justification = "Breaking Change")]
        [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Breaking change")]
        protected TextBox TxtDesktopHTML;

        // ReSharper restore InconsistentNaming
        private const string MyFileName = "TextEditor.ascx";
        private HtmlEditorProvider richTextEditor;

        /// <summary>Initializes a new instance of the <see cref="TextEditor"/> class.</summary>
        public TextEditor()
        {
            this.HtmlEncode = true;
            this.ChooseRender = true;
            this.ChooseMode = true;
        }

        public bool IsRichEditorAvailable
        {
            get
            {
                return this.richTextEditor != null;
            }
        }

        /// <summary>Gets allows public access ot the HtmlEditorProvider.</summary>
        public HtmlEditorProvider RichText
        {
            get
            {
                return this.richTextEditor;
            }
        }

        /// <summary>Gets allows public access of the BasicTextEditor.</summary>
        public TextBox BasicTextEditor
        {
            get
            {
                return this.TxtDesktopHTML;
            }
        }

        public string OptViewClientId
        {
            get
            {
                return this.OptView.ClientID;
            }
        }

        public string LocalResourceFile
        {
            get
            {
                return this.TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + MyFileName;
            }
        }

        /// <summary>Gets or sets a value indicating whether enables/Disables the option to allow the user to select between Rich/Basic Mode, Default is true.</summary>
        public bool ChooseMode { get; set; }

        /// <summary>Gets or sets a value indicating whether the Text/Html button is rendered for Basic mode, Default is True.</summary>
        public bool ChooseRender { get; set; }

        /// <summary>Gets or sets /Sets the Default mode of the control, either "RICH" or "BASIC", Defaults to Rich.</summary>
        public string DefaultMode
        {
            get
            {
                return this.ViewState["DefaultMode"] == null || string.IsNullOrEmpty(this.ViewState["DefaultMode"].ToString()) ? "RICH" : this.ViewState["DefaultMode"].ToString();
            }

            set
            {
                if (!value.Equals("BASIC", StringComparison.OrdinalIgnoreCase))
                {
                    this.ViewState["DefaultMode"] = "RICH";
                }
                else
                {
                    this.ViewState["DefaultMode"] = "BASIC";
                }
            }
        }

        /// <summary>Gets or sets /Sets the Height of the control.</summary>
        public Unit Height { get; set; }

        /// <summary>Gets or sets a value indicating whether turns on HtmlEncoding of text.  If this option is on the control will assume, it is being passed encoded text and will decode.</summary>
        public bool HtmlEncode { get; set; }

        /// <summary>Gets or sets the current mode of the control "RICH",  "BASIC".</summary>
        public string Mode
        {
            get
            {
                string strMode = string.Empty;
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

                // Check if Personal Preference is set
                if (objUserInfo.UserID >= 0)
                {
                    if (Personalization.GetProfile("DotNetNuke.TextEditor", "PreferredTextEditor") != null)
                    {
                        strMode = Convert.ToString(Personalization.GetProfile("DotNetNuke.TextEditor", "PreferredTextEditor"));
                    }
                }

                // If no Preference Check if Viewstate has been saved
                if (string.IsNullOrEmpty(strMode))
                {
                    if (this.ViewState["DesktopMode"] != null && !string.IsNullOrEmpty(this.ViewState["DesktopMode"].ToString()))
                    {
                        strMode = Convert.ToString(this.ViewState["DesktopMode"]);
                    }
                }

                // Finally if still no value Use default
                if (string.IsNullOrEmpty(strMode))
                {
                    strMode = this.DefaultMode;
                }

                if (strMode == "RICH" && !this.IsRichEditorAvailable)
                {
                    strMode = "BASIC";
                }

                return strMode;
            }

            set
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

                if (!value.Equals("BASIC", StringComparison.OrdinalIgnoreCase))
                {
                    this.ViewState["DesktopMode"] = "RICH";

                    if (objUserInfo.UserID >= 0)
                    {
                        Personalization.SetProfile("DotNetNuke.TextEditor", "PreferredTextEditor", "RICH");
                    }
                }
                else
                {
                    this.ViewState["DesktopMode"] = "BASIC";

                    if (objUserInfo.UserID >= 0)
                    {
                        Personalization.SetProfile("DotNetNuke.TextEditor", "PreferredTextEditor", "BASIC");
                    }
                }
            }
        }

        /// <summary>Gets or sets /Sets the Text of the control.</summary>
        public string Text
        {
            get
            {
                switch (this.OptView.SelectedItem.Value)
                {
                    case "BASIC":
                        switch (this.OptRender.SelectedItem.Value)
                        {
                            case "T":
                                return this.Encode(HtmlUtils.ConvertToHtml(RemoveBaseTags(this.TxtDesktopHTML.Text)));

                            // break;
                            case "R":
                                return RemoveBaseTags(this.TxtDesktopHTML.Text);

                            // break;
                            default:
                                return this.Encode(RemoveBaseTags(this.TxtDesktopHTML.Text));

                                // break;
                        }

                    default:
                        return this.IsRichEditorAvailable ? this.Encode(RemoveBaseTags(this.richTextEditor.Text)) : this.Encode(RemoveBaseTags(this.TxtDesktopHTML.Text));
                }
            }

            set
            {
                this.TxtDesktopHTML.Text = HtmlUtils.ConvertToText(this.Decode(value));
                if (this.IsRichEditorAvailable)
                {
                    this.richTextEditor.Text = this.Decode(value);
                }
            }
        }

        /// <summary>Gets or sets the render mode for Basic mode.  {Raw | HTML | Text}.</summary>
        public string TextRenderMode
        {
            get
            {
                return Convert.ToString(this.ViewState["textrender"]);
            }

            set
            {
                var strMode = value.ToUpperInvariant().Substring(0, 1);
                if (!string.Equals(strMode, "R", StringComparison.Ordinal)
                    && !string.Equals(strMode, "H", StringComparison.Ordinal)
                    && !string.Equals(strMode, "T", StringComparison.Ordinal))
                {
                    strMode = "H";
                }

                this.ViewState["textrender"] = strMode;
            }
        }

        /// <summary>Gets or sets /Sets the Width of the control.</summary>
        public Unit Width { get; set; }

        public void ChangeMode(string mode)
        {
            this.OptView.SelectedItem.Value = mode;
            this.OptViewSelectedIndexChanged(this.OptView, EventArgs.Empty);
        }

        public void ChangeTextRenderMode(string textRenderMode)
        {
            this.OptRender.SelectedItem.Value = textRenderMode;
            this.OptRenderSelectedIndexChanged(this.OptRender, EventArgs.Empty);
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            this.richTextEditor = HtmlEditorProvider.Instance();

            if (this.IsRichEditorAvailable)
            {
                this.richTextEditor.ControlID = this.ID;
                this.richTextEditor.Initialize();
            }
        }

        /// <summary>Page_Load runs when the control is loaded.</summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.OptRender.SelectedIndexChanged += this.OptRenderSelectedIndexChanged;
            this.OptView.SelectedIndexChanged += this.OptViewSelectedIndexChanged;

            try
            {
                // Populate Radio Button Lists
                this.PopulateLists();

                // Get the current user
                // UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

                // Set the width and height of the controls
                if (this.IsRichEditorAvailable)
                {
                    this.richTextEditor.Width = this.Width;
                    this.richTextEditor.Height = this.Height;
                }

                this.TxtDesktopHTML.Height = this.Height;
                this.TxtDesktopHTML.Width = this.Width;
                this.PanelView.Width = this.Width;
                this.PanelTextEditor.Width = this.Width;

                // Optionally display the radio button lists
                if (!this.ChooseMode)
                {
                    this.PanelView.Visible = false;
                }

                if (!this.ChooseRender)
                {
                    this.DivBasicRender.Visible = false;
                }

                // Load the editor
                if (this.IsRichEditorAvailable)
                {
                    this.PlcEditor.Controls.Add(this.richTextEditor.HtmlEditorControl);
                }

                this.SetPanels();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>optRender_SelectedIndexChanged runs when Basic Text Box mode is changed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void OptRenderSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.OptRender.SelectedIndex != -1)
            {
                this.TextRenderMode = this.OptRender.SelectedItem.Value;
            }

            if (this.Mode == "BASIC")
            {
                this.TxtDesktopHTML.Text = this.TextRenderMode == "H" ? HtmlUtils.ConvertToHtml(this.TxtDesktopHTML.Text) : HtmlUtils.ConvertToText(this.TxtDesktopHTML.Text);
            }

            this.SetPanels();
        }

        /// <summary>optView_SelectedIndexChanged runs when Editor Mode is changed.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        protected void OptViewSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.OptView.SelectedIndex != -1)
            {
                this.Mode = this.OptView.SelectedItem.Value;
            }

            if (this.Mode == "BASIC")
            {
                switch (this.TextRenderMode)
                {
                    case "T":
                        this.TxtDesktopHTML.Text = HtmlUtils.ConvertToText(this.richTextEditor.Text);
                        break;
                    default:
                        this.TxtDesktopHTML.Text = this.richTextEditor.Text;
                        break;
                }
            }
            else
            {
                switch (this.TextRenderMode)
                {
                    case "T":
                        this.richTextEditor.Text = HtmlUtils.ConvertToHtml(this.TxtDesktopHTML.Text);
                        break;
                    default:
                        this.richTextEditor.Text = this.TxtDesktopHTML.Text;
                        break;
                }
            }

            this.SetPanels();
        }

        private static string RemoveBaseTags(string strInput)
        {
            return Globals.BaseTagRegex.Replace(strInput, " ");
        }

        /// <summary>Decodes the html.</summary>
        /// <param name="strHtml">Html to decode.</param>
        /// <returns>The decoded html.</returns>
        private string Decode(string strHtml)
        {
            return this.HtmlEncode ? this.Server.HtmlDecode(strHtml) : strHtml;
        }

        /// <summary>Encodes the html.</summary>
        /// <param name="strHtml">Html to encode.</param>
        /// <returns>The encoded html.</returns>
        private string Encode(string strHtml)
        {
            return this.HtmlEncode ? this.Server.HtmlEncode(strHtml) : strHtml;
        }

        /// <summary>Builds the radio button lists.</summary>
        private void PopulateLists()
        {
            if (this.OptRender.Items.Count == 0)
            {
                this.OptRender.Items.Add(new ListItem(Localization.GetString("Text", Localization.GetResourceFile(this, MyFileName)), "T"));
                this.OptRender.Items.Add(new ListItem(Localization.GetString("Html", Localization.GetResourceFile(this, MyFileName)), "H"));
                this.OptRender.Items.Add(new ListItem(Localization.GetString("Raw", Localization.GetResourceFile(this, MyFileName)), "R"));
            }

            if (this.OptView.Items.Count == 0)
            {
                this.OptView.Items.Add(new ListItem(Localization.GetString("BasicTextBox", Localization.GetResourceFile(this, MyFileName)), "BASIC"));
                if (this.IsRichEditorAvailable)
                {
                    this.OptView.Items.Add(new ListItem(Localization.GetString("RichTextBox", Localization.GetResourceFile(this, MyFileName)), "RICH"));
                }
            }
        }

        /// <summary>Sets the Mode displayed.</summary>
        private void SetPanels()
        {
            if (this.OptView.SelectedIndex != -1)
            {
                this.Mode = this.OptView.SelectedItem.Value;
            }

            if (!string.IsNullOrEmpty(this.Mode))
            {
                this.OptView.Items.FindByValue(this.Mode).Selected = true;
            }
            else
            {
                this.OptView.SelectedIndex = 0;
            }

            // Set the text render mode for basic mode
            if (this.OptRender.SelectedIndex != -1)
            {
                this.TextRenderMode = this.OptRender.SelectedItem.Value;
            }

            if (!string.IsNullOrEmpty(this.TextRenderMode))
            {
                this.OptRender.Items.FindByValue(this.TextRenderMode).Selected = true;
            }
            else
            {
                this.OptRender.SelectedIndex = 0;
            }

            if (this.OptView.SelectedItem.Value == "BASIC")
            {
                this.DivBasicTextBox.Visible = true;
                this.DivRichTextBox.Visible = false;
                this.PanelView.CssClass = "dnnTextPanelView dnnTextPanelView-basic";
            }
            else
            {
                this.DivBasicTextBox.Visible = false;
                this.DivRichTextBox.Visible = true;
                this.PanelView.CssClass = "dnnTextPanelView";
            }
        }
    }
}
