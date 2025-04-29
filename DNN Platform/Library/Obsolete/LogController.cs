// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System.ComponentModel;

using DotNetNuke.Internal.SourceGenerators;

public partial class LogController
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use GetLogTypeInfo and use the LoggingIsActive property.", RemovalVersion = 10)]
    public partial bool LoggingIsEnabled(string logType, int portalID)
    {
        return LoggingProvider.Instance().LoggingIsEnabled(logType, portalID);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use LoggingProvider.Instance().SupportsEmailNotification().", RemovalVersion = 10)]
    public virtual partial bool SupportsEmailNotification()
    {
        return LoggingProvider.Instance().SupportsEmailNotification();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [DnnDeprecated(7, 3, 0, "Use LoggingProvider.Instance().SupportsInternalViewer().", RemovalVersion = 10)]
    public virtual partial bool SupportsInternalViewer()
    {
        return LoggingProvider.Instance().SupportsInternalViewer();
    }
}
