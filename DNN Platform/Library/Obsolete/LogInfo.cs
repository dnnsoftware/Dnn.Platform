// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    public partial class LogInfo
    {
        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogInfo.LogGuid' instead. Scheduled for removal in v11.0.0.")]
        public string LogGUID
        {
            get => this.LogGuid;
            set => this.LogGuid = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogInfo.LogFileId' instead. Scheduled for removal in v11.0.0.")]
        public string LogFileID
        {
            get => this.LogFileId;
            set => this.LogFileId = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogInfo.LogUserId' instead. Scheduled for removal in v11.0.0.")]
        public int LogUserID
        {
            get => this.LogUserId;
            set => this.LogUserId = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogInfo.LogEventId' instead. Scheduled for removal in v11.0.0.")]
        public int LogEventID
        {
            get => this.LogEventId;
            set => this.LogEventId = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogInfo.LogPortalId' instead. Scheduled for removal in v11.0.0.")]
        public int LogPortalID
        {
            get => this.LogPortalId;
            set => this.LogPortalId = value;
        }

        [Obsolete("Deprecated in 9.8.0. Use 'DotNetNuke.Services.Log.EventLog.LogInfo.LogConfigId' instead. Scheduled for removal in v11.0.0.")]
        public string LogConfigID
        {
            get => this.LogConfigId;
            set => this.LogConfigId = value;
        }
    }
}
