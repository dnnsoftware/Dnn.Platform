// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.EditBar.UI;

using System;

using Microsoft.Extensions.Logging;

/// <summary>Extension methods for <see cref="ILogger"/> for pre-defined logging messages.</summary>
/// <remarks>The Dnn.EditBar.UI project has been assigned event IDs from 6,000,000 to 6,099,999.</remarks>
internal static partial class LoggerMessages
{
    [LoggerMessage(EventId = 6_000_000, Level = LogLevel.Error, Message = "Unable to create {TypeFullName} while getting all edit bar menu items.")]
    public static partial void EditBarControllerUnableToCreateMenuItem(this ILogger logger, Exception exception, string typeFullName);
}
