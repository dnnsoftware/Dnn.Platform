﻿// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;

namespace Dnn.DynamicContent
{
    public interface IDynamicContentTypeManager
    {
        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        int AddContentType(DynamicContentType contentType);

        /// <summary>
        /// Deletes the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        void DeleteContentType(DynamicContentType contentType);

        /// <summary>
        /// GetContentType retrieves a dynamic content type for a portal, optionally including system types
        /// </summary>
        /// <param name="contentTypeId">The Id of the content type</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System content types (ie. content types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content type</returns>
        DynamicContentType GetContentType(int contentTypeId, int portalId, bool includeSystem = false);

        /// <summary>
        /// Gets the content types.
        /// </summary>
        /// <param name="portalId">The portalId</param>
        /// <param name="includeSystem">A flag to determine if System Content Types (ie. Content Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content type collection.</returns>
        IQueryable<DynamicContentType> GetContentTypes(int portalId, bool includeSystem = false);

        /// <summary>
        /// Gets a page of content types for a specific portal.
        /// </summary>
        /// <param name="searchTerm">The search term to use</param>
        /// <param name="portalId">The portalId</param>
        /// <param name="pageIndex">The page index to return</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeSystem">A flag to determine if System Content Types (ie. Content Types that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content type collection.</returns>
        IPagedList<DynamicContentType> GetContentTypes(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false);

        /// <summary>
        /// Updates the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        void UpdateContentType(DynamicContentType contentType);
    }
}
