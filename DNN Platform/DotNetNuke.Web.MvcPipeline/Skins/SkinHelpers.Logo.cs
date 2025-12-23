// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Skins
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Xml;
    using System.Xml.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Web.MvcPipeline.Models;

    /// <summary>
    /// Skin helper methods for rendering the portal logo.
    /// </summary>
    public static partial class SkinHelpers
    {
        /// <summary>
        /// Renders the portal logo as a link to the home page, optionally injecting SVG content.
        /// </summary>
        /// <param name="helper">The HTML helper for the current <see cref="PageModel"/>.</param>
        /// <param name="borderWidth">Optional CSS border width to apply to the logo image.</param>
        /// <param name="cssClass">Optional CSS class applied to the logo image element.</param>
        /// <param name="linkCssClass">Optional CSS class applied to the anchor element.</param>
        /// <param name="injectSvg">If set to <c>true</c>, inlines SVG logo content instead of using an <c>&lt;img&gt;</c> tag.</param>
        /// <returns>An HTML string representing the logo link.</returns>
        public static IHtmlString Logo(this HtmlHelper<PageModel> helper, string borderWidth = "", string cssClass = "", string linkCssClass = "", bool injectSvg = false)
        {
            var portalSettings = PortalSettings.Current;
            var navigationManager = helper.ViewData.Model.NavigationManager;

            TagBuilder tbImage = new TagBuilder("img");
            if (!string.IsNullOrEmpty(borderWidth))
            {
                tbImage.Attributes.Add("style", $"border-width:{borderWidth};");
            }

            if (!string.IsNullOrEmpty(cssClass))
            {
                tbImage.AddCssClass(cssClass);
            }

            tbImage.Attributes.Add("alt", portalSettings.PortalName);

            TagBuilder tbLink = new TagBuilder("a");
            tbLink.GenerateId("dnn_dnnLOGO_");
            if (!string.IsNullOrEmpty(linkCssClass))
            {
                tbLink.AddCssClass(linkCssClass);
            }

            if (!string.IsNullOrEmpty(portalSettings.LogoFile))
            {
                var fileInfo = GetLogoFileInfo(portalSettings);
                if (fileInfo != null)
                {
                    if (injectSvg && "svg".Equals(fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
                    {
                        string svgContent = GetSvgContent(fileInfo, portalSettings, cssClass);
                        if (!string.IsNullOrEmpty(svgContent))
                        {
                            tbLink.InnerHtml = svgContent;
                        }
                    }
                    else
                    {
                        string imageUrl = FileManager.Instance.GetUrl(fileInfo);
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            tbImage.Attributes.Add("src", imageUrl);
                            tbLink.InnerHtml = tbImage.ToString();
                        }
                    }
                }
            }

            tbLink.Attributes.Add("title", portalSettings.PortalName);
            tbLink.Attributes.Add("aria-label", portalSettings.PortalName);

            if (portalSettings.HomeTabId != -1)
            {
                tbLink.Attributes.Add("href", navigationManager.NavigateURL(portalSettings.HomeTabId));
            }
            else
            {
                tbLink.Attributes.Add("href", Globals.AddHTTP(portalSettings.PortalAlias.HTTPAlias));
            }

            return new MvcHtmlString(tbLink.ToString());
        }

        private static IFileInfo GetLogoFileInfo(PortalSettings portalSettings)
        {
            string cacheKey = string.Format(DataCache.PortalCacheKey, portalSettings.PortalId, portalSettings.CultureCode) + "LogoFile";
            var file = CBO.GetCachedObject<FileInfo>(
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority),
                (CacheItemArgs itemArgs) =>
                {
                    return FileManager.Instance.GetFile(portalSettings.PortalId, portalSettings.LogoFile);
                });

            return file;
        }

        private static string GetSvgContent(IFileInfo svgFile, PortalSettings portalSettings, string cssClass)
        {
            var cacheKey = string.Format(DataCache.PortalCacheKey, portalSettings.PortalId, portalSettings.CultureCode) + "LogoSvg";
            return CBO.GetCachedObject<string>(
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority, svgFile),
                (_) =>
                {
                    try
                    {
                        XDocument svgDocument;
                        using (var fileContent = FileManager.Instance.GetFileContent(svgFile))
                        {
                            svgDocument = XDocument.Load(fileContent);
                        }

                        var svgXmlNode = svgDocument.Descendants()
                            .SingleOrDefault(x => x.Name.LocalName.Equals("svg", StringComparison.Ordinal));
                        if (svgXmlNode == null)
                        {
                            throw new InvalidFileContentException("The svg file has no svg node.");
                        }

                        if (!string.IsNullOrEmpty(cssClass))
                        {
                            // Append the css class.
                            var classes = svgXmlNode.Attribute("class")?.Value;
                            svgXmlNode.SetAttributeValue("class", string.IsNullOrEmpty(classes) ? cssClass : $"{classes} {cssClass}");
                        }

                        if (svgXmlNode.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("title", StringComparison.Ordinal)) == null)
                        {
                            // Add the title for ADA compliance.
                            var ns = svgXmlNode.GetDefaultNamespace();
                            var titleNode = new XElement(
                                ns + "title",
                                new XAttribute("id", "logoTitle"),
                                portalSettings.PortalName);

                            svgXmlNode.AddFirst(titleNode);

                            // Link the title to the svg node.
                            svgXmlNode.SetAttributeValue("aria-labelledby", "logoTitle");
                        }

                        // Ensure we have the image role for ADA Compliance
                        svgXmlNode.SetAttributeValue("role", "img");

                        return svgDocument.ToString();
                    }
                    catch (XmlException ex)
                    {
                        throw new InvalidFileContentException("Invalid SVG file: " + ex.Message);
                    }
                });
        }
    }
}
