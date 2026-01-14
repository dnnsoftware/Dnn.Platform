// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Logging;

/// <summary>Notification threshold time.</summary>
public enum NotificationThresholdTimeType
{
    /// <summary>No threshold time.</summary>
    None = 0,

    /// <summary>Seconds.</summary>
    Seconds = 1,

    /// <summary>Minutes.</summary>
    Minutes = 2,

    /// <summary>Hours.</summary>
    Hours = 3,

    /// <summary>Days.</summary>
    Days = 4,
}
