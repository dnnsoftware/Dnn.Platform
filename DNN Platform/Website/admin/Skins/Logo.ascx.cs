// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.UI.Skins.Controls
{
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;
    using System.Xml;
    using System.Xml.Linq;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.FileSystem;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Logo Skin Object.</summary>
    public partial class Logo : SkinObjectBase
    {
        private readonly INavigationManager navigationManager;

        /// <summary>Initializes a new instance of the <see cref="Logo"/> class.</summary>
        public Logo()
        {
            this.navigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }

        /// <summary>Gets or sets the width of the border around the image.</summary>
        public string BorderWidth { get; set; }

        /// <summary>Gets or sets the CSS class for the image.</summary>
        public string CssClass { get; set; }

        /// <summary>Gets or sets the CSS class for the hyperlink.</summary>
        public string LinkCssClass { get; set; }

        /// <summary>Gets or sets a value indicating whether to inject the SVG content inline instead of wrapping it in an img tag.</summary>
        public bool InjectSvg { get; set; }

        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                if (!string.IsNullOrEmpty(this.BorderWidth))
                {
                    this.imgLogo.BorderWidth = Unit.Parse(this.BorderWidth);
                }

                if (!string.IsNullOrEmpty(this.CssClass))
                {
                    this.imgLogo.CssClass = this.CssClass;
                }

                if (!string.IsNullOrEmpty(this.LinkCssClass))
                {
                    this.hypLogo.CssClass = this.LinkCssClass;
                }

                this.litLogo.Visible = false;
                this.imgLogo.Visible = false;
                if (!string.IsNullOrEmpty(this.PortalSettings.LogoFile))
                {
                    var fileInfo = this.GetLogoFileInfo();
                    if (fileInfo != null)
                    {
                        if (this.InjectSvg && "svg".Equals(fileInfo.Extension, StringComparison.OrdinalIgnoreCase))
                        {
                            this.litLogo.Text = this.GetSvgContent(fileInfo);
                            this.litLogo.Visible = !string.IsNullOrEmpty(this.litLogo.Text);
                        }

                        if (this.litLogo.Visible == false)
                        {
                            string imageUrl = FileManager.Instance.GetUrl(fileInfo);
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                this.imgLogo.ImageUrl = imageUrl;
                                this.imgLogo.Visible = true;
                            }
                        }
                    }
                }

                this.imgLogo.AlternateText = this.PortalSettings.PortalName;
                this.hypLogo.ToolTip = this.PortalSettings.PortalName;
                this.hypLogo.Attributes.Add("aria-label", this.PortalSettings.PortalName);

                if (this.PortalSettings.HomeTabId != -1)
                {
                    this.hypLogo.NavigateUrl = this.navigationManager.NavigateURL(this.PortalSettings.HomeTabId);
                }
                else
                {
                    this.hypLogo.NavigateUrl = Globals.AddHTTP(this.PortalSettings.PortalAlias.HTTPAlias);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private IFileInfo GetLogoFileInfo()
        {
            string cacheKey = string.Format(DataCache.PortalCacheKey, this.PortalSettings.PortalId, this.PortalSettings.CultureCode) + "LogoFile";
            var file = CBO.GetCachedObject<FileInfo>(
                new CacheItemArgs(cacheKey, DataCache.PortalCacheTimeOut, DataCache.PortalCachePriority),
                this.GetLogoFileInfoCallBack);

            return file;
        }

        private IFileInfo GetLogoFileInfoCallBack(CacheItemArgs itemArgs)
        {
            return FileManager.Instance.GetFile(this.PortalSettings.PortalId, this.PortalSettings.LogoFile);
        }

        private string GetSvgContent(IFileInfo svgFile)
        {
            var cacheKey = string.Format(DataCache.PortalCacheKey, this.PortalSettings.PortalId, this.PortalSettings.CultureCode) + "LogoSvg";
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

                        if (!string.IsNullOrEmpty(this.CssClass))
                        {
                            // Append the css class.
                            var classes = svgXmlNode.Attribute("class")?.Value;
                            svgXmlNode.SetAttributeValue("class", string.IsNullOrEmpty(classes) ? this.CssClass : $"{classes} {this.CssClass}");
                        }

                        if (svgXmlNode.Descendants().FirstOrDefault(x => x.Name.LocalName.Equals("title", StringComparison.Ordinal)) == null)
                        {
                            // Add the title for ADA compliance.
                            var ns = svgXmlNode.GetDefaultNamespace();
                            var titleNode = new XElement(
                                ns + "title",
                                new XAttribute("id", this.litLogo.ClientID),
                                this.PortalSettings.PortalName);

                            svgXmlNode.AddFirst(titleNode);

                            // Link the title to the svg node.
                            svgXmlNode.SetAttributeValue("aria-labelledby", this.litLogo.ClientID);
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
