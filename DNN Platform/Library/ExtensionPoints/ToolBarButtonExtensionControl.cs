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

using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;

using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.ExtensionPoints.Filters;

namespace DotNetNuke.ExtensionPoints
{
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:ToolBarButtonExtensionControl runat=server></{0}:ToolBarButtonExtensionControl>")]
    public class ToolBarButtonExtensionControl : DefaultExtensionControl
    {
        private string content = "";
        private IExtensionControlRenderer btnRenderer = null;

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            var extensionPointManager = new ExtensionPointManager();

            var str = new StringBuilder();

            var filter = new CompositeFilter()
                .And(new FilterByHostMenu(Globals.IsHostTab(PortalController.GetCurrentPortalSettings().ActiveTab.TabID)))
                .And(new FilterByUnauthenticated(HttpContext.Current.Request.IsAuthenticated));
            
            foreach (var extension in extensionPointManager.GetToolBarButtonExtensionPoints(Module, Group, filter))
            {
                if (extension is IToolBarMenuButtonExtensionPoint)
                {
                    btnRenderer = new ToolBarMenuButtonRenderer();
                    str.AppendFormat(btnRenderer.GetOutput(extension));
                } 
                else 
                {
                    btnRenderer = new ToolBarButtonRenderer();
                    str.AppendFormat(btnRenderer.GetOutput(extension));
                }                
            }

            content = str.ToString();
        }

        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(content);
        }

        protected override void Render(HtmlTextWriter writer)
        {
            RenderContents(writer);
        }
    }
}
