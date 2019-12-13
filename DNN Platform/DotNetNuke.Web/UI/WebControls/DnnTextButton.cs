// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTextButton : LinkButton, ILocalizable
    {
        private bool _localize = true;

        #region "Public Properties"

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ConfirmMessage
        {
            get
            {
                return ViewState["ConfirmMessage"] == null ? string.Empty : (string) ViewState["ConfirmMessage"];
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
        public override string CssClass
        {
            get
            {
                return ViewState["CssClass"] == null ? string.Empty : (string) ViewState["CssClass"];
            }
            set
            {
                ViewState["CssClass"] = value;
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
                return ViewState["DisabledCssClass"] == null ? string.Empty : (string) ViewState["DisabledCssClass"];
            }
            set
            {
                ViewState["DisabledCssClass"] = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public new string Text
        {
            get
            {
                return ViewState["Text"] == null ? string.Empty : (string) ViewState["Text"];
            }
            set
            {
                ViewState["Text"] = value;
            }
        }

        #endregion

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            LocalizeStrings();
            if (!Enabled && !string.IsNullOrEmpty(DisabledCssClass))
            {
                CssClass = DisabledCssClass;
            }
            writer.AddAttribute("class", CssClass.Trim());
            base.Render(writer);
        }

        #region "ILocalizable Implementation"

        public bool Localize
        {
            get
            {
                return _localize;
            }
            set
            {
                _localize = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
            if ((Localize))
            {
                if ((!string.IsNullOrEmpty(ToolTip)))
                {
                    ToolTip = Localization.GetString(ToolTip, LocalResourceFile);
                }

                if ((!string.IsNullOrEmpty(Text)))
                {
                    Text = Localization.GetString(Text, LocalResourceFile);

                    if ((string.IsNullOrEmpty(ToolTip)))
                    {
                        ToolTip = Localization.GetString(string.Format("{0}.ToolTip", Text), LocalResourceFile);
                    }

                    if ((string.IsNullOrEmpty(ToolTip)))
                    {
                        ToolTip = Text;
                    }
                }
            }
        }

        #endregion
    }
}
