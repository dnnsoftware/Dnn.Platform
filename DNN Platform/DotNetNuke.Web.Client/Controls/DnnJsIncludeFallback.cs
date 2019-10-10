﻿#region Copyright

// 
// DotNetNuke® - https://www.dnnsoftware.com
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

using System.Web.UI;
using System.Web.UI.WebControls;


namespace DotNetNuke.Web.Client.ClientResourceManagement
{
    /// <summary>
    ///     Emit a fallback block for a script in the same part of the page
    /// </summary>
    public class DnnJsIncludeFallback : WebControl
    {

        public DnnJsIncludeFallback(string objectName, string fileName)
        {
            ObjectName = objectName;
            FileName = fileName;
        }

        public string ObjectName { get; set; }
        public string FileName { get; set; }


        public override void RenderControl(HtmlTextWriter writer)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/javascript");
            writer.RenderBeginTag(HtmlTextWriterTag.Script);

            if (ObjectName.Contains("."))
            {
                //generate function check
                writer.Write("if (typeof " + ObjectName + " != 'function') {");
            }
            else
            {
                //generate object check
                writer.Write("if (typeof " + ObjectName + " == 'undefined') {");
            }

            writer.Write("document.write('<script src=\"" + FileName + "\" type=\"text/javascript\"></' + 'script>');");
            writer.Write("}");
            writer.RenderEndTag();
        }
    }
}