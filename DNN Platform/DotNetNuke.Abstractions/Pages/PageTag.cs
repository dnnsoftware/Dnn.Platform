// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Pages
{
    /// <summary>
    /// Represents a string item with priority information for inclusion in page content.
    /// This class is used for HTML tags, scripts, styles, and other string-based content that needs priority-based ordering.
    /// </summary>
    /// <remarks>
    /// This class is commonly used for HTML tags that need to be injected into different sections of a page
    /// (head, body top, body end) with specific priority ordering. Priority determines the order in which
    /// items are rendered.
    /// Common priority values are defined in <see cref="PagePriority"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a script tag with high priority for the head section
    /// var scriptTag = new PriorityItem(
    ///     "&lt;script src=\"/js/framework.js\"&gt;&lt;/script&gt;",
    ///     PagePriority.Site);
    ///
    /// // Create a style tag with module priority
    /// var styleTag = new PriorityItem(
    ///     "&lt;link rel=\"stylesheet\" href=\"/css/module.css\" /&gt;",
    ///     PagePriority.Module);
    ///
    /// // Create a meta tag with page priority
    /// var metaTag = new PriorityItem(
    ///     "&lt;meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" /&gt;",
    ///     PagePriority.Page);
    /// </code>
    /// </example>
    public class PageTag
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageTag"/> class.
        /// </summary>
        /// <param name="value">The string value of the item. Cannot be null.</param>
        /// <param name="priority">The priority of the item . Use values from <see cref="PagePriority"/> for consistency.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when value is null.</exception>
        public PageTag(string value, int priority)
        {
            this.Value = value;
            this.Priority = priority;
        }

        /// <summary>
        /// Gets or sets the string value of the item.
        /// </summary>
        /// <value>
        /// A string containing the content to be rendered. This is typically HTML content such as
        /// script tags, link tags, meta tags, or other HTML elements. The content should be properly
        /// formatted HTML that is valid for the intended injection location.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the priority of the item.
        /// </summary>
        /// <value>
        /// An integer representing the rendering priority of the item.
        /// Items with lower priority numbers will be rendered before those with higher numbers.
        /// Use constants from <see cref="PagePriority"/> for consistent priority values:
        /// - <see cref="PagePriority.Site"/> (10): Framework and site-level content
        /// - <see cref="PagePriority.Page"/> (20): Page-specific content
        /// - <see cref="PagePriority.Default"/> (100): Default priority for most content
        /// - <see cref="PagePriority.Module"/> (200): Module-specific content.
        /// </value>
        public int Priority { get; set; }
    }
}
