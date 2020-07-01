// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Text;
    using System.Web;

    using DotNetNuke.Entities.Portals;

    public class SitemapHandler : IHttpHandler
    {
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

                if (context.Request.QueryString["i"] != null)
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
    }
}
