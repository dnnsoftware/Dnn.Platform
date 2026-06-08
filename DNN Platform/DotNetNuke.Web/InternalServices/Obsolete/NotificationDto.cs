// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using DotNetNuke.Internal.SourceGenerators;

/// <summary>A data transfer object with information about a notification.</summary>
[DnnDeprecated(10, 3, 3, "Use NotificationRequest")]
public partial class NotificationDTO
{
    /// <summary>Gets or sets the ID of the notification.</summary>
    public int NotificationId { get; set; }

    /// <summary>Converts this instance into a <see cref="NotificationRequest"/>.</summary>
    /// <returns>A <see cref="NotificationRequest"/> instance.</returns>
    public NotificationRequest ToNotificationRequest()
    {
        return new NotificationRequest
        {
            NotificationId = this.NotificationId,
        };
    }
}
