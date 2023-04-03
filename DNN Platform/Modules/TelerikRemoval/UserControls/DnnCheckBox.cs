// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.TelerikRemoval.UserControls
{
    using System.Web.UI;

    using DotNetNuke.Web.UI;

    using WrappedDnnCheckBox = DotNetNuke.Web.UI.WebControls.DnnCheckBox;

    /// <summary>
    /// A wrapper around <see cref="WrappedDnnCheckBox"/>
    /// to support localization.
    /// </summary>
    public class DnnCheckBox : WrappedDnnCheckBox, ILocalizable
    {
        private bool localize = true;

        /// <inheritdoc />
        public string LocalResourceFile { get; set; }

        /// <inheritdoc/>
        public bool Localize
        {
            get
            {
                if (this.DesignMode)
                {
                    return false;
                }

                return this.localize;
            }

            set
            {
                this.localize = value;
            }
        }

        /// <inheritdoc/>
        public virtual void LocalizeStrings()
        {
            if (!this.Localize)
            {
                return;
            }

            if (!string.IsNullOrEmpty(this.ToolTip))
            {
                this.ToolTip = Utilities.GetLocalizedStringFromParent(this.ToolTip, this);
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                var unlocalizedText = this.Text;
                this.Text = Utilities.GetLocalizedStringFromParent(unlocalizedText, this);
                if (string.IsNullOrEmpty(this.Text))
                {
                    this.Text = unlocalizedText;
                }

                if (string.IsNullOrEmpty(this.ToolTip))
                {
                    this.ToolTip = Utilities.GetLocalizedStringFromParent(unlocalizedText + ".ToolTip", this);
                    if (string.IsNullOrEmpty(this.ToolTip))
                    {
                        this.ToolTip = unlocalizedText;
                    }
                }
            }
        }

        /// <inheritdoc />
        protected override void Render(HtmlTextWriter writer)
        {
            this.LocalizeStrings();
            base.Render(writer);
        }
    }
}
