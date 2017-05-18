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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals;

#endregion

namespace DotNetNuke.Services.Sitemap
{
    public class SitemapBuilder
    {
#region Fields
        private const int SITEMAP_MAXURLS = 50000;

        private const string SITEMAP_VERSION = "0.9";
        private readonly PortalSettings PortalSettings;
		private string _cacheFileName;
		private string _cacheIndexFileNameFormat;

		#endregion

		#region Properties

	    public string CacheFileName
	    {
		    get
		    {
			    if (string.IsNullOrEmpty(_cacheFileName))
			    {
                    var currentCulture = PortalSettings.CultureCode?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(currentCulture))
                    {
                        currentCulture = Localization.Localization.GetPageLocale(PortalSettings).Name.ToLowerInvariant();
                    }

					_cacheFileName = string.Format("sitemap" + ".{0}.xml", currentCulture);   
			    }

			    return _cacheFileName;
		    }
	    }

		public string CacheIndexFileNameFormat
		{
			get
			{
				if (string.IsNullOrEmpty(_cacheIndexFileNameFormat))
				{
					var currentCulture = Localization.Localization.GetPageLocale(PortalSettings).Name.ToLowerInvariant();
					_cacheIndexFileNameFormat = string.Format("sitemap_{{0}}" + ".{0}.xml", currentCulture);
				}

				return _cacheIndexFileNameFormat;
			}
		}

		#endregion

        /// <summary>
        ///   Creates an instance of the sitemap builder class
        /// </summary>
        /// <param name = "ps">Current PortalSettings for the portal being processed</param>
        /// <remarks>
        /// </remarks>
        public SitemapBuilder(PortalSettings ps)
        {
            PortalSettings = ps;

            LoadProviders();
        }

        #region "Sitemap Building"

        /// <summary>
        ///   Builds the complete portal sitemap
        /// </summary>
        /// <remarks>
        /// </remarks>
        public void BuildSiteMap(TextWriter output)
        {
            int cacheDays = Int32.Parse(PortalController.GetPortalSetting("SitemapCacheDays", PortalSettings.PortalId, "1"));
            bool cached = cacheDays > 0;
			  
            if (cached && CacheIsValid())
            {
				WriteSitemapFileToOutput(CacheFileName, output);
                return;
            }

            var allUrls = new List<SitemapUrl>();
            

            // excluded urls by priority
            float excludePriority = 0;
            excludePriority = float.Parse(PortalController.GetPortalSetting("SitemapExcludePriority", PortalSettings.PortalId, "0"), NumberFormatInfo.InvariantInfo);

            // get all urls
            bool isProviderEnabled = false;
            bool isProviderPriorityOverrided = false;
            float providerPriorityValue = 0;


            foreach (SitemapProvider _provider in Providers)
            {
                isProviderEnabled = bool.Parse(PortalController.GetPortalSetting(_provider.Name + "Enabled", PortalSettings.PortalId, "True"));

                if (isProviderEnabled)
                {
                    // check if we should override the priorities
                    isProviderPriorityOverrided = bool.Parse(PortalController.GetPortalSetting(_provider.Name + "Override", PortalSettings.PortalId, "False"));
                    // stored as an integer (pr * 100) to prevent from translating errors with the decimal point
                    providerPriorityValue = float.Parse(PortalController.GetPortalSetting(_provider.Name + "Value", PortalSettings.PortalId, "50")) / 100;

                    // Get all urls from provider
                    List<SitemapUrl> urls = new List<SitemapUrl>();
                    try
                    {
                        urls = _provider.GetUrls(PortalSettings.PortalId, PortalSettings, SITEMAP_VERSION);
                    }
                    catch (Exception ex)
                    {
                        Services.Exceptions.Exceptions.LogException(new Exception(Localization.Localization.GetExceptionMessage("SitemapProviderError",
                            "URL sitemap provider '{0}' failed with error: {1}",
                            _provider.Name, ex.Message)));
                    }

                    foreach (SitemapUrl url in urls)
                    {
                        if (isProviderPriorityOverrided)
                        {
                            url.Priority = providerPriorityValue;
                        }
                        if (url.Priority > 0 && url.Priority >= excludePriority) //#RS# a valid sitemap needs priorities larger then 0, otherwise the sitemap will be rejected by google as invalid
                        {
                            allUrls.Add(url);
                        }
                    }
                }
            }

            if (allUrls.Count > SITEMAP_MAXURLS)
            {
                // create a sitemap index file

                // enabled cache if it's not already
                if (!cached)
                {
                    cached = true;
                    PortalController.UpdatePortalSetting(PortalSettings.PortalId, "SitemapCacheDays", "1");
                }

                // create all the files
                int index = 0;
                int numFiles = (allUrls.Count / SITEMAP_MAXURLS) + 1;
                int elementsInFile = allUrls.Count / numFiles;

                for (index = 1; index <= numFiles; index++)
                {
                    int lowerIndex = elementsInFile * (index - 1);
                    int elements = 0;
                    if (index == numFiles)
                    {
                        // last file
                        elements = allUrls.Count - (elementsInFile * (numFiles - 1));
                    }
                    else
                    {
                        elements = elementsInFile;
                    }

                    WriteSitemap(cached, output, index, allUrls.GetRange(lowerIndex, elements));
                }

                // create the sitemap index
                WriteSitemapIndex(output, index - 1);
            }
            else
            {
                // create a regular sitemap file
                WriteSitemap(cached, output, 0, allUrls);
            }


            if (cached)
            {
				WriteSitemapFileToOutput(CacheFileName, output);
            }
        }

        /// <summary>
        ///   Returns the sitemap file that is part of a sitemapindex.
        /// </summary>
        /// <param name = "index">Index of the sitemap to return</param>
        /// <param name = "output">The output stream</param>
        /// <remarks>
        ///   The file should already exist since when using sitemapindexes the files are all cached to disk
        /// </remarks>
        public void GetSitemapIndexFile(string index, TextWriter output)
        {
			var currentCulture = Localization.Localization.GetPageLocale(PortalSettings).Name.ToLowerInvariant();
            WriteSitemapFileToOutput(string.Format("sitemap_{0}.{1}.xml", index, currentCulture), output);
        }

        /// <summary>
        ///   Generates a sitemap file
        /// </summary>
        /// <param name = "cached">Wheter the generated file should be cached or not</param>
        /// <param name = "output">The output stream</param>
        /// <param name = "index">For sitemapindex files the number of the file being generated, 0 otherwise</param>
        /// <param name = "allUrls">The list of urls to be included in the file</param>
        /// <remarks>
        ///   If the output should be cached it will generate a file under the portal directory (portals\[portalid]\sitemaps\) with 
        ///   the result of the generation. If the file is part of a sitemap, <paramref name = "index">index</paramref> will be appended to the
        ///   filename cached on disk ("sitemap_1.xml")
        /// </remarks>
        private void WriteSitemap(bool cached, TextWriter output, int index, List<SitemapUrl> allUrls)
        {
            // sitemap Output: can be a file is cache is enabled
            TextWriter sitemapOutput = output;
            try
            {
                if (cached)
                {
                    if (!Directory.Exists(PortalSettings.HomeSystemDirectoryMapPath + "Sitemap"))
                    {
                        Directory.CreateDirectory(PortalSettings.HomeSystemDirectoryMapPath + "Sitemap");
                    }
                    var cachedFile = (index > 0) ? string.Format(CacheIndexFileNameFormat, index) : CacheFileName;                
                    sitemapOutput = new StreamWriter(PortalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + cachedFile, false, Encoding.UTF8);
                }

                // Initialize writer
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.OmitXmlDeclaration = false;

                using (var writer = XmlWriter.Create(sitemapOutput, settings))
                {
                    // build header
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/" + SITEMAP_VERSION);
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
                    var schemaLocation = "http://www.sitemaps.org/schemas/sitemap/" + SITEMAP_VERSION;
                    writer.WriteAttributeString("xsi", "schemaLocation", null, string.Format("{0} {0}/sitemap.xsd", schemaLocation));

                    // write urls to output
                    foreach (SitemapUrl url in allUrls)
                    {
                        AddURL(url, writer);
                    }

                    writer.WriteEndElement();
                    writer.Close();
                }

                if (cached)
                {
                    sitemapOutput.Flush();
                    sitemapOutput.Close();
                }
            }
            finally
            {
                if (sitemapOutput != null)
                    sitemapOutput.Dispose();
            }
        }

        /// <summary>
        ///   Generates a sitemapindex file
        /// </summary>
        /// <param name = "output">The output stream</param>
        /// <param name = "totalFiles">Number of files that are included in the sitemap index</param>
        private void WriteSitemapIndex(TextWriter output, int totalFiles)
        {
            TextWriter sitemapOutput;
            using (sitemapOutput = new StreamWriter(PortalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + CacheFileName, false, Encoding.UTF8))
            {
                // Initialize writer
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.OmitXmlDeclaration = false;

                using (var writer = XmlWriter.Create(sitemapOutput, settings))
                {
                    // build header
                    writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/" + SITEMAP_VERSION);

                    // write urls to output
                    for (int index = 1; index <= totalFiles; index++)
                    {
                        string url = null;

                        url = "~/Sitemap.aspx?i=" + index;
                        if (IsChildPortal(PortalSettings, HttpContext.Current))
                        {
                            url += "&portalid=" + PortalSettings.PortalId;
                        }

                        writer.WriteStartElement("sitemap");
                        writer.WriteElementString("loc", Globals.AddHTTP(HttpContext.Current.Request.Url.Host + Globals.ResolveUrl(url)));
                        writer.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-dd"));
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.Close();
                }

                sitemapOutput.Flush();
                sitemapOutput.Close();
            }
        }

        #endregion

        #region "Helper methods"

        /// <summary>
        ///   Adds a new url to the sitemap
        /// </summary>
        /// <param name = "sitemapUrl">The url to be included in the sitemap</param>
        /// <remarks>
        /// </remarks>
        private void AddURL(SitemapUrl sitemapUrl, XmlWriter writer)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", sitemapUrl.Url);
            writer.WriteElementString("lastmod", sitemapUrl.LastModified.ToString("yyyy-MM-dd"));
            writer.WriteElementString("changefreq", sitemapUrl.ChangeFrequency.ToString().ToLower());
            writer.WriteElementString("priority", sitemapUrl.Priority.ToString("F01", CultureInfo.InvariantCulture));

            //if (sitemapUrl.AlternateUrls != null)
            //{
            //    foreach (AlternateUrl alternate in sitemapUrl.AlternateUrls)
            //    {
            //        writer.WriteStartElement("link", "http://www.w3.org/1999/xhtml");
            //        writer.WriteAttributeString("rel", "alternate");
            //        writer.WriteAttributeString("hreflang", alternate.Language);
            //        writer.WriteAttributeString("href", alternate.Url);
            //        writer.WriteEndElement();
            //    }
            //}
            writer.WriteEndElement();
        }

        /// <summary>
        ///   Is sitemap is cached, verifies is the cached file exists and is still valid
        /// </summary>
        /// <returns>True is the cached file exists and is still valid, false otherwise</returns>
        private bool CacheIsValid()
        {
            int cacheDays = int.Parse(PortalController.GetPortalSetting("SitemapCacheDays", PortalSettings.PortalId, "1"));
            var isValid = File.Exists(PortalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + CacheFileName);

            if (!isValid)
            {
                return isValid;
            }
            DateTime lastmod = File.GetLastWriteTime(PortalSettings.HomeSystemDirectoryMapPath + "/Sitemap/" + CacheFileName);
            if (lastmod.AddDays(cacheDays) < DateTime.Now)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>
        ///   When the sitemap is cached, reads the sitemap file and writes to the output stream
        /// </summary>
        /// <param name="file">File name</param>
        /// <param name = "output">The output stream</param>
        private void WriteSitemapFileToOutput(string file, TextWriter output)
        {
            if (!File.Exists(PortalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + file))
            {
                return;
            }
            // write the cached file to output
            using (var reader = new StreamReader(PortalSettings.HomeSystemDirectoryMapPath + "/Sitemap/" + file, Encoding.UTF8))
            {
                output.Write(reader.ReadToEnd());

                reader.Close();
            }
        }


        private bool IsChildPortal(PortalSettings ps, HttpContext context)
        {
            bool isChild = false;
            string portalName = null;
            var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(ps.PortalId).ToList();
            string serverPath = Globals.GetAbsoluteServerPath(context.Request);

            if (arr.Count > 0)
            {
                var portalAlias = (PortalAliasInfo)arr[0];
                portalName = Globals.GetPortalDomainName(ps.PortalAlias.HTTPAlias, null, true);
                if (portalAlias.HTTPAlias.IndexOf("/") > -1)
                {
                    portalName = PortalController.GetPortalFolder(portalAlias.HTTPAlias);
                }
                if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                {
                    isChild = true;
                }
            }
            return isChild;
        }

        #endregion

        #region "Provider configuration and setup"

        private static List<SitemapProvider> _providers;

        private static readonly object _lock = new object();

        public List<SitemapProvider> Providers
        {
            get
            {
                return _providers;
            }
        }


        private static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (_providers == null)
            {
                lock (_lock)
                {
                    _providers = new List<SitemapProvider>();


                    foreach (KeyValuePair<string, SitemapProvider> comp in ComponentFactory.GetComponents<SitemapProvider>())
                    {
                        comp.Value.Name = comp.Key;
                        comp.Value.Description = comp.Value.Description;
                        _providers.Add(comp.Value);
                    }


                    //'ProvidersHelper.InstantiateProviders(section.Providers, _providers, GetType(SiteMapProvider))
                }
            }
        }

        #endregion
    }
}
