#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Modules.HTMLEditorProvider;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Personalization;

#endregion

namespace DotNetNuke.UI.UserControls
{
    /// -----------------------------------------------------------------------------
    /// Class:  TextEditor
    /// Project: DotNetNuke
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// TextEditor is a user control that provides a wrapper for the HtmlEditor providers
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    [ValidationPropertyAttribute("Text")]
    public class TextEditor : UserControl
    {
        #region Private Members

        private const string MyFileName = "TextEditor.ascx";
        private HtmlEditorProvider _richTextEditor;
        protected Panel PanelTextEditor;
        protected RadioButtonList OptRender;
        protected RadioButtonList OptView;
        protected PlaceHolder PlcEditor;
        protected HtmlGenericControl DivBasicTextBox;
        protected HtmlGenericControl DivBasicRender;
        protected HtmlGenericControl DivRichTextBox;
        protected Panel PanelView;
        protected TextBox TxtDesktopHTML;

        public TextEditor()
        {
            HtmlEncode = true;
            ChooseRender = true;
            ChooseMode = true;
        }

        #endregion

		#region Properties

        ///<summary>Enables/Disables the option to allow the user to select between Rich/Basic Mode, Default is true.</summary>
        public bool ChooseMode { get; set; }

        ///<summary>Determines wether or not the Text/Html button is rendered for Basic mode, Default is True</summary>
        public bool ChooseRender { get; set; }

        ///<summary>Gets/Sets the Default mode of the control, either "RICH" or "BASIC", Defaults to Rich</summary>
		public string DefaultMode
        {
            get
            {
                return ViewState["DefaultMode"] == null || String.IsNullOrEmpty(ViewState["DefaultMode"].ToString()) ? "RICH" : ViewState["DefaultMode"].ToString();
            }
            set
            {
                if (value.ToUpper() != "BASIC")
                {
                    ViewState["DefaultMode"] = "RICH";
                }
                else
                {
                    ViewState["DefaultMode"] = "BASIC";
                }
            }
        }

        ///<summary>Gets/Sets the Height of the control</summary>
		public Unit Height { get; set; }

        ///<summary>Turns on HtmlEncoding of text.  If this option is on the control will assume, it is being passed encoded text and will decode.</summary>
        public bool HtmlEncode { get; set; }

        ///<summary>The current mode of the control "RICH",  "BASIC"</summary>
		public string Mode
        {
            get
            {
                string strMode = "";
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

                //Check if Personal Preference is set
                if (objUserInfo.UserID >= 0)
                {
                    if (Personalization.GetProfile("DotNetNuke.TextEditor", "PreferredTextEditor") != null)
                    {
                        strMode = Convert.ToString(Personalization.GetProfile("DotNetNuke.TextEditor", "PreferredTextEditor"));
                    }
                }
				
				//If no Preference Check if Viewstate has been saved
                if (String.IsNullOrEmpty(strMode))
                {
                    if (ViewState["DesktopMode"] != null && !String.IsNullOrEmpty(ViewState["DesktopMode"].ToString()))
                    {
                        strMode = Convert.ToString(ViewState["DesktopMode"]);
                    }
                }
				
				//Finally if still no value Use default
                if (String.IsNullOrEmpty(strMode))
                {
                    strMode = DefaultMode;
                }

                if (strMode == "RICH" && !IsRichEditorAvailable)
                {
                    strMode = "BASIC";
                }
                return strMode;
            }
            set
            {
                UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

                if (value.ToUpper() != "BASIC")
                {
                    ViewState["DesktopMode"] = "RICH";

                    if (objUserInfo.UserID >= 0)
                    {
                        Personalization.SetProfile("DotNetNuke.TextEditor", "PreferredTextEditor", "RICH");
                    }
                }
                else
                {
                    ViewState["DesktopMode"] = "BASIC";

                    if (objUserInfo.UserID >= 0)
                    {
                        Personalization.SetProfile("DotNetNuke.TextEditor", "PreferredTextEditor", "BASIC");
                    }
                }
            }
        }

        ///<summary>Gets/Sets the Text of the control</summary>
		public string Text
        {
            get
            {
                switch (OptView.SelectedItem.Value)
                {
                    case "BASIC":
                        switch (OptRender.SelectedItem.Value)
                        {
                            case "T":
                                return Encode(HtmlUtils.ConvertToHtml(RemoveBaseTags(TxtDesktopHTML.Text)));
                                //break;
                            case "R":
                                return RemoveBaseTags(TxtDesktopHTML.Text);
                                //break;
                            default:
                                return Encode(RemoveBaseTags(TxtDesktopHTML.Text));
                                //break;
                        }
                    default:
                        return IsRichEditorAvailable ? Encode(RemoveBaseTags(_richTextEditor.Text)) : Encode(RemoveBaseTags(TxtDesktopHTML.Text));
                }
            }
            set
            {
				TxtDesktopHTML.Text = HtmlUtils.ConvertToText(Decode(value));
                if (IsRichEditorAvailable)
                {
                    _richTextEditor.Text = Decode(value);
                }
            }
        }

        ///<summary>Sets the render mode for Basic mode.  {Raw | HTML | Text}</summary>
		public string TextRenderMode
        {
            get
            {
                return Convert.ToString(ViewState["textrender"]);
            }
            set
            {
                var strMode = value.ToUpper().Substring(0, 1);
                if (strMode != "R" && strMode != "H" && strMode != "T")
                {
                    strMode = "H";
                }
                ViewState["textrender"] = strMode;
            }
        }

        ///<summary>Gets/Sets the Width of the control</summary>
		public Unit Width { get; set; }

        public bool IsRichEditorAvailable
        {
            get
            {
                return _richTextEditor != null;
            }
        }

        ///<summary>Allows public access ot the HtmlEditorProvider</summary>
		public HtmlEditorProvider RichText
        {
            get
            {
                return _richTextEditor;
            }
        }

        /// <summary>Allows public access of the BasicTextEditor</summary>
        public TextBox BasicTextEditor
        {
            get
            {
                return TxtDesktopHTML;
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
                return TemplateSourceDirectory + "/" + Localization.LocalResourceDirectory + "/" + MyFileName;
            }
        }
		
		#endregion

		#region Private Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Decodes the html
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strHtml">Html to decode</param>
        /// <returns>The decoded html</returns>
        /// -----------------------------------------------------------------------------
        private string Decode(string strHtml)
        {
            return HtmlEncode ? Server.HtmlDecode(strHtml) : strHtml;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Encodes the html
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="strHtml">Html to encode</param>
        /// <returns>The encoded html</returns>
        /// -----------------------------------------------------------------------------
        private string Encode(string strHtml)
        {
            return HtmlEncode ? Server.HtmlEncode(strHtml) : strHtml;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Builds the radio button lists
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void PopulateLists()
        {
            if (OptRender.Items.Count == 0)
            {
                OptRender.Items.Add(new ListItem(Localization.GetString("Text", Localization.GetResourceFile(this, MyFileName)), "T"));
                OptRender.Items.Add(new ListItem(Localization.GetString("Html", Localization.GetResourceFile(this, MyFileName)), "H"));
                OptRender.Items.Add(new ListItem(Localization.GetString("Raw", Localization.GetResourceFile(this, MyFileName)), "R"));
            }
            if (OptView.Items.Count == 0)
            {
                OptView.Items.Add(new ListItem(Localization.GetString("BasicTextBox", Localization.GetResourceFile(this, MyFileName)), "BASIC"));
                if (IsRichEditorAvailable)
                {
                    OptView.Items.Add(new ListItem(Localization.GetString("RichTextBox", Localization.GetResourceFile(this, MyFileName)), "RICH"));
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Sets the Mode displayed
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        private void SetPanels()
        {
            if (OptView.SelectedIndex != -1)
            {
                Mode = OptView.SelectedItem.Value;
            }
            if (!String.IsNullOrEmpty(Mode))
            {
                OptView.Items.FindByValue(Mode).Selected = true;
            }
            else
            {
                OptView.SelectedIndex = 0;
            }
			
			//Set the text render mode for basic mode
            if (OptRender.SelectedIndex != -1)
            {
                TextRenderMode = OptRender.SelectedItem.Value;
            }
            if (!String.IsNullOrEmpty(TextRenderMode))
            {
                OptRender.Items.FindByValue(TextRenderMode).Selected = true;
            }
            else
            {
                OptRender.SelectedIndex = 0;
            }
            if (OptView.SelectedItem.Value == "BASIC")
            {
                DivBasicTextBox.Visible = true;
                DivRichTextBox.Visible = false;
                PanelView.CssClass = "dnnTextPanelView dnnTextPanelView-basic";
            }
            else
            {
                DivBasicTextBox.Visible = false;
                DivRichTextBox.Visible = true;
                PanelView.CssClass = "dnnTextPanelView";
            }
        }

        private static string RemoveBaseTags(String strInput)
		{
            return Globals.BaseTagRegex.Replace(strInput, " ");
		}
		#endregion

        #region Public Methods

        public void ChangeMode(string mode)
        {
            OptView.SelectedItem.Value = mode;
            OptViewSelectedIndexChanged(OptView, EventArgs.Empty);
        }
        public void ChangeTextRenderMode(string textRenderMode)
        {
            OptRender.SelectedItem.Value = textRenderMode;
            OptRenderSelectedIndexChanged(OptRender, EventArgs.Empty);
        }

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            _richTextEditor = HtmlEditorProvider.Instance();

            if (IsRichEditorAvailable)
            {
                _richTextEditor.ControlID = ID;
                _richTextEditor.Initialize();
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Page_Load runs when the control is loaded
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            OptRender.SelectedIndexChanged += OptRenderSelectedIndexChanged;
            OptView.SelectedIndexChanged += OptViewSelectedIndexChanged;
            
            try
            {
				//Populate Radio Button Lists
                PopulateLists();

                //Get the current user
                //UserInfo objUserInfo = UserController.Instance.GetCurrentUserInfo();

                //Set the width and height of the controls
                if (IsRichEditorAvailable)
                {
                    _richTextEditor.Width = Width;
                    _richTextEditor.Height = Height;
                }

                TxtDesktopHTML.Height = Height;
                TxtDesktopHTML.Width = Width;
                PanelView.Width = Width;
                PanelTextEditor.Width = Width;

                //Optionally display the radio button lists
                if (!ChooseMode)
                {
                    PanelView.Visible = false;
                }
                if (!ChooseRender)
                {
                    DivBasicRender.Visible = false;
                }

                //Load the editor
                if (IsRichEditorAvailable)
                {
                    PlcEditor.Controls.Add(_richTextEditor.HtmlEditorControl);
                }

                SetPanels();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// optRender_SelectedIndexChanged runs when Basic Text Box mode is changed
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected void OptRenderSelectedIndexChanged(Object sender, EventArgs e)
        {
            if (OptRender.SelectedIndex != -1)
            {
                TextRenderMode = OptRender.SelectedItem.Value;
            }
            if (Mode == "BASIC")
            {
                TxtDesktopHTML.Text = TextRenderMode == "H" ? HtmlUtils.ConvertToHtml(TxtDesktopHTML.Text) : HtmlUtils.ConvertToText(TxtDesktopHTML.Text);
            }
            SetPanels();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// optView_SelectedIndexChanged runs when Editor Mode is changed
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// -----------------------------------------------------------------------------
        protected void OptViewSelectedIndexChanged(Object sender, EventArgs e)
        {
            if (OptView.SelectedIndex != -1)
            {
                Mode = OptView.SelectedItem.Value;
            }
            if (Mode == "BASIC")
            {
                switch (TextRenderMode)
                {
                    case "T":
                        TxtDesktopHTML.Text = HtmlUtils.ConvertToText(_richTextEditor.Text);
                        break;
                    default:
                        TxtDesktopHTML.Text = _richTextEditor.Text;
                        break;
                }
            }
            else
            {
                switch (TextRenderMode)
                {
                    case "T":
                        _richTextEditor.Text = HtmlUtils.ConvertToHtml(TxtDesktopHTML.Text);
                        break;
                    default:
                        _richTextEditor.Text = TxtDesktopHTML.Text;
                        break;
                }
            }
            SetPanels();
        }
		
		#endregion

    }
}
