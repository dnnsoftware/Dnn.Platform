// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap;

using System;
using System.Text;
using System.Web;

using DotNetNuke.Entities.Portals;

public class SitemapHandler : IHttpHandler
{
    /// <inheritdoc/>
    public bool IsReusable
    {
        get
        {
            return true;
        }
    }

    /// <inheritdoc/>
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            HttpResponse response = context.Response;
            PortalSettings ps = PortalController.Instance.GetCurrentPortalSettings();

            response.ContentType = "text/xml";
            response.ContentEncoding = Encoding.UTF8;

            var builder = new SitemapBuilder(ps);

            if (context.Request.QueryString["i"] != null)
            {
                // This is a request for one of the files that build the sitemapindex
                builder.GetSitemapIndexFile(context.Request.QueryString["i"], response.Output);
            }
            else
            {
                builder.BuildSiteMap(response.Output);
            }
        }
        catch (Exception exc)
        {
            Exceptions.Exceptions.LogException(exc);
        }
    }
}
