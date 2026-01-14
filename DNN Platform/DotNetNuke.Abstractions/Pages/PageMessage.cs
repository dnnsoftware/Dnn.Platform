// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Pages
{
    /// <summary>
    /// Represents a message with priority information for display on a page.
    /// Messages can include success notifications, warnings, errors, or informational content.
    /// </summary>
    /// <remarks>
    /// This class is used to store messages that will be displayed to users on a page.
    /// Priority determines the order in which messages are displayed, with lower numbers having higher priority.
    /// Common priority values are defined in <see cref="PagePriority"/>.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a success message with high priority
    /// var successMessage = new PriorityMessage(
    ///     "Operation Successful",
    ///     "The data has been saved successfully.",
    ///     MessageType.Success,
    ///     "/images/success-icon.png",
    ///     PagePriority.Site);
    ///
    /// // Create an error message with default priority
    /// var errorMessage = new PriorityMessage(
    ///     "Validation Error",
    ///     "Please check the required fields.",
    ///     MessageType.Error,
    ///     "",
    ///     PagePriority.Default);
    /// </code>
    /// </example>
    public class PageMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageMessage"/> class.
        /// </summary>
        /// <param name="heading">The heading text for the message.</param>
        /// <param name="message">The message text content. Cannot be null or empty.</param>
        /// <param name="messageType">The type/severity of the message.</param>
        /// <param name="iconSrc">The optional icon source URL for the message. Use empty string if no icon is needed.</param>
        /// <param name="priority">The priority of the message. Use values from <see cref="PagePriority"/> for consistency.</param>
        public PageMessage(string heading, string message, PageMessageType messageType, string iconSrc, int priority)
        {
            this.Heading = heading;
            this.Message = message;
            this.MessageType = messageType;
            this.IconSrc = iconSrc;
            this.Priority = priority;
        }

        /// <summary>
        /// Gets or sets the heading text for the message.
        /// </summary>
        /// <value>
        /// A string containing the heading text that will be displayed prominently.
        /// This should be a concise summary of the message.
        /// </value>
        public string Heading { get; set; }

        /// <summary>
        /// Gets or sets the message text content.
        /// </summary>
        /// <value>
        /// A string containing the detailed message content that will be displayed to the user.
        /// This can contain HTML markup for formatting.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the type/severity of the message.
        /// </summary>
        /// <value>
        /// A <see cref="MessageType"/> value indicating the severity or type of the message.
        /// This affects how the message is styled and displayed to the user.
        /// </value>
        public PageMessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets the optional icon source URL for the message.
        /// </summary>
        /// <value>
        /// A string containing the URL or path to an icon image, or an empty string if no icon is needed.
        /// The icon will be displayed alongside the message to provide visual context.
        /// </value>
        public string IconSrc { get; set; }

        /// <summary>
        /// Gets or sets the priority of the message (lower number = higher priority).
        /// </summary>
        /// <value>
        /// An integer representing the display priority of the message.
        /// Messages with lower priority numbers will be displayed before those with higher numbers.
        /// Use constants from <see cref="PagePriority"/> for consistent priority values.
        /// </value>
        public int Priority { get; set; }
    }
}
