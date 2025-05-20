// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Log.EventLog
{
    using System;

    using DotNetNuke.Abstractions.Logging;

    /// <content>The obsolete properties for <see cref="LogInfo"/>.</content>
    public partial class LogInfo
    {
        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogGuid' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string LogGUID
        {
            get => ((ILogInfo)this).LogGuid;
            set => ((ILogInfo)this).LogGuid = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogFileId' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string LogFileID
        {
            get => ((ILogInfo)this).LogFileId;
            set => ((ILogInfo)this).LogFileId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogUserId' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int LogUserID
        {
            get => ((ILogInfo)this).LogUserId;
            set => ((ILogInfo)this).LogUserId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogEventId' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int LogEventID
        {
            get => ((ILogInfo)this).LogEventId;
            set => ((ILogInfo)this).LogEventId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogPortalId' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public int LogPortalID
        {
            get => ((ILogInfo)this).LogPortalId;
            set => ((ILogInfo)this).LogPortalId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant

        [Obsolete("Deprecated in DotNetNuke 9.8.0. Use 'DotNetNuke.Abstractions.Logging.ILogInfo.LogConfigId' instead. Scheduled removal in v11.0.0.")]
#pragma warning disable CS3005 // Identifier differing only in case is not CLS-compliant
        public string LogConfigID
        {
            get => ((ILogInfo)this).LogConfigId;
            set => ((ILogInfo)this).LogConfigId = value;
        }
#pragma warning restore CS3005 // Identifier differing only in case is not CLS-compliant
    }
}
