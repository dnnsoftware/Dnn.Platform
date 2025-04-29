// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog;

using System;

using DotNetNuke.Abstractions.Logging;

public partial class LogInfo
{
    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogGuid' instead. Scheduled removal in v11.0.0.")]
    public string LogGUID
    {
        get => ((ILogInfo)this).LogGuid;
        set => ((ILogInfo)this).LogGuid = value;
    }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogFileId' instead. Scheduled removal in v11.0.0.")]
    public string LogFileID
    {
        get => ((ILogInfo)this).LogFileId;
        set => ((ILogInfo)this).LogFileId = value;
    }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogUserId' instead. Scheduled removal in v11.0.0.")]
    public int LogUserID
    {
        get => ((ILogInfo)this).LogUserId;
        set => ((ILogInfo)this).LogUserId = value;
    }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogEventId' instead. Scheduled removal in v11.0.0.")]
    public int LogEventID
    {
        get => ((ILogInfo)this).LogEventId;
        set => ((ILogInfo)this).LogEventId = value;
    }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogPortalId' instead. Scheduled removal in v11.0.0.")]
    public int LogPortalID
    {
        get => ((ILogInfo)this).LogPortalId;
        set => ((ILogInfo)this).LogPortalId = value;
    }

    [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogConfigId' instead. Scheduled removal in v11.0.0.")]
    public string LogConfigID
    {
        get => ((ILogInfo)this).LogConfigId;
        set => ((ILogInfo)this).LogConfigId = value;
    }
}
