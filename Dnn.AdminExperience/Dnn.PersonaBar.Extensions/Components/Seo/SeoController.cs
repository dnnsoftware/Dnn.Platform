﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Sitemap;

#endregion

namespace Dnn.PersonaBar.Seo.Components
{
    public class SeoController
    {
        private PortalSettings _portalSettings;

        public SeoController()
        {
            _portalSettings = PortalController.Instance.GetCurrentPortalSettings();
        }

        public IEnumerable<SitemapProvider> GetSitemapProviders()
        {
            var builder = new SitemapBuilder(_portalSettings);
            return builder.Providers;
        }

        public void ResetCache()
        {
            var cacheFolder = new DirectoryInfo(_portalSettings.HomeSystemDirectoryMapPath + "sitemap\\");

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
                    if (!string.IsNullOrEmpty(_portalSettings.PortalName))
                    {
                        strURL += Globals.HTTPPOSTEncode(_portalSettings.PortalName);
                    }
                    if (!string.IsNullOrEmpty(_portalSettings.Description))
                    {
                        strURL += Globals.HTTPPOSTEncode(_portalSettings.Description);
                    }
                    if (!string.IsNullOrEmpty(_portalSettings.KeyWords))
                    {
                        strURL += Globals.HTTPPOSTEncode(_portalSettings.KeyWords);
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
                    string portalAlias = !String.IsNullOrEmpty(_portalSettings.DefaultPortalAlias)
                                        ? _portalSettings.DefaultPortalAlias
                                        : _portalSettings.PortalAlias.HTTPAlias;

                    //write SiteMap verification file
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
