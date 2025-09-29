// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Pages
{
    /// <summary>
    /// Defines the types and severity levels for page messages.
    /// Message types determine how messages are styled and presented to users.
    /// </summary>
    /// <remarks>
    /// These enumeration values are used to categorize messages by their purpose and severity.
    /// The UI layer typically uses these values to apply appropriate styling, colors, and icons
    /// to provide visual context to users about the nature of the message.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Success message for completed operations
    /// var successMsg = new PageMessage("Operation Complete", "Data saved successfully", MessageType.Success, "", PagePriority.Default);
    ///
    /// // Warning message for potential issues
    /// var warningMsg = new PageMessage("Warning", "This action cannot be undone", MessageType.Warning, "", PagePriority.Default);
    ///
    /// // Error message for failed operations
    /// var errorMsg = new PageMessage("Error", "Failed to save data", MessageType.Error, "", PagePriority.Default);
    ///
    /// // Informational message for general notifications
    /// var infoMsg = new PageMessage("Notice", "System maintenance scheduled", MessageType.Info, "", PagePriority.Default);
    /// </code>
    /// </example>
    public enum PageMessageType
    {
        /// <summary>
        /// Success message type (value: 1).
        /// Used for messages indicating successful completion of operations.
        /// Typically displayed with green styling and success icons.
        /// </summary>
        /// <example>
        /// Use for: Data saved, user created, operation completed, etc.
        /// </example>
        Success = 1,

        /// <summary>
        /// Warning message type (value: 2).
        /// Used for messages indicating potential issues or important notices that require user attention.
        /// Typically displayed with yellow/orange styling and warning icons.
        /// </summary>
        /// <example>
        /// Use for: Validation warnings, deprecation notices, cautionary information, etc.
        /// </example>
        Warning = 2,

        /// <summary>
        /// Error message type (value: 3).
        /// Used for messages indicating failed operations or critical issues.
        /// Typically displayed with red styling and error icons.
        /// </summary>
        /// <example>
        /// Use for: Validation errors, operation failures, system errors, etc.
        /// </example>
        Error = 3,

        /// <summary>
        /// Informational message type (value: 4).
        /// Used for general notifications and informational content.
        /// Typically displayed with blue styling and info icons.
        /// </summary>
        /// <example>
        /// Use for: General notifications, tips, system status updates, etc.
        /// </example>
        Info = 4,
    }
}
