// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{

    public class DnnLabel : Label, ILocalizable
    {

        private bool _localize = true;

        #region Constructors

        public DnnLabel()
        {
            CssClass = "dnnFormLabel";
        }

        #endregion

        #region "Protected Methods"

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            LocalResourceFile = Utilities.GetLocalResourceFile(this);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            LocalizeStrings();
            base.Render(writer);
        }

        #endregion

        #region ILocalizable Implementation

        public bool Localize
        {
            get
            {
                return !DesignMode && _localize;
            }
            set
            {
                _localize = value;
            }
        }

        public string LocalResourceFile { get; set; }

        public virtual void LocalizeStrings()
        {
            if (Localize)
            {
                if (!string.IsNullOrEmpty(ToolTip))
                {
                    ToolTip = Localization.GetString(ToolTip, LocalResourceFile);
                }

                if (!string.IsNullOrEmpty(Text))
                {
                    var unLocalized = Text;

                    Text = Localization.GetString(unLocalized, LocalResourceFile);

                    if (string.IsNullOrEmpty(ToolTip))
                    {
                        ToolTip = Localization.GetString(unLocalized + ".ToolTip", LocalResourceFile);
                    }
                }
            }
        }

        #endregion

    }
}
