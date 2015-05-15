// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using DotNetNuke.Collections;

namespace Dnn.DynamicContent
{
    public interface IContentTemplateManager
    {
        /// <summary>
        /// Adds a new content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="contentTemplate">The content template to add.</param>
        /// <returns>content template id.</returns>
        int AddContentTemplate(ContentTemplate contentTemplate);

        /// <summary>
        /// Deletes the content template for use with Structured(Dynamic) Content Types.
        /// </summary>
        void DeleteContentTemplate(ContentTemplate contentTemplate);

        /// <summary>
        /// This GetContentTemplates overloads retrieves all the content templates for a portal, optionally including system types
        /// </summary>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content template collection.</returns>
        IQueryable<ContentTemplate> GetContentTemplates(int portalId, bool includeSystem = false);

        /// <summary>
        /// Gets the content templates for a content Type
        /// </summary>
        /// <param name="contentTypeId">The Id of the content type which this template is for</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>content template collection.</returns>
        IQueryable<ContentTemplate> GetContentTemplates(int contentTypeId, int portalId, bool includeSystem = false);

        /// <summary>
        /// This GetContentTemplates overload retrieves a page of content templates for a given portal, based on a Search Term that can appear anywhere in the Name.
        /// </summary>
        /// <param name="searchTerm">The search term to use</param>
        /// <param name="portalId">The Id of the portal</param>
        /// <param name="pageIndex">The page number - 0 represents the first page</param>
        /// <param name="pageSize">The size of the page</param>
        /// <param name="includeSystem">A flag to determine if System Templates (ie. Templates that are available for all portals)
        /// should be returned. Defaults to false</param>
        /// <returns>a PagedList of ContentTemplates</returns>
        IPagedList<ContentTemplate> GetContentTemplates(string searchTerm, int portalId, int pageIndex, int pageSize, bool includeSystem = false);

        /// <summary>
        /// Updates the content template.
        /// </summary>
        /// <param name="contentTemplate">The content template.</param>
        void UpdateContentTemplate(ContentTemplate contentTemplate);
    }
}
