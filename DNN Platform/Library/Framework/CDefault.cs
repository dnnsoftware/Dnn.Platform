#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.HtmlControls;

//using DotNetNuke.UI.Utilities;
using DotNetNuke.Collections.Internal;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Host;
using DotNetNuke.UI.Utilities;

using Globals = DotNetNuke.Common.Globals;

#endregion

namespace DotNetNuke.Framework
{
    /// -----------------------------------------------------------------------------
    /// Project	 : DotNetNuke
    /// Class	 : CDefault
    ///
    /// -----------------------------------------------------------------------------
    /// <summary>
    ///
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <history>
    /// 	[sun1]	1/19/2004	Created
    /// </history>
    /// -----------------------------------------------------------------------------
    public class CDefault : PageBase
    {
        public string Author = "";
        public string Comment = "";
        public string Copyright = "";
        public string Description = "";
        public string Generator = "";
        public string KeyWords = "";
        public new string Title = "";

        protected override void RegisterAjaxScript()
        {
            if (Page.Form != null)
            {
                ServicesFrameworkInternal.Instance.RegisterAjaxScript(Page);
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Allows the scroll position on the page to be moved to the top of the passed in control.
        /// </summary>
        /// <param name="objControl">Control to scroll to</param>
        /// <remarks>
        /// </remarks>
        /// <history>
        /// 	[Jon Henning]	3/30/2005	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public void ScrollToControl(Control objControl)
        {
            if (ClientAPI.BrowserSupportsFunctionality(ClientAPI.ClientFunctionality.Positioning))
            {
                ClientAPI.RegisterClientReference(this, ClientAPI.ClientNamespaceReferences.dnn_dom_positioning);
                ClientAPI.RegisterClientVariable(this, "ScrollToControl", objControl.ClientID, true);
                DNNClientAPI.SetScrollTop(Page);
            }
        }

        [Obsolete("Deprecated in DotNetNuke 6.0.  Replaced by RegisterStyleSheet")]
        public void AddStyleSheet(string id, string href, bool isFirst)
        {
            RegisterStyleSheet(this, href, isFirst);
        }

        [Obsolete("Deprecated in DotNetNuke 6.0.  Replaced by RegisterStyleSheet")]
        public void AddStyleSheet(string id, string href)
        {
            RegisterStyleSheet(this, href, false);
        }

    }
}
