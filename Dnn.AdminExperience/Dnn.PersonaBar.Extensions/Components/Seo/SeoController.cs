﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.PersonaBar.Seo.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Sitemap;

    public class SeoController
    {
        private PortalSettings portalSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeoController"/> class.
        /// </summary>
        public SeoController()
        {
            this.portalSettings = PortalController.Instance.GetCurrentPortalSettings();
        }

        public IEnumerable<SitemapProvider> GetSitemapProviders()
        {
            var builder = new SitemapBuilder(this.portalSettings);
            return builder.Providers;
        }

        public void ResetCache()
        {
            var cacheFolder = new DirectoryInfo(this.portalSettings.HomeSystemDirectoryMapPath + "sitemap\\");

            if (cacheFolder.Exists)
            {
                foreach (FileInfo file in cacheFolder.GetFiles("sitemap*.xml"))
                {
                    file.Delete();
                }
            }
        }

        public string GetSearchEngineSubmissionUrl(string searchEngine)
        {
            var strURL = string.Empty;

            switch (searchEngine.ToLower().Trim())
            {
                case "google":
                    strURL += "http://www.google.com/addurl?q=" + Globals.HTTPPOSTEncode(Globals.AddHTTP(Globals.GetDomainName(HttpContext.Current.Request)));
                    strURL += "&dq=";
                    if (!string.IsNullOrEmpty(this.portalSettings.PortalName))
                    {
                        strURL += Globals.HTTPPOSTEncode(this.portalSettings.PortalName);
                    }

                    if (!string.IsNullOrEmpty(this.portalSettings.Description))
                    {
                        strURL += Globals.HTTPPOSTEncode(this.portalSettings.Description);
                    }

                    if (!string.IsNullOrEmpty(this.portalSettings.KeyWords))
                    {
                        strURL += Globals.HTTPPOSTEncode(this.portalSettings.KeyWords);
                    }

                    strURL += "&submit=Add+URL";
                    break;
                case "yahoo!":
                    strURL = "http://siteexplorer.search.yahoo.com/submit";
                    break;
                case "bing":
                    strURL = "http://www.bing.com/webmaster";
                    break;
            }

            return strURL;
        }

        public void CreateVerification(string verification)
        {
            if (!string.IsNullOrEmpty(verification) && verification.EndsWith(".html"))
            {
                if (!File.Exists(Globals.ApplicationMapPath + "\\" + verification))
                {
                    string portalAlias = !string.IsNullOrEmpty(this.portalSettings.DefaultPortalAlias)
                                        ? this.portalSettings.DefaultPortalAlias
                                        : this.portalSettings.PortalAlias.HTTPAlias;

                    // write SiteMap verification file
                    var objStream = File.CreateText(Globals.ApplicationMapPath + "\\" + verification);
                    objStream.WriteLine("Google SiteMap Verification File");
                    objStream.WriteLine(" - " + Globals.AddHTTP(portalAlias) + @"/SiteMap.aspx");
                    objStream.WriteLine(" - " + UserController.Instance.GetCurrentUserInfo().DisplayName);
                    objStream.Close();
                }
            }
        }
    }
}
