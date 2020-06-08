// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
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
