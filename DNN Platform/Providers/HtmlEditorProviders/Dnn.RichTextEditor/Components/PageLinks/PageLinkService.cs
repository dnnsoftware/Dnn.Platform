// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.PageLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Lists the pages the editor's Insert Page Link button offers.</summary>
    public class PageLinkService
    {
        private readonly IPageSource pageSource;

        /// <summary>Initializes a new instance of the <see cref="PageLinkService"/> class.</summary>
        /// <param name="pageSource">The source of pages the current user can view.</param>
        public PageLinkService(IPageSource pageSource)
        {
            this.pageSource = pageSource ?? throw new ArgumentNullException(nameof(pageSource));
        }

        /// <summary>
        /// Makes a page URL site-relative (path, query and fragment), so links keep working under every alias of the site.
        /// Relative URLs are returned unchanged.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The site-relative URL.</returns>
        public static string ToSiteRelativeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var absolute)
                && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                return absolute.PathAndQuery + absolute.Fragment;
            }

            return url;
        }

        /// <summary>Gets the pages in the portal the current user can link to, in page tree order.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The pages.</returns>
        public IList<PageLink> GetPageLinks(int portalId)
        {
            return this.pageSource.GetViewablePages(portalId)
                .Where(page => !page.IsDeleted)
                .Select(page => new PageLink
                {
                    Id = page.TabID,
                    Name = page.TabName,
                    Level = Math.Max(page.Level, 0),
                    Url = ToSiteRelativeUrl(this.pageSource.GetUrl(page)),
                    Selectable = !page.DisableLink,
                })
                .ToList();
        }
    }
}
