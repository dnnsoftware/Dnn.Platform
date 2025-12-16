// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Sitemap
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Xml;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;

    using Localization = DotNetNuke.Services.Localization.Localization;

    public class SitemapBuilder
    {
        private const int SitemapMaxUrls = 50000;

        private const string SitemapVersion = "0.9";
        private static readonly object Lock = new object();

        private static List<SitemapProvider> providers;

        private readonly PortalSettings portalSettings;
        private string cacheFileName;
        private string cacheIndexFileNameFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="SitemapBuilder"/> class.
        ///   Creates an instance of the sitemap builder class.
        /// </summary>
        /// <param name="ps">Current PortalSettings for the portal being processed.</param>
        public SitemapBuilder(PortalSettings ps)
        {
            this.portalSettings = ps;

            LoadProviders();
        }

        public string CacheFileName
        {
            get
            {
                if (string.IsNullOrEmpty(this.cacheFileName))
                {
                    var currentCulture = this.portalSettings.CultureCode?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(currentCulture))
                    {
                        currentCulture = Localization.GetPageLocale(this.portalSettings).Name.ToLowerInvariant();
                    }

                    this.cacheFileName = $"sitemap.{currentCulture}.xml";
                }

                return this.cacheFileName;
            }
        }

        public string CacheIndexFileNameFormat
        {
            get
            {
                if (string.IsNullOrEmpty(this.cacheIndexFileNameFormat))
                {
                    var currentCulture = Localization.GetPageLocale(this.portalSettings).Name.ToLowerInvariant();
                    this.cacheIndexFileNameFormat = $"sitemap_{{0}}.{currentCulture}.xml";
                }

                return this.cacheIndexFileNameFormat;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
        public List<SitemapProvider> Providers => providers;

        /// <summary>Builds the complete portal sitemap.</summary>
        /// <param name="output">The writer to which the sitemap is to be written.</param>
        public void BuildSiteMap(TextWriter output)
        {
            int cacheDays = int.Parse(PortalController.GetPortalSetting("SitemapCacheDays", this.portalSettings.PortalId, "1"), CultureInfo.InvariantCulture);
            bool cached = cacheDays > 0;

            if (cached && this.CacheIsValid())
            {
                this.WriteSitemapFileToOutput(this.CacheFileName, output);
                return;
            }

            var allUrls = new List<SitemapUrl>();

            // excluded urls by priority
            float excludePriority = 0;
            excludePriority = float.Parse(PortalController.GetPortalSetting("SitemapExcludePriority", this.portalSettings.PortalId, "0"), NumberFormatInfo.InvariantInfo);

            // get all urls
            bool isProviderEnabled = false;
            bool isProviderPriorityOverrided = false;
            float providerPriorityValue = 0;

            foreach (SitemapProvider provider in this.Providers)
            {
                isProviderEnabled = bool.Parse(PortalController.GetPortalSetting(provider.Name + "Enabled", this.portalSettings.PortalId, "True"));

                if (isProviderEnabled)
                {
                    // check if we should override the priorities
                    isProviderPriorityOverrided = bool.Parse(PortalController.GetPortalSetting(provider.Name + "Override", this.portalSettings.PortalId, "False"));

                    // stored as an integer (pr * 100) to prevent from translating errors with the decimal point
                    providerPriorityValue = float.Parse(PortalController.GetPortalSetting(provider.Name + "Value", this.portalSettings.PortalId, "50"), CultureInfo.InvariantCulture) / 100;

                    // Get all urls from provider
                    List<SitemapUrl> urls = [];
                    try
                    {
                        urls = provider.GetUrls(this.portalSettings.PortalId, this.portalSettings, SitemapVersion);
                    }
                    catch (Exception ex)
                    {
                        var exceptionMessage = Localization.GetExceptionMessage(
                            "SitemapProviderError",
                            "URL sitemap provider '{0}' failed with error: {1}",
                            provider.Name,
                            ex.Message);
                        Services.Exceptions.Exceptions.LogException(new SitemapException(exceptionMessage, ex));
                    }

                    foreach (SitemapUrl url in urls)
                    {
                        if (isProviderPriorityOverrided)
                        {
                            url.Priority = providerPriorityValue;
                        }

                        if (url.Priority > 0 && url.Priority >= excludePriority)
                        {
                            // #RS# a valid sitemap needs priorities larger then 0, otherwise the sitemap will be rejected by google as invalid
                            allUrls.Add(url);
                        }
                    }
                }
            }

            if (allUrls.Count > SitemapMaxUrls)
            {
                // create a sitemap index file

                // enabled cache if it's not already
                if (!cached)
                {
                    cached = true;
                    PortalController.UpdatePortalSetting(this.portalSettings.PortalId, "SitemapCacheDays", "1");
                }

                // create all the files
                int index = 0;
                int numFiles = (allUrls.Count / SitemapMaxUrls) + 1;
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

                    this.WriteSitemap(cached, output, index, allUrls.GetRange(lowerIndex, elements));
                }

                // create the sitemap index
                this.WriteSitemapIndex(output, index - 1);
            }
            else
            {
                // create a regular sitemap file
                this.WriteSitemap(cached, output, 0, allUrls);
            }

            if (cached)
            {
                this.WriteSitemapFileToOutput(this.CacheFileName, output);
            }
        }

        /// <summary>  Returns the sitemap file that is part of a sitemapindex.</summary>
        /// <param name="index">Index of the sitemap to return.</param>
        /// <param name="output">The output stream.</param>
        /// <remarks>
        ///   The file should already exist since when using sitemapindexes the files are all cached to disk.
        /// </remarks>
        public void GetSitemapIndexFile(string index, TextWriter output)
        {
            var currentCulture = Localization.GetPageLocale(this.portalSettings).Name.ToLowerInvariant();
            this.WriteSitemapFileToOutput($"sitemap_{index}.{currentCulture}.xml", output);
        }

        private static void LoadProviders()
        {
            // Avoid claiming lock if providers are already loaded
            if (providers == null)
            {
                lock (Lock)
                {
                    providers = [];

                    foreach (var comp in ComponentFactory.GetComponents<SitemapProvider>())
                    {
                        comp.Value.Name = comp.Key;
                        comp.Value.Description = comp.Value.Description;
                        providers.Add(comp.Value);
                    }

                    // 'ProvidersHelper.InstantiateProviders(section.Providers, _providers, GetType(SiteMapProvider))
                }
            }
        }

        /// <summary>  Adds a new url to the sitemap.</summary>
        /// <param name="sitemapUrl">The url to be included in the sitemap.</param>
        private static void AddUrl(SitemapUrl sitemapUrl, XmlWriter writer)
        {
            writer.WriteStartElement("url");
            writer.WriteElementString("loc", sitemapUrl.Url);
            writer.WriteElementString("lastmod", sitemapUrl.LastModified.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            writer.WriteElementString("changefreq", sitemapUrl.ChangeFrequency.ToString().ToLowerInvariant());
            writer.WriteElementString("priority", sitemapUrl.Priority.ToString("F01", CultureInfo.InvariantCulture));

            // if (sitemapUrl.AlternateUrls != null)
            // {
            //    foreach (AlternateUrl alternate in sitemapUrl.AlternateUrls)
            //    {
            //        writer.WriteStartElement("link", "http://www.w3.org/1999/xhtml");
            //        writer.WriteAttributeString("rel", "alternate");
            //        writer.WriteAttributeString("hreflang", alternate.Language);
            //        writer.WriteAttributeString("href", alternate.Url);
            //        writer.WriteEndElement();
            //    }
            // }
            writer.WriteEndElement();
        }

        private static bool IsChildPortal(PortalSettings ps, HttpContext context)
        {
            bool isChild = false;
            string portalName = null;
            var arr = PortalAliasController.Instance.GetPortalAliasesByPortalId(ps.PortalId).ToList();
            string serverPath = Globals.GetAbsoluteServerPath(context.Request);

            if (arr.Count > 0)
            {
                IPortalAliasInfo portalAlias = arr[0];
                portalName = Globals.GetPortalDomainName(ps.PortalAlias.HTTPAlias, null, true);
                if (portalAlias.HttpAlias.IndexOf("/") > -1)
                {
                    portalName = PortalController.GetPortalFolder(portalAlias.HttpAlias);
                }

                if (!string.IsNullOrEmpty(portalName) && Directory.Exists(serverPath + portalName))
                {
                    isChild = true;
                }
            }

            return isChild;
        }

        /// <summary>  Generates a sitemap file.</summary>
        /// <param name="cached">Wheter the generated file should be cached or not.</param>
        /// <param name="output">The output stream.</param>
        /// <param name="index">For sitemapindex files the number of the file being generated, 0 otherwise.</param>
        /// <param name="allUrls">The list of urls to be included in the file.</param>
        /// <remarks>
        ///   If the output should be cached it will generate a file under the portal directory (portals\[portalid]\sitemaps\) with
        ///   the result of the generation. If the file is part of a sitemap, <paramref name = "index">index</paramref> will be appended to the
        ///   filename cached on disk ("sitemap_1.xml").
        /// </remarks>
        private void WriteSitemap(bool cached, TextWriter output, int index, List<SitemapUrl> allUrls)
        {
            // sitemap Output: can be a file is cache is enabled
            TextWriter sitemapOutput = output;
            try
            {
                if (cached)
                {
                    if (!Directory.Exists(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap"))
                    {
                        Directory.CreateDirectory(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap");
                    }

                    var cachedFile = (index > 0) ? string.Format(CultureInfo.InvariantCulture, this.CacheIndexFileNameFormat, index) : this.CacheFileName;
                    sitemapOutput = new StreamWriter(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + cachedFile, false, Encoding.UTF8);
                }

                // Initialize writer
                var settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.Encoding = Encoding.UTF8;
                settings.OmitXmlDeclaration = false;

                using (var writer = XmlWriter.Create(sitemapOutput, settings))
                {
                    // build header
                    writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/" + SitemapVersion);
                    writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                    writer.WriteAttributeString("xmlns", "xhtml", null, "http://www.w3.org/1999/xhtml");
                    const string schemaLocation = "http://www.sitemaps.org/schemas/sitemap/" + SitemapVersion;
                    writer.WriteAttributeString("xsi", "schemaLocation", null, $"{schemaLocation} {schemaLocation}/sitemap.xsd");

                    // write urls to output
                    foreach (SitemapUrl url in allUrls)
                    {
                        AddUrl(url, writer);
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
                sitemapOutput?.Dispose();
            }
        }

        /// <summary>Generates a sitemap index file.</summary>
        /// <param name="output">The output stream.</param>
        /// <param name="totalFiles">Number of files that are included in the sitemap index.</param>
        private void WriteSitemapIndex(TextWriter output, int totalFiles)
        {
            using var sitemapOutput = new StreamWriter($@"{this.portalSettings.HomeSystemDirectoryMapPath}Sitemap\{this.CacheFileName}", false, Encoding.UTF8);

            // Initialize writer
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false,
            };

            using (var writer = XmlWriter.Create(sitemapOutput, settings))
            {
                // build header
                writer.WriteStartElement("sitemapindex", "http://www.sitemaps.org/schemas/sitemap/" + SitemapVersion);

                // write urls to output
                for (int index = 1; index <= totalFiles; index++)
                {
                    string url = null;

                    url = "~/Sitemap.aspx?i=" + index;
                    if (IsChildPortal(this.portalSettings, HttpContext.Current))
                    {
                        url += "&portalid=" + this.portalSettings.PortalId;
                    }

                    writer.WriteStartElement("sitemap");
                    writer.WriteElementString("loc", Globals.AddHTTP(HttpContext.Current.Request.Url.Host + Globals.ResolveUrl(url)));
                    writer.WriteElementString("lastmod", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.Close();
            }

            sitemapOutput.Flush();
            sitemapOutput.Close();
        }

        /// <summary>  Is sitemap is cached, verifies is the cached file exists and is still valid.</summary>
        /// <returns>True is the cached file exists and is still valid, false otherwise.</returns>
        private bool CacheIsValid()
        {
            int cacheDays = int.Parse(PortalController.GetPortalSetting("SitemapCacheDays", this.portalSettings.PortalId, "1"), CultureInfo.InvariantCulture);
            var isValid = File.Exists(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + this.CacheFileName);

            if (!isValid)
            {
                return isValid;
            }

            DateTime lastmod = File.GetLastWriteTime(this.portalSettings.HomeSystemDirectoryMapPath + "/Sitemap/" + this.CacheFileName);
            if (lastmod.AddDays(cacheDays) < DateTime.Now)
            {
                isValid = false;
            }

            return isValid;
        }

        /// <summary>  When the sitemap is cached, reads the sitemap file and writes to the output stream.</summary>
        /// <param name="file">File name.</param>
        /// <param name="output">The output stream.</param>
        private void WriteSitemapFileToOutput(string file, TextWriter output)
        {
            if (!File.Exists(this.portalSettings.HomeSystemDirectoryMapPath + "Sitemap\\" + file))
            {
                return;
            }

            // write the cached file to output
            using var reader = new StreamReader(this.portalSettings.HomeSystemDirectoryMapPath + "/Sitemap/" + file, Encoding.UTF8);
            output.Write(reader.ReadToEnd());

            reader.Close();
        }
    }
}
