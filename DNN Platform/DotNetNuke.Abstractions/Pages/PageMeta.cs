// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Pages
{
    /// <summary>
    /// Represents a meta tag with priority information for inclusion in the HTML head section.
    /// Meta tags provide metadata about the HTML document and are used by search engines and browsers.
    /// </summary>
    /// <remarks>
    /// This class is used to store meta tag information that will be rendered in the HTML head section.
    /// Priority determines the order in which meta tags are rendered, with lower numbers having higher priority.
    /// Common priority values are defined in <see cref="PagePriority"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a description meta tag with page priority
    /// var descriptionMeta = new PriorityMeta(
    ///     "description",
    ///     "This is the page description for SEO purposes.",
    ///     PagePriority.Page);
    ///
    /// // Create a keywords meta tag with default priority
    /// var keywordsMeta = new PriorityMeta(
    ///     "keywords",
    ///     "DNN, CMS, content management",
    ///     PagePriority.Default);
    ///
    /// // Create a viewport meta tag with site priority
    /// var viewportMeta = new PriorityMeta(
    ///     "viewport",
    ///     "width=device-width, initial-scale=1.0",
    ///     PagePriority.Site);
    /// </code>
    /// </example>
    public class PageMeta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageMeta"/> class.
        /// </summary>
        /// <param name="name">The name attribute of the meta tag. Cannot be null or empty.</param>
        /// <param name="content">The content attribute of the meta tag. Cannot be null.</param>
        /// <param name="priority">The priority of the meta tag (lower number = higher priority). Use values from <see cref="PagePriority"/> for consistency.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when name or content is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when name is empty.</exception>
        public PageMeta(string name, string content, int priority)
        {
            this.Name = name;
            this.Content = content;
            this.Priority = priority;
        }

        /// <summary>
        /// Gets or sets the name attribute of the meta tag.
        /// </summary>
        /// <value>
        /// A string containing the name attribute value for the meta tag.
        /// Common values include "description", "keywords", "author", "viewport", etc.
        /// This will be rendered as: &lt;meta name="[Name]" content="[Content]" /&gt;.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content attribute of the meta tag.
        /// </summary>
        /// <value>
        /// A string containing the content attribute value for the meta tag.
        /// This contains the actual metadata information associated with the name attribute.
        /// The content should be appropriate for the specified name attribute.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the priority of the meta tag (lower number = higher priority).
        /// </summary>
        /// <value>
        /// An integer representing the rendering priority of the meta tag in the HTML head section.
        /// Meta tags with lower priority numbers will be rendered before those with higher numbers.
        /// Use constants from <see cref="PagePriority"/> for consistent priority values.
        /// </value>
        public int Priority { get; set; }
    }
}
