// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Application;

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

/// <summary>The method for triggering scheduled tasks.</summary>
public enum SchedulerMode
{
    /// <summary>The scheduler is disabled.</summary>
    Disabled = 0,

    /// <summary>The scheduler is running based on a timer.</summary>
    TimerMethod = 1,

    /// <inheritdoc cref="TimerMethod"/>
    [Obsolete("Use TimerMethod instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
    TIMER_METHOD = TimerMethod,

    /// <summary>The scheduler is running when triggered by HTTP requests.</summary>
    RequestMethod = 2,

    /// <inheritdoc cref="RequestMethod"/>
    [Obsolete("Use RequestMethod instead")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [SuppressMessage("Microsoft.Design", "CA1707:IdentifiersShouldNotContainUnderscores", Justification = "Breaking change")]
    REQUEST_METHOD = RequestMethod,
}
