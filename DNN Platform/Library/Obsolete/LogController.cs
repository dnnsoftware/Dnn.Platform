﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;

#endregion

namespace DotNetNuke.Services.Log.EventLog
{
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
