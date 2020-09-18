// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Content
{
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;

    using DotNetNuke.Entities.Content.Common;
    using DotNetNuke.Entities.Content.Taxonomy;

    /// <summary>
    /// IContentController provides the business layer of ContentItem.
    /// </summary>
    /// <seealso cref="Util.GetContentController"/>
    /// <example>
    /// <code lang="C#">
    /// IContentController contentController = Util.GetContentController();
    /// desktopModule.Content = desktopModule.FriendlyName;
    /// desktopModule.Indexed = false;
    /// desktopModule.ContentTypeId = contentType.ContentTypeId;
    /// desktopModule.ContentItemId = contentController.AddContentItem(desktopModule);
    /// </code>
    /// </example>
    public interface IContentController
    {
        /// <summary>
        /// Adds the content item.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        /// <returns>content item id.</returns>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        int AddContentItem(ContentItem contentItem);

        /// <summary>
        /// Deletes the content item.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content item's id less than 0.</exception>
        void DeleteContentItem(ContentItem contentItem);

        /// <summary>Delete a ContentItem object by ID.</summary>
        /// <param name="contentItemId">The ID of the ContentItem object (ContentItemId).</param>
        void DeleteContentItem(int contentItemId);

        /// <summary>
        /// Gets the content item.
        /// </summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns>content item.</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">Content item id is less than 0.</exception>
        ContentItem GetContentItem(int contentItemId);

        /// <summary>Return ContentItems that have the specified term attached.</summary>
        /// <exception cref="System.ArgumentException">Term name is empty.</exception>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByTerm(string term);

        /// <summary>Return ContentItems that have the specified term attached.</summary>
        /// <exception cref="System.ArgumentException">Term name is empty.</exception>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByTerm(Term term);

        /// <summary>
        /// Get a list of content items by ContentType ID.
        /// </summary>
        /// <param name="contentTypeId">The Content Type ID of the content items we want to query.</param>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByContentType(int contentTypeId);

        /// <summary>
        /// Get a list of content items by ContentType.
        /// </summary>
        /// <param name="contentType">The Content Type of the content items we want to query.</param>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByContentType(ContentType contentType);

        /// <summary>
        /// Return a list of ContentItems that have all of the specified terms attached.
        /// </summary>
        /// <param name="terms">A list of terms that should be attached to the ContentItems returned.</param>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByTerms(IList<Term> terms);

        /// <summary>Return a list of ContentItems that have all of the specified terms attached.</summary>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByTerms(string[] terms);

        /// <summary>
        /// Gets the un indexed content items.
        /// </summary>
        /// <returns>content item collection.</returns>
        IQueryable<ContentItem> GetUnIndexedContentItems();

        /// <summary>
        /// Retrieve all content items associated with the specified module ID, <paramref name="moduleId"/>.
        /// </summary>
        /// <param name="moduleId">The module ID to use in the content item lookup.</param>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByModuleId(int moduleId);

        /// <summary>
        /// Retrieve all content items on the specified page (tab).
        /// </summary>
        /// <param name="tabId">The page ID to use in the lookup of content items.</param>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByTabId(int tabId);

        /// <summary>
        /// Get a list of content items tagged with terms from the specified Vocabulary ID.
        /// </summary>
        /// <returns></returns>
        IQueryable<ContentItem> GetContentItemsByVocabularyId(int vocabularyId);

        /// <summary>
        /// Updates the content item.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content item's id less than 0.</exception>
        void UpdateContentItem(ContentItem contentItem);

        /// <summary>
        /// Adds the meta data.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content item's id less than 0.</exception>
        /// <exception cref="System.ArgumentException">Meta name is empty.</exception>
        void AddMetaData(ContentItem contentItem, string name, string value);

        /// <summary>
        /// Deletes the meta data.
        /// </summary>
        /// <param name="contentItem">The content item.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="System.ArgumentNullException">content item is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">content item's id less than 0.</exception>
        /// <exception cref="System.ArgumentException">Meta name is empty.</exception>
        void DeleteMetaData(ContentItem contentItem, string name, string value);

        /// <summary>
        /// Similar to DeleteMetaData that requires a value, but this one looks it up for you.
        /// </summary>
        void DeleteMetaData(ContentItem contentItem, string name);

        /// <summary>
        /// Gets the meta data.
        /// </summary>
        /// <param name="contentItemId">The content item id.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">content item's id less than 0.</exception>
        NameValueCollection GetMetaData(int contentItemId);
    }
}
