﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Linq;

#endregion

namespace DotNetNuke.Entities.Content
{
	/// <summary>
	/// Interface of ContentTypeController.
	/// </summary>
	/// <seealso cref="ContentTypeController"/>
    public interface IContentTypeController
    {
        /// <summary>
        /// Adds the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>content type id.</returns>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        int AddContentType(ContentType contentType);

        /// <summary>
        /// Deletes the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        void DeleteContentType(ContentType contentType);

        /// <summary>
        /// Gets the content types.
        /// </summary>
        /// <returns>content type collection.</returns>
        IQueryable<ContentType> GetContentTypes();

        /// <summary>
        /// Updates the type of the content.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <exception cref="System.ArgumentNullException">content type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">contentType.ContentType is empty.</exception>
        void UpdateContentType(ContentType contentType);

        [Obsolete("Deprecated in DNN 8.  ContentTypeController methods use DAL2 which manages the cache automagically. Scheduled removal in v11.0.0.")]
        void ClearContentTypeCache();
    }
}
