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