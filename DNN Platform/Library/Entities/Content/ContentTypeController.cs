#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System;
using System.Linq;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content.Data;

namespace DotNetNuke.Entities.Content
{
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
    public class ContentTypeController : ControllerBase<IContentTypeController, ContentTypeController>,  IContentTypeController
    {
	    internal const string StructuredWhereClause = "WHERE PortalID = @0 AND IsStructured = 1";

        protected override Func<IContentTypeController> GetFactory()
        {
            return () => new ContentTypeController();
        }

        public ContentTypeController() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public ContentTypeController(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        public int AddContentType(ContentType contentType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentType, "ContentType");

            Add(contentType);

            return contentType.ContentTypeId;
        }

        /// <summary>
        /// Deletes the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        public void DeleteContentType(ContentType contentType)
        {
            Delete(contentType);
        }

        /// <summary>
        /// Gets the content types.
        /// </summary>
        /// <returns>content type collection.</returns>
        public IQueryable<ContentType> GetContentTypes()
		{
		    return Get<ContentType>().AsQueryable();
        }

        /// <summary>
        /// Gets the content types for a specific portal.
        /// </summary>
        /// <param name="portalId">The portalId</param>
        /// <returns>content type collection.</returns>
        public IQueryable<ContentType> GetContentTypes(int portalId)
	    {
            IQueryable<ContentType> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<ContentType>();

                contentTypes = rep.Get(portalId).AsQueryable();
            }

            return contentTypes;
        }

        /// <summary>
        /// Gets a page of content types for a specific portal.
        /// </summary>
        /// <param name="portalId">The portalId</param>
        /// <param name="pageIndex">The page index to return</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>content type collection.</returns>
        public IPagedList<ContentType> GetContentTypes(int portalId, int pageIndex, int pageSize)
        {
            IPagedList<ContentType> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<ContentType>();

                contentTypes = rep.GetPage(portalId, pageIndex, pageSize);
            }

            return contentTypes;
        }

        /// <summary>
        /// Gets the structured content types for a specific portal.
        /// </summary>
        /// <remarks>For the most part this will return the same daa set as GetContentTypes, but in this 
        /// case we ensure that IsStructured flag is true.</remarks>
        /// <param name="portalId">The portalId</param>
        /// <returns>content type collection.</returns>
        public IQueryable<ContentType> GetStructuredContentTypes(int portalId)
        {
            IQueryable<ContentType> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<ContentType>();

                contentTypes = rep.Find(StructuredWhereClause, portalId).AsQueryable();
            }

            return contentTypes;
        }

        /// <summary>
        /// Gets a page of structured content types for a specific portal.
        /// </summary>
        /// <remarks>For the most part this will return the same daa set as GetContentTypes, but in this 
        /// case we ensure that IsStructured flag is true.</remarks>
        /// <param name="portalId">The portalId</param>
        /// <param name="pageIndex">The page index to return</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>content type collection.</returns>
        public IPagedList<ContentType> GetStructuredContentTypes(int portalId, int pageIndex, int pageSize)
        {
            IPagedList<ContentType> contentTypes;
            using (DataContext)
            {
                var rep = DataContext.GetRepository<ContentType>();

                contentTypes = rep.Find(pageIndex, pageSize, StructuredWhereClause, portalId);
            }

            return contentTypes;
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
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(contentType, "ContentType");

            Update(contentType);
        }

        [Obsolete("Deprecated in DNN 8.  ContentTypeController methods use DAL2 which manages the cache automagically")]
        public void ClearContentTypeCache()
        {
            DataCache.RemoveCache(DataCache.ContentTypesCacheKey);
        }

        [Obsolete("Deprecated in DNN 8.  ContentTypeController methods use DAL2 so IDataService is no longer needed")]
        // ReSharper disable once UnusedParameter.Local
        public ContentTypeController(IDataService dataService) { }
    }
}