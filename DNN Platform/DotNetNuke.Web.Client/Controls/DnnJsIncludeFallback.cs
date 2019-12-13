// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
