#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using DotNetNuke.Web.Client;
using DotNetNuke.Web.Client.ClientResourceManagement;
using Lucene.Net.Index;

namespace DotNetNuke.UI.Modules.Html5
{
    public class Html5HostControl : ModuleControlBase
    {
        private readonly string _html5File;

        public Html5HostControl(string html5File)
        {
            _html5File = html5File;
        }

        protected virtual string Html5File
        {
            get { return _html5File; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (!(string.IsNullOrEmpty(Html5File)))
            {
                //Check if css file exists
                var cssFile = Path.ChangeExtension(Html5File, ".css");
                if (File.Exists(Page.Server.MapPath(cssFile)))
                {
                    ClientResourceManager.RegisterStyleSheet(Page, cssFile, FileOrder.Css.DefaultPriority);
                }

                //Check if js file exists
                var jsFile = Path.ChangeExtension(Html5File, ".js");
                if (File.Exists(Page.Server.MapPath(cssFile)))
                {
                    ClientResourceManager.RegisterScript(Page, jsFile, FileOrder.Js.DefaultPriority);
                }
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            if (!(string.IsNullOrEmpty(Html5File)))
            {
                var reader = new StreamReader(Page.Server.MapPath(Html5File));
                var fileContent = reader.ReadToEnd();
                Controls.Add(new LiteralControl(HttpUtility.HtmlDecode(fileContent)));
            }
        }
    }
}
