// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;
    using System.ComponentModel;

    public partial class LogController
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in 7.3. Use GetLogTypeInfo and use the LoggingIsActive property.. Scheduled removal in v10.0.0.")]
        public bool LoggingIsEnabled(string logType, int portalID)
        {
            return LoggingProvider.Instance().LoggingIsEnabled(logType, portalID);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in 7.3. Use LoggingProvider.Instance().SupportsEmailNotification().. Scheduled removal in v10.0.0.")]
        public virtual bool SupportsEmailNotification()
        {
            return LoggingProvider.Instance().SupportsEmailNotification();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Deprecated in 7.3. Use LoggingProvider.Instance().SupportsInternalViewer().. Scheduled removal in v10.0.0.")]
        public virtual bool SupportsInternalViewer()
        {
            return LoggingProvider.Instance().SupportsInternalViewer();
        }
    }
}
