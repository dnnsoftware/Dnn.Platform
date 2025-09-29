// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Pages
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides services for managing page content with priority-based storage.
    /// </summary>
    public interface IPageService
    {
        /// <summary>
        /// Set the Page Title.
        /// </summary>
        /// <param name="value">The title value to set.</param>
        /// <param name="priority">The priority of this title (lower number = higher priority).</param>
        void SetTitle(string value, int priority = PagePriority.Default);

        /// <summary>
        /// Set the Page Description.
        /// </summary>
        /// <param name="value">The description value to set.</param>
        /// <param name="priority">The priority of this description (lower number = higher priority).</param>
        void SetDescription(string value, int priority = PagePriority.Default);

        /// <summary>
        /// Set the Page Keywords.
        /// </summary>
        /// <param name="value">The keywords value to set.</param>
        /// <param name="priority">The priority of these keywords (lower number = higher priority).</param>
        void SetKeyWords(string value, int priority = PagePriority.Default);

        /// <summary>
        /// Set the Page Canonical Link URL.
        /// </summary>
        /// <param name="value">The canonical link URL value to set.</param>
        /// <param name="priority">The priority of this canonical link URL (lower number = higher priority).</param>
        void SetCanonicalLinkUrl(string value, int priority = PagePriority.Default);

        /// <summary>
        /// Add a tag to the header of the page.
        /// </summary>
        /// <param name="tagItem">Priority item containing the tag and priority information.</param>
        void AddToHead(PageTag tagItem);

        /// <summary>
        /// Add a standard meta header tag.
        /// </summary>
        /// <param name="metaItem">Priority meta item containing the meta tag information and priority.</param>
        void AddMeta(PageMeta metaItem);

        /// <summary>
        /// Add a message to be displayed on the page.
        /// </summary>
        /// <param name="messageItem">Priority message item containing the message information and priority.</param>
        void AddMessage(PageMessage messageItem);

        /// <summary>
        /// Gets the current title value with highest priority.
        /// </summary>
        /// <returns>The title value or null if not set.</returns>
        string GetTitle();

        /// <summary>
        /// Gets the current description value with highest priority.
        /// </summary>
        /// <returns>The description value or null if not set.</returns>
        string GetDescription();

        /// <summary>
        /// Gets the current keywords value with highest priority.
        /// </summary>
        /// <returns>The keywords value or null if not set.</returns>
        string GetKeyWords();

        /// <summary>
        /// Gets the canonical link URL.
        /// </summary>
        /// <returns>The canonical link URL or null if not set.</returns>
        string GetCanonicalLinkUrl();

        /// <summary>
        /// Gets all head tags ordered by priority (lowest priority first).
        /// </summary>
        /// <returns>List of head tags ordered by priority.</returns>
        List<PageTag> GetHeadTags();

        /// <summary>
        /// Gets all meta tags ordered by priority (lowest priority first).
        /// </summary>
        /// <returns>List of meta tags ordered by priority.</returns>
        List<PageMeta> GetMetaTags();

        /// <summary>
        /// Gets all messages ordered by priority (lowest priority first).
        /// </summary>
        /// <returns>List of messages ordered by priority.</returns>
        List<PageMessage> GetMessages();

        /// <summary>
        /// Clears all stored page data. Useful for testing or resetting state.
        /// </summary>
        void Clear();
    }
}
