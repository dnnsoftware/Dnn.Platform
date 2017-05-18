#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnButton : Button, ILocalizable
    {
        private bool _Localize = true;

        #region "Constructors"

        public DnnButton()
        {
            CssClass = "CommandButton";
            DisabledCssClass = "CommandButtonDisabled";
        }

        #endregion

        #region "Public Properties"

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ConfirmMessage
        {
            get
            {
                return ViewState["ConfirmMessage"] == null ? string.Empty : ViewState["ConfirmMessage"].ToString();
            }
            set
            {
                ViewState["ConfirmMessage"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public new string DisabledCssClass
        {
            get
            {
                return ViewState["DisabledCssClass"] == null ? string.Empty : ViewState["DisabledCssClass"].ToString();
            }
            set
            {
                ViewState["DisabledCssClass"] = value;
            }
        }

        #endregion

        #region "Protected Methods"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if ((!Enabled))
            {
                CssClass = DisabledCssClass;
            }

            if ((!string.IsNullOrEmpty(ConfirmMessage)))
            {
                string msg = ConfirmMessage;
                if ((Localize))
                {
                    msg = Utilities.GetLocalizedStringFromParent(ConfirmMessage, this);
                }
                //must be done before render
                OnClientClick = Utilities.GetOnClientClickConfirm(this, msg);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            LocalizeStrings();
            base.Render(writer);
        }

        #endregion

        #region "ILocalizable Implementation"

        public bool Localize
        {
            get
            {
                if (DesignMode)
                {
                    return false;
                }
                return _Localize;
            }
            set
            {
                _Localize = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
            if ((Localize))
            {
                if ((!string.IsNullOrEmpty(ToolTip)))
                {
                    ToolTip = Utilities.GetLocalizedStringFromParent(ToolTip, this);
                }

                if ((!string.IsNullOrEmpty(Text)))
                {
                    string unlocalizedText = Text;
                    Text = Utilities.GetLocalizedStringFromParent(unlocalizedText, this);
                    if (String.IsNullOrEmpty(Text))
                    {
                        Text = unlocalizedText;
                    }

                    if ((string.IsNullOrEmpty(ToolTip)))
                    {
                        ToolTip = Utilities.GetLocalizedStringFromParent(unlocalizedText + ".ToolTip", this);
                        if (String.IsNullOrEmpty(ToolTip))
                        {
                            ToolTip = unlocalizedText;
                        }
                    }
                }
            }
        }

        #endregion
    }
}