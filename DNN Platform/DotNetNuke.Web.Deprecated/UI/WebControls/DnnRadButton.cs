﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Web.UI;

using DotNetNuke.Services.Localization;

using Telerik.Web.UI;

#endregion

namespace DotNetNuke.Web.UI.WebControls
{
    public class DnnRadButton : RadButton
    {
         private bool _Localize = true;

        #region Protected Methods

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
                    ToolTip = Localization.GetString(ToolTip, LocalResourceFile);
                }

                if ((!string.IsNullOrEmpty(Text)))
                {
                    Text = Localization.GetString(Text, LocalResourceFile);

                    if ((string.IsNullOrEmpty(ToolTip)))
                    {
                        ToolTip = Localization.GetString(string.Format("{0}.ToolTip", Text), LocalResourceFile);
                    }
                }
            }
        }

        #endregion   
    }
}
