#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;

#endregion

namespace DotNetNuke.Common.Controls
{
	/// <summary>
	/// The Form will reset action to raw url instead of rewrite url.
	/// </summary>
    public class Form : HtmlForm
    {
        protected override void RenderAttributes(HtmlTextWriter writer)
        {
            var stringWriter = new StringWriter();
            var htmlWriter = new HtmlTextWriter(stringWriter);
            base.RenderAttributes(htmlWriter);
            string html = stringWriter.ToString();
            // Locate and replace action attribute
            int StartPoint = html.IndexOf("action=\"");
            if (StartPoint >= 0)
            {
                int EndPoint = html.IndexOf("\"", StartPoint + 8) + 1;
                html = html.Remove(StartPoint, EndPoint - StartPoint);
                html = html.Insert(StartPoint, "action=\"" + HttpUtility.HtmlEncode(HttpContext.Current.Request.RawUrl) + "\"");
            }
            if (base.ID != null)
            {
            // Locate and replace id attribute
                StartPoint = html.IndexOf("id=\"");
                if (StartPoint >= 0)
                {
                    int EndPoint = html.IndexOf("\"", StartPoint + 4) + 1;
                    html = html.Remove(StartPoint, EndPoint - StartPoint);
                    html = html.Insert(StartPoint, "id=\"" + base.ClientID + "\"");
                }
            }
            writer.Write(html);
        }
    }
}