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
#region Usings

using System;
using System.Web;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.UI.Skins.Controls
{
    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
    /// <history>
    /// 	[bchapman]	12/11/2014	New control for default skin links
        /// </history>
    /// -----------------------------------------------------------------------------
    public partial class DnnLink : SkinObjectBase
    {
        private const string MyFileName = "DnnLink.ascx";
        public string CssClass { get; set; }
        public string Target { get; set; }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!string.IsNullOrEmpty(this.CssClass))
                aDnnLink.Attributes.Add("class",this.CssClass);

            if (!string.IsNullOrEmpty(Target))
                aDnnLink.Target = this.Target;
            //set home page link to community URL

            string url = "http://www.dnnsoftware.com/community?utm_source=dnn-install&utm_medium=web-link&utm_content=gravity-skin-link&utm_campaign=dnn-install";
            string utmTerm = "&utm_term=cms-by-dnn";
            string hostName = this.Request.Url.Host.ToLower().Replace("www.", "");
            int charPos = 0; string linkText= "CMS by DNN";
            if (hostName.Length > 0)
            {
                //convert first letter of hostname to int pos in alphabet
                charPos = char.ToUpper(hostName[0]) - 64;
            }
            //vary link by first letter of host name
            if (charPos <= 5)
            {
                linkText = "Open Source ASP.NET CMS by DNN"; 
                utmTerm = "&utm_term=open+source+asp.net+by+dnn";
            }
            if (charPos > 5 && charPos <= 10)
            {
                linkText = "DNN - .NET Open Source CMS"; 
                utmTerm = "&utm_term=dnn+.net+open+source+cms";
            }

            if (charPos > 10 && charPos <= 15)
            {
                linkText = "Web Content Management by DNN";
                utmTerm = "&utm_term=web+content+management+by+dnn";
            }

            if (charPos > 15 && charPos <= 20)
            {
                linkText = "DNN .NET CMS"; 
                utmTerm = "&utm_term=dnn+.net+cms";
            }

            if (charPos > 20 && charPos <= 25)
            {
                linkText = "WCM by DNN"; 
                utmTerm = "&utm_term=wcm+by+dnn";
            }
            

            aDnnLink.InnerText = linkText;
            aDnnLink.HRef = HttpUtility.HtmlEncode(url + utmTerm);

        }
    }
}