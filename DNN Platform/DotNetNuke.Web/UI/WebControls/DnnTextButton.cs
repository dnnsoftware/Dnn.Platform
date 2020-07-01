// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Services.Localization;

    public class DnnTextButton : LinkButton, ILocalizable
    {
        private bool _localize = true;

        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue("")]
        [Localizable(true)]
        public string ConfirmMessage
        {
            get
            {
                return this.ViewState["ConfirmMessage"] == null ? string.Empty : (string)this.ViewState["ConfirmMessage"];
            }

            set
            {
                this.ViewState["ConfirmMessage"] = value;
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
                return this.ViewState["CssClass"] == null ? string.Empty : (string)this.ViewState["CssClass"];
            }

            set
            {
                this.ViewState["CssClass"] = value;
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
                return this.ViewState["DisabledCssClass"] == null ? string.Empty : (string)this.ViewState["DisabledCssClass"];
            }

            set
            {
                this.ViewState["DisabledCssClass"] = value;
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
                return this.ViewState["Text"] == null ? string.Empty : (string)this.ViewState["Text"];
            }

            set
            {
                this.ViewState["Text"] = value;
            }
        }

        public bool Localize
        {
            get
            {
                return this._localize;
            }

            set
            {
                this._localize = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
            if (this.Localize)
            {
                if (!string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = Localization.GetString(this.ToolTip, this.LocalResourceFile);
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.Text = Localization.GetString(this.Text, this.LocalResourceFile);

                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = Localization.GetString(string.Format("{0}.ToolTip", this.Text), this.LocalResourceFile);
                    }

                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = this.Text;
                    }
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            this.LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.LocalizeStrings();
            if (!this.Enabled && !string.IsNullOrEmpty(this.DisabledCssClass))
            {
                this.CssClass = this.DisabledCssClass;
            }

            writer.AddAttribute("class", this.CssClass.Trim());
            base.Render(writer);
        }
    }
}
