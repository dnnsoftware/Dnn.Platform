// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Components.PageLinks
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Tabs;

    /// <summary>Supplies the pages the current user may link to.</summary>
    public interface IPageSource
    {
        /// <summary>Gets the pages in the portal the current user can view, in page tree order.</summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The pages.</returns>
        IEnumerable<TabInfo> GetViewablePages(int portalId);

        /// <summary>Gets the URL of a page.</summary>
        /// <param name="page">The page.</param>
        /// <returns>The page's URL, which may be absolute.</returns>
        string GetUrl(TabInfo page);
    }
}
