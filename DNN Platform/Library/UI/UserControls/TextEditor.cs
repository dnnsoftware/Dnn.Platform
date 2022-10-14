// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.UserControls
{
    using System;
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

    /// -----------------------------------------------------------------------------
    /// Class:  TextEditor
    /// Project: DotNetNuke
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// TextEditor is a user control that provides a wrapper for the HtmlEditor providers.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ValidationPropertyAttribute("Text")]
    public class TextEditor : UserControl
    {
        private const string MyFileName = "TextEditor.ascx";
        protected Panel panelTextEditor;
        protected RadioButtonList optRender;
        protected RadioButtonList optView;
        protected PlaceHolder plcEditor;
        protected HtmlGenericControl divBasicTextBox;
        protected HtmlGenericControl divBasicRender;
        protected HtmlGenericControl divRichTextBox;
        protected Panel panelView;
        protected TextBox txtDesktopHTML;
        private HtmlEditorProvider richTextEditor;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditor"/> class.
        /// </summary>
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
                return this.txtDesktopHTML;
            }
        }

        public string OptViewClientId
        {
            get
            {
                return this.optView.ClientID;
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

        /// <summary>Gets or sets a value indicating whether determines wether or not the Text/Html button is rendered for Basic mode, Default is True.</summary>
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
                switch (this.optView.SelectedItem.Value)
                {
                    case "BASIC":
                        switch (this.optRender.SelectedItem.Value)
                        {
                            case "T":
                                return this.Encode(HtmlUtils.ConvertToHtml(RemoveBaseTags(this.txtDesktopHTML.Text)));

                            // break;
                            case "R":
                                return RemoveBaseTags(this.txtDesktopHTML.Text);

                            // break;
                            default:
                                return this.Encode(RemoveBaseTags(this.txtDesktopHTML.Text));

                                // break;
                        }

                    default:
                        return this.IsRichEditorAvailable ? this.Encode(RemoveBaseTags(this.richTextEditor.Text)) : this.Encode(RemoveBaseTags(this.txtDesktopHTML.Text));
                }
            }

            set
            {
                this.txtDesktopHTML.Text = HtmlUtils.ConvertToText(this.Decode(value));
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
                var strMode = value.ToUpper().Substring(0, 1);
                if (strMode != "R" && strMode != "H" && strMode != "T")
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
            this.optView.SelectedItem.Value = mode;
            this.OptViewSelectedIndexChanged(this.optView, EventArgs.Empty);
        }

        public void ChangeTextRenderMode(string textRenderMode)
        {
            this.optRender.SelectedItem.Value = textRenderMode;
            this.OptRenderSelectedIndexChanged(this.optRender, EventArgs.Empty);
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

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.optRender.SelectedIndexChanged += this.OptRenderSelectedIndexChanged;
            this.optView.SelectedIndexChanged += this.OptViewSelectedIndexChanged;

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

                this.txtDesktopHTML.Height = this.Height;
                this.txtDesktopHTML.Width = this.Width;
                this.panelView.Width = this.Width;
                this.panelTextEditor.Width = this.Width;

                // Optionally display the radio button lists
                if (!this.ChooseMode)
                {
                    this.panelView.Visible = false;
                }

                if (!this.ChooseRender)
                {
                    this.divBasicRender.Visible = false;
                }

                // Load the editor
                if (this.IsRichEditorAvailable)
                {
                    this.plcEditor.Controls.Add(this.richTextEditor.HtmlEditorControl);
                }

                this.SetPanels();
            }
            catch (Exception exc) // Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// optRender_SelectedIndexChanged runs when Basic Text Box mode is changed.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected void OptRenderSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.optRender.SelectedIndex != -1)
            {
                this.TextRenderMode = this.optRender.SelectedItem.Value;
            }

            if (this.Mode == "BASIC")
            {
                this.txtDesktopHTML.Text = this.TextRenderMode == "H" ? HtmlUtils.ConvertToHtml(this.txtDesktopHTML.Text) : HtmlUtils.ConvertToText(this.txtDesktopHTML.Text);
            }

            this.SetPanels();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// optView_SelectedIndexChanged runs when Editor Mode is changed.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected void OptViewSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.optView.SelectedIndex != -1)
            {
                this.Mode = this.optView.SelectedItem.Value;
            }

            if (this.Mode == "BASIC")
            {
                switch (this.TextRenderMode)
                {
                    case "T":
                        this.txtDesktopHTML.Text = HtmlUtils.ConvertToText(this.richTextEditor.Text);
                        break;
                    default:
                        this.txtDesktopHTML.Text = this.richTextEditor.Text;
                        break;
                }
            }
            else
            {
                switch (this.TextRenderMode)
                {
                    case "T":
                        this.richTextEditor.Text = HtmlUtils.ConvertToHtml(this.txtDesktopHTML.Text);
                        break;
                    default:
                        this.richTextEditor.Text = this.txtDesktopHTML.Text;
                        break;
                }
            }

            this.SetPanels();
        }

        private static string RemoveBaseTags(string strInput)
        {
            return Globals.BaseTagRegex.Replace(strInput, " ");
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Decodes the html.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strHtml">Html to decode.</param>
        /// <returns>The decoded html.</returns>
        /// -----------------------------------------------------------------------------
        private string Decode(string strHtml)
        {
            return this.HtmlEncode ? this.Server.HtmlDecode(strHtml) : strHtml;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Encodes the html.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strHtml">Html to encode.</param>
        /// <returns>The encoded html.</returns>
        /// -----------------------------------------------------------------------------
        private string Encode(string strHtml)
        {
            return this.HtmlEncode ? this.Server.HtmlEncode(strHtml) : strHtml;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Builds the radio button lists.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void PopulateLists()
        {
            if (this.optRender.Items.Count == 0)
            {
                this.optRender.Items.Add(new ListItem(Localization.GetString("Text", Localization.GetResourceFile(this, MyFileName)), "T"));
                this.optRender.Items.Add(new ListItem(Localization.GetString("Html", Localization.GetResourceFile(this, MyFileName)), "H"));
                this.optRender.Items.Add(new ListItem(Localization.GetString("Raw", Localization.GetResourceFile(this, MyFileName)), "R"));
            }

            if (this.optView.Items.Count == 0)
            {
                this.optView.Items.Add(new ListItem(Localization.GetString("BasicTextBox", Localization.GetResourceFile(this, MyFileName)), "BASIC"));
                if (this.IsRichEditorAvailable)
                {
                    this.optView.Items.Add(new ListItem(Localization.GetString("RichTextBox", Localization.GetResourceFile(this, MyFileName)), "RICH"));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the Mode displayed.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void SetPanels()
        {
            if (this.optView.SelectedIndex != -1)
            {
                this.Mode = this.optView.SelectedItem.Value;
            }

            if (!string.IsNullOrEmpty(this.Mode))
            {
                this.optView.Items.FindByValue(this.Mode).Selected = true;
            }
            else
            {
                this.optView.SelectedIndex = 0;
            }

            // Set the text render mode for basic mode
            if (this.optRender.SelectedIndex != -1)
            {
                this.TextRenderMode = this.optRender.SelectedItem.Value;
            }

            if (!string.IsNullOrEmpty(this.TextRenderMode))
            {
                this.optRender.Items.FindByValue(this.TextRenderMode).Selected = true;
            }
            else
            {
                this.optRender.SelectedIndex = 0;
            }

            if (this.optView.SelectedItem.Value == "BASIC")
            {
                this.divBasicTextBox.Visible = true;
                this.divRichTextBox.Visible = false;
                this.panelView.CssClass = "dnnTextPanelView dnnTextPanelView-basic";
            }
            else
            {
                this.divBasicTextBox.Visible = false;
                this.divRichTextBox.Visible = true;
                this.panelView.CssClass = "dnnTextPanelView";
            }
        }
    }
}
