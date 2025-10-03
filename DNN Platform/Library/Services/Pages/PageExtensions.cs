// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Pages
{
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.UI.Skins.Controls;

    /// <summary>
    /// Extension methods for <see cref="IPageService"/> that provide convenient overloads with simple parameters.
    /// These methods create the appropriate priority objects internally for easier usage.
    /// </summary>
    public static class PageExtensions
    {
        /// <summary>
        /// Adds a tag to the header of the page using simple parameters.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="tag">The HTML tag to add to the head section.</param>
        /// <param name="priority">The priority of this tag. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddToHead(this IPageService pageService, string tag, int priority = PagePriority.Default)
        {
            var priorityItem = new PageTag(tag, priority);
            pageService.AddToHead(priorityItem);
        }

        /// <summary>
        /// Adds a meta tag using simple parameters.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="name">The name attribute of the meta tag.</param>
        /// <param name="content">The content attribute of the meta tag.</param>
        /// <param name="priority">The priority of this meta tag. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddMeta(this IPageService pageService, string name, string content, int priority = PagePriority.Default)
        {
            var priorityMeta = new PageMeta(name, content, priority);
            pageService.AddMeta(priorityMeta);
        }

        /// <summary>
        /// Adds a message using simple parameters with default message type.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="heading">The heading text for the message.</param>
        /// <param name="message">The message text content.</param>
        /// <param name="priority">The priority of this message. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddMessage(this IPageService pageService, string heading, string message, int priority = PagePriority.Default)
        {
            var priorityMessage = new PageMessage(heading, message, PageMessageType.Info, string.Empty, priority);
            pageService.AddMessage(priorityMessage);
        }

        /// <summary>
        /// Adds a message using simple parameters with specified message type and icon.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="heading">The heading text for the message.</param>
        /// <param name="message">The message text content.</param>
        /// <param name="messageType">The type/severity of the message.</param>
        /// <param name="iconSrc">The optional icon source URL for the message.</param>
        /// <param name="priority">The priority of this message. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddMessage(this IPageService pageService, string heading, string message, PageMessageType messageType, string iconSrc, int priority = PagePriority.Default)
        {
            var priorityMessage = new PageMessage(heading, message, messageType, iconSrc ?? string.Empty, priority);
            pageService.AddMessage(priorityMessage);
        }

        /// <summary>
        /// Adds a success message using simple parameters.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="heading">The heading text for the success message.</param>
        /// <param name="message">The success message text content.</param>
        /// <param name="priority">The priority of this message. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddSuccessMessage(this IPageService pageService, string heading, string message, int priority = PagePriority.Default)
        {
            var priorityMessage = new PageMessage(heading, message, PageMessageType.Success, string.Empty, priority);
            pageService.AddMessage(priorityMessage);
        }

        /// <summary>
        /// Adds an error message using simple parameters.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="heading">The heading text for the error message.</param>
        /// <param name="message">The error message text content.</param>
        /// <param name="priority">The priority of this message. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddErrorMessage(this IPageService pageService, string heading, string message, int priority = PagePriority.Default)
        {
            var priorityMessage = new PageMessage(heading, message, PageMessageType.Error, string.Empty, priority);
            pageService.AddMessage(priorityMessage);
        }

        /// <summary>
        /// Adds a warning message using simple parameters.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="heading">The heading text for the warning message.</param>
        /// <param name="message">The warning message text content.</param>
        /// <param name="priority">The priority of this message. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddWarningMessage(this IPageService pageService, string heading, string message, int priority = PagePriority.Default)
        {
            var priorityMessage = new PageMessage(heading, message, PageMessageType.Warning, string.Empty, priority);
            pageService.AddMessage(priorityMessage);
        }

        /// <summary>
        /// Adds an informational message using simple parameters.
        /// </summary>
        /// <param name="pageService">The page service instance.</param>
        /// <param name="heading">The heading text for the info message.</param>
        /// <param name="message">The info message text content.</param>
        /// <param name="priority">The priority of this message. Defaults to <see cref="PagePriority.Default"/>.</param>
        public static void AddInfoMessage(this IPageService pageService, string heading, string message, int priority = PagePriority.Default)
        {
            var priorityMessage = new PageMessage(heading, message, PageMessageType.Info, string.Empty, priority);
            pageService.AddMessage(priorityMessage);
        }

        /// <summary>
        /// Converts a <see cref="PageMessageType"/> to a <see cref="ModuleMessage.ModuleMessageType"/>.
        /// </summary>
        /// <param name="priorityMessage">The <see cref="PageMessageType"/> to convert.</param>
        /// <returns>The <see cref="ModuleMessage.ModuleMessageType"/>.</returns>
        public static ModuleMessage.ModuleMessageType ToModuleMessageType(this PageMessageType priorityMessage)
        {
            return priorityMessage switch
            {
                PageMessageType.Success => ModuleMessage.ModuleMessageType.GreenSuccess,
                PageMessageType.Warning => ModuleMessage.ModuleMessageType.YellowWarning,
                PageMessageType.Error => ModuleMessage.ModuleMessageType.RedError,
                PageMessageType.Info => ModuleMessage.ModuleMessageType.BlueInfo,
                _ => ModuleMessage.ModuleMessageType.GreenSuccess,
            };
        }
    }
}
