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

using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:DefaultExtensionControl runat=server></{0}:DefaultExtensionControl>")]
    public class DefaultExtensionControl : WebControl
    {
        [Bindable(true)]
        [DefaultValue("")]
        public string Module
        {
            get
            {
                var s = (string)ViewState["Module"];
                return s ?? string.Empty;
            }
            set
            {
                ViewState["Module"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string Group
        {
            get
            {
                var s = (string)ViewState["Group"];
                return s ?? string.Empty;
            }
            set
            {
                ViewState["Group"] = value;
            }
        }

        [Bindable(true)]
        [DefaultValue("")]
        public string Name
        {
            get
            {
                var s = (string)ViewState["Name"];
                return s ?? string.Empty;
            }
            set
            {
                ViewState["Name"] = value;
            }
        }
    }
}
