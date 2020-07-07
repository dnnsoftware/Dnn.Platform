// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content
{
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Data;

    /// <summary>
    /// ContentTypeController provides the business layer of ContentType.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// <example>
    /// <code lang="C#">
    /// IContentTypeController typeController = new ContentTypeController();
    /// ContentType contentType = (from t in typeController.GetContentTypes()
    ///                            where t.ContentType == "DesktopModule"
    ///                            select t).SingleOrDefault();
    /// if(contentType == null)
    /// {
    ///     contentType = new ContentType {ContentType = "DesktopModule"};
    ///     contentType.ContentTypeId = typeController.AddContentType(contentType);
    /// }
    /// </code>
    /// </example>
    public class ContentTypeController : IContentTypeController
    {
        private readonly IDataService _DataService;

        public ContentTypeController()
            : this(Util.GetDataService())
        {
        }

        public ContentTypeController(IDataService dataService)
        {
            this._DataService = dataService;
        }

        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public int AddContentType(ContentType contentType)
        {
            // Argument Contract
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNullOrEmpty("contentType", "ContentType", contentType.ContentType);

            contentType.ContentTypeId = this._DataService.AddContentType(contentType);

            // Refresh cached collection of types
            this.ClearContentTypeCache();

            return contentType.ContentTypeId;
        }

        /// <summary>
        /// Clears the content type cache.
        /// </summary>
        public void ClearContentTypeCache()
        {
            DataCache.RemoveCache(DataCache.ContentTypesCacheKey);
        }

        /// <summary>
        /// Deletes the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        public void DeleteContentType(ContentType contentType)
        {
            // Argument Contract
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNegative("contentType", "ContentTypeId", contentType.ContentTypeId);

            this._DataService.DeleteContentType(contentType);

            // Refresh cached collection of types
            this.ClearContentTypeCache();
        }

        /// <summary>
        /// Gets the content types.
        /// </summary>
        /// <returns>content type collection.</returns>
        public IQueryable<ContentType> GetContentTypes()
        {
            return CBO.GetCachedObject<List<ContentType>>(
                new CacheItemArgs(
                DataCache.ContentTypesCacheKey,
                DataCache.ContentTypesCacheTimeOut,
                DataCache.ContentTypesCachePriority),
                c => CBO.FillQueryable<ContentType>(this._DataService.GetContentTypes()).ToList()).AsQueryable();
        }

        /// <summary>
        /// Updates the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public void UpdateContentType(ContentType contentType)
        {
            // Argument Contract
            Requires.NotNull("contentType", contentType);
            Requires.PropertyNotNegative("contentType", "ContentTypeId", contentType.ContentTypeId);
            Requires.PropertyNotNullOrEmpty("contentType", "ContentType", contentType.ContentType);

            this._DataService.UpdateContentType(contentType);

            // Refresh cached collection of types
            this.ClearContentTypeCache();
        }
    }
}
