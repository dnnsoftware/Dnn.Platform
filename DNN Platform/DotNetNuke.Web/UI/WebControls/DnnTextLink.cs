// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Usings

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnTextLink : WebControl, ILocalizable
    {
        private bool _localize = true;
        private HyperLink _textHyperlinkControl;

        public DnnTextLink() : base("span")
        {
            CssClass = "dnnTextLink";
            DisabledCssClass = "dnnTextLink disabled";
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Text
        {
            get
            {
                return TextHyperlinkControl.Text;
            }
            set
            {
                TextHyperlinkControl.Text = value;
            }
        }

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public override string ToolTip
        {
            get
            {
                return TextHyperlinkControl.ToolTip;
            }
            set
            {
                TextHyperlinkControl.ToolTip = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Localizable(true)]
        public string NavigateUrl
        {
            get
            {
                return TextHyperlinkControl.NavigateUrl;
            }
            set
            {
                TextHyperlinkControl.NavigateUrl = value;
            }
        }

        [Bindable(true)]
        [Category("Behavior")]
        [DefaultValue("")]
        [Localizable(true)]
        public string Target
        {
            get
            {
                return TextHyperlinkControl.Target;
            }
            set
            {
                TextHyperlinkControl.Target = value;
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

        private HyperLink TextHyperlinkControl
        {
            get
            {
                if (_textHyperlinkControl == null)
                {
                    _textHyperlinkControl = new HyperLink();
                }
                return _textHyperlinkControl;
            }
        }

        protected override void CreateChildControls()
        {
            Controls.Clear();
            Controls.Add(TextHyperlinkControl);
        }

        #region "Protected Methods"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            LocalizeStrings();

            if ((!Enabled))
            {
                if ((!string.IsNullOrEmpty(DisabledCssClass)))
                {
                    CssClass = DisabledCssClass;
                }
                NavigateUrl = "javascript:void(0);";
            }

            base.RenderBeginTag(writer);
            base.RenderChildren(writer);
            base.RenderEndTag(writer);
        }

        #endregion

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
