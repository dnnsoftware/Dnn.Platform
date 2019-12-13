// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Text;
using System.Web;

using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Services.Sitemap
{
    public class SitemapHandler : IHttpHandler
    {
        #region IHttpHandler Members

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            try
            {
                HttpResponse Response = context.Response;
                PortalSettings ps = PortalController.Instance.GetCurrentPortalSettings();

                Response.ContentType = "text/xml";
                Response.ContentEncoding = Encoding.UTF8;

                var builder = new SitemapBuilder(ps);

                if ((context.Request.QueryString["i"] != null))
                {
                    // This is a request for one of the files that build the sitemapindex
                    builder.GetSitemapIndexFile(context.Request.QueryString["i"], Response.Output);
                }
                else
                {
                    builder.BuildSiteMap(Response.Output);
                }
            }
            catch (Exception exc)
            {
                Exceptions.Exceptions.LogException(exc);
            }
        }

        #endregion
    }
}
