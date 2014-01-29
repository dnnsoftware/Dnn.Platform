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
using System.Text;

namespace DotNetNuke.ExtensionPoints
{
    public class ToolBarButtonRenderer : IExtensionControlRenderer
    {
        public string GetOutput(IExtensionPoint extensionPoint)
        {
            var extension = (IToolBarButtonExtensionPoint)extensionPoint;

            var str = new StringBuilder();
            str.AppendFormat(
                        "<button id='{0}' class='{1}' onclick='{2}; return false;' title='{3}'>",
                        extension.ButtonId, extension.CssClass, extension.Action, extension.Text);

            str.AppendFormat(
                "<span id='{0}_text' style='{1} background-image: url(\"{2}\");'>{3}</span>",
                extension.ButtonId,
                !extension.ShowText ? "text-indent: -10000000px;" : "",
                extension.ShowIcon ? extension.Icon : "",
                extension.Text);

            str.AppendLine("</button>");

            return str.ToString();
        }
    }
}
