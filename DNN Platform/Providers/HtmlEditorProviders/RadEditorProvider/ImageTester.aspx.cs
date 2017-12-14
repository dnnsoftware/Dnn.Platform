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
using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Modules;
using DotNetNuke.Security;
using DotNetNuke.Services;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using System.Web.SessionState;
using System.Web.Security;
using System.Web.Profile;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace DotNetNuke.Providers.RadEditorProvider
{
	public partial class ImageTester : System.Web.UI.Page
	{

        protected void Page_Load(object sender, System.EventArgs e)
        {

            string strResult = "NOTFOUND";

            string strFile = Request.QueryString["file"];
            if (strFile != null && IsImageFile(strFile))
            {
                string path = strFile.Replace("http://", "");
                path = path.Substring(path.IndexOf("/"));
                strFile = Server.MapPath(path);
                if (System.IO.File.Exists(strFile))
                {
                    strResult = "OK";
                }
            }

            Response.Write(strResult);
            Response.Flush();

        }

        private static bool IsImageFile(string relativePath)
        {
            var acceptedExtensions = new List<string> { "jpg", "png", "gif", "jpe", "jpeg", "tiff", "bmp" };
            var extension = relativePath.Substring(relativePath.LastIndexOf(".",
            StringComparison.Ordinal) + 1).ToLower();
            return acceptedExtensions.Contains(extension);
        }


	override protected void OnInit(EventArgs e)
	{
		base.OnInit(e);

//INSTANT C# NOTE: Converted event handler wireups:
		this.Load += new System.EventHandler(Page_Load);
	}
	}
}
