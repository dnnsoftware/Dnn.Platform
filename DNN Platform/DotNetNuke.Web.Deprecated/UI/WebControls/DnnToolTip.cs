// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.UI.WebControls
{
    using System;
    using System.Web.UI;

    using DotNetNuke.Web.UI;
    using Telerik.Web.UI;

    public class DnnToolTip : RadToolTip, ILocalizable
    {
        private bool _localize = true;

        public string ResourceKey { get; set; }

        public bool Localize
        {
            get
            {
                if (this.DesignMode)
                {
                    return false;
                }

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
            if (this.Localize && (!string.IsNullOrEmpty(this.ResourceKey)))
            {
                if (!string.IsNullOrEmpty(this.ManualCloseButtonText))
                {
                    this.ManualCloseButtonText = Utilities.GetLocalizedStringFromParent(string.Format("{0}.ManualCloseButtonText", this.ResourceKey), this);
                }

                if (!string.IsNullOrEmpty(this.Text))
                {
                    this.Text = Utilities.GetLocalizedStringFromParent(string.Format("{0}.Text", this.ResourceKey), this);
                }

                if (!string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = Utilities.GetLocalizedStringFromParent(string.Format("{0}.ToolTip", this.ResourceKey), this);
                }
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            this.LocalizeStrings();
            base.Render(writer);
        }
    }
}
