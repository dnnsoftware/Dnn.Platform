// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Pages
{
    /// <summary>
    /// Defines standard priority values for page content ordering.
    /// Lower numbers indicate higher priority and will be processed/rendered first.
    /// </summary>
    /// <remarks>
    /// These constants provide a standardized way to assign priorities to page content such as
    /// scripts, styles, meta tags, and messages. Using these predefined values ensures consistent
    /// ordering across the application and makes the code more maintainable.
    ///
    /// Priority processing order:
    /// 1. Site-level content (priority 10)
    /// 2. Page-level content (priority 20)
    /// 3. Default content (priority 100)
    /// 4. Module-level content (priority 200).
    /// </remarks>
    public static class PagePriority
    {
        /// <summary>
        /// Site-level priority (value: 10).
        /// Used for framework  site-wide content
        /// that should be loaded before any other content.
        /// </summary>
        public const int Site = 10;

        /// <summary>
        /// Page-level priority (value: 20).
        /// Used for page-specific content that should be loaded after site-level content
        /// but before default and module content.
        /// </summary>
        /// <example>
        /// <code>
        /// // Page-specific JavaScript
        /// pageService.AddToHead(new PriorityItem("&lt;script src=\"page-analytics.js\"&gt;&lt;/script&gt;", PagePriority.Page));
        ///
        /// // Page-specific meta description
        /// pageService.AddMeta(new PriorityMeta("description", "Page-specific description", PagePriority.Page));
        /// </code>
        /// </example>
        public const int Page = 20;

        /// <summary>
        /// Default priority (value: 100).
        /// Used as the standard priority when no specific priority is needed.
        /// Most content should use this priority unless there's a specific ordering requirement.
        /// </summary>
        /// <example>
        /// <code>
        /// // Standard content without specific ordering requirements
        /// pageService.AddMessage(new PriorityMessage("Info", "General information", MessageType.Info, "", PagePriority.Default));
        ///
        /// // Regular meta tags
        /// pageService.AddMeta(new PriorityMeta("author", "John Doe", PagePriority.Default));
        /// </code>
        /// </example>
        public const int Default = 100;

        /// <summary>
        /// Module-level priority (value: 200).
        /// Used for module-specific content that should be loaded after all other content.
        /// This ensures that module content doesn't interfere with core functionality.
        /// </summary>
        /// <example>
        /// <code>
        /// // Module-specific success message
        /// pageService.AddMessage(new PriorityMessage("Success", "Module operation completed", MessageType.Success, "", PagePriority.Module));
        /// </code>
        /// </example>
        public const int Module = 200;
    }
}
