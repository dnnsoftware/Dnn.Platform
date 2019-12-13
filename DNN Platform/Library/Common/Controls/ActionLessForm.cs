// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
