// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Web;

    using DotNetNuke.Services.Localization;

    /// -----------------------------------------------------------------------------
    /// <summary></summary>
    /// <returns></returns>
    /// <remarks></remarks>
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
            {
                this.aDnnLink.Attributes.Add("class", this.CssClass);
            }

            if (!string.IsNullOrEmpty(this.Target))
            {
                this.aDnnLink.Target = this.Target;
            }

            // set home page link to community URL
            string url = "http://www.dnnsoftware.com/community?utm_source=dnn-install&utm_medium=web-link&utm_content=gravity-skin-link&utm_campaign=dnn-install";
            string utmTerm = "&utm_term=cms-by-dnn";
            string hostName = this.Request.Url.Host.ToLowerInvariant().Replace("www.", string.Empty);
            int charPos = 0;
            string linkText = "CMS by DNN";
            if (hostName.Length > 0)
            {
                // convert first letter of hostname to int pos in alphabet
                charPos = char.ToUpper(hostName[0]) - 64;
            }

            // vary link by first letter of host name
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

            this.aDnnLink.InnerText = linkText;
            this.aDnnLink.HRef = HttpUtility.HtmlEncode(url + utmTerm);
        }
    }
}
